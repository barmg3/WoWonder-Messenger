using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SocketSystem;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.PageChat;
using WoWonderClient.JobWorker;
using Console = System.Console;

namespace WoWonder.Helpers.Jobs
{
    public class UploadSingleFileToServerWorker : FileUploaderContract.IView
    {
        private FileModel MessageModel;
        private FileUploaderPresenter MUploaderPresenter;
        private readonly ChatWindowActivity ChatWindow;
        private readonly GroupChatWindowActivity GroupActivityView;
        private readonly PageChatWindowActivity PageActivityView;
        private readonly string TypePage;

        public UploadSingleFileToServerWorker(Activity activity, string typePage)
        {
            try
            {
                TypePage = typePage;
                switch (typePage)
                {
                    // Create your fragment here
                    case "ChatWindowActivity":
                        ChatWindow = (ChatWindowActivity)activity;
                        break;
                    case "PageChatWindowActivity":
                        PageActivityView = (PageChatWindowActivity)activity;
                        break;
                    case "GroupChatWindowActivity":
                        GroupActivityView = (GroupChatWindowActivity)activity;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UploadFileToServer(Context context, FileModel messageModel)
        {
            try
            {
                MUploaderPresenter = new FileUploaderPresenter(this, new FileUploaderModel());
                MessageModel = messageModel;
                if (MessageModel != null)
                {
                    if (AppSettings.ShowNotificationWithUpload)
                        AppNotificationsManager.Instance.ShowUpDownNotification(MessageModel.UserName, MessageModel.MessageHashId, MessageModel.UserId, MessageModel.ChatId);

                    MUploaderPresenter?.OnFileSelected(TypePage, messageModel.FilePath, messageModel);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowErrorMessage(string message)
        {
            try
            {
                Console.WriteLine(message);
                if (AppSettings.ShowNotificationWithUpload)
                    AppNotificationsManager.Instance.CancelNotification(MessageModel.MessageHashId);

                Toast.MakeText(Application.Context, message, ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UploadCompleted(string json)
        {
            try
            {
                Console.WriteLine("File upload successful");

                if (AppSettings.ShowNotificationWithUpload)
                    AppNotificationsManager.Instance.CancelNotification(MessageModel.MessageHashId);

                switch (TypePage)
                {
                    // Create your fragment here
                    case "ChatWindowActivity":
                        {
                            var data = JsonConvert.DeserializeObject<SendMessageObject>(json);
                            if (data?.Status == 200)
                            {
                                MessageController.UpdateLastIdMessage(data);
                            }
                            else
                            {
                                var error = JsonConvert.DeserializeObject<ErrorObject>(json);
                                Console.WriteLine(error);
                            }
                        }
                        break;
                    case "PageChatWindowActivity":
                        {
                            var data = JsonConvert.DeserializeObject<PageSendMessageObject>(json);
                            if (data?.Status == 200)
                            {
                                PageMessageController.UpdateLastIdMessage(data.Data, MessageModel.PageId, MessageModel.UserId);
                            }
                            else
                            {
                                var error = JsonConvert.DeserializeObject<ErrorObject>(json);
                                Console.WriteLine(error);
                            }
                        }
                        break;
                    case "GroupChatWindowActivity":
                        {
                            var data = JsonConvert.DeserializeObject<GroupSendMessageObject>(json);
                            if (data?.Status == 200)
                            {
                                GroupMessageController.UpdateLastIdMessage(data.Data);
                            }
                            else
                            {
                                var error = JsonConvert.DeserializeObject<ErrorObject>(json);
                                Console.WriteLine(error);
                            }
                        }
                        break;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UploadProgress(int uploadProgress)
        {
            try
            {
                if (AppSettings.ShowNotificationWithUpload)
                    AppNotificationsManager.Instance.UpdateUpDownNotification(MessageModel.MessageHashId, uploadProgress);

                switch (TypePage)
                {
                    // Create your fragment here
                    case "ChatWindowActivity":
                        {
                            var checker = ChatWindow?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == MessageModel.MessageHashId);
                            if (checker != null)
                            {
                                checker.MesData.MessageProgress = uploadProgress;
                                ChatWindow.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var index = ChatWindow.MAdapter.DifferList.IndexOf(checker);
                                        if (index > -1)
                                            ChatWindow?.MAdapter.NotifyItemChanged(index, "WithoutBlobUploadProgress");
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        break;
                    case "PageChatWindowActivity":
                        {
                            var checker = PageActivityView?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == MessageModel.MessageHashId);
                            if (checker != null)
                            {
                                checker.MesData.MessageProgress = uploadProgress;
                                PageActivityView.RunOnUiThread(() => { PageActivityView?.MAdapter.NotifyItemChanged(PageActivityView.MAdapter.DifferList.IndexOf(checker), "WithoutBlobUploadProgress"); });
                            }
                        }
                        break;
                    case "GroupChatWindowActivity":
                        {
                            var checker = GroupActivityView?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == MessageModel.MessageHashId);
                            if (checker != null)
                            {
                                checker.MesData.MessageProgress = uploadProgress;
                                GroupActivityView.RunOnUiThread(() => { GroupActivityView?.MAdapter.NotifyItemChanged(GroupActivityView.MAdapter.DifferList.IndexOf(checker), "WithoutBlobUploadProgress"); });
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}