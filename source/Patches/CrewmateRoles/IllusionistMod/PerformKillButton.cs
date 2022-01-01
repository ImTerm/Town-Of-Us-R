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

namespace TownOfUs.CrewmateRoles.IllusionistMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPriority(Priority.Last)]
    public class PerformKillButton

    {
        public static bool Prefix(KillButton __instance)
        {
            // if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Illusionist)) return true;
            var role = Role.GetRole<Illusionist>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!__instance.enabled) return false;

            if (__instance != role.EndIllusionButton)
            {
                if (!role.IsUsingIllusion && role.IllusionTimer() == 0f && role.IllusionList == null)
                {
                    role.IllusionList = UnityEngine.Object.Instantiate(HudManager.Instance.Chat);

                    role.IllusionList.transform.SetParent(Camera.main.transform);
                    role.IllusionList.SetVisible(true);
                    role.IllusionList.Toggle();

                    role.IllusionList.TextBubble.enabled = false;
                    role.IllusionList.TextBubble.gameObject.SetActive(false);

                    role.IllusionList.TextArea.enabled = false;
                    role.IllusionList.TextArea.gameObject.SetActive(false);

                    role.IllusionList.BanButton.enabled = false;
                    role.IllusionList.BanButton.gameObject.SetActive(false);

                    role.IllusionList.CharCount.enabled = false;
                    role.IllusionList.CharCount.gameObject.SetActive(false);
                    
                    role.IllusionList.OpenKeyboardButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    role.IllusionList.OpenKeyboardButton.SetActive(false);

                    role.IllusionList.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>()
                        .enabled = false;
                    role.IllusionList.gameObject.transform.GetChild(0).gameObject.SetActive(false);

                    role.IllusionList.BackgroundImage.enabled = false;

                    foreach (var rend in role.IllusionList.Content
                        .GetComponentsInChildren<SpriteRenderer>())
                        if (rend.name == "SendButton" || rend.name == "QuickChatButton")
                        {
                            rend.enabled = false;
                            rend.gameObject.SetActive(false);
                        }

                    foreach (var bubble in role.IllusionList.chatBubPool.activeChildren)
                    {
                        bubble.enabled = false;
                        bubble.gameObject.SetActive(false);
                    }

                    role.IllusionList.chatBubPool.activeChildren.Clear();

                    foreach (var player in PlayerControl.AllPlayerControls.ToArray()
                        .Where(x => x != PlayerControl.LocalPlayer && !x.Data.Disconnected))
                    {
                        if (!player.Data.IsDead)
                            role.IllusionList.AddChat(player, "Click here");
                        else
                        {
                            var deadBodies = Object.FindObjectsOfType<DeadBody>();
                            foreach (var body in deadBodies)
                                if (body.ParentId == player.PlayerId)
                                {
                                    player.Data.IsDead = false;
                                    role.IllusionList.AddChat(player, "Click here");
                                    player.Data.IsDead = true;
                                }
                        }
                    }
                }
                else
                {
                    role.IllusionList.Toggle();
                    role.IllusionList.SetVisible(false);
                    role.IllusionList = null;
                }
            }
            else if (role.IsUsingIllusion && role.EndIllusionTimer() == 0f)
                role.EndIllusion = true;
            return false;
        }
    }
}