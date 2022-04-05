using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.Tab;
using WoWonder.Activities.Tab.Services;
using WoWonder.Adapters;
using WoWonder.Helpers.Jobs;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Message;
using WoWonderClient.JobWorker;
using WoWonderClient.Requests;
using MessageData = WoWonderClient.Classes.Message.MessageData;

namespace WoWonder.Helpers.Controller
{
    public static class MessageController
    {
        //############# DON'T  MODIFY HERE ############# 

        private static ChatWindowActivity WindowActivity;

        private static TabbedMainActivity GlobalContext;
        //========================= Functions =========================
        public static async Task SendMessageTask(ChatWindowActivity windowActivity, string userId, string chatId, string messageHashId, string text = "", string contact = "", string filePath = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string productId = "", string lat = "", string lng = "", string storyId = "", string replyId = "")
        {
            try
            {
                WindowActivity = windowActivity;

                GlobalContext = TabbedMainActivity.GetInstance();

                if (!string.IsNullOrEmpty(filePath))
                {
                    new UploadSingleFileToServerWorker(windowActivity, "ChatWindowActivity").UploadFileToServer(windowActivity, new FileModel
                    {
                        MessageHashId = messageHashId,
                        ChatId = chatId,
                        UserId = userId,
                        FilePath = filePath,
                        ReplyId = replyId,
                        StoryId = storyId,
                    });
                }
                else
                {
                    StartApiService(userId, messageHashId, text, contact, filePath, imageUrl, stickerId, gifUrl, productId, lat, lng, storyId, replyId);
                }
            }
            catch (Exception ex)
            {
                await Task.CompletedTask;
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void StartApiService(string userId, string messageHashId, string text = "", string contact = "", string filePath = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string productId = "", string lat = "", string lng = "", string storyId = "", string replyId = "")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(WindowActivity, WindowActivity?.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SendMessage(userId, messageHashId, text, contact, filePath, imageUrl, stickerId, gifUrl, productId, lat, lng, storyId, replyId) });
        }

        private static async Task SendMessage(string userId, string messageHashId, string text = "", string contact = "", string filePath = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string productId = "", string lat = "", string lng = "", string storyId = "", string replyId = "")
        {
            var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(userId, messageHashId, text, contact, filePath, imageUrl, stickerId, gifUrl, productId, lat, lng, storyId, replyId);
            if (apiStatus == 200)
            {
                if (respond is SendMessageObject result)
                {
                    UpdateLastIdMessage(result);
                }
            }
            else Methods.DisplayReportResult(WindowActivity, respond);
        }

        public static void UpdateLastIdMessage(SendMessageObject chatMessages)
        {
            try
            {
                MessageData messageInfo = chatMessages?.MessageData?.FirstOrDefault();
                if (messageInfo != null)
                {
                    var typeModel = Holders.GetTypeModel(messageInfo);
                    if (typeModel == MessageModelType.None)
                        return;

                    AdapterModelsClassMessage checker = WindowActivity?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == messageInfo.MessageHashId);
                    if (checker != null)
                    {
                        var message = WoWonderTools.MessageFilter(messageInfo.ToId, messageInfo, typeModel, true);
                        message.ModelType = typeModel;
                        message.ErrorSendMessage = false;
                        message.Seen ??= "0";
                        message.BtnDownload = true;

                        checker.MesData = message;
                        checker.Id = Java.Lang.Long.ParseLong(message.Id);
                        checker.TypeView = typeModel;

                        #region LastChat

                        var updaterUser = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == message.ToId && a.LastChat?.ChatType == "user");
                        if (updaterUser?.LastChat != null)
                        {
                            var index = GlobalContext.LastChatTab.MAdapter.LastChatsList.IndexOf(updaterUser);
                            if (index > -1)
                            {
                                updaterUser.LastChat.LastMessage.LastMessageClass.Text = typeModel switch
                                {
                                    MessageModelType.RightGif => WindowActivity?.GetText(Resource.String.Lbl_SendGifFile),
                                    MessageModelType.RightText => !string.IsNullOrEmpty(message.Text) ? Methods.FunString.DecodeString(message.Text) : WindowActivity?.GetText(Resource.String.Lbl_SendMessage),
                                    MessageModelType.RightSticker => WindowActivity?.GetText(Resource.String.Lbl_SendStickerFile),
                                    MessageModelType.RightContact => WindowActivity?.GetText(Resource.String.Lbl_SendContactnumber),
                                    MessageModelType.RightFile => WindowActivity?.GetText(Resource.String.Lbl_SendFile),
                                    MessageModelType.RightVideo => WindowActivity?.GetText(Resource.String.Lbl_SendVideoFile),
                                    MessageModelType.RightImage => WindowActivity?.GetText(Resource.String.Lbl_SendImageFile),
                                    MessageModelType.RightAudio => WindowActivity?.GetText(Resource.String.Lbl_SendAudioFile),
                                    MessageModelType.RightMap => WindowActivity?.GetText(Resource.String.Lbl_SendLocationFile),
                                    _ => updaterUser.LastChat?.LastMessage.LastMessageClass.Text
                                };

                                GlobalContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (!updaterUser.LastChat.IsPin)
                                        {
                                            var checkPin = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                            if (checkPin != null)
                                            {
                                                var toIndex = GlobalContext.LastChatTab.MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                                if (index != toIndex)
                                                {
                                                    GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, toIndex);
                                                    GlobalContext?.LastChatTab?.MAdapter?.NotifyItemMoved(index, toIndex);
                                                }

                                                GlobalContext?.LastChatTab?.MAdapter?.NotifyItemChanged(toIndex, "WithoutBlobText");
                                            }
                                            else
                                            {
                                                if (ListUtils.FriendRequestsList.Count > 0)
                                                {
                                                    if (index != 1)
                                                    {
                                                        GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 1);
                                                        GlobalContext?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 1);
                                                    }

                                                    GlobalContext?.LastChatTab?.MAdapter?.NotifyItemChanged(1, "WithoutBlobText");
                                                }
                                                else
                                                {
                                                    if (index != 0)
                                                    {
                                                        GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.Move(index, 0);
                                                        GlobalContext?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                                    }

                                                    GlobalContext?.LastChatTab?.MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });

                                SqLiteDatabase dbSqLite = new SqLiteDatabase();
                                //Update All data users to database
                                dbSqLite.Insert_Or_Update_one_LastUsersChat(updaterUser?.LastChat);

                            }
                        }
                        else
                        {
                            //insert new user  
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ChatUpdaterHelper.LoadChatAsync });
                        }

                        #endregion

                        //Update All data users to database
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Update_To_one_MessagesTable(checker.MesData);

                        WindowActivity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                //Update data RecyclerView Messages.
                                //if (message.ModelType == MessageModelType.RightSticker || message.ModelType == MessageModelType.RightImage || message.ModelType == MessageModelType.RightMap || message.ModelType == MessageModelType.RightVideo)
                                WindowActivity?.Update_One_Messages(checker.MesData);

                                if (UserDetails.SoundControl)
                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}