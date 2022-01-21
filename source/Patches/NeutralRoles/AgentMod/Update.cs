using System.Linq;
using HarmonyLib;
using TownOfUs.ImpostorRoles.CamouflageMod;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.AgentMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Update
    {
        public static string NameText(PlayerControl player, string str = "", bool meeting = false)
        {
            if (CamouflageUnCamouflage.IsCamoed)
            {
                if (meeting && !CustomGameOptions.MeetingColourblind) return player.name + str;

                return "";
            }

            return player.name + str;
        }

        private static void UpdateMeeting(MeetingHud __instance, Agent agent)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!agent.IntelPlayers.Contains(player.PlayerId)) continue;
                foreach (var state in __instance.playerStates)
                {
                    if (player.PlayerId != state.TargetPlayerId) continue;
                    var roleType = Utils.GetRole(player);
                    switch (roleType)
                    {
                        case RoleEnum.Crewmate:
                            state.NameText.color = Color.green;
                            break;
                        case RoleEnum.Impostor:
                            state.NameText.color = Color.red;
                            break;
                        default:
                            var role = Role.GetRole(player);
                            state.NameText.color = role.FactionColor;
                            break;
                    }
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(HudManager __instance)
        {
            if (CustomGameOptions.IntelInfo == IntelInfo.Role) return;

            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;

            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Agent)) return;
            var agent = Role.GetRole<Agent>(PlayerControl.LocalPlayer);
            if (MeetingHud.Instance != null) UpdateMeeting(MeetingHud.Instance, agent);


            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!agent.IntelPlayers.Contains(player.PlayerId)) continue;
                var roleType = Utils.GetRole(player);
                player.nameText.transform.localPosition = new Vector3(0f, 2f, -0.5f);
                switch (roleType)
                {
                    case RoleEnum.Crewmate:
                        player.nameText.color = Color.green;
                        break;
                    case RoleEnum.Impostor:
                        player.nameText.color = Color.red;
                        break;
                    default:
                        var role = Role.GetRole(player);
                        player.nameText.color = role.FactionColor;
                        break;
                }
            }
        }
    }
}