using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.SharedFiles.Adapter
{
    public class SharedFilesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SharedFilesAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<SharedFilesAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<Classes.SharedFile> SharedFilesList = new ObservableCollection<Classes.SharedFile>();
        private readonly string UserId;
        private readonly string TypeStyle;
        public SharedFilesAdapter(Activity context, string userId, string typeStyle)
        {
            try
            {
                ActivityContext = context;
                UserId = userId;
                TypeStyle = typeStyle;
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
                //Setup your layout here >> Style_SharedFiles_View
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SharedFilesView, parent, false);

                var vh = new SharedFilesAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is SharedFilesAdapterViewHolder holder)
                {
                    var item = SharedFilesList[position];
                    if (item == null) return;
                    switch (item.FileType)
                    {
                        case "Video":
                            {
                                var fileName = item.FilePath.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();

                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.PlayIcon, IonIconsFonts.Play);
                                holder.PlayIcon.Visibility = ViewStates.Visible;

                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Camera);
                                holder.TypeIcon.Visibility = ViewStates.Visible;

                                var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + "/" + UserId, fileNameWithoutExtension + ".png");
                                if (videoPlaceHolderImage == "File Dont Exists")
                                {
                                    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(ActivityContext, item.FilePath);
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDiskVideo + "/" + UserId);

                                    var imageVideo = Methods.Path.FolderDiskVideo + "/" + UserId + "/" + fileNameWithoutExtension + ".png";

                                    File file2 = new File(imageVideo);
                                    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                    Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                                }
                                else
                                {
                                    File file2 = new File(videoPlaceHolderImage);
                                    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                    Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                                }

                                break;
                            }
                        case "Gif":
                            {
                                holder.TypeIcon.Text = ActivityContext.GetText(Resource.String.Lbl_Gif);

                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                holder.TypeIcon.Visibility = ViewStates.Visible;

                                File file2 = new File(item.FilePath);
                                var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                                break;
                            }
                        case "Sticker":
                            {
                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                holder.TypeIcon.Visibility = ViewStates.Gone;

                                File file2 = new File(item.FilePath);
                                var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                                break;
                            }
                        case "Image":
                            {
                                holder.PlayIcon.Visibility = ViewStates.Gone;
                                holder.TypeIcon.Visibility = ViewStates.Gone;

                                File file2 = new File(item.FilePath);
                                var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                                break;
                            }
                        case "Sounds":
                            holder.PlayIcon.Visibility = ViewStates.Gone;
                            holder.TypeIcon.Visibility = ViewStates.Gone;

                            Glide.With(ActivityContext).Load(ActivityContext.GetDrawable(Resource.Drawable.Audio_File)).Apply(new RequestOptions()).Into(holder.Image);
                            break;
                        case "File":
                            holder.PlayIcon.Visibility = ViewStates.Gone;
                            holder.TypeIcon.Visibility = ViewStates.Gone;

                            Glide.With(ActivityContext).Load(ActivityContext.GetDrawable(Resource.Drawable.Image_File)).Apply(new RequestOptions()).Into(holder.Image);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public override int ItemCount => SharedFilesList?.Count ?? 0;


        public Classes.SharedFile GetItem(int position)
        {
            return SharedFilesList[position];
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

        void OnClick(SharedFilesAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SharedFilesAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SharedFilesList[p0];
                if (item == null)
                    return d;

                d.Add(item.ImageExtra);

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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }

    public class SharedFilesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; set; }

        public ImageView Image { get; private set; }
        public TextView PlayIcon { get; private set; }
        public TextView TypeIcon { get; private set; }
        #endregion

        public SharedFilesAdapterViewHolder(View itemView, Action<SharedFilesAdapterViewHolderClickEventArgs> clickListener, Action<SharedFilesAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)MainView.FindViewById(Resource.Id.Image);
                TypeIcon = (TextView)MainView.FindViewById(Resource.Id.typeicon);
                PlayIcon = (TextView)MainView.FindViewById(Resource.Id.playicon);


                //Create an Event
                MainView.Click += (sender, e) => clickListener(new SharedFilesAdapterViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SharedFilesAdapterViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class SharedFilesAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}