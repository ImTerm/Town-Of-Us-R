// using HarmonyLib;
// using UnityEngine;
// using System.Reflection;
// using System.Collections.Generic;
// using Hazel;
// using System;
// using UnhollowerBaseLib;

// namespace TownOfUs.Patches
// {
//     public class PlayerSpeedPatch
//     {
//         [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed))]
//         public class PlayerTrueSpeedPatch
//         {
//             public static bool Prefix(LightSource __instance)
//             {
//                 return false;
//             }
//         }
//     }
// }