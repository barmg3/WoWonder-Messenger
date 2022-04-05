using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Story;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Story.Adapter
{
    public class StoryAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<StoryAdapterClickEventArgs> ItemClick;
        public event EventHandler<StoryAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<StoryDataObject> StoryList = new ObservableCollection<StoryDataObject>();

        public StoryAdapter(Activity context)
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

       public override int ItemCount => StoryList?.Count ?? 0;  

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Story_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HStoryView, parent, false);
                var vh = new StoryAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is StoryAdapterViewHolder holder)
                {
                    var item = StoryList[position];
                    if (item != null)
                    {
                        switch (item.Stories?.Count)
                        {
                            case > 0 when item.Stories[0].Thumbnail.Contains("http"):
                                GlideImageLoader.LoadImage(ActivityContext, item.Stories[0]?.Thumbnail, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                break;
                            case > 0:
                                Glide.With(ActivityContext).Load(new File(item.Stories[0].Thumbnail)).Apply(new RequestOptions().CircleCrop().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.Image);
                                break;
                        }

                        if (item.Stories != null) holder.TimeText.Text =Methods.Time.TimeAgo(Convert.ToInt32(item.Stories[0].Posted),false) ;

                        if (item.ProfileIndicator == null)
                            item.ProfileIndicator = AppSettings.MainColor;
                         
                        holder.Circleindicator.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(item.ProfileIndicator)); // Default_Color 
                         
                        holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item), 22);

                        if (!holder.Circleindicator.HasOnClickListeners)
                            holder.Circleindicator.Click += (sender, e) => Click(new StoryAdapterClickEventArgs { View = holder.MainView, Position = position });

                       
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public StoryDataObject GetItem(int position)
        {
            return StoryList[position];
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

        private void Click(StoryAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(StoryAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoryList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.Stories[0].Thumbnail))
                        d.Add(item.Stories[0].Thumbnail);

                    return d;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class StoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        public StoryAdapterViewHolder(View itemView, Action<StoryAdapterClickEventArgs> clickListener, Action<StoryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.userProfileImage);
                Name = MainView.FindViewById<TextView>(Resource.Id.Txt_Username);
                Circleindicator = MainView.FindViewById<CircleImageView>(Resource.Id.profile_indicator);
                TimeText = MainView.FindViewById<TextView>(Resource.Id.Txt_last_time);
                //Event
                itemView.Click += (sender, e) => clickListener(new StoryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StoryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

             
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; set; }
        public TextView Name { get; private set; }
        public CircleImageView Circleindicator { get; private set; }
        public TextView TimeText { get; set; }


        #endregion
    }

    public class StoryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}