﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.GroupChat.Adapter
{
    public class MembersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MembersAdapterClickEventArgs> MoreItemClick;
        public event EventHandler<MembersAdapterClickEventArgs> ItemClick;
        public event EventHandler<MembersAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        private readonly bool ShowBtn;

        public MembersAdapter(Activity activity, bool showBtn)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
                ShowBtn = showBtn;
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
                //Setup your layout here >> Style_HContactMore_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactMore_view, parent, false);
                var vh = new MembersAdapterViewHolder(itemView, MoreClick, Click, LongClick);
                return vh;
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
                switch (viewHolder)
                {
                    case MembersAdapterViewHolder holder:
                        {
                            var item = UserList[position];
                            if (item != null)
                            {
                                Initialize(holder, item);
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

        private void Initialize(MembersAdapterViewHolder holder, UserDataObject users)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, users.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(users), 25);

                switch (users.Verified)
                {
                    case "1":
                        holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);
                        break;
                }

                holder.About.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetAboutFinal(users), 25);

                if (users.Avatar == "addImage")
                {
                    holder.ImageLastSeen.Visibility = ViewStates.Gone;
                }
                else
                {
                    //Online Or offline
                    var online = WoWonderTools.GetStatusOnline(Convert.ToInt32(users.LastseenUnixTime), users.LastseenStatus);
                    holder.ImageLastSeen.SetImageResource(online ? Resource.Drawable.Green_Online : Resource.Drawable.Grey_Offline);
                }

                if (users.UserId == UserDetails.UserId || users.Avatar == "addImage" || !ShowBtn)
                    holder.ButtonMore.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case MembersAdapterViewHolder viewHolder:
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

        private void MoreClick(MembersAdapterClickEventArgs args)
        {
            MoreItemClick?.Invoke(this, args);
        }

        private void Click(MembersAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MembersAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class MembersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MembersAdapterViewHolder(View itemView, Action<MembersAdapterClickEventArgs> moreClickListener, Action<MembersAdapterClickEventArgs> clickListener, Action<MembersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);
                ButtonMore = MainView.FindViewById<TextView>(Resource.Id.more);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, ButtonMore, FontAwesomeIcon.EllipsisH);

                //Event
                ButtonMore.Click += (sender, e) => moreClickListener(new MembersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.Click += (sender, e) => clickListener(new MembersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MembersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
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
        public TextView ButtonMore { get; private set; }
        public CircleImageView ImageLastSeen { get; private set; }

        #endregion
    }

    public class MembersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}