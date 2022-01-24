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
                if (role.ButtonUsable)
                    mediateButton.SetCoolDown(role.MediateTimer(), CustomGameOptions.MediateCooldown);
            }

            if (role.UsesText == null && role.UsesLeft > 0)
            {
                role.UsesText = Object.Instantiate(mediateButton.cooldownTimerText, mediateButton.transform);
                role.UsesText.gameObject.SetActive(true);
                role.UsesText.transform.localPosition = new Vector3(
                    role.UsesText.transform.localPosition.x + 0.26f,
                    role.UsesText.transform.localPosition.y + 0.29f,
                    role.UsesText.transform.localPosition.z);
                role.UsesText.transform.localScale = role.UsesText.transform.localScale * 0.6f;
                role.UsesText.alignment = TMPro.TextAlignmentOptions.Right;
                role.UsesText.fontStyle = TMPro.FontStyles.Bold;
            }
            if (role.UsesText != null)
            {
                role.UsesText.text = role.UsesLeft + "";
            }
            
            var renderer = mediateButton.graphic;
            if (!mediateButton.isCoolingDown && role.ButtonUsable)
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                role.UsesText.color = Palette.EnabledColor;
                role.UsesText.material.SetFloat("_Desat", 0f);
                return;
            }
            
            renderer.color = Palette.DisabledClear;
            renderer.material.SetFloat("_Desat", 1f);
            role.UsesText.color = Palette.DisabledClear;
            role.UsesText.material.SetFloat("_Desat", 1f);
            
        }
    }
}