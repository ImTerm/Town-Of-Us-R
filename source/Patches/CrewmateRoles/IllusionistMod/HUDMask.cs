// using HarmonyLib;
// using TownOfUs.Roles;

// namespace TownOfUs.CrewmateRoles.IllusionistMod
// {
//     [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
//     public class HUDMask
//     {
//         public static void Postfix(PlayerControl __instance)
//         {
//             UpdateMaskButton(__instance);
//         }

//         public static void UpdateMaskButton(PlayerControl __instance)
//         {
//             if (PlayerControl.AllPlayerControls.Count <= 1) return;
//             if (PlayerControl.LocalPlayer == null) return;
//             if (PlayerControl.LocalPlayer.Data == null) return;
//             if (!PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist)) return;
//             var data = PlayerControl.LocalPlayer.Data;
//             var isDead = data.IsDead;
//             var maskButton = DestroyableSingleton<HudManager>.Instance.KillButton;

//             var role = Role.GetRole<Illusionist>(PlayerControl.LocalPlayer);


//             if (isDead)
//             {
//                 maskButton.gameObject.SetActive(false);
//               //  rewindButton.isActive = false;
//             }
//             else
//             {
//                 maskButton.gameObject.SetActive(!MeetingHud.Instance);
//               //  rewindButton.isActive = !MeetingHud.Instance;
//                 maskButton.SetCoolDown(role.TimeLordRewindTimer(), role.GetCooldown());
//             }

//             var renderer = maskButton.graphic;
//             if (!mask.isCoolingDown & !RecordRewind.rewinding & rewindButton.enabled)
//             {
//                 renderer.color = Palette.EnabledColor;
//                 renderer.material.SetFloat("_Desat", 0f);
//                 return;
//             }

//             renderer.color = Palette.DisabledClear;
//             renderer.material.SetFloat("_Desat", 1f);
//         }
//     }
// }