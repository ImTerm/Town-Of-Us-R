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

                role.LastIntel = DateTime.UtcNow;

                if (Role.GetRole(role.ClosestPlayer).Faction != role.GuessMode) return false;

                role.LastIntel = role.LastIntel.AddSeconds(CustomGameOptions.IntelCorrect * -1);

                role.IntelPlayers.Add(role.ClosestPlayer.PlayerId);
            }
            else if (__instance == role.CycleButton)
            {
                if (role.GuessMode == Faction.Crewmates)
                    role.GuessMode = Faction.Neutral;
                else if (role.GuessMode == Faction.Neutral)
                    role.GuessMode = Faction.Impostors;
                else if (role.GuessMode == Faction.Impostors)
                    role.GuessMode = Faction.Crewmates;
            }
            return false;
        }
    }
}