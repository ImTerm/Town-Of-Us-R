using HarmonyLib;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.MediumMod
{
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    public class NoVitals
    {
        public static bool Prefix(VitalsMinigame __instance)
        {
            if (CustomGameOptions.MediumVitals) return true;
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Medium) && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                Object.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }
}