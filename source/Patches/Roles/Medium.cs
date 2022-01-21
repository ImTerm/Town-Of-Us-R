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
    public class Medium : Role
    {
        public DateTime LastMediate { get; set; }

        public bool PressedButton;

        public ChatController MediateBubble { get; set; }
        public List<byte> MediatedPlayers { get; set; }
        
        public Medium(PlayerControl player) : base(player)
        {
            Name = "Medium";
            ImpostorText = () => "Gain info from the dead";
            TaskText = () => "Gain info from dead players";
            Color = new Color(0.65f, 0.5f, 1f, 1f);
            RoleType = RoleEnum.Medium;
            Scale = 1.4f;
            LastMediate = DateTime.UtcNow;
            PressedButton = false;
            MediateBubble = null;
            MediatedPlayers = new List<byte>();
        }

        public float MediateTimer()
        {
            var timeSpan = DateTime.UtcNow - LastMediate;
            var num = CustomGameOptions.MediateCooldown * 1000f;
            if (num - (float) timeSpan.TotalMilliseconds < 0f)
                return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
        
        public void Update(HudManager __instance)
        {
            FixedUpdate(__instance);
        }

        public void FixedUpdate(HudManager __instance)
        {
            if (PressedButton)
            {
                PressedButton = false;

                LastMediate = DateTime.UtcNow;

                var UnMediatedBody = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => !MediatedPlayers.Contains(x.ParentId));
                
                MediateBubble = Object.Instantiate(__instance.Chat);

                MediateBubble.transform.SetParent(Camera.main.transform);
                MediateBubble.SetVisible(true);
                MediateBubble.Toggle();

                MediateBubble.TextBubble.enabled = false;
                MediateBubble.TextBubble.gameObject.SetActive(false);

                MediateBubble.TextArea.enabled = false;
                MediateBubble.TextArea.gameObject.SetActive(false);

                MediateBubble.BanButton.enabled = false;
                MediateBubble.BanButton.gameObject.SetActive(false);

                MediateBubble.CharCount.enabled = false;
                MediateBubble.CharCount.gameObject.SetActive(false);

                MediateBubble.scroller.DragScrollSpeed = 0f;
                MediateBubble.scroller.ScrollWheelSpeed = 0f;
                // MediateBubble.scroller.enabled = false;
                // MediateBubble.scroller.gameObject.SetActive(false);

                MediateBubble.OpenKeyboardButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                // MediateBubble.OpenKeyboardButton.gameObject.SetActive(false);

                MediateBubble.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>()
                    .enabled = false;
                MediateBubble.gameObject.transform.GetChild(0).gameObject.SetActive(false);

                MediateBubble.BackgroundImage.enabled = false;

                foreach (var rend in MediateBubble.Content
                    .GetComponentsInChildren<SpriteRenderer>())
                    if (rend.name == "SendButton" || rend.name == "QuickChatButton")
                    {
                        rend.enabled = false;
                        rend.gameObject.SetActive(false);
                    }

                foreach (var bubble in MediateBubble.chatBubPool.activeChildren)
                {
                    bubble.enabled = false;
                    bubble.gameObject.SetActive(false);
                }

                MediateBubble.chatBubPool.activeChildren.Clear();
                
                foreach (var TempPlayer in PlayerControl.AllPlayerControls)
                {
                    if (TempPlayer != null &&
                        TempPlayer.Data != null &&
                        !TempPlayer.Data.IsDead &&
                        !TempPlayer.Data.Disconnected &&
                        TempPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        MediateBubble.AddChat(TempPlayer, "No spirits respond to your beckoning...");
                        for (int i = 0; i < 4; i++)
                        {
                            MediateBubble.AddChat(TempPlayer, "");
                            MediateBubble.chatBubPool.activeChildren[MediateBubble.chatBubPool.activeChildren._size - 1].Cast<ChatBubble>().gameObject.SetActive(false);
                        }
                        break;
                    }
                }

                GameData.PlayerInfo TempData = new GameData.PlayerInfo(PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x != null && x.Data != null));
                TempData.SetOutfit(PlayerOutfitType.Default, new GameData.PlayerOutfit()
                    {
                        ColorId = 15,
                        HatId = "",
                        SkinId = "",
                        VisorId = "",
                        _playerName = " "
                    });
                MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().SetName(" ", true, false, Color.white);
                MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().SetCosmetics(TempData);
                
                if (UnMediatedBody == null)
                {
                    var color = Color.clear;
                    MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().Xmark.color = color;
                    color.a = 0.5f;
                    MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().Player.Body.color = color;
                }
                else
                {
                    if (CustomGameOptions.ShowMediatePlayer)
                    {
                        MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().SetName(GameData.Instance.GetPlayerById(UnMediatedBody.ParentId).PlayerName, true, false, Color.white);
                        MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().SetCosmetics(GameData.Instance.GetPlayerById(UnMediatedBody.ParentId));
                    }
                    else
                    {
                        MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().SetName("Unknown Player", true, false, Color.white);
                    }
                    System.Console.WriteLine("Reached here 1");

                    // List<PlainShipRoom> Rooms = ShipStatus.Instance.AllRooms.ToArray().Cast<PlainShipRoom>().Where(x => x.roomArea.IsTouching(UnMediatedBody.myCollider)).ToList();
                    PlainShipRoom ClosestRoom = new PlainShipRoom();
                    float SmallestDistance = float.MaxValue;
                    System.Console.WriteLine("Reached here 2");
                    
                    foreach (PlainShipRoom Room in ShipStatus.Instance.AllRooms)
                    {
                        System.Console.WriteLine("Reached here 2.0");
                        var ClosestBodyPoint = (Vector2)UnMediatedBody.myCollider.bounds.ClosestPoint((Vector2)Room.roomArea.bounds.ClosestPoint((Vector2)UnMediatedBody.TruePosition));
                        System.Console.WriteLine("Reached here 2.1");
                        var ClosestRoomPoint = (Vector2)Room.roomArea.bounds.ClosestPoint((Vector2)ClosestBodyPoint);
                        System.Console.WriteLine("Reached here 2.2");
                        var DistToRoom = Vector3.Distance(ClosestBodyPoint, ClosestRoomPoint);
                        System.Console.WriteLine("Reached here 2.3");
                        if (DistToRoom < SmallestDistance)
                        {
                            SmallestDistance = DistToRoom;
                            ClosestRoom = Room;
                        }
                    }
                    System.Console.WriteLine("Reached here 3");

                    string BubbleText = "I died ";

                    if (SmallestDistance == 0f)
                    {
                        BubbleText += "in... ";
                    }
                    // else if (SmallestDistance < 1f)
                    // {
                    //     BubbleText += "close to... ";
                    // }
                    else if (SmallestDistance < 1.5f)
                    {
                        BubbleText += "near... ";
                    }
                    System.Console.WriteLine("Reached here 4");

                    if (SmallestDistance < 2f && ClosestRoom != null && ClosestRoom.RoomId != (SystemTypes)0)
                    {
                        switch (ClosestRoom.RoomId)
                        {
                            case (SystemTypes)1:
                                BubbleText += "Storage...";
                                break;
                            case (SystemTypes)2:
                                BubbleText += "Cafeteria...";
                                break;
                            case (SystemTypes)3:
                                BubbleText += "Reactor...";
                                break;
                            case (SystemTypes)4:
                                BubbleText += "Upper Engine...";
                                break;
                            case (SystemTypes)5:
                                BubbleText += "Navigation...";
                                break;
                            case (SystemTypes)6:
                                BubbleText += "Admin...";
                                break;
                            case (SystemTypes)7:
                                BubbleText += "Electrical...";
                                break;

                            case (SystemTypes)9:
                                BubbleText += "Shields...";
                                break;
                            case (SystemTypes)10:
                                BubbleText += "MedBay...";
                                break;
                            case (SystemTypes)11:
                                BubbleText += "Security...";
                                break;
                            case (SystemTypes)12:
                                BubbleText += "Weapons...";
                                break;
                            case (SystemTypes)13:
                                BubbleText += "Lower Engine...";
                                break;
                            case (SystemTypes)14:
                                BubbleText += "Comms...";
                                break;
                                
                            case (SystemTypes)18:
                                BubbleText += "Decontamination...";
                                break;
                            case (SystemTypes)19:
                                BubbleText += "Launchpad...";
                                break;
                            case (SystemTypes)20:
                                BubbleText += "Locker Room...";
                                break;
                            case (SystemTypes)21:
                                BubbleText += "Laboratory...";
                                break;
                            case (SystemTypes)22:
                                BubbleText += "Balcony...";
                                break;
                            case (SystemTypes)23:
                                BubbleText += "Office...";
                                break;
                            case (SystemTypes)24:
                                BubbleText += "Greenhouse...";
                                break;
                            case (SystemTypes)25:
                                BubbleText += "Dropship...";
                                break;
                            case (SystemTypes)26:
                                BubbleText += "Lower Decontamination...";
                                break;

                            case (SystemTypes)28:
                                BubbleText += "Specimen...";
                                break;
                            case (SystemTypes)29:
                                BubbleText += "Boiler Room...";
                                break;
                            case (SystemTypes)30:
                                BubbleText += "Vault Room...";
                                break;
                            case (SystemTypes)31:
                                BubbleText += "Cockpit...";
                                break;
                            case (SystemTypes)32:
                                BubbleText += "Armory...";
                                break;
                            case (SystemTypes)33:
                                BubbleText += "Kitchen...";
                                break;
                            case (SystemTypes)34:
                                BubbleText += "Viewing Deck...";
                                break;
                            case (SystemTypes)35:
                                BubbleText += "Hall of Portraits...";
                                break;
                            case (SystemTypes)36:
                                BubbleText += "Cargo Bay...";
                                break;
                            case (SystemTypes)37:
                                BubbleText += "Ventilation...";
                                break;
                            case (SystemTypes)38:
                                BubbleText += "Showers...";
                                break;
                            case (SystemTypes)39:
                                BubbleText += "Engine...";
                                break;
                            case (SystemTypes)40:
                                BubbleText += "Brig...";
                                break;
                            case (SystemTypes)41:
                                BubbleText += "Meeting Room...";
                                break;
                            case (SystemTypes)42:
                                BubbleText += "Records...";
                                break;
                            case (SystemTypes)43:
                                BubbleText += "Lounge...";
                                break;
                            case (SystemTypes)44:
                                BubbleText += "Gap Room...";
                                break;
                            case (SystemTypes)45:
                                BubbleText += "Main Hall...";
                                break;
                            case (SystemTypes)46:
                                BubbleText += "Medical...";
                                break;
                        }
                    }
                    else {
                        BubbleText += "somewhere... outside...";
                    }
                    MediateBubble.chatBubPool.activeChildren[0].Cast<ChatBubble>().SetText(BubbleText);

                    MediatedPlayers.Add(UnMediatedBody.ParentId);
                    System.Console.WriteLine("Reached here 5");
                }
            }

            if (MeetingHud.Instance || ((LastMediate.AddSeconds(8.0f) - DateTime.UtcNow).TotalMilliseconds <= 0f && MediateBubble != null && MediateBubble.IsOpen))
            {
                MediateBubble.Toggle();
            }
            if (MeetingHud.Instance || ((LastMediate.AddSeconds(9.0f) - DateTime.UtcNow).TotalMilliseconds <= 0f && MediateBubble != null))
            {
                MediateBubble.SetVisible(false);
                MediateBubble = null;
            }
        }
    }
}