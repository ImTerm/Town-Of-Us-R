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

        public KillButton _endIllusionButton;
        public bool EndIllusion;
        public bool lastMouse;
        public DateTime LastIllusioned { get; set; }
        public DateTime LastIllusionActivation { get; set; }
        public ChatController IllusionList { get; set; }
        public bool IsUsingIllusion { get; set; }
        public PlayerControl IllusionTarget { get; set; }
        public float TimeRemaining;
        
        public Illusionist(PlayerControl player) : base(player)
        {
            Name = "Illusionist";
            ImpostorText = () => "Swap appearances to confuse impostors";
            TaskText = () => "Swap appearances with other players to confuse impostors";
            Color = new Color(0.8f, 0.3f, 1f, 1f);
            RoleType = RoleEnum.Illusionist;
            Scale = 1.4f;
            LastIllusioned = DateTime.UtcNow;
            LastIllusionActivation = DateTime.UtcNow;
            IllusionList = null;
            IsUsingIllusion = false;
            IllusionTarget = null;
        }

        public KillButton EndIllusionButton
        {
            get => _endIllusionButton;
            set
            {
                _endIllusionButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

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
            var timeSpan = DateTime.UtcNow - LastIllusionActivation;
            var num = CustomGameOptions.IllusionEndCooldown * 1000f;
            if (num - (float) timeSpan.TotalMilliseconds < 0f)
                return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }

        public void Update(HudManager __instance)
        {
            if (HudManager.Instance != null && HudManager.Instance.Chat != null)
                foreach (var bubble in HudManager.Instance.Chat.chatBubPool.activeChildren)
                    if (bubble.Cast<ChatBubble>().NameText != null &&
                        Player.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
                        bubble.Cast<ChatBubble>().NameText.color = Color;

            if (IllusionList != null)
            {
                if (Minigame.Instance)
                    Minigame.Instance.Close();

                if (!IllusionList.IsOpen || MeetingHud.Instance || Input.GetKeyInt(KeyCode.Escape))
                {
                    IllusionList.Toggle();
                    IllusionList.SetVisible(false);
                    IllusionList = null;
                }
                else
                {
                    foreach (var bubble in IllusionList.chatBubPool.activeChildren)
                        if (!IsUsingIllusion && IllusionList != null)
                        {
                            Vector2 ScreenMin =
                                Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.min);
                            Vector2 ScreenMax =
                                Camera.main.WorldToScreenPoint(bubble.Cast<ChatBubble>().Background.bounds.max);
                            if (Input.mousePosition.x > ScreenMin.x && Input.mousePosition.x < ScreenMax.x)
                                if (Input.mousePosition.y > ScreenMin.y && Input.mousePosition.y < ScreenMax.y)
                                {
                                    if (!Input.GetMouseButtonDown(0) && lastMouse)
                                    {
                                        lastMouse = false;
                                        IllusionList.Toggle();
                                        IllusionList.SetVisible(false);
                                        IllusionList = null;
                                        Coroutines.Start(AbilityCoroutine.Illusion(this, PlayerControl.AllPlayerControls.ToArray().Where(x =>
                                                x.Data.PlayerName == bubble.Cast<ChatBubble>().NameText.text)
                                            .FirstOrDefault()));
                                        break;
                                    }
                                    lastMouse = Input.GetMouseButtonDown(0);
                                }
                        }
                }
            }
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
        }

        public static class AbilityCoroutine
        {
            public static Dictionary<byte, DateTime> tickDictionary = new Dictionary<byte, DateTime>();

            public static IEnumerator Illusion(Illusionist __instance, PlayerControl illusionPlayer)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte)CustomRPC.SetIllusion, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(illusionPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                __instance.IllusionTarget = illusionPlayer;
                Utils.Morph(__instance.Player, illusionPlayer, true);
                Utils.Morph(illusionPlayer, __instance.Player, true);

                // Coroutines.Start(Utils.FlashCoroutine(__instance.Color));
                __instance.LastIllusionActivation = DateTime.UtcNow;
                __instance.IsUsingIllusion = true;

                var illusionText = new GameObject("_Player").AddComponent<ImportantTextTask>();
                illusionText.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
                if (CustomGameOptions.InfiniteIllusion)
                    illusionText.Text =
                        $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName}</color>";
                else
                    illusionText.Text =
                        $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName} ({CustomGameOptions.IllusionDuration}s)</color>";
                PlayerControl.LocalPlayer.myTasks.Insert(0, illusionText);

                while (true)
                {
                    __instance.IsUsingIllusion = true;
                    __instance.IllusionTarget = illusionPlayer;
                    __instance.TimeRemaining = CustomGameOptions.IllusionDuration - ((float) (DateTime.UtcNow - __instance.LastIllusionActivation).TotalMilliseconds / 1000f);
                    if (__instance.TimeRemaining < 0f)
                        __instance.TimeRemaining = 0f;
                    if (CustomGameOptions.InfiniteIllusion)
                        illusionText.Text =
                            $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName}</color>";
                    else
                        illusionText.Text =
                            $"{__instance.ColorString}Swapped appearances with {illusionPlayer.Data.PlayerName} ({Math.Round(__instance.TimeRemaining)}s)</color>";
                    if ((__instance.TimeRemaining == 0f && !CustomGameOptions.InfiniteIllusion) ||
                        __instance.EndIllusion ||
                        PlayerControl.LocalPlayer.Data.IsDead ||
                        AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended)
                    {
                        __instance.EndIllusion = false;
                        PlayerControl.LocalPlayer.myTasks.Remove(illusionText);
                        __instance.LastIllusioned = DateTime.UtcNow;
                        __instance.IsUsingIllusion = false;
                        Utils.Unmorph(__instance.Player);
                        Utils.Unmorph(__instance.IllusionTarget);
                        __instance.IllusionTarget = null;

                        var writer2 = AmongUsClient.Instance.StartRpcImmediately(
                            PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndIllusion, SendOption.Reliable,
                            -1);
                        writer2.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer2.Write(illusionPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        yield break;
                    }

                    Utils.Morph(__instance.Player, illusionPlayer, true);
                    Utils.Morph(illusionPlayer, __instance.Player, true);
                    // var endCooldown = CustomGameOptions.IllusionEndCooldown - (float)totalIllusionTime;
                    // if (endCooldown < 0f)
                    //     endCooldown = 0f;
                    // if (/*CustomGameOptions.IllusionEndCooldown < CustomGameOptions.IllusionDuration ||*/ CustomGameOptions.InfiniteIllusion)
                    //     DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(0f, CustomGameOptions.IllusionDuration);
                    // else
                    //     DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(CustomGameOptions.IllusionDuration - (float)totalIllusionTime,
                    //         CustomGameOptions.IllusionDuration);

                    yield return null;
                }
                // return null;
            }
        }
    }
}