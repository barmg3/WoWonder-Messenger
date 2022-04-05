using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using WoWonder.Helpers.Utils;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.StickersView.Adapters
{
    public class IconAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<IconAdapterClickEventArgs> ItemClick;
        public event EventHandler<IconAdapterClickEventArgs> ItemLongClick;

        public ObservableCollection<string> StickerList = new ObservableCollection<string>();
        private readonly RequestOptions Options;
         
        public IconAdapter(List<string> shopStickers)
        {
            try
            {
                HasStableIds = true;
                StickerList = new ObservableCollection<string>(shopStickers);
                Options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                    .CenterCrop()
                    .SetPriority(Priority.High)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).AutoClone()
                    .Error(Resource.Drawable.ImagePlacholder_circle_grey)
                    .Placeholder(Resource.Drawable.ImagePlacholder_circle_grey));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StickerList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_StickerView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_StickerView, parent, false);
                var vh = new IconAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is IconAdapterViewHolder holder)
                {
                    var item = StickerList[position];
                    if (item == null) return;

                    Glide.With(holder.Image)
                        .Load(item)
                        .Apply(Options)
                        .Error(Resource.Drawable.ImagePlacholder_circle_grey)
                        .Placeholder(Resource.Drawable.ImagePlacholder_circle_grey)
                        .Into(holder.Image);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public string GetItem(int position)
        {
            return StickerList[position];
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

        private void Click(IconAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(IconAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StickerList[p0];
                if (item == null)
                    return d;

                d.Add(item);

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var d = new List<string>();
                return d;
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(Application.Context).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
        }

    }

    public class IconAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }

        #endregion
        public IconAdapterViewHolder(View itemView, Action<IconAdapterClickEventArgs> clickListener, Action<IconAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = itemView.FindViewById<ImageView>(Resource.Id.stickerImage);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new IconAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new IconAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class IconAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}