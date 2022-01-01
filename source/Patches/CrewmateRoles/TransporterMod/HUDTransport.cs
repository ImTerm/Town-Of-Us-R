using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.TransporterMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class HUDTransport
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Transporter)) return;
            var data = PlayerControl.LocalPlayer.Data;
            if (data.IsDead) return;
            var transportButton = DestroyableSingleton<HudManager>.Instance.KillButton;

            var role = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);

            transportButton.gameObject.SetActive(!MeetingHud.Instance);

            transportButton.SetCoolDown(role.TransportTimer(), CustomGameOptions.TransportCooldown);

            var renderer = transportButton.graphic;
            if (!transportButton.isCoolingDown)
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