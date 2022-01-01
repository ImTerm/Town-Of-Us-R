using System;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.IllusionistMod
{
    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__18), nameof(IntroCutscene._CoBegin_d__18.MoveNext))]
    public static class Start
    {
        public static void Postfix(IntroCutscene._CoBegin_d__18 __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Illusionist))
            {
                var illusionist = (Illusionist) role;
                illusionist.LastIllusioned = DateTime.UtcNow;
                illusionist.LastIllusioned = illusionist.LastIllusioned.AddSeconds(CustomGameOptions.InitialCooldowns - CustomGameOptions.IllusionCooldown);
            }
        }
    }
}