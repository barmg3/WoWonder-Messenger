using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Activities.Call.Adapters
{
    public class AddNewCallAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AddNewCallAdapterClickEventArgs> ItemClick;
        public event EventHandler<AddNewCallAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<AddNewCallAdapterClickEventArgs> AudioCallClick;
        public event EventHandler<AddNewCallAdapterClickEventArgs> VideoCallClick;
        private readonly Activity ActivityContext;

        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        private readonly List<string> ListOnline = new List<string>();

        public AddNewCallAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> AddNewCall_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AddNewCallView, parent, false);
                var holder = new AddNewCallAdapterViewHolder(itemView, OnClick, OnLongClick, AudioCallOnClick, VideoCallOnClick);
                return holder;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is AddNewCallAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Initialize(AddNewCallAdapterViewHolder holder, UserDataObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                holder.TxtUsername.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item), 25);

                holder.TxtUsername.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                holder.TxtPlatform.Text = ActivityContext.GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(item.LastseenUnixTime), true);

                //Online Or offline
                if (item.Lastseen == "on")
                {
                    holder.ImageLastseen.SetImageResource(Resource.Drawable.Green_Online);
                    if (AppSettings.ShowOnlineOfflineMessage)
                    {
                        var data = ListOnline.Contains(item.Name);
                        if (data == false)
                        {
                            ListOnline.Add(item.Name);

                            Toast toast = Toast.MakeText(ActivityContext, item.Name + " " + ActivityContext.GetString(Resource.String.Lbl_Online), ToastLength.Short);
                            toast?.SetGravity(GravityFlags.Center, 0, 0);
                            toast?.Show();
                        }
                    }
                }
                else
                {
                    holder.ImageLastseen.SetImageResource(Resource.Drawable.Grey_Offline);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;

        public UserDataObject GetItem(int position)
        {
            return UserList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void OnClick(AddNewCallAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AddNewCallAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void AudioCallOnClick(AddNewCallAdapterClickEventArgs args) => AudioCallClick?.Invoke(this, args);
        void VideoCallOnClick(AddNewCallAdapterClickEventArgs args) => VideoCallClick?.Invoke(this, args);

    }

    public class AddNewCallAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }

        public RelativeLayout RelativeLayoutMain { get; set; }
        public TextView TxtUsername { get; set; }
        public TextView TxtPlatform { get; set; }
        public ImageView ImageAvatar { get; set; }
        public CircleImageView ImageLastseen { get; set; }
        public TextView TxtIconAudioCall { get; set; }
        public TextView TxtIconVideoCall { get; set; }

        #endregion 

        public AddNewCallAdapterViewHolder(View itemView, Action<AddNewCallAdapterClickEventArgs> clickListener, Action<AddNewCallAdapterClickEventArgs> longClickListener
            , Action<AddNewCallAdapterClickEventArgs> audioCallclickListener, Action<AddNewCallAdapterClickEventArgs> videoCallclickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                RelativeLayoutMain = (RelativeLayout)MainView.FindViewById(Resource.Id.main);
                TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_Username);
                TxtPlatform = (TextView)MainView.FindViewById(Resource.Id.Txt_Userplatform);
                ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.Img_Avatar);
                ImageLastseen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);
                TxtIconAudioCall = (TextView)MainView.FindViewById(Resource.Id.IconAudioCall);
                TxtIconVideoCall = (TextView)MainView.FindViewById(Resource.Id.IconVideoCall);

                itemView.Click += (sender, e) => clickListener(new AddNewCallAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new AddNewCallAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                TxtIconAudioCall.Click += (sender, e) => audioCallclickListener(new AddNewCallAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                TxtIconVideoCall.Click += (sender, e) => videoCallclickListener(new AddNewCallAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                if (AppSettings.EnableVideoCall)
                {
                    TxtIconVideoCall.Visibility = ViewStates.Visible;
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtIconVideoCall, IonIconsFonts.IosVideocam);
                    TxtIconVideoCall.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#444444"));
                }
                else
                {
                    TxtIconVideoCall.Visibility = ViewStates.Gone;
                }

                if (AppSettings.EnableAudioCall)
                {
                    TxtIconAudioCall.Visibility = ViewStates.Visible;
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtIconAudioCall, IonIconsFonts.Call);
                    TxtIconAudioCall.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                }
                else
                {
                    TxtIconAudioCall.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class AddNewCallAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}