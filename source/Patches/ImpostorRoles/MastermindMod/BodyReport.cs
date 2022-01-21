using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.ImpostorRoles.MastermindMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    internal class BodyReport
    {
        private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo info)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Mastermind)) return;

            Role.GetRole<Mastermind>(__instance).Reported.Add(info.PlayerId);
        }
    }
}