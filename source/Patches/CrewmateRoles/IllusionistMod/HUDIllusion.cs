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
                // || (role.IsUsingIllusion && CustomGameOptions.IllusionEndCooldown >= CustomGameOptions.IllusionDuration && !CustomGameOptions.InfiniteIllusion))
            {
                illusionButton.SetCoolDown(role.IllusionTimer(), CustomGameOptions.IllusionCooldown);
            }
            else {
                if (CustomGameOptions.InfiniteIllusion)
                    illusionButton.SetCoolDown(0f, CustomGameOptions.IllusionDuration);
                else
                    illusionButton.SetCoolDown(role.TimeRemaining,
                        CustomGameOptions.IllusionDuration);
            }
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

            if (role.EndIllusionButton != null) {
                
                role.EndIllusionButton.SetCoolDown(role.EndIllusionTimer(), CustomGameOptions.IllusionEndCooldown);

                var renderer2 = role.EndIllusionButton.graphic;
                if (!illusionButton.isCoolingDown)
                {
                    renderer2.color = Palette.EnabledColor;
                    renderer2.material.SetFloat("_Desat", 0f);
                    return;
                }

                renderer2.color = Palette.DisabledClear;
                renderer2.material.SetFloat("_Desat", 1f);
            }
        }
    }
}