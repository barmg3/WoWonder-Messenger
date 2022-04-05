using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.Interpolator.View.Animation;
using Bumptech.Glide;
using Com.Aghajari.Emojiview.View;
using Java.IO;
using Java.Lang;
using MaterialDialogsCore;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.ChatWindow.Fragment;
using WoWonder.Activities.Gif;
using WoWonder.Activities.StickersView;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Stories.DragView;
using WoWonder.Library.Anjo.XRecordView;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using SupportFragment = AndroidX.Fragment.App.Fragment;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/DragTransparentBlack", ResizeableActivity = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class StoryReplyActivity : AppCompatActivity, DragToClose.IDragListener, MaterialDialog.IListCallback, IOnRecordClickListener, IOnRecordListener
    {
        #region Variables Basic

        private static StoryReplyActivity Instance;
        private DragToClose DragToClose;
        public ImageView ChatEmojImage;
        private LinearLayout RootView;
        private LinearLayout LayoutEditText;
        public AXEmojiEditText EmojIconEditTextView;
        public ImageView ChatMediaButton;
        private ChatRecordSoundFragment ChatRecordSoundBoxFragment;
        public FrameLayout ButtonFragmentHolder;
        public FrameLayout TopFragmentHolder;
        private Fragment MainFragmentOpened;
        private Methods.AudioRecorderAndPlayer RecorderService;
        private FastOutSlowInInterpolator Interpolation;
        private string PermissionsType;
        private bool IsRecording;
        public string StoryId, UserId; // to_id  
        private StoryDataObject.Story DataStories;
        private LinearLayout FirstBoxOnButton;

        private RecordView RecordView;
        public RecordButton RecordButton;

        private LinearLayout RepliedMessageView;
        private TextView TxtOwnerName, TxtMessageType, TxtShortMessage;
        private ImageView MessageFileThumbnail, BtnCloseReply;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window?.SetSoftInputMode(SoftInput.AdjustResize);
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this, true);

                // Create your application here
                SetContentView(Resource.Layout.StoryReplyLayout);

                Instance = this;

                UserId = Intent?.GetStringExtra("recipientId") ?? "";
                StoryId = Intent?.GetStringExtra("StoryId") ?? "";
                DataStories = JsonConvert.DeserializeObject<StoryDataObject.Story>(Intent?.GetStringExtra("DataNowStory") ?? "");

                //Get Value And Set Toolbar
                InitComponent();
                ReplyItems();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                //Audio FrameWork initialize 
                RecorderService = new Methods.AudioRecorderAndPlayer(UserId);

                Interpolation = new FastOutSlowInInterpolator();

                ChatRecordSoundBoxFragment = new ChatRecordSoundFragment("StoryReplyActivity");

                DragToClose = FindViewById<DragToClose>(Resource.Id.drag_to_close);
                DragToClose.SetCloseOnClick(true);
                DragToClose.SetDragListener(this);

                RootView = FindViewById<LinearLayout>(Resource.Id.reply_story);
                ChatEmojImage = FindViewById<ImageView>(Resource.Id.emojiicon);
                LayoutEditText = FindViewById<LinearLayout>(Resource.Id.LayoutEditText);
                EmojIconEditTextView = FindViewById<AXEmojiEditText>(Resource.Id.EmojiconEditText5);

                //ChatColorButton = FindViewById<ImageView>(Resource.Id.colorButton);
                //ChatColorButton.Visibility = ViewStates.Gone;

                //ChatStickerButton = FindViewById<ImageView>(Resource.Id.stickerButton);
                ChatMediaButton = FindViewById<ImageView>(Resource.Id.mediaButton);
                ButtonFragmentHolder = FindViewById<FrameLayout>(Resource.Id.ButtomFragmentHolder);
                TopFragmentHolder = FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);
                FirstBoxOnButton = FindViewById<LinearLayout>(Resource.Id.firstBoxonButtom);

                RecordView = FindViewById<RecordView>(Resource.Id.record_view);
                RecordButton = FindViewById<RecordButton>(Resource.Id.record_button);

                RecordButton.SetRecordView(RecordView);
                RecordButton.SetOnRecordClickListener(this); //Send Text Messeages

                //Cancel Bounds is when the Slide To Cancel text gets before the timer . default is 8
                RecordView.SetCancelBounds(8);
                RecordView.SetSmallMicColor(Color.ParseColor("#c2185b"));

                //prevent recording under one Second
                RecordView.SetLessThanSecondAllowed(false);
                RecordView.SetSlideToCancelText(GetText(Resource.String.Lbl_SlideToCancelAudio));
                RecordView.SetCustomSounds(Resource.Raw.record_start, Resource.Raw.record_finished, Resource.Raw.record_error);

                RecordView.SetOnRecordListener(this);

                RepliedMessageView = FindViewById<LinearLayout>(Resource.Id.replied_message_view);
                TxtOwnerName = FindViewById<TextView>(Resource.Id.owner_name);
                TxtMessageType = FindViewById<TextView>(Resource.Id.message_type);
                TxtShortMessage = FindViewById<TextView>(Resource.Id.short_message);
                MessageFileThumbnail = FindViewById<ImageView>(Resource.Id.message_file_thumbnail);
                BtnCloseReply = FindViewById<ImageView>(Resource.Id.clear_btn_reply_view);
                BtnCloseReply.Visibility = ViewStates.Visible;
                TxtOwnerName.SetTextColor(Color.White);
                TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                BtnCloseReply.SetColorFilter(Color.White);
                MessageFileThumbnail.Visibility = ViewStates.Gone;

                SupportFragmentManager.BeginTransaction().Add(TopFragmentHolder.Id, ChatRecordSoundBoxFragment, "Chat_Recourd_Sound_Fragment");
                TopFragmentHolder.SetBackgroundColor(Color.ParseColor("#282828"));


                if (AppSettings.ShowButtonRecordSound)
                {
                    //ChatSendButton.LongClickable = true;
                    RecordButton.Tag = "Free";
                    RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                    RecordButton.SetListenForRecord(true);
                }
                else
                {
                    RecordButton.Tag = "Text";
                    RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                    RecordButton.SetListenForRecord(false);
                }

                RecordButton.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));

                //if (AppSettings.ShowButtonStickers)
                //{
                //    ChatStickerButton.Visibility = ViewStates.Visible;
                //    ChatStickerButton.Tag = "Closed";
                //}
                //else
                //{
                //    ChatStickerButton.Visibility = ViewStates.Gone;
                //}

                Methods.SetColorEditText(EmojIconEditTextView, Color.White);
                ChatEmojImage.SetColorFilter(Color.White);

                if (AppSettings.SetTabDarkTheme)
                    EmojisViewTools.LoadDarkTheme();
                else
                    EmojisViewTools.LoadTheme(AppSettings.MainColor);

                EmojisViewTools.MStickerView = true;
                EmojisViewTools.LoadView(this, EmojIconEditTextView, "StoryReplyActivity", ChatEmojImage);

                EmojIconEditTextView.AddTextChangedListener(new MyTextWatcher(this));

                EmojIconEditTextView.PerformClick();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -= 
                if (addEvent)
                {
                    ChatMediaButton.Click += ChatMediaButtonOnClick;
                    //ChatStickerButton.Click += ChatStickerButtonOnClick;
                    BtnCloseReply.Click += BtnCloseReplyOnClick;
                }
                else
                {
                    ChatMediaButton.Click -= ChatMediaButtonOnClick;
                    //ChatStickerButton.Click -= ChatStickerButtonOnClick;
                    BtnCloseReply.Click -= BtnCloseReplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static StoryReplyActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Events

        private void BtnCloseReplyOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();

                Intent resultIntent = new Intent();
                resultIntent.PutExtra("isReply", true);
                SetResult(Result.Ok, resultIntent);

                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void EmojIconEditTextViewOnTextChanged(/*object sender, TextChangedEventArgs e*/)
        {
            try
            {
                if (AppSettings.ShowButtonRecordSound)
                {
                    if (!ButtonFragmentHolder.TranslationY.Equals(1200))
                        ButtonFragmentHolder.TranslationY = 1200;

                    if (IsRecording && EmojIconEditTextView.Text == GetString(Resource.String.Lbl_Recording))
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);

                        EditTextOpen();
                    }
                    else if (!string.IsNullOrEmpty(EmojIconEditTextView.Text))
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);

                        EditTextOpen();
                    }
                    else if (IsRecording)
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);

                        EditTextOpen();
                    }
                    else
                    {
                        RecordButton.Tag = "Free";
                        RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        RecordButton.SetListenForRecord(true);

                        EditTextClose();

                        RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "stopped").ConfigureAwait(false);
                    }
                }
                else
                {
                    RecordButton.Tag = "Text";
                    RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                    RecordButton.SetListenForRecord(false);

                    EditTextOpen();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditTextClose()
        {
            try
            {
                ChatMediaButton.SetImageResource(Resource.Drawable.icon_attach_vector);
                ChatMediaButton.SetColorFilter(Color.ParseColor("#444444"));
                ChatMediaButton.Tag = "attachment";
                ViewGroup.LayoutParams layoutParams = ChatMediaButton.LayoutParameters;
                layoutParams.Width = 52;
                layoutParams.Height = 52;
                ChatMediaButton.LayoutParameters = layoutParams;
                //ChatStickerButton.Visibility = ViewStates.Visible;
                //ChatColorButton.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditTextOpen()
        {
            try
            {
                ChatMediaButton.SetImageResource(Resource.Drawable.ic_next);
                ChatMediaButton.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                ChatMediaButton.Tag = "arrow";
                //ChatStickerButton.Visibility = ViewStates.Gone;
                //ChatColorButton.Visibility = ViewStates.Gone;
                ViewGroup.LayoutParams layoutParams = ChatMediaButton.LayoutParameters;
                layoutParams.Width = 42;
                layoutParams.Height = 42;
                ChatMediaButton.LayoutParameters = layoutParams;

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    UserDetails.Socket?.EmitAsync_TypingEvent(UserId, UserDetails.AccessToken);
                }
                else
                {
                    RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "typing").ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Intent Contact (result is 506 , Permissions is 101 )
        private void ChatContactButtonOnClick()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //request code of result is 506
                    new IntentController(this).OpenIntentGetContactNumberPhone();
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted)
                    {
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                    }
                    else
                    {
                        //101 >> ReadContacts && ReadPhoneNumbers
                        new PermissionsController(this).RequestPermission(101);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Intent Location (result is 506 , Permissions is 101 )
        private void ChatLocationButtonOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send Sticker
        //private void ChatStickerButtonOnClick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (ChatStickerButton?.Tag?.ToString() == "Closed")
        //        {
        //            ResetButtonTags();
        //            ChatStickerButton.Tag = "Opened";
        //            ChatStickerButton?.Drawable?.SetTint(Color.ParseColor(AppSettings.MainColor));
        //            ReplaceButtonFragment(ChatStickersTabBoxFragment);
        //        }
        //        else
        //        {
        //            ResetButtonTags();
        //            ChatStickerButton?.Drawable?.SetTint(Color.ParseColor("#888888"));
        //            TopFragmentHolder?.Animate()?.SetInterpolator(Interpolation)?.TranslationY(1200)?.SetDuration(300);
        //            SupportFragmentManager.BeginTransaction().Remove(ChatStickersTabBoxFragment)?.Commit();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        // Event sent media (image , Camera , video , file , music )
        private void ChatMediaButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (ChatMediaButton?.Tag?.ToString())
                {
                    case "attachment":
                        {
                            var arrayAdapter = new List<string>();
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            if (AppSettings.ShowButtonImage)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_ImageGallery));
                            if (AppSettings.ShowButtonCamera)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_TakeImageFromCamera));
                            if (AppSettings.ShowButtonVideo && WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                            if (AppSettings.ShowButtonVideo && WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));
                            if (AppSettings.ShowButtonAttachFile && WoWonderTools.CheckAllowedFileSharingInServer("File"))
                                arrayAdapter.Add(GetText(Resource.String.Lbl_File));
                            if (AppSettings.ShowButtonMusic && WoWonderTools.CheckAllowedFileSharingInServer("Audio"))
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Music));
                            if (AppSettings.ShowButtonGif)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Gif));
                            if (AppSettings.ShowButtonContact)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Contact));
                            if (AppSettings.ShowButtonLocation)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Location));

                            dialogList.Title(GetString(Resource.String.Lbl_Select_what_you_want));
                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                            break;
                        }
                    case "arrow":
                        EditTextClose();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send Message type => "right_audio" Or "right_text"
        public void OnClick_OfSendButton()
        {
            try
            {
                IsRecording = false;

                //var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                //string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                switch (RecordButton?.Tag?.ToString())
                {
                    case "Audio":
                        {
                            var interTortola = new FastOutSlowInInterpolator();
                            TopFragmentHolder.Animate().SetInterpolator(interTortola).TranslationY(1200).SetDuration(300);
                            SupportFragmentManager.BeginTransaction().Remove(ChatRecordSoundBoxFragment)?.Commit();

                            string filePath = RecorderService.GetRecorded_Sound_Path();
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                //Here on This function will send Selected audio file to the user
                                if (Methods.CheckConnectivity())
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        SendMess(UserId, "", "", filePath).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                }
                            }

                            RecordButton.Tag = "Free";
                            RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                            RecordButton.SetListenForRecord(true);
                            break;
                        }
                    case "Text":
                        {
                            if (string.IsNullOrEmpty(EmojIconEditTextView.Text))
                            {

                            }
                            else
                            {
                                //remove \n in a string
                                string replacement = Regex.Replace(EmojIconEditTextView.Text, @"\t|\n|\r", "");

                                if (Methods.CheckConnectivity())
                                {
                                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                                    {
                                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                        var time2 = unixTimestamp.ToString();

                                        UserDetails.Socket?.EmitAsync_SendMessage(UserId, UserDetails.AccessToken, UserDetails.Username, replacement, AppSettings.MainColor, "", time2, StoryId);
                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            SendMess(UserId, replacement).ConfigureAwait(false);
                                        });
                                    }
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                }

                                EmojIconEditTextView.Text = "";
                            }

                            if (AppSettings.ShowButtonRecordSound)
                            {
                                RecordButton.Tag = "Free";
                                RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                                RecordButton.SetListenForRecord(true);
                            }
                            else
                            {
                                RecordButton.Tag = "Text";
                                RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                                RecordButton.SetListenForRecord(false);
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

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                string timeNow = DateTime.Now.ToShortTimeString();
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = Convert.ToString(unixTimestamp);

                switch (requestCode)
                {
                    // right_contact
                    case 506 when resultCode == Result.Ok:
                        {
                            var contact = Methods.PhoneContactManager.Get_ContactInfoBy_Id(data.Data.LastPathSegment);
                            if (contact != null)
                            {
                                var name = contact.UserDisplayName;
                                var phone = contact.PhoneNumber;

                                var dictionary = new Dictionary<string, string>();

                                if (!dictionary.ContainsKey(name))
                                {
                                    dictionary.Add(name, phone);
                                }

                                string dataContact = JsonConvert.SerializeObject(dictionary.ToArray().FirstOrDefault(a => a.Key == name));

                                if (Methods.CheckConnectivity())
                                {
                                    //Send contact function
                                    await Task.Factory.StartNew(() =>
                                    {
                                        SendMess(UserId, dataContact, "1").ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                }
                            }

                            break;
                        }
                    // right_image 
                    case 500 when resultCode == Result.Ok:
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                                if (!check)
                                {
                                    if (info == "AdultImages")
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                                    }
                                    else
                                    {
                                        //this file not supported on the server , please select another file 
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                                    }
                                    return;
                                }

                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Image")
                                {
                                    //Send image function
                                    if (Methods.CheckConnectivity())
                                    {
                                        await Task.Factory.StartNew(() =>
                                        {
                                            SendMess(UserId, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                        });
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                                }
                            }

                            break;
                        }
                    // right_image 
                    case CropImage.CropImageActivityRequestCode when resultCode == Result.Ok:
                        {
                            var result = CropImage.GetActivityResult(data);
                            if (resultCode == Result.Ok)
                            {
                                if (result.IsSuccessful)
                                {
                                    var resultUri = result.Uri;

                                    if (!string.IsNullOrEmpty(resultUri.Path))
                                    {
                                        //Send image function
                                        if (Methods.CheckConnectivity())
                                        {
                                            await Task.Factory.StartNew(() =>
                                            {
                                                SendMess(UserId, EmojIconEditTextView.Text, "", resultUri.Path).ConfigureAwait(false);
                                            });
                                        }
                                        else
                                        {
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                        }
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                    }
                                }
                            }

                            break;
                        }
                    // Add right_image using camera   
                    case 503 when resultCode == Result.Ok:
                        {
                            if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                            }
                            else
                            {
                                //var thumbnail = MediaStore.Images.Media.GetBitmap(ContentResolver, IntentController.ImageCameraUri); 
                                //Bitmap bitmap = BitmapFactory.DecodeFile(IntentController.currentPhotoPath);

                                if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                                {
                                    //Send image function
                                    if (Methods.CheckConnectivity())
                                    {
                                        await Task.Factory.StartNew(() =>
                                        {
                                            SendMess(UserId, EmojIconEditTextView.Text, "", IntentController.CurrentPhotoPath).ConfigureAwait(false);
                                        });
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }
                                }
                                else
                                {
                                    //ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load),ToastLength.Short);
                                }
                            }

                            break;
                        }
                    // right_video 
                    case 501 when resultCode == Result.Ok:
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                                if (!check)
                                {
                                    if (info == "AdultImages")
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                                    }
                                    else
                                    {
                                        //this file not supported on the server , please select another file 
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                                    }
                                    return;
                                }

                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Video")
                                {
                                    var fileName = filepath.Split('/').Last();
                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var pathWithoutFilename = Methods.Path.FolderDiskVideo + UserId;
                                    //var fullPathFile = new File(Methods.Path.FolderDiskVideo + UserId, fileNameWithoutExtension + ".png");

                                    var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                    if (videoPlaceHolderImage == "File Dont Exists")
                                    {
                                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                    }

                                    //Send Video function
                                    if (Methods.CheckConnectivity())
                                    {
                                        await Task.Factory.StartNew(() =>
                                        {
                                            SendMess(UserId, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                        });
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }
                                }
                            }

                            break;
                        }
                    // right_video camera 
                    case 513 when resultCode == Result.Ok:
                        {
                            if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentVideoPath) != "File Dont Exists" && Build.VERSION.SdkInt <= BuildVersionCodes.OMr1)
                            {
                                //var fileName = IntentController.CurrentVideoPath.Split('/').Last();
                                //var fileNameWithoutExtension = fileName.Split('.').First();
                                //var path = Methods.Path.FolderDiskVideo + "/" + fileNameWithoutExtension + ".png";

                                //Send Video function
                                if (Methods.CheckConnectivity())
                                {
                                    await Task.Factory.StartNew(() =>
                                    {
                                        SendMess(UserId, EmojIconEditTextView.Text, "", IntentController.CurrentVideoPath).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                }
                            }
                            else
                            {
                                var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                                if (filepath != null)
                                {
                                    var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                    if (type == "Video")
                                    {
                                        //Send Video function
                                        if (Methods.CheckConnectivity())
                                        {
                                            await Task.Factory.StartNew(() =>
                                            {
                                                SendMess(UserId, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                            });
                                        }
                                        else
                                        {
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    // right_file
                    case 504 when resultCode == Result.Ok:
                        {
                            string filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                                if (!check)
                                {
                                    if (info == "AdultImages")
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                                    }
                                    else
                                    {
                                        //this file not supported on the server , please select another file 
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                                    }
                                    return;
                                }

                                //Send Video function
                                if (Methods.CheckConnectivity())
                                {
                                    await Task.Factory.StartNew(() =>
                                    {
                                        SendMess(UserId, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                }
                            }

                            break;
                        }
                    // right_audio
                    case 505 when resultCode == Result.Ok:
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                                if (!check)
                                {
                                    if (info == "AdultImages")
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                                    }
                                    else
                                    {
                                        //this file not supported on the server , please select another file 
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                                    }
                                    return;
                                }

                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Audio")
                                {
                                    //Send Video function
                                    if (Methods.CheckConnectivity())
                                    {
                                        await Task.Factory.StartNew(() =>
                                        {
                                            SendMess(UserId, "", "", filepath).ConfigureAwait(false);
                                        });
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                            }

                            break;
                        }
                    // right_gif
                    case 300 when resultCode == Result.Ok:
                        {
                            // G_fixed_height_small_url, // UrlGif - view  >>  mediaFileName
                            // G_fixed_height_small_mp4, //MediaGif - sent >>  media

                            var gifLink = data.GetStringExtra("MediaGif") ?? "Data not available";
                            if (gifLink != "Data not available" && !string.IsNullOrEmpty(gifLink))
                            {
                                var gifUrl = data.GetStringExtra("UrlGif") ?? "Data not available";

                                //Send image function
                                if (Methods.CheckConnectivity())
                                {
                                    await Task.Factory.StartNew(() =>
                                    {
                                        SendMess(UserId, EmojIconEditTextView.Text, "", "", "", "", gifUrl).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_check_your_details) + " ", ToastLength.Long);
                            }

                            break;
                        }
                    // Location
                    case 502 when resultCode == Result.Ok:
                        {
                            //var placeAddress = data.GetStringExtra("Address") ?? "";
                            var placeLatLng = data.GetStringExtra("latLng") ?? "";
                            if (!string.IsNullOrEmpty(placeLatLng))
                            {
                                string[] latLng = placeLatLng.Split(',');
                                if (latLng?.Length > 0)
                                {
                                    string lat = latLng[0];
                                    string lng = latLng[1];

                                    //Send image function
                                    if (Methods.CheckConnectivity())
                                    {
                                        await Task.Factory.StartNew(() =>
                                        {
                                            SendMess(UserId, "", "", "", "", "", "", lat, lng).ConfigureAwait(false);
                                        });
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }
                                }
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

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 123 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Methods.Path.Chack_MyFolder(UserId);
                        break;
                    case 123:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        switch (PermissionsType)
                        {
                            //requestCode >> 500 => Image Gallery
                            //case "Image" when AppSettings.ImageCropping:
                            //    OpenDialogGallery("Image");
                            //    break;
                            case "Image": //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false);
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "Video_camera":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            case "Camera":
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                                break;
                        }

                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        switch (PermissionsType)
                        {
                            case "File":
                                //requestCode >> 504 => File
                                new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                                break;
                            case "Music":
                                //requestCode >> 505 => Music
                                new IntentController(this).OpenIntentAudio();
                                break;
                        }

                        break;
                    case 100:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 101 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                        break;
                    case 101:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 102 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        {
                            if (RecordButton?.Tag?.ToString() == "Free")
                            {
                                //Set Record Style
                                IsRecording = true;

                                EmojIconEditTextView.Visibility = ViewStates.Invisible;

                                RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                                //Start Audio record
                                //await Task.Delay(600);
                                RecorderService.StartRecording();
                            }

                            break;
                        }
                    case 102:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_ImageGallery)) // image 
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //if (AppSettings.ImageCropping)
                        //    OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                        //else
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                        {
                            //if (AppSettings.ImageCropping)
                            //    OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                            //else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_TakeImageFromCamera)) // Camera 
                {
                    PermissionsType = "Camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_VideoGallery)) // video  
                {
                    PermissionsType = "Video";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_RecordVideoFromCamera)) // video camera
                {
                    PermissionsType = "Video_camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 513 => video camera
                        new IntentController(this).OpenIntentVideoCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                        {
                            //requestCode >> 513 => video camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_File)) // File  
                {
                    PermissionsType = "File";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 504 => File
                        new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                    }
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage())
                        {
                            //requestCode >> 504 => File
                            new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(100);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_Music)) // Music  
                {
                    PermissionsType = "Music";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage())
                            new IntentController(this).OpenIntentAudio(); //505
                        else
                            new PermissionsController(this).RequestPermission(100);
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_Gif)) // Gif  
                {
                    StartActivityForResult(new Intent(this, typeof(GifActivity)), 300);
                }
                else if (itemString == GetText(Resource.String.Lbl_Contact)) // Contact  
                {
                    ChatContactButtonOnClick();
                }
                else if (itemString == GetText(Resource.String.Lbl_Location)) // Location  
                {
                    ChatLocationButtonOnClick();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Fragment

        private void ReplaceTopFragment(SupportFragment fragmentView)
        {
            try
            {
                HideKeyboard();

                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(TopFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                TopFragmentHolder.TranslationY = 1200;
                TopFragmentHolder.Animate()?.SetInterpolator(new FastOutSlowInInterpolator())?.TranslationYBy(-1200)?.SetDuration(500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReplaceButtonFragment(SupportFragment fragmentView)
        {
            try
            {
                if (fragmentView != MainFragmentOpened)
                {
                    //if (MainFragmentOpened == ChatStickersTabBoxFragment)
                    //{
                    //    //ChatStickerButton?.Drawable?.SetTint(Color.ParseColor("#888888"));
                    //}
                }

                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(ButtonFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                ButtonFragmentHolder.TranslationY = 1200;
                ButtonFragmentHolder?.Animate()?.SetInterpolator(new FastOutSlowInInterpolator())?.TranslationYBy(-1200)?.SetDuration(500);
                MainFragmentOpened = fragmentView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveButtonFragment()
        {
            try
            {
                if (SupportFragmentManager.BackStackEntryCount > 0)
                {
                    SupportFragmentManager.PopBackStack();
                    ResetButtonTags();
                    //ChatStickerButton.Drawable?.SetTint(Color.ParseColor("#888888"));

                    if (SupportFragmentManager.Fragments.Count > 0)
                    {
                        var fragmentManager = SupportFragmentManager.BeginTransaction();
                        foreach (var vrg in SupportFragmentManager.Fragments)
                        {
                            Console.WriteLine(vrg);
                            //if (SupportFragmentManager.Fragments.Contains(ChatStickersTabBoxFragment))
                            //{
                            //    fragmentManager.Remove(ChatStickersTabBoxFragment);
                            //}
                        }

                        fragmentManager.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Mic Record 

        public void OnClick(View v)
        {
            try
            {
                //ToastUtils.ShowToast(this, "RECORD BUTTON CLICKED", ToastLength.Short);
                OnClick_OfSendButton();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnStartRecord()
        {
            //ToastUtils.ShowToast(this, "OnStartRecord", ToastLength.Short);

            //record voices ( Permissions is 102 )
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (RecordButton?.Tag?.ToString() == "Free")
                    {
                        //Set Record Style
                        IsRecording = true;

                        LayoutEditText.Visibility = ViewStates.Invisible;
                        //ChatColorButton.Visibility = ViewStates.Gone;
                        //ChatStickerButton.Visibility = ViewStates.Invisible;
                        ChatMediaButton.Visibility = ViewStates.Invisible;

                        RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                        //Start Audio record
                        //await Task.Delay(600);
                        RecorderService.StartRecording();
                    }
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        if (RecordButton?.Tag?.ToString() == "Free")
                        {
                            //Set Record Style
                            IsRecording = true;

                            LayoutEditText.Visibility = ViewStates.Invisible;
                            //ChatColorButton.Visibility = ViewStates.Gone;
                            //ChatStickerButton.Visibility = ViewStates.Invisible;
                            ChatMediaButton.Visibility = ViewStates.Invisible;

                            RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                            //Start Audio record
                            //await Task.Delay(600);
                            RecorderService.StartRecording();
                        }
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(102);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnCancelRecord()
        {
            //ToastUtils.ShowToast(this, "OnCancelRecord", ToastLength.Short);
            // reset mic nd show edittext
            LayoutEditText.Visibility = ViewStates.Visible;
            //ChatColorButton.Visibility = ViewStates.Gone;
            //ChatStickerButton.Visibility = ViewStates.Visible;
            ChatMediaButton.Visibility = ViewStates.Visible;
        }

        public void OnFinishRecord(long recordTime)
        {
            //ToastUtils.ShowToast(this, "OnFinishRecord " + recordTime, ToastLength.Short);
            //open fragemt recoud and show edittext
            try
            {
                if (IsRecording)
                {
                    RecorderService.StopRecording();
                    var filePath = RecorderService.GetRecorded_Sound_Path();

                    RecordButton.Tag = "Text";
                    RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                    RecordButton.SetListenForRecord(false);

                    if (recordTime > 0)
                    {
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            Bundle bundle = new Bundle();
                            bundle.PutString("FilePath", filePath);
                            ChatRecordSoundBoxFragment.Arguments = bundle;
                            ReplaceTopFragment(ChatRecordSoundBoxFragment);
                        }
                    }

                    IsRecording = false;
                }

                LayoutEditText.Visibility = ViewStates.Visible;
                //ChatColorButton.Visibility = ViewStates.Gone;
                //ChatStickerButton.Visibility = ViewStates.Visible;
                ChatMediaButton.Visibility = ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnLessThanSecond()
        {
            //ToastUtils.ShowToast(this, "OnLessThanSecond", ToastLength.Short); 
        }

        #endregion

        #region Drag

        public void OnStartDraggingView()
        {

        }

        public void OnDraggingView(float offset)
        {
            try
            {
                RepliedMessageView.Alpha = offset;
                RootView.Alpha = offset;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnViewClosed()
        {
            try
            {
                Intent resultIntent = new Intent();
                resultIntent.PutExtra("isReply", true);
                SetResult(Result.Ok, resultIntent);

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override void OnBackPressed()
        {
            try
            {
                if (SupportFragmentManager.BackStackEntryCount > 0)
                {
                    RemoveButtonFragment();
                }
                else
                {
                    Intent resultIntent = new Intent();
                    resultIntent.PutExtra("isReply", true);
                    SetResult(Result.Ok, resultIntent);

                    base.OnBackPressed();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ResetButtonTags()
        {
            try
            {
                //ChatStickerButton.Tag = "Closed";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyTextWatcher : Java.Lang.Object, ITextWatcher
        {
            private readonly StoryReplyActivity ChatWindow;
            public MyTextWatcher(StoryReplyActivity chatWindow)
            {
                ChatWindow = chatWindow;
            }
            public void AfterTextChanged(IEditable s)
            {
                ChatWindow.EmojIconEditTextViewOnTextChanged();
            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {

            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {

            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Reply Messages //wael Todo
        private void ReplyItems()
        {
            try
            {
                if (DataStories != null)
                {
                    RepliedMessageView.Visibility = ViewStates.Visible;
                    var animation = new TranslateAnimation(0, 0, RepliedMessageView.Height, 0) { Duration = 300 };

                    RepliedMessageView.StartAnimation(animation);

                    TxtOwnerName.Text = WoWonderTools.GetNameFinal(DataStories.UserData);
                    MessageFileThumbnail.Visibility = ViewStates.Gone;
                    TxtMessageType.Visibility = ViewStates.Visible;
                    TxtMessageType.Text = " • " + GetText(Resource.String.Lbl_Story);

                    //TxtShortMessage.Text = message.Text;

                    MessageFileThumbnail.Visibility = ViewStates.Visible;

                    string mediaFile = "";
                    //image and video 
                    if (!DataStories.Thumbnail.Contains("avatar") && DataStories.Videos.Count == 0)
                        mediaFile = DataStories.Thumbnail;
                    else if (DataStories.Videos.Count > 0)
                        mediaFile = DataStories.Videos[0].Filename;

                    var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                    if (type == "Video")
                    {
                        TxtShortMessage.Text = GetText(Resource.String.video);

                        var fileName = mediaFile.Split('/').Last();
                        mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile, "other");
                        File file2 = new File(mediaFile);
                        try
                        {
                            Uri photoUri = mediaFile.Contains("http") ? Uri.Parse(mediaFile) : FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this)
                                .AsBitmap()
                                .Apply(GlideImageLoader.GetRequestOptions(ImageStyle.CenterCrop, ImagePlaceholders.Drawable))
                                .Load(photoUri) // or URI/path
                                .Into(MessageFileThumbnail);  //image view to set thumbnail to 
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);

                            Glide.With(this)
                                .AsBitmap()
                                .Apply(GlideImageLoader.GetRequestOptions(ImageStyle.CenterCrop, ImagePlaceholders.Drawable))
                                .Load(file2) // or URI/path
                                .Into(MessageFileThumbnail);  //image view to set thumbnail to 
                        }
                    }
                    else
                    {
                        TxtShortMessage.Text = GetText(Resource.String.image);

                        GlideImageLoader.LoadImage(this, mediaFile, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task SendMess(string userId = "", string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "")
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = unixTimestamp.ToString();

                    //Here on This function will send Selected audio file to the user 
                    var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(userId, time2, text, contact, pathFile, imageUrl, stickerId, gifUrl, "", lat, lng, StoryId);
                    if (apiStatus == 200)
                    {
                        if (respond is SendMessageObject result)
                        {
                            Console.WriteLine(result.MessageData);
                            MessageController.UpdateLastIdMessage(result);

                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    Intent resultIntent = new Intent();
                                    resultIntent.PutExtra("isReply", true);
                                    SetResult(Result.Ok, resultIntent);

                                    Finish();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}