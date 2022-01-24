using System;
using UnityEngine;
using TMPro;

namespace TownOfUs.Roles
{
    public class Sheriff : Role
    {
        public int UsesLeft;
        public TextMeshPro UsesText;
        public bool UsedThisRound;

        public bool ButtonUsable => UsesLeft != 0 && (!UsedThisRound || !CustomGameOptions.SheriffPerRound);

        public Sheriff(PlayerControl player) : base(player)
        {
            Name = "Sheriff";
            ImpostorText = () => "Shoot the <color=#FF0000FF>Impostor</color>";
            TaskText = () => "Kill off the impostor but don't kill crewmates.";
            Color = Color.yellow;
            RoleType = RoleEnum.Sheriff;
            UsesLeft = (int) CustomGameOptions.SheriffMaxUses;
            if (UsesLeft == 0) UsesLeft = -1;
            UsedThisRound = false;
        }

        public PlayerControl ClosestPlayer;
        public DateTime LastKilled { get; set; }

        public float SheriffKillTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastKilled;
            var num = CustomGameOptions.SheriffKillCd * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }

        internal override bool Criteria()
        {
            return CustomGameOptions.ShowSheriff || base.Criteria();
        }
    }
}