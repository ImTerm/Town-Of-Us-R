using System;
using UnityEngine;
using Hazel;
using InnerNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reactor;
using TownOfUs.CrewmateRoles.MedicMod;
using Reactor.Extensions;
using TownOfUs.Extensions;
using TownOfUs.Roles.Modifiers;
using Object = UnityEngine.Object;
using HarmonyLib;

namespace TownOfUs.Roles
{
    public class Illusionist : Role
    {
        public DateTime LastIllusioned { get; set; }

        public bool LastMouse;
        public bool IsUsingIllusion;
        public bool PressedButton;
        public bool MenuClick;

        public ChatController IllusionList1 { get; set; }
        public ChatController IllusionList2 { get; set; }
        public PlayerControl IllusionPlayer1 { get; set; }
        public PlayerControl IllusionPlayer2 { get; set; }
        
        public Illusionist(PlayerControl player) : base(player)
        {
            Name = "Illusionist";
            ImpostorText = () => "Change players' appearance to impostors";
            TaskText = () => "Change players' appearance to impostors";
            Color = new Color(0.8f, 0.3f, 1f, 1f);
            RoleType = RoleEnum.Illusionist;
            Scale = 1.4f;
            LastIllusioned = DateTime.UtcNow;
            IllusionList1 = null;
            IllusionList2 = null;
            IllusionPlayer1 = null;
            IllusionPlayer2 = null;
            IsUsingIllusion = false;
        }

        // public KillButton EndIllusionButton
        // {
        //     get => _endIllusionButton;
        //     set
        //     {
        //         _endIllusionButton = value;
        //         ExtraButtons.Clear();
        //         ExtraButtons.Add(value);
        //     }
        // }

        public float IllusionTimer()
        {
            var timeSpan = DateTime.UtcNow - LastIllusioned;
            var num = CustomGameOptions.IllusionCooldown * 1000f;
            if (num - (float) timeSpan.TotalMilliseconds < 0f)
                return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }

        public float EndIllusionTimer()
        {
            var timeSpan = DateTime.UtcNow - LastIllusioned;
            var num = CustomGameOptions.IllusionEndCooldown * 1000f;
            if (num - (float) timeSpan.TotalMilliseconds < 0f)
                return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
        
        public void Update(HudManager __instance)
        {
            // if (HudManager.Instance != null && HudManager.Instance.Chat != null)
            //     foreach (var bubble in HudManager.Instance.Chat.chatBubPool.activeChildren)
            //         if (bubble.Cast<ChatBubble>().NameText != null &&
            //             Player.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
            //             bubble.Cast<ChatBubble>().NameText.color = Color;
            if (IsUsingIllusion && IllusionPlayer1 != null && !IllusionPlayer1.Data.Disconnected && !IllusionPlayer1.Data.IsDead && !Player.Data.IsDead)
            {
                Utils.Morph(IllusionPlayer1, IllusionPlayer2);

                var write = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.UpdateIllusion, SendOption.Reliable, -1);
                write.Write(IllusionPlayer1.PlayerId);
                write.Write(IllusionPlayer2.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(write);
            }

            FixedUpdate(__instance);
        }

        public void FixedUpdate(HudManager __instance)
        {
            if ((!IsUsingIllusion || Player.Data.IsDead || IllusionPlayer1.Data.IsDead || IllusionPlayer1.Data.Disconnected) && IllusionPlayer1 != null && IllusionPlayer2 != null)
            {
                if (!Player.Data.IsDead)
                {
                    LastIllusioned = DateTime.UtcNow;

                    if (IllusionPlayer1 == PlayerControl.LocalPlayer)
                        Coroutines.Start(Utils.FlashCoroutine(Color));
                }

                Utils.Unmorph(IllusionPlayer1);
                
                var write = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.EndIllusion, SendOption.Reliable, -1);
                write.Write(IllusionPlayer1.PlayerId);
                write.Write(IllusionPlayer2.PlayerId);
                write.Write(Player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(write);

                IllusionPlayer1 = null;
                IllusionPlayer2 = null;
            }

            if (PressedButton && IllusionPlayer1 == null && IllusionPlayer2 == null && IllusionList1 == null)
            {
                IllusionList1 = Object.Instantiate(__instance.Chat);

                IllusionList1.transform.SetParent(Camera.main.transform);
                IllusionList1.SetVisible(true);
                IllusionList1.Toggle();

                IllusionList1.TextBubble.enabled = false;
                IllusionList1.TextBubble.gameObject.SetActive(false);

                IllusionList1.TextArea.enabled = false;
                IllusionList1.TextArea.gameObject.SetActive(false);

                IllusionList1.BanButton.enabled = false;
                IllusionList1.BanButton.gameObject.SetActive(false);

                IllusionList1.CharCount.enabled = false;
                IllusionList1.CharCount.gameObject.SetActive(false);

                IllusionList1.OpenKeyboardButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                IllusionList1.OpenKeyboardButton.SetActive(false);

                IllusionList1.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>()
                    .enabled = false;
                IllusionList1.gameObject.transform.GetChild(0).gameObject.SetActive(false);

                IllusionList1.BackgroundImage.enabled = false;

                foreach (var rend in IllusionList1.Content
                    .GetComponentsInChildren<SpriteRenderer>())
                    if (rend.name == "SendButton" || rend.name == "QuickChatButton")
                    {
                        rend.enabled = false;
                        rend.gameObject.SetActive(false);
                    }

                foreach (var bubble in IllusionList1.chatBubPool.activeChildren)
                {
                    bubble.enabled = false;
                    bubble.gameObject.SetActive(false);
                }

                IllusionList1.chatBubPool.activeChildren.Clear();

                foreach (var TempPlayer in PlayerControl.AllPlayerControls)
                    if (!TempPlayer.Data.IsDead && !TempPlayer.Data.Disconnected && TempPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        foreach (var player in PlayerControl.AllPlayerControls)
                            if (!player.Data.Disconnected || Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId).ParentId == player.PlayerId)
                            {
                                IllusionList1.AddChat(TempPlayer, "Click here");
                                IllusionList1.chatBubPool.activeChildren[IllusionList1.chatBubPool.activeChildren._size - 1].Cast<ChatBubble>().SetName(player.Data.PlayerName, false, false,
                                    PlayerControl.LocalPlayer.PlayerId == player.PlayerId ? Color : Color.white);
                                var IsDeadTemp = player.Data.IsDead;
                                player.Data.IsDead = false;
                                IllusionList1.chatBubPool.activeChildren[IllusionList1.chatBubPool.activeChildren._size - 1].Cast<ChatBubble>().SetCosmetics(player.Data);
                                player.Data.IsDead = IsDeadTemp;
                            }
                        break;
                    }
            }
            if (IllusionList1 != null)
            {
                if (Minigame.Instance)
                    Minigame.Instance.Close();

                if (!IllusionList1.IsOpen || MeetingHud.Instance || Input.GetKeyInt(KeyCode.Escape))
                {
                    IllusionList1.Toggle();
                    IllusionList1.SetVisible(false);
                    IllusionList1 = null;
                    PressedButton = false;
                }
                else
                {
                    foreach (var bubble in IllusionList1.chatBubPool.activeChildren)
                        if (IllusionTimer() == 0f && IllusionList1 != null)
                        {
                            // System.Console.WriteLine("Reached Here - 1");
                            Vector2 ScreenMin =
                                Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.min);
                            Vector2 ScreenMax =
                                Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.max);
                            if (Input.mousePosition.x > ScreenMin.x && Input.mousePosition.x < ScreenMax.x)
                                if (Input.mousePosition.y > ScreenMin.y && Input.mousePosition.y < ScreenMax.y)
                                {
                                    // System.Console.WriteLine("Reached Here - 2");
                                    // System.Console.WriteLine(Input.GetMouseButtonDown(0)+"");
                                    // System.Console.WriteLine(LastMouse+"");
                                    if (!Input.GetMouseButtonDown(0) && LastMouse)
                                    {
                                        // System.Console.WriteLine("Reached Here - 3");
                                        LastMouse = false;
                                        IllusionList1.Toggle();
                                        IllusionList1.SetVisible(false);
                                        IllusionList1 = null;
                                        PressedButton = false;

                                        // System.Console.WriteLine(bubble.Cast<ChatBubble>().NameText.text);
                                        foreach (var player in PlayerControl.AllPlayerControls)
                                            if (player.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
                                            {
                                                IllusionPlayer1 = player;
                                                // System.Console.WriteLine(player.Data.PlayerName+"");
                                            }
                                    }
                                }
                        }
                    if (!Input.GetMouseButtonDown(0) && LastMouse)
                    {
                        if (MenuClick)
                            MenuClick = false;
                        else {
                            IllusionList1.Toggle();
                            IllusionList1.SetVisible(false);
                            IllusionList1 = null;
                            PressedButton = false;
                        }
                    }
                    LastMouse = Input.GetMouseButtonDown(0);
                }
            }
            if (IllusionPlayer1 != null && IllusionPlayer2 == null && IllusionList2 == null)
            {
                IllusionList2 = Object.Instantiate(__instance.Chat);

                IllusionList2.transform.SetParent(Camera.main.transform);
                IllusionList2.SetVisible(true);
                IllusionList2.Toggle();

                IllusionList2.TextBubble.enabled = false;
                IllusionList2.TextBubble.gameObject.SetActive(false);

                IllusionList2.TextArea.enabled = false;
                IllusionList2.TextArea.gameObject.SetActive(false);

                IllusionList2.BanButton.enabled = false;
                IllusionList2.BanButton.gameObject.SetActive(false);

                IllusionList2.CharCount.enabled = false;
                IllusionList2.CharCount.gameObject.SetActive(false);

                IllusionList2.OpenKeyboardButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                IllusionList2.OpenKeyboardButton.SetActive(false);

                IllusionList2.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>()
                    .enabled = false;
                IllusionList2.gameObject.transform.GetChild(0).gameObject.SetActive(false);

                IllusionList2.BackgroundImage.enabled = false;

                foreach (var rend in IllusionList2.Content
                    .GetComponentsInChildren<SpriteRenderer>())
                    if (rend.name == "SendButton" || rend.name == "QuickChatButton")
                    {
                        rend.enabled = false;
                        rend.gameObject.SetActive(false);
                    }

                foreach (var bubble in IllusionList2.chatBubPool.activeChildren)
                {
                    bubble.enabled = false;
                    bubble.gameObject.SetActive(false);
                }

                IllusionList2.chatBubPool.activeChildren.Clear();

                foreach (var TempPlayer in PlayerControl.AllPlayerControls)
                    if (!TempPlayer.Data.IsDead && !TempPlayer.Data.Disconnected && TempPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        foreach (var player in PlayerControl.AllPlayerControls)
                            if (!player.Data.Disconnected || Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId).ParentId == player.PlayerId)
                            {
                                IllusionList2.AddChat(TempPlayer, "Click here");
                                IllusionList2.chatBubPool.activeChildren[IllusionList2.chatBubPool.activeChildren._size - 1].Cast<ChatBubble>().SetName(player.Data.PlayerName, false, false,
                                    PlayerControl.LocalPlayer.PlayerId == player.PlayerId ? Color : Color.white);
                                var IsDeadTemp = player.Data.IsDead;
                                player.Data.IsDead = false;
                                IllusionList2.chatBubPool.activeChildren[IllusionList2.chatBubPool.activeChildren._size - 1].Cast<ChatBubble>().SetCosmetics(player.Data);
                                player.Data.IsDead = IsDeadTemp;
                            }
                        break;
                    }
            }
            
            if (IllusionList2 != null)
            {
                if (Minigame.Instance)
                    Minigame.Instance.Close();

                if (!IllusionList2.IsOpen || MeetingHud.Instance || Input.GetKeyInt(KeyCode.Escape))
                {
                    IllusionList2.Toggle();
                    IllusionList2.SetVisible(false);
                    IllusionList2 = null;
                    IllusionPlayer1 = null;
                }
                else
                {
                    foreach (var bubble in IllusionList2.chatBubPool.activeChildren)
                        if (IllusionTimer() == 0f && IllusionList2 != null)
                        {
                            Vector2 ScreenMin =
                                Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.min);
                            Vector2 ScreenMax =
                                Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.max);
                            if (Input.mousePosition.x > ScreenMin.x && Input.mousePosition.x < ScreenMax.x)
                                if (Input.mousePosition.y > ScreenMin.y && Input.mousePosition.y < ScreenMax.y)
                                {
                                    if (!Input.GetMouseButtonDown(0) && LastMouse)
                                    {
                                        LastMouse = false;
                                        IllusionList2.Toggle();
                                        IllusionList2.SetVisible(false);
                                        IllusionList2 = null;
                                        // Coroutines.Start(AbilityCoroutine.Illusion(this, PlayerControl.AllPlayerControls.ToArray().Where(x =>
                                        //         x.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
                                        //     .FirstOrDefault()));
                                        foreach (var player in PlayerControl.AllPlayerControls)
                                            if (player.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
                                            {
                                                IllusionPlayer2 = player;

                                                LastIllusioned = DateTime.UtcNow;

                                                IsUsingIllusion = true;

                                                var write = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                                                    (byte) CustomRPC.StartIllusion, SendOption.Reliable, -1);
                                                write.Write(IllusionPlayer1.PlayerId);
                                                write.Write(IllusionPlayer2.PlayerId);
                                                write.Write(Player.PlayerId);
                                                AmongUsClient.Instance.FinishRpcImmediately(write);

                                                Utils.Morph(IllusionPlayer1, IllusionPlayer2);

                                                if (PlayerControl.LocalPlayer.PlayerId == IllusionPlayer1.PlayerId)
                                                    Coroutines.Start(Utils.FlashCoroutine(Color));
                                            }
                                    }
                                }
                        }
                    if (!Input.GetMouseButtonDown(0) && LastMouse)
                    {
                        if (MenuClick)
                            MenuClick = false;
                        else {
                            IllusionList2.Toggle();
                            IllusionList2.SetVisible(false);
                            IllusionList2 = null;
                            IllusionPlayer1 = null;
                        }
                    }
                    LastMouse = Input.GetMouseButtonDown(0);
                }
            }
        }
        

        // public void Update(HudManager __instance)
        // {
        //     if (HudManager.Instance != null && HudManager.Instance.Chat != null)
        //         foreach (var bubble in HudManager.Instance.Chat.chatBubPool.activeChildren)
        //             if (bubble.Cast<ChatBubble>().NameText != null &&
        //                 Player.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
        //                 bubble.Cast<ChatBubble>().NameText.color = Color;

        //     if (IllusionList != null)
        //     {
        //         if (Minigame.Instance)
        //             Minigame.Instance.Close();

        //         if (!IllusionList.IsOpen || MeetingHud.Instance || Input.GetKeyInt(KeyCode.Escape))
        //         {
        //             IllusionList.Toggle();
        //             IllusionList.SetVisible(false);
        //             IllusionList = null;
        //         }
        //         else
        //         {
        //             foreach (var bubble in IllusionList.chatBubPool.activeChildren)
        //                 if (!IsUsingIllusion && IllusionList != null)
        //                 {
        //                     Vector2 ScreenMin =
        //                         Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.min);
        //                     Vector2 ScreenMax =
        //                         Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.max);
        //                     if (Input.mousePosition.x > ScreenMin.x && Input.mousePosition.x < ScreenMax.x)
        //                         if (Input.mousePosition.y > ScreenMin.y && Input.mousePosition.y < ScreenMax.y)
        //                         {
        //                             if (!Input.GetMouseButtonDown(0) && lastMouse)
        //                             {
        //                                 lastMouse = false;
        //                                 IllusionList.Toggle();
        //                                 IllusionList.SetVisible(false);
        //                                 IllusionList = null;
        //                                 Coroutines.Start(AbilityCoroutine.Illusion(this, PlayerControl.AllPlayerControls.ToArray().Where(x =>
        //                                         x.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
        //                                     .FirstOrDefault()));
        //                                 break;
        //                             }
        //                             lastMouse = Input.GetMouseButtonDown(0);
        //                         }
        //                 }
        //         }
        //     }
            // if (IsUsingIllusion) {
            //     var illusionText = new GameObject("_Player").AddComponent<ImportantTextTask>();
            //     illusionText.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
            //     // if (CustomGameOptions.InfiniteIllusion)
            //     //     illusionText.Text =
            //     //         $"{ColorString}Swapped appearances with {PlayerControl.LocalPlayer.Data.PlayerName}</color>";
            //     // else
            //     //     illusionText.Text =
            //     //         $"{ColorString}Swapped appearances with {PlayerControl.LocalPlayer.Data.PlayerName} ({CustomGameOptions.IllusionDuration}s)</color>";

            //     var totalIllusionTime = (DateTime.UtcNow - LastIllusionActivation).TotalMilliseconds / 1000;
            //     if (CustomGameOptions.InfiniteIllusion)
            //         illusionText.Text =
            //             $"{ColorString}Swapped appearances with {IllusionTarget.Data.PlayerName}</color>";
            //     else
            //         illusionText.Text =
            //             $"{ColorString}Swapped appearances with {IllusionTarget.Data.PlayerName} ({CustomGameOptions.IllusionDuration - Math.Round(totalIllusionTime)}s)</color>";
            //     PlayerControl.LocalPlayer.myTasks.Insert(0, illusionText);

            //     if ((!CustomGameOptions.InfiniteIllusion && IllusionTimer() == 0f) ||
            //         EndIllusion || PlayerControl.LocalPlayer.Data.IsDead ||
            //         AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended)
            //     {
            //         EndIllusion = false;
            //         PlayerControl.LocalPlayer.myTasks.Remove(illusionText);
            //         LastIllusioned = DateTime.UtcNow;
            //         IsUsingIllusion = false;
            //         Utils.Unmorph(Player);
            //         Utils.Unmorph(IllusionTarget);
            //         IllusionTarget = null;

            //         var writer2 = AmongUsClient.Instance.StartRpcImmediately(
            //             PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcResetAnim, SendOption.Reliable, -1);
            //         writer2.Write(PlayerControl.LocalPlayer.PlayerId);
            //         writer2.Write(PlayerControl.LocalPlayer.PlayerId);
            //         AmongUsClient.Instance.FinishRpcImmediately(writer2);
            //     }
            //     else {
            //         Utils.Morph(Player, IllusionTarget, true);
            //         Utils.Morph(IllusionTarget, Player, true);
            //         // DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(CustomGameOptions.IllusionDuration - (float)totalIllusionTime,
            //         //     CustomGameOptions.IllusionDuration);
            //     }
            // }
        // }

        // public static class AbilityCoroutine
        // {
        //     public static Dictionary<byte, DateTime> tickDictionary = new Dictionary<byte, DateTime>();

        //     public static IEnumerator Illusion(Illusionist __instance, PlayerControl illusionPlayer)
        //     {
        //         var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
        //             (byte)CustomRPC.SetIllusion, SendOption.Reliable, -1);
        //         writer.Write(PlayerControl.LocalPlayer.PlayerId);
        //         writer.Write(illusionPlayer.PlayerId);
        //         AmongUsClient.Instance.FinishRpcImmediately(writer);

        //         __instance.IllusionTarget = illusionPlayer;
        //         Utils.Morph(__instance.Player, illusionPlayer, true);
        //         Utils.Morph(illusionPlayer, __instance.Player, true);

        //         // Coroutines.Start(Utils.FlashCoroutine(__instance.Color));
        //         __instance.LastIllusionActivation = DateTime.UtcNow;
        //         __instance.IsUsingIllusion = true;

        //         var illusionText = new GameObject("_Player").AddComponent<ImportantTextTask>();
        //         illusionText.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
        //         if (CustomGameOptions.InfiniteIllusion)
        //             illusionText.Text =
        //                 $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName}</color>";
        //         else
        //             illusionText.Text =
        //                 $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName} ({CustomGameOptions.IllusionDuration}s)</color>";
        //         PlayerControl.LocalPlayer.myTasks.Insert(0, illusionText);

        //         while (true)
        //         {
        //             __instance.IsUsingIllusion = true;
        //             __instance.IllusionTarget = illusionPlayer;
        //             __instance.TimeRemaining = CustomGameOptions.IllusionDuration - ((float) (DateTime.UtcNow - __instance.LastIllusionActivation).TotalMilliseconds / 1000f);
        //             if (__instance.TimeRemaining < 0f)
        //                 __instance.TimeRemaining = 0f;
        //             if (CustomGameOptions.InfiniteIllusion)
        //                 illusionText.Text =
        //                     $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName}</color>";
        //             else
        //                 illusionText.Text =
        //                     $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName} ({Math.Round(__instance.TimeRemaining)}s)</color>";
        //             if ((__instance.TimeRemaining == 0f && !CustomGameOptions.InfiniteIllusion) ||
        //                 __instance.EndIllusion ||
        //                 PlayerControl.LocalPlayer.Data.IsDead ||
        //                 AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended)
        //             {
        //                 __instance.EndIllusion = false;
        //                 PlayerControl.LocalPlayer.myTasks.Remove(illusionText);
        //                 __instance.LastIllusioned = DateTime.UtcNow;
        //                 __instance.IsUsingIllusion = false;
        //                 Utils.Unmorph(__instance.Player);
        //                 Utils.Unmorph(__instance.IllusionTarget);
        //                 __instance.IllusionTarget = null;

        //                 var writer2 = AmongUsClient.Instance.StartRpcImmediately(
        //                     PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndIllusion, SendOption.Reliable,
        //                     -1);
        //                 writer2.Write(PlayerControl.LocalPlayer.PlayerId);
        //                 writer2.Write(illusionPlayer.PlayerId);
        //                 AmongUsClient.Instance.FinishRpcImmediately(writer2);
        //                 yield break;
        //             }

        //             Utils.Morph(__instance.Player, illusionPlayer, true);
        //             Utils.Morph(illusionPlayer, __instance.Player, true);
        //             // var endCooldown = CustomGameOptions.IllusionEndCooldown - (float)totalIllusionTime;
        //             // if (endCooldown < 0f)
        //             //     endCooldown = 0f;
        //             // if (/*CustomGameOptions.IllusionEndCooldown < CustomGameOptions.IllusionDuration ||*/ CustomGameOptions.InfiniteIllusion)
        //             //     DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(0f, CustomGameOptions.IllusionDuration);
        //             // else
        //             //     DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(CustomGameOptions.IllusionDuration - (float)totalIllusionTime,
        //             //         CustomGameOptions.IllusionDuration);

        //             yield return null;
        //         }
        //         // return null;
        //     }
        // }
    }
}