using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.IllusionistMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class HUDIllusion
    {
        
        // public static Sprite Illusion => TownOfUs.IllusionSprite;
        // public static Sprite EndIllusion => TownOfUs.EndIllusionSprite;

        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist)) return;
            var data = PlayerControl.LocalPlayer.Data;
            if (data.IsDead) return;
            var illusionButton = DestroyableSingleton<HudManager>.Instance.KillButton;

            var role = Role.GetRole<Illusionist>(PlayerControl.LocalPlayer);

            illusionButton.gameObject.SetActive(!data.IsDead && !MeetingHud.Instance);

            if (!role.IsUsingIllusion)
                illusionButton.SetCoolDown(role.IllusionTimer(), CustomGameOptions.IllusionCooldown);
            else
                illusionButton.SetCoolDown(role.EndIllusionTimer(), CustomGameOptions.IllusionEndCooldown);
            // else if (role.IsUsingIllusion && (CustomGameOptions.IllusionEndCooldown < CustomGameOptions.IllusionDuration || CustomGameOptions.InfiniteIllusion))
            // {
            //     illusionButton.SetCoolDown(role.EndIllusionTimer(), CustomGameOptions.IllusionEndCooldown);
            // }

            var renderer = illusionButton.graphic;
            if (!illusionButton.isCoolingDown)
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                return;
            }

            renderer.color = Palette.DisabledClear;
            renderer.material.SetFloat("_Desat", 1f);
            
        }
    }
}