using System.Linq;
using HarmonyLib;
using InnerNet;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.IllusionistMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Update
    {
        public static Sprite EndIllusion => TownOfUs.EndIllusionSprite;

        private static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist)) return;
            var role = Role.GetRole<Illusionist>(PlayerControl.LocalPlayer);

            if ((CustomGameOptions.IllusionEndCooldown < CustomGameOptions.IllusionDuration || CustomGameOptions.InfiniteIllusion) &&
                role.IsUsingIllusion)
            {
                if (role.EndIllusionButton == null) {
                    role.EndIllusionButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                    role.EndIllusionButton.graphic.enabled = true;
                    role.EndIllusionButton.graphic.sprite = EndIllusion;
                    role.EndIllusionButton.GetComponent<AspectPosition>().DistanceFromEdge = TownOfUs.ButtonPosition;
                    role.EndIllusionButton.gameObject.SetActive(false);
                    role.EndIllusionButton.GetComponent<AspectPosition>().Update();
                }
            }
            else
                role.EndIllusionButton = null;

            // if (!role.IsUsingIllusion ||
            //     (role.IsUsingIllusion && CustomGameOptions.IllusionEndCooldown >= CustomGameOptions.IllusionDuration && !CustomGameOptions.InfiniteIllusion))
            // {
            //     __instance.KillButton.graphic.sprite = Illusion;
            //     // __instance.KillButton.SetCoolDown(illusionist.IllusionTimer(), CustomGameOptions.IllusionCooldown);
            // }
            // else if (role.IsUsingIllusion && (CustomGameOptions.IllusionEndCooldown < CustomGameOptions.IllusionDuration || CustomGameOptions.InfiniteIllusion))
            // {
            //     role.EndIllusionButton.graphic.sprite = EndIllusion;
            //     // __instance.KillButton.SetCoolDown(illusionist.EndIllusionTimer(), CustomGameOptions.IllusionEndCooldown);
            // }
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                if (role != null)
                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist))
                        Role.GetRole<Illusionist>(PlayerControl.LocalPlayer).Update(__instance);
        }
    }
}