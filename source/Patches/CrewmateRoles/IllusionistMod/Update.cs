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
        public static Sprite Illusion => TownOfUs.IllusionSprite;
        public static Sprite EndIllusion => TownOfUs.EndIllusionSprite;

        private static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist)) return;
            var role = Role.GetRole<Illusionist>(PlayerControl.LocalPlayer);

            if (role.IsUsingIllusion)
                __instance.KillButton.graphic.sprite = EndIllusion;
            else
                __instance.KillButton.graphic.sprite = Illusion;

            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                if (role != null)
                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist))
                        Role.GetRole<Illusionist>(PlayerControl.LocalPlayer).Update(__instance);
        }
    }
}