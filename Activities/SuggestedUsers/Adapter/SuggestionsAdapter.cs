using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.SuggestedUsers.Adapter
{ 
    public class SuggestionsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestionsAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestionsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        
        public SuggestionsAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    if (viewHolder is SuggestionsAdapterViewHolder holder)
                    {
                        var users = UserList[position];

                        var data = (string)payloads[0];
                        if (data == "true")
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                            holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                            if (AppSettings.ConnectivitySystem == 1) // Following
                            {
                                holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                                holder.Button.Tag = "true";
                                users.IsFollowing = "1";
                            }
                            else // Request Friend
                            {
                                holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                holder.Button.SetTextColor(Color.ParseColor("#444444"));
                                holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                                holder.Button.Tag = "Request";
                                users.IsFollowing = "2";
                            }
                        }
                        else
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.Button.Text = ActivityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                            holder.Button.Tag = "false";
                            users.IsFollowing = "0";
                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(users, "Delete"); 
                            
                        }
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SuggestionsView, parent, false);
                var vh = new SuggestionsAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is SuggestionsAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        holder.Username.Text = Methods.FunString.SubStringCutOf("@" + item.Username, 15) ;
                        holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item),15) ;

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        switch (item.IsFollowing)
                        {
                            // My Friend
                            case "1":
                                {
                                    holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                                    holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                                    if (AppSettings.ConnectivitySystem == 1) // Following
                                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                                    else // Friend
                                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Friends);
                                    holder.Button.Tag = "true";
                                    break;
                                }
                            // Request
                            case "2":
                                holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                holder.Button.SetTextColor(Color.ParseColor("#444444"));
                                holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                                holder.Button.Tag = "Request";
                                break;
                            //Not Friend
                            case "0":
                                {
                                    holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                    holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    if (AppSettings.ConnectivitySystem == 1) // Following
                                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                                    else // Friend
                                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_AddFriends);
                                    holder.Button.Tag = "false";

                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete"); 
                                    
                                    break;
                                }
                            default:
                                {
                                    holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                                    holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                                    if (AppSettings.ConnectivitySystem == 1) // Following
                                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                                    else // Friend
                                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Friends);

                                    item.IsFollowing = "1";
                                    holder.Button.Tag = "true";
                                    break;
                                }
                        }

                        if (!holder.Button.HasOnClickListeners)
                            holder.Button.Click += (sender, e) => FollowButtonClick(new FollowSuggestionsClickEventArgs { View = viewHolder.ItemView, UserClass = item, Position = position, ButtonFollow = holder.Button });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FollowButtonClick(FollowSuggestionsClickEventArgs e)
        {
            try
            {
                if (e.UserClass != null)
                {
                    if (e.ButtonFollow?.Tag?.ToString() == "false")
                        NotifyItemChanged(e.Position, "true");
                    else
                        NotifyItemChanged(e.Position, "false");

                    if (Methods.CheckConnectivity())
                        RequestsAsync.Global.FollowUserAsync(e.UserClass.UserId).ConfigureAwait(false);
                    else
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                return int.Parse(UserList[position].UserId);
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

        void Click(SuggestionsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(SuggestionsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.Avatar))
                        d.Add(item.Avatar);

                    return d;
                }
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

    public class SuggestionsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic


        public View MainView { get;  set; }

        
        public ImageView Image { get; set; }
        public CircleImageView ImageOnline { get; set; }

        public TextView Name { get; set; }
        public TextView Username { get; set; }
        public AppCompatButton Button { get; set; }

        #endregion

        public SuggestionsAdapterViewHolder(View itemView, Action<SuggestionsAdapterClickEventArgs> clickListener,Action<SuggestionsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.people_profile_sos);
                Name = MainView.FindViewById<TextView>(Resource.Id.people_profile_name);
                Username = MainView.FindViewById<TextView>(Resource.Id.people_profile_username);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.btn_follow_people);
               
                //Event
                itemView.Click += (sender, e) => clickListener(new SuggestionsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SuggestionsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class SuggestionsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class FollowSuggestionsClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public dynamic UserClass { get; set; }
        public AppCompatButton ButtonFollow { get; set; }

    }
}