using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.GroupChat.Adapter
{ 
    public class MentionAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MentionAdapterClickEventArgs> ItemClick;
        public event EventHandler<MentionAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> MentionList = new ObservableCollection<UserDataObject>();

        public MentionAdapter(Activity context)
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
         
        public override int ItemCount => MentionList?.Count ?? 0;  
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Mention_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MentionView, parent, false);
                var vh = new MentionAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is MentionAdapterViewHolder holder)
                {
                    var item = MentionList[position];
                    if (item != null)
                    { 
                        holder.CheckBox.Checked = item.Selected;
                         
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
   
                        holder.Name.Text = WoWonderTools.GetNameFinal(item);

                        holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                        holder.About.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetAboutFinal(item), 25);
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
            return MentionList[position];
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


        private void Click(MentionAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MentionAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MentionList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

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

    public class MentionAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MentionAdapterViewHolder(View itemView, Action<MentionAdapterClickEventArgs> clickListener,Action<MentionAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                CheckBox = MainView.FindViewById<CheckBox>(Resource.Id.cont);

                //Event
                itemView.Click += (sender, e) => clickListener(new MentionAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MentionAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

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
        public CheckBox CheckBox { get; private set; }

        #endregion
    }

    public class MentionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public UserDataObject Mention { get; set; }
    }
}