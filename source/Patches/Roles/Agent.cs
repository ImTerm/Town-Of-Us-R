using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TownOfUs.NeutralRoles.AgentMod;
using TownOfUs.Extensions;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Agent : Role
    {
        public KillButton IntelButton;
        public KillButton CycleButton;
        public Faction GuessMode;

        public PlayerControl ClosestPlayer;
        public List<byte> IntelPlayers = new List<byte>();

        public DateTime LastIntel;
        public DateTime LastKill;

        public bool AgentWins;

        public bool CrewmatesHijacked;
        public bool NeutralsHijacked;
        public bool ImpostorsHijacked;

        public Agent(PlayerControl player) : base(player)
        {
            Name = "The Agent";
            ImpostorText = () => "Investigate players' teams to hijack them";
            TaskText = () => "Investigate players' teams to hijack them\nFake Tasks:";
            Color = new Color(0.2f, 0.2f, 0.2f, 1f);
            RoleType = RoleEnum.Agent;
            Faction = Faction.Neutral;
            GuessMode = Faction.Crewmates;
            IntelButton = null;
            CycleButton = null;
            AgentWins = false;
            CrewmatesHijacked = false;
            NeutralsHijacked = false;
            ImpostorsHijacked = false;
        }

        // public KillButton IntelButton
        // {
        //     get => intelButton;
        //     set
        //     {
        //         intelButton = value;
        //         ExtraButtons.Clear();
        //         ExtraButtons.Add(value);
        //     }
        // }
        // public KillButton CycleButton
        // {
        //     get => cycleButton;
        //     set
        //     {
        //         cycleButton = value;
        //         ExtraButtons.Add(value);
        //     }
        // }

        // internal override bool EABBNOODFGL(ShipStatus __instance)
        // {
        //     if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) == 0)
        //     {
        //         var writer = AmongUsClient.Instance.StartRpcImmediately(
        //             PlayerControl.LocalPlayer.NetId,
        //             (byte) CustomRPC.AgentWin,
        //             SendOption.Reliable,
        //             -1
        //         );
        //         writer.Write(Player.PlayerId);
        //         Wins();
        //         AmongUsClient.Instance.FinishRpcImmediately(writer);
        //         Utils.EndGame();
        //         return false;
        //     }

        //     if (IgniteUsed || Player.Data.IsDead) return true;

        //     return !CustomGameOptions.AgentGameEnd;
        // }


        public void Wins()
        {
            AgentWins = true;
        }

        public void Loses()
        {
            LostByRPC = true;
        }

        public bool CheckCrewmateHijack()
        {
            return PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x.Data != null &&
                Role.GetRole(x).Faction == Faction.Crewmates && !x.Data.IsDead && !x.Data.Disconnected && IntelPlayers.Contains(x.PlayerId))
                == PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x.Data != null &&
                Role.GetRole(x).Faction == Faction.Crewmates && !x.Data.IsDead && !x.Data.Disconnected);
        }

        public bool CheckNeutralHijack()
        {
            return PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x.Data != null &&
                Role.GetRole(x).Faction == Faction.Neutral && !x.Data.IsDead && !x.Data.Disconnected && IntelPlayers.Contains(x.PlayerId))
                == PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x.Data != null &&
                Role.GetRole(x).Faction == Faction.Neutral && !x.Data.IsDead && !x.Data.Disconnected);
        }

        public bool CheckImpostorHijack()
        {
            return PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x.Data != null &&
                Role.GetRole(x).Faction == Faction.Impostors && !x.Data.IsDead && !x.Data.Disconnected && IntelPlayers.Contains(x.PlayerId))
                == PlayerControl.AllPlayerControls.ToArray().Count(x => x != null && x.Data != null &&
                Role.GetRole(x).Faction == Faction.Impostors && !x.Data.IsDead && !x.Data.Disconnected);
        }

        protected override void IntroPrefix(IntroCutscene._CoBegin_d__18 __instance)
        {
            var agentTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            agentTeam.Add(PlayerControl.LocalPlayer);
            __instance.yourTeam = agentTeam;
        }

        public float IntelTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastIntel;
            var num = CustomGameOptions.IntelCooldown * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
        
        public float KillTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastKill;
            var num = PlayerControl.GameOptions.KillCooldown * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
    }
}