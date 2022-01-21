using System.Linq;
using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.NeutralRoles.AgentMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static Sprite CrewmateSprite => TownOfUs.CrewmateGuessSprite;
        public static Sprite NeutralSprite => TownOfUs.NeutralGuessSprite;
        public static Sprite ImpostorSprite => TownOfUs.ImpostorGuessSprite;
        public static Sprite CycleSprite => TownOfUs.CycleButtonSprite;
        
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Agent)) return;
            var role = Role.GetRole<Agent>(PlayerControl.LocalPlayer);
            var KillButton = __instance.KillButton;

            if (role.IntelButton == null)
            {
                role.IntelButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.IntelButton.graphic.enabled = true;
                role.IntelButton.gameObject.SetActive(true);
            }
            role.IntelButton.transform.position = new Vector3(
                Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).x + 0.75f,
                __instance.UseButton.transform.position.y, __instance.UseButton.transform.position.z);

            if (role.CycleButton == null)
            {
                role.CycleButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.CycleButton.graphic.enabled = true;
                role.CycleButton.gameObject.SetActive(true);
            }
            role.CycleButton.transform.position = new Vector3(role.IntelButton.transform.position.x,
                role.CycleButton.transform.position.y, __instance.UseButton.transform.position.z);

            if (role.GuessMode == Faction.Crewmates)
                role.IntelButton.graphic.sprite = CrewmateSprite;
            else if (role.GuessMode == Faction.Neutral)
                role.IntelButton.graphic.sprite = NeutralSprite;
            else if (role.GuessMode == Faction.Impostors)
                role.IntelButton.graphic.sprite = ImpostorSprite;

            role.CycleButton.graphic.sprite = CycleSprite;

            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                KillButton.gameObject.SetActive(false);
                role.IntelButton.gameObject.SetActive(false);
                role.CycleButton.gameObject.SetActive(false);
            }
            else
            {
                if (CustomGameOptions.ImpHijackKill && role.ImpostorsHijacked)
                {
                    KillButton.enabled = true;
                    KillButton.gameObject.SetActive(!MeetingHud.Instance);
                    KillButton.SetCoolDown(role.KillTimer(), PlayerControl.GameOptions.KillCooldown);
                    Utils.SetTarget(ref role.ClosestPlayer, KillButton);
                }
                
                role.IntelButton.gameObject.SetActive(!MeetingHud.Instance);
                var notInteld = PlayerControl.AllPlayerControls.ToArray().Where(x => !role.IntelPlayers.Contains(x.PlayerId)).ToList();
                Utils.SetTarget(ref role.ClosestPlayer, role.IntelButton, float.NaN, notInteld);
                role.IntelButton.SetCoolDown(role.IntelTimer(), CustomGameOptions.IntelCooldown);
                
                role.CycleButton.gameObject.SetActive(!MeetingHud.Instance);
                role.CycleButton.SetCoolDown(0f, 1f);
            }

            role.CycleButton.graphic.color = Palette.EnabledColor;
            role.CycleButton.graphic.material.SetFloat("_Desat", 0f);

            if (!role.IntelButton.isCoolingDown)
            {
                role.IntelButton.graphic.color = Palette.EnabledColor;
                role.IntelButton.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                role.IntelButton.graphic.color = Palette.DisabledClear;
                role.IntelButton.graphic.material.SetFloat("_Desat", 1f);
            }

            if (!KillButton.isCoolingDown)
            {
                KillButton.graphic.color = Palette.EnabledColor;
                KillButton.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                KillButton.graphic.color = Palette.DisabledClear;
                KillButton.graphic.material.SetFloat("_Desat", 1f);
            }
        }
    }
}