using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Hazel;
using System;
using UnhollowerBaseLib;

namespace TownOfUs.Patches
{
    public class GameStartManagerPatch 
    {
        private static float timer = 600f;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                timer = 600f;
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";
            // private static bool hasEntered = false;
            // private static bool lastEnter = false;
        
            public static void Prefix(GameStartManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return;
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance)
            {
                __instance.GameRoomName.SetText(Input.GetKeyInt(KeyCode.Return) ? lobbyCodeText : $"Code\n<color=#00FF00FF>Hold [Enter]\nTo Reveal</color>");

                // if (!Input.GetKeyInt(KeyCode.Return) && lastEnter)
                //     hasEntered = !hasEntered;
                
                // lastEnter = Input.GetKeyInt(KeyCode.Return);

                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return;

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $"\n<color=#FFFFFFFF>({minutes:00}:{seconds:00})</color>";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;

            }
        }
    }
}