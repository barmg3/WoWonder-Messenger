using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.SettingsPreferences.InviteFriends.Adapter
{
    public class InviteFriendsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<InviteFriendsAdapterClickEventArgs> ItemClick;
        public event EventHandler<InviteFriendsAdapterClickEventArgs> ItemLongClick;

        public ObservableCollection<Methods.PhoneContactManager.UserContact> MUsersPhoneContacts =
            new ObservableCollection<Methods.PhoneContactManager.UserContact>();

        private readonly Activity ActivityContext;

        public InviteFriendsAdapter(Activity activity)
        {
            try
            {
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here >> Style_HContact_view
            View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactView, parent, false);

            var vh = new InviteFriendsAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

       public override int ItemCount => MUsersPhoneContacts?.Count ?? 0;  
       
        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is InviteFriendsAdapterViewHolder holder)
                {
                    var item = MUsersPhoneContacts[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, "no_profile_image", holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        string name = Methods.FunString.DecodeString(item.UserDisplayName);
                        holder.Name.Text = name;
                        holder.About.Text = item.PhoneNumber;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public Methods.PhoneContactManager.UserContact GetItem(int position)
        {
            return MUsersPhoneContacts[position];
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

        void OnClick(InviteFriendsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(InviteFriendsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class InviteFriendsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public AppCompatButton Button { get; private set; }
        public CircleImageView ImageLastseen { get; private set; }
        #endregion

        public InviteFriendsAdapterViewHolder(View itemView, Action<InviteFriendsAdapterClickEventArgs> clickListener,
            Action<InviteFriendsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);
                ImageLastseen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);

                //Event
                itemView.Click += (sender, e) => clickListener(new InviteFriendsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new InviteFriendsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                Button.Visibility = ViewStates.Gone;
                ImageLastseen.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class InviteFriendsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}