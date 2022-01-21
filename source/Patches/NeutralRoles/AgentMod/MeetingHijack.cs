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
            public static TextMeshPro HijackText;

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
                System.Console.WriteLine("Reached here 1");

                if (HijackText == null)
                {
                    HijackText = Object.Instantiate(__instance.TitleText);
                }
                System.Console.WriteLine("Reached here 2");
                HijackText.transform.position.Set(Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).x + (Camera.main.pixelWidth / 2),
                    Camera.main.ScreenToWorldPoint(new Vector3(0, 0)).y + (Camera.main.pixelHeight / 2),
                    HijackText.transform.position.z);
                System.Console.WriteLine(HijackText.transform.position);
                HijackText.text = "The Agent is Hijacking your team! Find out who they are and stop them!";
                System.Console.WriteLine(HijackText.text);
                HijackText.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                System.Console.WriteLine("Reached here 3");
                
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