using HarmonyLib;
using TMPro;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
using Hazel;
using System.Linq;
namespace TownOfUs.NeutralRoles.AgentMod
{
    public class MeetingHijack
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public class MeetingHudUpdate
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (__instance.state == MeetingHud.VoteStates.Animating) return;

                var Hijacked = false;
                
                if (Role.GetRole(PlayerControl.LocalPlayer).Faction == Faction.Crewmates)
                {
                    if (PlayerControl.AllPlayerControls.ToArray().Any(x => x != null && x.Data != null && x.Is(RoleEnum.Agent) && Role.GetRole<Agent>(x).CrewmatesHijacked))
                        Hijacked = true;
                }
                else if (Role.GetRole(PlayerControl.LocalPlayer).Faction == Faction.Neutral)
                {
                    if (PlayerControl.AllPlayerControls.ToArray().Any(x => x != null && x.Data != null && x.Is(RoleEnum.Agent) && Role.GetRole<Agent>(x).NeutralsHijacked))
                        Hijacked = true;
                }
                else if (Role.GetRole(PlayerControl.LocalPlayer).Faction == Faction.Impostors)
                {
                    if (PlayerControl.AllPlayerControls.ToArray().Any(x => x != null && x.Data != null && x.Is(RoleEnum.Agent) && Role.GetRole<Agent>(x).ImpostorsHijacked))
                        Hijacked = true;
                }

                if (!Hijacked) return;

                var HijackText = Object.Instantiate(__instance.TitleText);
                HijackText.transform.position.Set(HijackText.transform.position.x, HijackText.transform.position.y - 10f, HijackText.transform.position.z);
                HijackText.text = "The Agent is Hijacking your team! Find out who they are and stop them!";
                HijackText.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class MeetingHudStart
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (!PlayerControl.LocalPlayer.Is(RoleEnum.Agent)) return;
                var role = Role.GetRole<Agent>(PlayerControl.LocalPlayer);

                if (role.CheckCrewmateHijack()) role.CrewmatesHijacked = true;
                if (role.CheckNeutralHijack()) role.NeutralsHijacked = true;
                if (role.CheckImpostorHijack()) role.ImpostorsHijacked = true;

                var write = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.Hijack, SendOption.Reliable, -1);
                write.Write(PlayerControl.LocalPlayer.PlayerId);
                write.Write(role.CrewmatesHijacked);
                write.Write(role.NeutralsHijacked);
                write.Write(role.ImpostorsHijacked);
                AmongUsClient.Instance.FinishRpcImmediately(write);
            }
        }
    }
}