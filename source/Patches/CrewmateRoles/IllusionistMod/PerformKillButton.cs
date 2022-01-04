using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Reactor;
using Reactor.Extensions;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.MedicMod;
using TownOfUs.CrewmateRoles.SnitchMod;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.CrewmateRoles.IllusionistMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPriority(Priority.Last)]
    public class PerformKillButton

    {
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist)) return true;
            var role = Role.GetRole<Illusionist>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!__instance.enabled) return false;

            if (!role.IsUsingIllusion && role.IllusionTimer() == 0f && role.IllusionList1 == null && role.IllusionList2 == null)
            {
                role.PressedButton = true;
                role.MenuClick = true;
            }
            else if (role.IsUsingIllusion && role.EndIllusionTimer() == 0f)
                role.IsUsingIllusion = false;

            return false;
        }
    }
}