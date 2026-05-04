using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

public static class BaseLibIntegration
{
    private static bool _checked;
    private static bool _isLoaded;
    private static Type? _dynamicConfigType;
    private static object? _configInstance;

    public static bool IsBaseLibLoaded
    {
        get
        {
            if (!_checked)
            {
                _checked = true;
                _isLoaded = CheckBaseLib();
            }

            return _isLoaded;
        }
    }

    private static bool CheckBaseLib()
    {
        try
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "BaseLib")
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static void RegisterBaseLibConfig()
    {
        if (!IsBaseLibLoaded)
        {
            return;
        }

        try
        {
            Assembly? baseLibAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "BaseLib");

            if (baseLibAssembly == null)
            {
                return;
            }

            Type? simpleModConfigType = baseLibAssembly.GetType("BaseLib.Config.SimpleModConfig");
            Type? registryType = baseLibAssembly.GetType("BaseLib.Config.ModConfigRegistry");

            if (simpleModConfigType == null || registryType == null)
            {
                return;
            }

            MethodInfo? registerMethod = registryType.GetMethod(
                "Register",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), simpleModConfigType },
                null);

            if (registerMethod == null)
            {
                return;
            }

            object? config = CreateConfig(baseLibAssembly, simpleModConfigType);
            if (config == null)
            {
                return;
            }

            _dynamicConfigType = config.GetType();
            _configInstance = config;

            registerMethod.Invoke(null, new object[] { ModConfig.ModId, config });
            MainFile.Logger.Info("[Config] BaseLib config registered");
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[Config] Failed to register BaseLib config: {e.Message}");
        }
    }

    public static bool GetAncientStyleConfig(string cardTypeName)
    {
        try
        {
            if (_configInstance != null && _dynamicConfigType != null)
            {
                string propertyName = $"{cardTypeName}AncientStyle";
                PropertyInfo? prop = _dynamicConfigType.GetProperty(propertyName);

                if (prop != null && prop.CanRead)
                {
                    MethodInfo? getMethod = prop.GetGetMethod();
                    if (getMethod != null)
                    {
                        object? result = getMethod.Invoke(_configInstance, null);
                        if (result is bool boolValue)
                        {
                            MainFile.Logger.Debug($"[Config] {propertyName} = {boolValue}");
                            return boolValue;
                        }
                    }

                    MainFile.Logger.Debug($"[Config] Could not get getMethod for {propertyName}");
                }
                else
                {
                    MainFile.Logger.Debug($"[Config] Property {propertyName} not found");
                }
            }
            else
            {
                MainFile.Logger.Debug($"[Config] Config instance not available");
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[Config] Error getting config: {e.Message}");
        }

        MainFile.Logger.Debug($"[Config] Using default value true for {cardTypeName}");
        return true;
    }

    private static object? CreateConfig(Assembly baseLibAssembly, Type baseType)
    {
        try
        {
            Type? configType = BuildConfigType(baseLibAssembly, baseType);
            if (configType == null)
            {
                return null;
            }

            return Activator.CreateInstance(configType);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[Config] Error creating config: {e.Message}");
            return null;
        }
    }

    private static Type? BuildConfigType(Assembly baseLibAssembly, Type baseType)
    {
        try
        {
            AssemblyName name = new AssemblyName($"AnimeWaifuSilentDynamicConfig_{Guid.NewGuid():N}");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                name,
                AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("ConfigModule");

            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "AnimeWaifuSilent.Config.AnimeWaifuSilentConfig",
                TypeAttributes.Public | TypeAttributes.Class,
                baseType);

            Type? tipsAttrType = baseLibAssembly.GetType("BaseLib.Config.ConfigHoverTipsByDefaultAttribute");
            if (tipsAttrType != null)
            {
                ConstructorInfo? ctor = tipsAttrType.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(ctor, Array.Empty<object>());
                    typeBuilder.SetCustomAttribute(attrBuilder);
                }
            }

            Type? hoverTipAttrType = baseLibAssembly.GetType("BaseLib.Config.ConfigHoverTipAttribute");
            var ancientCardTypes = CardReplacementConfig.AncientCardTypes;

            foreach (string cardTypeName in ancientCardTypes)
            {
                string propertyName = $"{cardTypeName}AncientStyle";
                CreateStaticPropertyWithAttributes(typeBuilder, propertyName, typeof(bool), hoverTipAttrType, true);
            }

            return typeBuilder.CreateType();
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[Config] Error building type: {e.Message}");
            return null;
        }
    }

    private static void CreateStaticPropertyWithAttributes(
        TypeBuilder tb,
        string name,
        Type type,
        Type? hoverTipAttr,
        object defaultValue)
    {
        FieldBuilder fb = tb.DefineField($"_{name}", type, FieldAttributes.Private | FieldAttributes.Static);

        if (defaultValue != null)
        {
            fb.SetConstant(defaultValue);
        }

        PropertyBuilder pb = tb.DefineProperty(name, PropertyAttributes.None, type, null);

        MethodBuilder getMb = tb.DefineMethod(
            $"get_{name}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Static,
            type,
            Type.EmptyTypes);
        ILGenerator getIl = getMb.GetILGenerator();
        getIl.Emit(OpCodes.Ldsfld, fb);
        getIl.Emit(OpCodes.Ret);

        MethodBuilder setMb = tb.DefineMethod(
            $"set_{name}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Static,
            null,
            new[] { type });
        ILGenerator setIl = setMb.GetILGenerator();
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Stsfld, fb);
        setIl.Emit(OpCodes.Ret);

        pb.SetGetMethod(getMb);
        pb.SetSetMethod(setMb);

        if (hoverTipAttr != null)
        {
            ConstructorInfo? ctor = hoverTipAttr.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(ctor, Array.Empty<object>());
                pb.SetCustomAttribute(attrBuilder);
            }
        }
    }
}
