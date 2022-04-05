using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.DefaultUser.Adapters
{
    public class BlockedUsersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<BlockedUsersAdapterClickEventArgs> UnBlockButtonItemClick;

        public event EventHandler<BlockedUsersAdapterClickEventArgs> ItemClick;
        public event EventHandler<BlockedUsersAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        
        public BlockedUsersAdapter(Activity activity)
        {
            try
            {
                //HasStableIds = true;
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_BlockedUsersView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_BlockedUsersView, parent, false);
                var vh = new BlockedUsersAdapterViewHolder(itemView, UnBlockButtonClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case BlockedUsersAdapterViewHolder holder:
                    {
                        var item = UserList[position];
                        if (item != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable, true);

                            holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item), 20);

                            switch (item.Verified)
                            {
                                case "1":
                                    holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);
                                    break;
                            }

                            holder.About.Text = ActivityContext.GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(item.LastseenUnixTime), true);

                            //Online Or offline
                            //var online = WoWonderTools.GetStatusOnline(Convert.ToInt32(users.LastseenUnixTime), users.LastseenStatus);
                            //holder.ImageLastSeen.SetImageResource(online ? Resource.Drawable.Green_Online : Resource.Drawable.Grey_Offline); 
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
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

        private void UnBlockButtonClick(BlockedUsersAdapterClickEventArgs args)
        {
            UnBlockButtonItemClick?.Invoke(this, args);
        }

        private void Click(BlockedUsersAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(BlockedUsersAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case BlockedUsersAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString()).Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }

    }

    public class BlockedUsersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public BlockedUsersAdapterViewHolder(View itemView, Action<BlockedUsersAdapterClickEventArgs> unBlockButtonClickListener, Action<BlockedUsersAdapterClickEventArgs> clickListener,
            Action<BlockedUsersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                UnblockButton = MainView.FindViewById<AppCompatButton>(Resource.Id.BtnUnblock);
                About.Visibility = ViewStates.Visible;

                //Event
                UnblockButton.Click += (sender, e) => unBlockButtonClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                //itemView.Click += (sender, e) => clickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition }); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public AppCompatButton UnblockButton { get; private set; } 

        #endregion
    }

    public class BlockedUsersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    } 
}