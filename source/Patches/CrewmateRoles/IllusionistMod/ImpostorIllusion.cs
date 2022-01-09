using HarmonyLib;
using UnityEngine;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using System.Linq;
using System.Collections.Generic;

namespace TownOfUs
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    class HudUpdateManager
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (!PlayerControl.LocalPlayer.Data.IsImpostor()) return;
            if (!CustomGameOptions.ImpIllusion && !CustomGameOptions.IllusionedImpKill) return;

            List<PlayerControl> ValidPlayers = new List<PlayerControl>();

            var maxDistance = GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance];

            foreach (var player in PlayerControl.AllPlayerControls)
                if (PlayerControl.LocalPlayer.PlayerId != player.PlayerId && (!player.Data.IsImpostor() || Role.GetRole(player).IllusionTarget != null))
                {
                    if (Vector2.Distance(player.GetTruePosition(), PlayerControl.LocalPlayer.GetTruePosition()) <= maxDistance)
                        ValidPlayers.Add(player);
                    if (CustomGameOptions.ImpIllusion)
                        player.nameText.color = Color.white;
                }
                else if (CustomGameOptions.ImpIllusion)
                    player.nameText.color = Color.red;
                    
            if (CustomGameOptions.IllusionedImpKill)
                __instance.KillButton.SetTarget(Utils.getClosestPlayer(PlayerControl.LocalPlayer, ValidPlayers));
        }
    }
}