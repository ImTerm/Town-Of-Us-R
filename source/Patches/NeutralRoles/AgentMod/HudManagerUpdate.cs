using System.Linq;
using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.AgentMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static Sprite IntelSprite => TownOfUs.IntelSprite;
        
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Agent)) return;
            var role = Role.GetRole<Agent>(PlayerControl.LocalPlayer);
            if (role.IntelButton == null)
            {
                role.IntelButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.IntelButton.graphic.enabled = true;
                role.IntelButton.GetComponent<AspectPosition>().DistanceFromEdge = TownOfUs.ButtonPosition;
                role.IntelButton.gameObject.SetActive(false);
            }
            role.IntelButton.GetComponent<AspectPosition>().Update();
            role.IntelButton.graphic.sprite = IntelSprite;
            role.IntelButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && !MeetingHud.Instance);

            var notInteld = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => !role.IntelPlayers.Contains(x.PlayerId))
                .ToList();

            Utils.SetTarget(ref role.ClosestPlayer, role.IntelButton, float.NaN, notInteld);

            role.IntelButton.SetCoolDown(role.IntelTimer(), CustomGameOptions.IntelCooldown);

            if (!role.IntelButton.isCoolingDown)
            {
                role.IntelButton.graphic.color = Palette.EnabledColor;
                role.IntelButton.graphic.material.SetFloat("_Desat", 0f);
                return;
            }

            role.IntelButton.graphic.color = Palette.DisabledClear;
            role.IntelButton.graphic.material.SetFloat("_Desat", 1f);
        }
    }
}