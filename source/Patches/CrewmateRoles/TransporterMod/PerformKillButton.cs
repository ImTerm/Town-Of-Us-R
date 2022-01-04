using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Reactor;
using Reactor.Extensions;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.MedicMod;
using TownOfUs.CrewmateRoles.SnitchMod;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.CrewmateRoles.TransporterMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPriority(Priority.Last)]
    public class PerformKillButton

    {
        public static bool Prefix(KillButton __instance)
        {
            // if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Transporter)) return true;
            var role = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!__instance.enabled) return false;
            if (role.TransportTimer() != 0f) return false;

            if (role.TransportList1 == null && role.TransportList2 == null)
            {
                // role.TransportList1 = Object.Instantiate(HudManager.Instance.Chat);

                // role.TransportList1.transform.SetParent(Camera.main.transform);
                // role.TransportList1.SetVisible(true);
                // role.TransportList1.Toggle();

                // role.TransportList1.TextBubble.enabled = false;
                // role.TransportList1.TextBubble.gameObject.SetActive(false);

                // role.TransportList1.TextArea.enabled = false;
                // role.TransportList1.TextArea.gameObject.SetActive(false);

                // role.TransportList1.BanButton.enabled = false;
                // role.TransportList1.BanButton.gameObject.SetActive(false);

                // role.TransportList1.CharCount.enabled = false;
                // role.TransportList1.CharCount.gameObject.SetActive(false);

                // role.TransportList1.OpenKeyboardButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                // role.TransportList1.OpenKeyboardButton.SetActive(false);

                // role.TransportList1.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>()
                //     .enabled = false;
                // role.TransportList1.gameObject.transform.GetChild(0).gameObject.SetActive(false);

                // role.TransportList1.BackgroundImage.enabled = false;

                // foreach (var rend in role.TransportList1.Content
                //     .GetComponentsInChildren<SpriteRenderer>())
                // {
                //     if (rend.name == "SendButton" || rend.name == "QuickChatButton")
                //     {
                //         rend.enabled = false;
                //         rend.gameObject.SetActive(false);
                //     }
                // }
                // foreach (var bubble in role.TransportList1.chatBubPool.activeChildren)
                // {
                //     bubble.enabled = false;
                //     bubble.gameObject.SetActive(false);
                // }

                // role.TransportList1.chatBubPool.activeChildren.Clear();

                // foreach (var player in PlayerControl.AllPlayerControls.ToArray()
                //     .Where(x => !x.Data.Disconnected))
                // {
                //     if (!player.Data.IsDead)
                //         role.TransportList1.AddChat(player, "Click here");
                //     else
                //     {
                //         var deadBodies = Object.FindObjectsOfType<DeadBody>();
                //         foreach (var body in deadBodies)
                //             if (body.ParentId == player.PlayerId)
                //             {
                //                 player.Data.IsDead = false;
                //                 role.TransportList1.AddChat(player, "Click here");
                //                 player.Data.IsDead = true;
                //             }
                //     }
                // }
                role.PressedButton = true;
                role.MenuClick = true;
            }
            // else
            // {
            //     role.ActiveMenu = false;

            //     role.TransportList1.Toggle();
            //     role.TransportList1.SetVisible(false);
            //     role.TransportList1 = null;

            //     role.TransportList2.Toggle();
            //     role.TransportList2.SetVisible(false);
            //     role.TransportList2 = null;

            //     role.TransportPlayer1 = null;
            //     role.TransportPlayer2 = null;
            // }
            return false;
        }
    }
}