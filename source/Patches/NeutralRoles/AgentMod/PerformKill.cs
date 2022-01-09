using System;
using HarmonyLib;
using Hazel;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.AgentMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Agent)) return true;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            var role = Role.GetRole<Agent>(PlayerControl.LocalPlayer);

            if (__instance == role.IntelButton)
            {
                if (role.ClosestPlayer == null) return false;
                if (role.IntelTimer() != 0f) return false;
                if (!__instance.enabled) return false;
                var maxDistance = GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance];
                if (Vector2.Distance(role.ClosestPlayer.GetTruePosition(),
                    PlayerControl.LocalPlayer.GetTruePosition()) > maxDistance) return false;


                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.Intel, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(role.ClosestPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                role.IntelPlayers.Add(role.ClosestPlayer.PlayerId);
                role.LastIntel = DateTime.UtcNow;
            }
            return false;
        }
    }
}