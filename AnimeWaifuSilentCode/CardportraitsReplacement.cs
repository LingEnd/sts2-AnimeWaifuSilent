using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
public static class CardPortraitReplacementPatch
{
    private static readonly Dictionary<Type, string> Replacements = new()
    {
        // Silent 卡池
        { typeof(Accelerant), "res://AnimeWaifuSilent/card_portraits/silent/accelerant.png" }, // 触媒
        { typeof(Accuracy), "res://AnimeWaifuSilent/card_portraits/silent/accuracy.png" }, // 精准
        { typeof(Acrobatics), "res://AnimeWaifuSilent/card_portraits/silent/acrobatics.png" }, // 杂技
        { typeof(Adrenaline), "res://AnimeWaifuSilent/card_portraits/silent/adrenaline.png" }, // 肾上腺素
        { typeof(Afterimage), "res://AnimeWaifuSilent/card_portraits/silent/afterimage.png" }, // 余像
        { typeof(BladeDance), "res://AnimeWaifuSilent/card_portraits/silent/blade_dance.png" }, // 刀刃之舞
        { typeof(Blur), "res://AnimeWaifuSilent/card_portraits/silent/blur.png" }, // 残影
        { typeof(BulletTime), "res://AnimeWaifuSilent/card_portraits/silent/bullet_time.png" }, // 子弹时间
        { typeof(CalculatedGamble), "res://AnimeWaifuSilent/card_portraits/silent/calculated_gamble.png" }, // 计算下注
        { typeof(DaggerThrow), "res://AnimeWaifuSilent/card_portraits/silent/dagger_throw.png" }, // 投掷匕首
        { typeof(Dash), "res://AnimeWaifuSilent/card_portraits/silent/dash.png" }, // 冲刺
        { typeof(DefendSilent), "res://AnimeWaifuSilent/card_portraits/silent/defend_silent.png" }, // 防御
        { typeof(Distraction), "res://AnimeWaifuSilent/card_portraits/silent/distraction.png" }, // 声东击西
        { typeof(Envenom), "res://AnimeWaifuSilent/card_portraits/silent/envenom.png" }, // 涂毒
        { typeof(Expertise), "res://AnimeWaifuSilent/card_portraits/silent/expertise.png" }, // 独门绝技
        { typeof(Expose), "res://AnimeWaifuSilent/card_portraits/silent/expose.png" }, // 暴露
        { typeof(FanOfKnives), "res://AnimeWaifuSilent/card_portraits/silent/fan_of_knives.png" }, // 刀扇
        { typeof(Finisher), "res://AnimeWaifuSilent/card_portraits/silent/finisher.png" }, // 终结技
        { typeof(FlickFlack), "res://AnimeWaifuSilent/card_portraits/silent/flick_flack.png" }, // 翻越撑击
        { typeof(Footwork), "res://AnimeWaifuSilent/card_portraits/silent/footwork.png" }, // 灵动步法
        { typeof(GrandFinale), "res://AnimeWaifuSilent/card_portraits/silent/grand_finale.png" }, // 华丽收场
        { typeof(InfiniteBlades), "res://AnimeWaifuSilent/card_portraits/silent/infinite_blades.png" }, // 无尽刀刃
        { typeof(Malaise), "res://AnimeWaifuSilent/card_portraits/silent/malaise.png" }, // 萎靡
        { typeof(MementoMori), "res://AnimeWaifuSilent/card_portraits/silent/memento_mori.png" }, // 铭记死亡
        { typeof(Murder), "res://AnimeWaifuSilent/card_portraits/silent/murder.png" }, // 谋杀
        { typeof(Neutralize), "res://AnimeWaifuSilent/card_portraits/silent/neutralize.png" }, // 中和
        { typeof(Nightmare), "res://AnimeWaifuSilent/card_portraits/silent/nightmare.png" }, // 夜魇
        { typeof(NoxiousFumes), "res://AnimeWaifuSilent/card_portraits/silent/noxious_fumes.png" }, // 毒雾
        { typeof(Outmaneuver), "res://AnimeWaifuSilent/card_portraits/silent/outmaneuver.png" }, // 抢占先机
        { typeof(PiercingWail), "res://AnimeWaifuSilent/card_portraits/silent/piercing_wail.png" }, // 尖啸
        { typeof(Prepared), "res://AnimeWaifuSilent/card_portraits/silent/prepared.png" }, // 早有准备
        { typeof(Reflex), "res://AnimeWaifuSilent/card_portraits/silent/reflex.png" }, // 本能反应
        { typeof(Skewer), "res://AnimeWaifuSilent/card_portraits/silent/skewer.png" }, // 串刺
        { typeof(Slice), "res://AnimeWaifuSilent/card_portraits/silent/slice.png" }, // 切割
        { typeof(Strangle), "res://AnimeWaifuSilent/card_portraits/silent/strangle.png" }, // 紧勒
        { typeof(StrikeSilent), "res://AnimeWaifuSilent/card_portraits/silent/strike_silent.png" }, // 打击
        { typeof(Survivor), "res://AnimeWaifuSilent/card_portraits/silent/survivor.png" }, // 生存者
        { typeof(Tactician), "res://AnimeWaifuSilent/card_portraits/silent/tactician.png" }, // 战术大师
        { typeof(TheHunt), "res://AnimeWaifuSilent/card_portraits/silent/the_hunt.png" }, // 狩猎
        { typeof(WellLaidPlans), "res://AnimeWaifuSilent/card_portraits/silent/well_laid_plans.png" }, // 计划妥当

        // Colorless 卡池
        { typeof(Alchemize), "res://AnimeWaifuSilent/card_portraits/colorless/alchemize.png" }, // 炼制药水
        { typeof(Panache), "res://AnimeWaifuSilent/card_portraits/colorless/panache.png" }, // 神气制胜

        // Event 卡池
        { typeof(Apparition), "res://AnimeWaifuSilent/card_portraits/event/apparition.png" }, // 灵体
        { typeof(WraithForm), "res://AnimeWaifuSilent/card_portraits/event/wraith_form.png" }, // 幽魂形态

        // Curse 卡池
        { typeof(AscendersBane), "res://AnimeWaifuSilent/card_portraits/curse/ascenders_bane.png" }, // 进阶之灾

        // Necrobinder 卡池
        { typeof(Fear), "res://AnimeWaifuSilent/card_portraits/necrobinder/fear.png" }, // 恐惧

        // Status 卡池
        { typeof(Void), "res://AnimeWaifuSilent/card_portraits/status/void.png" }, // 虚空
        { typeof(Infection), "res://AnimeWaifuSilent/card_portraits/status/infection.png" }, // 感染

        // Token 卡池
        { typeof(Shiv), "res://AnimeWaifuSilent/card_portraits/token/shiv.png" }, // 小刀
    };

    static bool Prefix(CardModel __instance, ref Texture2D __result)
    {
        if (Replacements.TryGetValue(__instance.GetType(), out var path))
        {
            if (!ResourceLoader.Exists(path))
            {
                return true;
            }

            Texture2D portrait = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
            if (portrait != null)
            {
                __result = portrait;
                return false;
            }
        }

        return true;
    }
}
