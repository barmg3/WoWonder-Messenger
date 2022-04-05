using System;
using System.Linq;
using Android.Widget;
using Java.Lang;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Story;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Message;
using Exception = System.Exception;

namespace WoWonder.Activities.StickersView
{
    public class StickerItemClickListener
    {
        private readonly string Type;
        private readonly ChatWindowActivity ChatWindow;
        private readonly GroupChatWindowActivity GroupActivityView;
        private readonly PageChatWindowActivity PageActivityView;
        private readonly StoryReplyActivity StoryReplyActivity;
        private readonly string TimeNow = DateTime.Now.ToString("hh:mm");

        public StickerItemClickListener(string type)
        {
            try
            {
                Type = type;

                switch (Type)
                {
                    // Create your fragment here
                    case "ChatWindowActivity":
                        ChatWindow = ChatWindowActivity.GetInstance();
                        break;
                    case "PageChatWindowActivity":
                        PageActivityView = PageChatWindowActivity.GetInstance();
                        break;
                    case "GroupChatWindowActivity":
                        GroupActivityView = GroupChatWindowActivity.GetInstance();
                        break;
                    case "StoryReplyActivity":
                        StoryReplyActivity = StoryReplyActivity.GetInstance();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StickerAdapterOnOnItemClick(string stickerUrl)
        {
            try
            {
                var position = "1";
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                switch (Type)
                {
                    case "ChatWindowActivity":
                        {
                            MessageDataExtra m1 = new MessageDataExtra
                            {
                                Id = unixTimestamp.ToString(),
                                FromId = UserDetails.UserId,
                                ToId = ChatWindow.UserId,
                                Media = stickerUrl,
                                TimeText = TimeNow,
                                Position = "right",
                                Seen = "-1",
                                Time = unixTimestamp.ToString(),
                                ModelType = MessageModelType.RightSticker,
                                SendFile = true,
                            };

                            if (ChatWindow.SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ChatWindow.ReplyId))
                            {
                                m1.ReplyId = ChatWindow.ReplyId;
                                m1.Reply = new MessageData.ReplyUnion
                                {
                                    ReplyClass = ChatWindow.SelectedItemPositions.MesData
                                };
                            }

                            ChatWindow.MAdapter.DifferList.Add(new AdapterModelsClassMessage
                            {
                                TypeView = MessageModelType.RightSticker,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = ChatWindow.MAdapter.DifferList.IndexOf(ChatWindow.MAdapter.DifferList.FirstOrDefault(a => a.MesData == m1));
                            if (indexMes > -1)
                            {
                                ChatWindow.MAdapter.NotifyItemInserted(indexMes);
                                ChatWindow.MRecycler.ScrollToPosition(ChatWindow.MAdapter.ItemCount - 1);
                            }

                            if (Methods.CheckConnectivity())
                            {
                                //Sticker Send Function
                                MessageController.SendMessageTask(ChatWindow, ChatWindow.UserId, ChatWindow.ChatId, unixTimestamp.ToString(), "", "", "", stickerUrl, "sticker" + position, "", "", "", "", "", ChatWindow.ReplyId).ConfigureAwait(false);
                            }
                            else
                            {
                                ToastUtils.ShowToast(ChatWindow, ChatWindow.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                            break;
                        }
                    case "GroupChatWindowActivity":
                        {
                            MessageDataExtra m1 = new MessageDataExtra
                            {
                                Id = unixTimestamp.ToString(),
                                FromId = UserDetails.UserId,
                                GroupId = GroupActivityView.GroupId,
                                Media = stickerUrl,
                                TimeText = TimeNow,
                                Position = "right",
                                Seen = "-1",
                                Time = unixTimestamp.ToString(),
                                ModelType = MessageModelType.RightSticker,
                                SendFile = true,
                            };

                            if (GroupActivityView.SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(GroupActivityView.ReplyId))
                            {
                                m1.ReplyId = GroupActivityView.ReplyId;
                                m1.Reply = new MessageData.ReplyUnion
                                {
                                    ReplyClass = GroupActivityView.SelectedItemPositions.MesData
                                };
                            }

                            GroupActivityView.MAdapter.DifferList.Add(new AdapterModelsClassMessage
                            {
                                TypeView = MessageModelType.RightSticker,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = GroupActivityView.MAdapter.DifferList.IndexOf(GroupActivityView.MAdapter.DifferList.FirstOrDefault(a => a.MesData == m1));
                            if (indexMes > -1)
                            {
                                GroupActivityView.MAdapter.NotifyItemInserted(indexMes);
                                GroupActivityView.MRecycler.ScrollToPosition(GroupActivityView.MAdapter.ItemCount - 1);
                            }

                            if (Methods.CheckConnectivity())
                            {
                                //Sticker Send Function
                                GroupMessageController.SendMessageTask(GroupActivityView, GroupActivityView.GroupId, GroupActivityView.ChatId, unixTimestamp.ToString(), "", "", "", stickerUrl, "sticker" + position, "", "", "", GroupActivityView.ReplyId).ConfigureAwait(false);
                            }
                            else
                            {
                                ToastUtils.ShowToast(GroupActivityView, GroupActivityView.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                            break;
                        }
                    case "PageChatWindowActivity":
                        {
                            MessageDataExtra m1 = new MessageDataExtra
                            {
                                Id = unixTimestamp.ToString(),
                                FromId = UserDetails.UserId,
                                PageId = PageActivityView.PageId,
                                Media = stickerUrl,
                                TimeText = TimeNow,
                                Position = "right",
                                Seen = "-1",
                                Time = unixTimestamp.ToString(),
                                ModelType = MessageModelType.RightSticker,
                                SendFile = true,
                            };

                            if (PageActivityView.SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(PageActivityView.ReplyId))
                            {
                                m1.ReplyId = PageActivityView.ReplyId;
                                m1.Reply = new MessageData.ReplyUnion
                                {
                                    ReplyClass = PageActivityView.SelectedItemPositions.MesData
                                };
                            }

                            PageActivityView.MAdapter.DifferList.Add(new AdapterModelsClassMessage
                            {
                                TypeView = MessageModelType.RightSticker,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = PageActivityView.MAdapter.DifferList.IndexOf(PageActivityView.MAdapter.DifferList.FirstOrDefault(a => a.MesData == m1));
                            if (indexMes > -1)
                            {
                                PageActivityView.MAdapter.NotifyItemInserted(indexMes);
                                PageActivityView.MRecycler.ScrollToPosition(PageActivityView.MAdapter.ItemCount - 1);
                            }

                            if (Methods.CheckConnectivity())
                            {
                                //Sticker Send Function
                                PageMessageController.SendMessageTask(PageActivityView, PageActivityView.PageId,PageActivityView.ChatId, PageActivityView.UserId, unixTimestamp.ToString(), "", "", "", stickerUrl, "sticker" + position, "", "", "", PageActivityView.ReplyId).ConfigureAwait(false);
                            }
                            else
                            {
                                ToastUtils.ShowToast(PageActivityView, PageActivityView.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                            break;
                        }
                    case "StoryReplyActivity":
                        {
                            if (Methods.CheckConnectivity())
                            {
                                //Sticker Send Function
                                StoryReplyActivity.SendMess(StoryReplyActivity.UserId, "", "", "", stickerUrl, "sticker" + position).ConfigureAwait(false);
                            }
                            else
                            {
                                ToastUtils.ShowToast(StoryReplyActivity, StoryReplyActivity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                            break;
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