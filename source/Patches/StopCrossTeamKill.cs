using HarmonyLib;
using Hazel;
using Reactor;
using TownOfUs.Roles;
using TownOfUs.Extensions;
using TownOfUs.CrewmateRoles.MedicMod;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class StopCrossTeamKill
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            if (!PlayerControl.LocalPlayer.Data.IsImpostor()) return true;
            var target = __instance.currentTarget;
            if (target == null) return true;

            if (Utils.isGlitchBlocked(target)
                && !Role.GetRole(PlayerControl.LocalPlayer).ProtectRevealed.Contains(target.PlayerId)) 
                {

                if (__instance.isActiveAndEnabled && !__instance.isCoolingDown)
                {
                    PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);

                    if (CustomGameOptions.GlitchShieldReveal)
                        Coroutines.Start(Utils.FlashCoroutine(Color.green));
                    else if (CustomGameOptions.MedicOn >= 1 && CustomGameOptions.NotificationShield == NotificationOptions.Everyone)
                        Coroutines.Start(Utils.FlashCoroutine(new Color(0f, 0.5f, 0f, 1f)));

                    Role.GetRole(PlayerControl.LocalPlayer).ProtectRevealed.Add(target.PlayerId);
                }
                return false;
            }
            return true;
        }
    }
}