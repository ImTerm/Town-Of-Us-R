using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.MediumMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class HUDMediate
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Medium)) return;
            var data = PlayerControl.LocalPlayer.Data;
            var mediateButton = DestroyableSingleton<HudManager>.Instance.KillButton;

            var role = Role.GetRole<Medium>(PlayerControl.LocalPlayer);

            if (data.IsDead)
            {
                mediateButton.gameObject.SetActive(false);
            }
            else
            {
                mediateButton.gameObject.SetActive(!MeetingHud.Instance);
                mediateButton.SetCoolDown(role.MediateTimer(), CustomGameOptions.MediateCooldown);
            }
            
            var renderer = mediateButton.graphic;
            if (!mediateButton.isCoolingDown)
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