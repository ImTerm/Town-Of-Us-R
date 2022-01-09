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
                            state.NameText.color =
                                CustomGameOptions.IntelInfo == IntelInfo.Faction ? Color.green : Color.white;
                            state.NameText.text = NameText(player,
                                CustomGameOptions.IntelInfo == IntelInfo.Role ? " (Crew)" : "", true);
                            break;
                        case RoleEnum.Impostor:
                            state.NameText.color = CustomGameOptions.IntelInfo == IntelInfo.Faction
                                ? Color.red
                                : Palette.ImpostorRed;
                            state.NameText.text = NameText(player,
                                CustomGameOptions.IntelInfo == IntelInfo.Role ? " (Imp)" : "", true);
                            break;
                        default:
                            var role = Role.GetRole(player);
                            state.NameText.color = CustomGameOptions.IntelInfo == IntelInfo.Faction
                                ? role.FactionColor
                                : role.Color;
                            state.NameText.text = NameText(player,
                                CustomGameOptions.IntelInfo == IntelInfo.Role ? $" ({role.Name})" : "", true);
                            break;
                    }
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(HudManager __instance)

        {
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
                        player.nameText.color =
                            CustomGameOptions.IntelInfo == IntelInfo.Faction ? Color.green : Color.white;
                        player.nameText.text = NameText(player,
                            CustomGameOptions.IntelInfo == IntelInfo.Role ? "\n Crewmate" : "");
                        break;
                    case RoleEnum.Impostor:
                        player.nameText.color = CustomGameOptions.IntelInfo == IntelInfo.Faction
                            ? Color.red
                            : Palette.ImpostorRed;
                        player.nameText.text = NameText(player,
                            CustomGameOptions.IntelInfo == IntelInfo.Role ? "\n Impostor" : "");
                        break;
                    default:
                        var role = Role.GetRole(player);
                        player.nameText.color = CustomGameOptions.IntelInfo == IntelInfo.Faction
                            ? role.FactionColor
                            : role.Color;
                        player.nameText.text = NameText(player,
                            CustomGameOptions.IntelInfo == IntelInfo.Role ? $"\n {role.Name}" : "");
                        break;
                }
            }
        }
    }
}