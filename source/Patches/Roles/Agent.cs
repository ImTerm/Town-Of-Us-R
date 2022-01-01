using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using TownOfUs.Extensions;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Agent : Role
    {
        private KillButton intelButton;
        public bool AgentWins;
        public PlayerControl ClosestPlayer;
        public List<byte> IntelPlayers = new List<byte>();
        public DateTime LastIntel;


        public Agent(PlayerControl player) : base(player)
        {
            Name = "The Agent";
            ImpostorText = () => "Investigate players' teams to hijack them";
            TaskText = () => "Investigate players' teams to hijack them\nFake Tasks:";
            Color = new Color(0.2f, 0.2f, 0.2f, 1f);
            RoleType = RoleEnum.Agent;
            Faction = Faction.Neutral;
        }

        public KillButton IntelButton
        {
            get => intelButton;
            set
            {
                intelButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

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
            //System.Console.WriteLine("Reached Here - Glitch Edition");
            AgentWins = true;
        }

        public void Loses()
        {
            LostByRPC = true;
        }

        public bool CheckEveryoneDoused()
        {
            var arsoId = Player.PlayerId;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (
                    player.PlayerId == arsoId ||
                    player.Data.IsDead ||
                    player.Data.Disconnected
                ) continue;
                if (!IntelPlayers.Contains(player.PlayerId)) return false;
            }

            return true;
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
    }
}