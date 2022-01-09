using System;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.NeutralRoles.AgentMod
{
    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__18), nameof(IntroCutscene._CoBegin_d__18.MoveNext))]
    public static class Start
    {
        public static void Postfix(IntroCutscene._CoBegin_d__18 __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Agent))
            {
                var agent = (Agent) role;
                agent.LastIntel = DateTime.UtcNow;
                agent.LastIntel = agent.LastIntel.AddSeconds(CustomGameOptions.InitialCooldowns - CustomGameOptions.IntelCooldown);
            }
        }
    }
}