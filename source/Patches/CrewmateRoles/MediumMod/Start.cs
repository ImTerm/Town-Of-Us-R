using System;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.MediumMod
{
    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__18), nameof(IntroCutscene._CoBegin_d__18.MoveNext))]
    public static class Start
    {
        public static void Postfix(IntroCutscene._CoBegin_d__18 __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Medium))
            {
                var medium = (Medium) role;
                medium.LastMediate = DateTime.UtcNow;
                medium.LastMediate = medium.LastMediate.AddSeconds(CustomGameOptions.InitialCooldowns - CustomGameOptions.MediateCooldown);
            }
        }
    }
}