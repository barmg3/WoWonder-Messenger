using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Com.Airbnb.Lottie; 
using Java.IO;
using Java.Util;
using WoWonder.Activities.DefaultUser;
using WoWonder.Adapters;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.Library.RangeSlider;
using WoWonder.SQLite;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Story;
using Console = System.Console;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;
using Priority = Bumptech.Glide.Priority;
using Task = System.Threading.Tasks.Task;

namespace WoWonder.Activities.ChatWindow.Adapters
{
    public class MessageAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, StTools.IXAutoLinkOnClickListener
    {
        public event EventHandler<Holders.MesClickEventArgs> DownloadItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ErrorLoadingItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ItemLongClick;

        public ObservableCollection<AdapterModelsClassMessage> DifferList = new ObservableCollection<AdapterModelsClassMessage>();

        public readonly Activity MainActivity;
        private readonly RequestOptions Options, OptionsRoundedCrop;
        private readonly RequestBuilder FullGlideRequestBuilder;
        public readonly string Id; // to_id 
        private readonly bool ShowName;

        public int PositionSound;
        public bool MSeekBarIsTracking;
        public ValueAnimator MValueAnimator;
        public MessageDataExtra MusicBarMessageData;

        public MessageAdapter(Activity activity, string userid, bool showName)
        {
            try
            {
                HasStableIds = true;
                MainActivity = activity;
                Id = userid;
                ShowName = showName;
                DifferList = new ObservableCollection<AdapterModelsClassMessage>();

                Glide.Get(MainActivity).SetMemoryCategory(MemoryCategory.Low);

                Options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                    .CenterCrop()
                    .SetPriority(Priority.High).Override(400)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder)
                    .Placeholder(Resource.Drawable.ImagePlacholder));

                FullGlideRequestBuilder = Glide.With(MainActivity).AsDrawable().Apply(RequestOptions.CircleCropTransform().CenterCrop().Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(25))).SetPriority(Priority.High).Override(450).SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).Placeholder(new ColorDrawable(Color.ParseColor("#888888"))));

                OptionsRoundedCrop = GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
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
                switch (viewType)
                {
                    case (int)MessageModelType.RightProduct:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Products, parent, false);
                            Holders.ProductViewHolder viewHolder = new Holders.ProductViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        }
                    case (int)MessageModelType.LeftProduct:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Products, parent, false);
                            Holders.ProductViewHolder viewHolder = new Holders.ProductViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        }
                    case (int)MessageModelType.RightText:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_view, parent, false);
                            Holders.TextViewHolder textViewHolder = new Holders.TextViewHolder(row, OnClick, OnLongClick, ShowName);
                            return textViewHolder;
                        }
                    case (int)MessageModelType.LeftText:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                            Holders.TextViewHolder textViewHolder = new Holders.TextViewHolder(row, OnClick, OnLongClick, ShowName);
                            return textViewHolder;
                        }
                    case (int)MessageModelType.RightImage:
                    case (int)MessageModelType.RightGif:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnDownloadClick, OnErrorLoadingClick, OnLongClick, ShowName, Holders.TypeClick.Image);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.LeftImage:
                    case (int)MessageModelType.LeftGif:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnDownloadClick, OnErrorLoadingClick, OnLongClick, ShowName, Holders.TypeClick.Image);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.RightMap:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnDownloadClick, OnErrorLoadingClick, OnLongClick, ShowName, Holders.TypeClick.Map);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.LeftMap:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnDownloadClick, OnErrorLoadingClick, OnLongClick, ShowName, Holders.TypeClick.Map);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.RightAudio:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Audio, parent, false);
                            Holders.SoundViewHolder soundViewHolder = new Holders.SoundViewHolder(row, OnClick, OnLongClick, this, ShowName);
                            return soundViewHolder;
                        }
                    case (int)MessageModelType.LeftAudio:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Audio, parent, false);
                            Holders.SoundViewHolder soundViewHolder = new Holders.SoundViewHolder(row, OnClick, OnLongClick, this, ShowName);
                            return soundViewHolder;
                        }
                    case (int)MessageModelType.RightContact:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Contact, parent, false);
                            Holders.ContactViewHolder contactViewHolder = new Holders.ContactViewHolder(row, OnClick, OnLongClick, this, ShowName);
                            return contactViewHolder;
                        }
                    case (int)MessageModelType.LeftContact:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Contact, parent, false);
                            Holders.ContactViewHolder contactViewHolder = new Holders.ContactViewHolder(row, OnClick, OnLongClick, this, ShowName);
                            return contactViewHolder;
                        }
                    case (int)MessageModelType.RightVideo:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Video, parent, false);
                            Holders.VideoViewHolder videoViewHolder = new Holders.VideoViewHolder(row, OnClick, OnDownloadClick, OnErrorLoadingClick, OnLongClick, ShowName);
                            return videoViewHolder;
                        }
                    case (int)MessageModelType.LeftVideo:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Video, parent, false);
                            Holders.VideoViewHolder videoViewHolder = new Holders.VideoViewHolder(row, OnClick, OnDownloadClick, OnErrorLoadingClick, OnLongClick, ShowName);
                            return videoViewHolder;
                        }
                    case (int)MessageModelType.RightSticker:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_sticker, parent, false);
                            Holders.StickerViewHolder stickerViewHolder = new Holders.StickerViewHolder(row, OnClick, OnLongClick, ShowName);
                            return stickerViewHolder;
                        }
                    case (int)MessageModelType.LeftSticker:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_sticker, parent, false);
                            Holders.StickerViewHolder stickerViewHolder = new Holders.StickerViewHolder(row, OnClick, OnLongClick, ShowName);
                            return stickerViewHolder;
                        }
                    case (int)MessageModelType.RightFile:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_file, parent, false);
                            Holders.FileViewHolder viewHolder = new Holders.FileViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        }
                    case (int)MessageModelType.LeftFile:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_file, parent, false);
                            Holders.FileViewHolder viewHolder = new Holders.FileViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        }
                    default:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                            Holders.NotSupportedViewHolder viewHolder = new Holders.NotSupportedViewHolder(row);
                            return viewHolder;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            try
            {
                var item = DifferList[position];

                var itemViewType = vh.ItemViewType;
                switch (itemViewType)
                {
                    case (int)MessageModelType.RightProduct:
                    case (int)MessageModelType.LeftProduct:
                        {
                            Holders.ProductViewHolder holder = vh as Holders.ProductViewHolder;
                            LoadProductOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightGif:
                    case (int)MessageModelType.LeftGif:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadGifOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightText:
                    case (int)MessageModelType.LeftText:
                        {
                            Holders.TextViewHolder holder = vh as Holders.TextViewHolder;
                            LoadTextOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightImage:
                    case (int)MessageModelType.LeftImage:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadImageOfChatItem(holder, position, item.MesData, false);
                            break;
                        }
                    case (int)MessageModelType.RightMap:
                    case (int)MessageModelType.LeftMap:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadMapOfChatItem(holder, position, item.MesData, false);
                            break;
                        }
                    case (int)MessageModelType.RightAudio:
                    case (int)MessageModelType.LeftAudio:
                        {
                            Holders.SoundViewHolder holder = vh as Holders.SoundViewHolder;
                            LoadAudioOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightContact:
                    case (int)MessageModelType.LeftContact:
                        {
                            Holders.ContactViewHolder holder = vh as Holders.ContactViewHolder;
                            LoadContactOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightVideo:
                    case (int)MessageModelType.LeftVideo:
                        {
                            Holders.VideoViewHolder holder = vh as Holders.VideoViewHolder;
                            LoadVideoOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightSticker:
                    case (int)MessageModelType.LeftSticker:
                        {
                            Holders.StickerViewHolder holder = vh as Holders.StickerViewHolder;
                            LoadStickerOfChatItem(holder, position, item.MesData, false);
                            break;
                        }
                    case (int)MessageModelType.RightFile:
                    case (int)MessageModelType.LeftFile:
                        {
                            Holders.FileViewHolder holder = vh as Holders.FileViewHolder;
                            LoadFileOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrEmpty(item.MesData.Text) || !string.IsNullOrWhiteSpace(item.MesData.Text))
                            {
                                if (vh is Holders.TextViewHolder holderText)
                                {
                                    LoadTextOfChatItem(holderText, position, item.MesData);
                                }
                            }
                            else
                            {
                                if (vh is Holders.NotSupportedViewHolder holder)
                                    holder.AutoLinkNotsupportedView.Text = MainActivity.GetText(Resource.String.Lbl_TextChatNotSupported);
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    var item = DifferList[position];
                    switch (payloads[0].ToString())
                    {
                        case "WithoutBlobImage":
                        case "WithoutBlobGIF":
                            {
                                if (viewHolder is Holders.ImageViewHolder holder)
                                    LoadImageOfChatItem(holder, position, item.MesData, true);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobSticker":
                            {
                                if (viewHolder is Holders.StickerViewHolder holder)
                                    LoadStickerOfChatItem(holder, position, item.MesData, true);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobMap":
                            {
                                if (viewHolder is Holders.ImageViewHolder holder)
                                    LoadMapOfChatItem(holder, position, item.MesData, true);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobVideo":
                            {
                                if (viewHolder is Holders.VideoViewHolder holder)
                                    LoadVideoOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobAudio":
                            {
                                if (viewHolder is Holders.SoundViewHolder holder)
                                    LoadAudioOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobFile":
                            {
                                if (viewHolder is Holders.FileViewHolder holder)
                                    LoadFileOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobUploadProgress":
                            {
                                if (viewHolder is Holders.ImageViewHolder imageViewHolder)
                                {
                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                        imageViewHolder.LoadingProgressview.SetProgress(item.MesData.MessageProgress, true);
                                    else // For API < 24 
                                        imageViewHolder.LoadingProgressview.Progress = item.MesData.MessageProgress;
                                }
                                else if (viewHolder is Holders.VideoViewHolder videoViewHolder)
                                {
                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                        videoViewHolder.LoadingProgressview.SetProgress(item.MesData.MessageProgress, true);
                                    else // For API < 24 
                                        videoViewHolder.LoadingProgressview.Progress = item.MesData.MessageProgress;
                                }
                                else if (viewHolder is Holders.SoundViewHolder soundViewHolder)
                                {
                                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                        soundViewHolder.LoadingProgressView.SetProgress(item.MesData.MessageProgress, true);
                                    else // For API < 24 
                                        soundViewHolder.LoadingProgressView.Progress = item.MesData.MessageProgress;
                                }
                                //NotifyItemChanged(position);
                                break;
                            }
                        default:
                            base.OnBindViewHolder(viewHolder, position, payloads);
                            break;
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

        #region Function Load Message

        //Reply Story Messages
        private void ReplyStoryItems(Holders.RepliedMessageView holder, StoryDataObject.Story story)
        {
            try
            {
                if (!string.IsNullOrEmpty(story?.Id))
                {
                    holder.RepliedMessageLayout.Visibility = ViewStates.Visible;
                    holder.TxtOwnerName.Text = MainActivity.GetText(Resource.String.Lbl_Story);

                    holder.MessageFileThumbnail.Visibility = ViewStates.Visible;

                    var mediaFile = !story.Thumbnail.Contains("avatar") && story.Videos.Count == 0 ? story.Thumbnail : story.Videos[0].Filename;
                    var fileName = mediaFile.Split('/').Last();

                    var typeView = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                    switch (typeView)
                    {
                        case "Video":
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.video);

                                var fileNameWithoutExtension = fileName.Split('.').First();

                                var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + Id, fileNameWithoutExtension + ".png");
                                if (videoImage == "File Dont Exists")
                                {
                                    File file2 = new File(mediaFile);
                                    try
                                    {
                                        Uri photoUri = mediaFile.Contains("http") ? Uri.Parse(mediaFile) : FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file2);
                                        Glide.With(MainActivity)
                                            .AsBitmap()
                                            .Apply(OptionsRoundedCrop)
                                            .Load(photoUri) // or URI/path
                                            .Into(holder.MessageFileThumbnail);  //image view to set thumbnail to 
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                        Glide.With(MainActivity)
                                            .AsBitmap()
                                            .Apply(OptionsRoundedCrop)
                                            .Load(file2) // or URI/path
                                            .Into(holder.MessageFileThumbnail);  //image view to set thumbnail to 
                                    }
                                }
                                else
                                {
                                    File file = new File(videoImage);
                                    try
                                    {
                                        Uri photoUri = FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file);
                                        FullGlideRequestBuilder.Load(photoUri).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                        FullGlideRequestBuilder.Load(file).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                    }
                                }
                                break;
                            }

                        case "Image":
                            {
                                holder.TxtMessageType.Visibility = ViewStates.Gone;
                                holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.image);

                                //mediaFile = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, fileName, mediaFile);

                                if (mediaFile.Contains("http"))
                                {
                                    GlideImageLoader.LoadImage(MainActivity, mediaFile, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                }
                                else
                                {
                                    var file = Uri.FromFile(new File(mediaFile));
                                    FullGlideRequestBuilder.Load(file.Path).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Reply Messages
        private void ReplyItems(Holders.RepliedMessageView holder, MessageModelType typeView, MessageData message, int position)
        {
            try
            {
                if (!string.IsNullOrEmpty(message?.Id))
                {
                    holder.RepliedMessageLayout.Visibility = ViewStates.Visible;
                    holder.TxtOwnerName.Text = message.MessageUser?.User?.UserId == UserDetails.UserId ? MainActivity.GetText(Resource.String.Lbl_You) : message.MessageUser?.User?.Name;

                    if (typeView is MessageModelType.LeftText or MessageModelType.RightText)
                    {
                        holder.MessageFileThumbnail.Visibility = ViewStates.Gone;
                        holder.TxtMessageType.Visibility = ViewStates.Gone;
                        holder.TxtShortMessage.Text = message.Text;
                    }
                    else
                    {
                        holder.MessageFileThumbnail.Visibility = ViewStates.Visible;
                        var fileName = message.Media.Split('/').Last();
                        switch (typeView)
                        {
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                {
                                    holder.TxtMessageType.Visibility = ViewStates.Gone;
                                    holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.video);

                                    if (!string.IsNullOrEmpty(message.ImageVideo))
                                    {
                                        FullGlideRequestBuilder.Load(message.ImageVideo).Into(holder.MessageFileThumbnail);
                                    }
                                    else
                                    {
                                        var fileNameWithoutExtension = fileName.Split('.').First();
                                        var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + Id, fileNameWithoutExtension + ".png");
                                        if (videoImage == "File Dont Exists")
                                        {
                                            File file2 = new File(message.Media);

                                            try
                                            {
                                                Uri photoUri = message.Media.Contains("http") ? Uri.Parse(message.Media) : FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file2);
                                                Glide.With(MainActivity)
                                                    .AsBitmap()
                                                    .Apply(OptionsRoundedCrop)
                                                    .Load(photoUri) // or URI/path
                                                    .Into(new MySimpleTarget(this, holder.MessageFileThumbnail, position));  //image view to set thumbnail to 
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                                Glide.With(MainActivity)
                                                    .AsBitmap()
                                                    .Apply(OptionsRoundedCrop)
                                                    .Load(file2) // or URI/path
                                                    .Into(new MySimpleTarget(this, holder.MessageFileThumbnail, position));  //image view to set thumbnail to 
                                            }
                                        }
                                        else
                                        {
                                            File file = new File(videoImage);
                                            try
                                            {
                                                Uri photoUri = FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file);
                                                FullGlideRequestBuilder.Load(photoUri).Into(holder.MessageFileThumbnail);
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                                FullGlideRequestBuilder.Load(file).Into(holder.MessageFileThumbnail);
                                            }
                                        }
                                    }

                                    break;
                                }
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                {
                                    holder.TxtMessageType.Visibility = ViewStates.Gone;
                                    holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.Lbl_Gif);

                                    if (message.Media.Contains("http"))
                                        message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskGif, fileName, message.Media, "image");

                                    if (message.Media.Contains("http"))
                                    {
                                        FullGlideRequestBuilder.Load(message.Media).Into(holder.MessageFileThumbnail);
                                        //GlideImageLoader.LoadImage(MainActivity, message.Media, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(message.Media));
                                        FullGlideRequestBuilder.Load(file.Path).Into(holder.MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                {
                                    holder.TxtMessageType.Visibility = ViewStates.Gone;
                                    holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.Lbl_Sticker);

                                    if (message.Media.Contains("http"))
                                        message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskSticker, fileName, message.Media, "sticker");

                                    if (message.Media.Contains("http"))
                                    {
                                        Glide.With(MainActivity)
                                            .Load(message.Media)
                                            .Apply(Options)
                                            .Into(holder.MessageFileThumbnail);

                                        FullGlideRequestBuilder.Load(message.Media).Into(holder.MessageFileThumbnail);
                                        //GlideImageLoader.LoadImage(MainActivity, message.Media, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(message.Media));

                                        Glide.With(MainActivity)
                                            .Load(file.Path)
                                            .Apply(Options)
                                            .Into(holder.MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                {
                                    holder.TxtMessageType.Visibility = ViewStates.Gone;
                                    holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.image);

                                    if (message.Media.Contains("http"))
                                        message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, fileName, message.Media, "image");

                                    if (message.Media.Contains("http"))
                                    {
                                        //GlideImageLoader.LoadImage(MainActivity, message.Media, holder.MessageFileThumbnail, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                        FullGlideRequestBuilder.Load(message.Media).Into(holder.MessageFileThumbnail);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(message.Media));
                                        FullGlideRequestBuilder.Load(file.Path).Into(holder.MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                {
                                    holder.TxtMessageType.Visibility = ViewStates.Gone;
                                    holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.Lbl_VoiceMessage) + " (" + message.MediaDuration + ")";
                                    FullGlideRequestBuilder.Load(MainActivity.GetDrawable(Resource.Drawable.Audio_File)).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                {
                                    //holder.TxtShortMessage.Visibility = ViewStates.Gone;
                                    holder.TxtMessageType.Text = MainActivity.GetText(Resource.String.Lbl_File);

                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var fileNameExtension = fileName.Split('.').Last();

                                    holder.TxtShortMessage.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                                    FullGlideRequestBuilder.Load(MainActivity.GetDrawable(Resource.Drawable.Image_File)).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                {
                                    holder.TxtShortMessage.Visibility = ViewStates.Gone;
                                    holder.TxtMessageType.Text = MainActivity.GetText(Resource.String.Lbl_Location);
                                    FullGlideRequestBuilder.Load(message.MessageMap).Apply(new RequestOptions().Placeholder(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map)).Into(holder.MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                {
                                    holder.TxtMessageType.Text = MainActivity.GetText(Resource.String.Lbl_Contact);
                                    holder.TxtShortMessage.Text = message.ContactName;
                                    FullGlideRequestBuilder.Load(Resource.Drawable.no_profile_image).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                {
                                    holder.TxtMessageType.Visibility = ViewStates.Gone;
                                    holder.TxtShortMessage.Text = MainActivity.GetText(Resource.String.Lbl_Product);
                                    string imageUrl = !string.IsNullOrEmpty(message.Media) ? message.Media : message.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;
                                    //Glide.With(MainActivity).Load(imageUrl).Apply(OptionsRoundedCrop).Into(holder.MessageFileThumbnail);
                                    FullGlideRequestBuilder.Load(imageUrl).Into(holder.MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Reaction Messages
        private void ReactionItems(Holders.ReactionMessageView holder, MessageDataExtra message)
        {
            try
            {
                if (message.Reaction != null)
                {
                    if (message.Reaction.IsReacted != null && message.Reaction.IsReacted.Value)
                    {
                        if (!string.IsNullOrEmpty(message.Reaction.Type))
                        {
                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == message.Reaction.Type).Value?.Id ?? "";
                            switch (react)
                            {
                                case "1":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                    break;
                                case "2":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                    break;
                                case "3":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                    break;
                                case "4":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                    break;
                                case "5":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                    break;
                                case "6":
                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                    break;
                                default:
                                    if (message.Reaction.Count > 0)
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                    break;
                            }
                            holder.CountLikeSection.Visibility = ViewStates.Visible;
                        }
                    }
                    else
                    {
                        if (message.Reaction.Count > 0)
                        {
                            if (message.Reaction.Like != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                            }
                            else if (message.Reaction.Love != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                            }
                            else if (message.Reaction.HaHa != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                            }
                            else if (message.Reaction.Wow != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                            }
                            else if (message.Reaction.Sad != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                            }
                            else if (message.Reaction.Angry != null)
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                            }
                            else
                            {
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                            }
                            holder.CountLikeSection.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            holder.CountLikeSection.Visibility = ViewStates.Gone;
                        }
                    }
                }
                else
                {
                    holder.CountLikeSection.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetStartedMessage(LottieAnimationView favAnimationView, ImageView starImage, string star)
        {
            try
            {
                if (favAnimationView != null)
                {
                    if (star == "yes")
                    {
                        favAnimationView.PlayAnimation();
                        favAnimationView.Visibility = ViewStates.Visible;
                        starImage.SetImageResource(Resource.Drawable.icon_star_filled_post_vector);
                        starImage.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        favAnimationView.Progress = 0;
                        favAnimationView.CancelAnimation();
                        favAnimationView.Visibility = ViewStates.Gone;
                        //starImage.SetImageResource(Resource.Drawable.icon_fav_post_vector);
                        starImage.Visibility = ViewStates.Invisible;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetForwardedMessage(LinearLayout forwardLayout, bool forward)
        {
            try
            {
                if (forwardLayout != null)
                {
                    forwardLayout.Visibility = forward ? ViewStates.Visible : ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetSeenMessage(TextView view, string seen)
        {
            try
            {
                if (view != null)
                {
                    if (seen == "-1")
                    {
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, view, FontAwesomeIcon.Clock);
                        view.SetTextColor(Color.ParseColor("#efefef"));
                    }
                    else if (seen == "0")
                    {
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, view, FontAwesomeIcon.Check);
                        view.SetTextColor(Color.ParseColor("#efefef"));
                    }
                    else if (seen != "-1" && seen != "0" && !string.IsNullOrEmpty(seen))
                    {
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, view, FontAwesomeIcon.CheckDouble);
                        view.SetTextColor(Color.LightBlue);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void LoadTextOfChatItem(Holders.TextViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                Console.WriteLine(position);
                holder.Time.Text = message.TimeText;

                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            await Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass?.Id != null && !string.IsNullOrEmpty(message.StoryId) && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                    var px = PixelUtil.DpToPx(MainActivity, 150);
                    holder.BubbleLayout.SetMinimumWidth(px);
                }
                else
                {
                    if (message.Reply?.ReplyClass?.Id != null && !string.IsNullOrEmpty(message.ReplyId) && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                        var px = PixelUtil.DpToPx(MainActivity, 150);
                        holder.BubbleLayout.SetMinimumWidth(px);
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                        holder.BubbleLayout.SetMinimumWidth(0);
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                holder.Time.Visibility = message.ShowTimeText ? ViewStates.Visible : ViewStates.Gone;

                if (message.ModelType == MessageModelType.LeftText)
                {
                    holder.SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModePhone_color)));
                    holder.SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeEmail_color)));
                    holder.SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeHashtag_color)));
                    holder.SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeUrl_color)));
                    holder.SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeMention_color)));
                    holder.SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeUrl_color)));
                    holder.SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.accent)));
                }
                else
                {
                    holder.SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModePhone_color)));
                    holder.SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeEmail_color)));
                    holder.SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeHashtag_color)));
                    holder.SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeUrl_color)));
                    holder.SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeMention_color)));
                    holder.SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeUrl_color)));
                    holder.SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(MainActivity, Resource.Color.accent)));
                }

                holder.SuperTextView.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());

                string laststring = message.Text.Replace(" /", " ");
                if (!string.IsNullOrEmpty(laststring))
                    holder.SuperTextView.SetText(laststring, TextView.BufferType.Spannable);

                if (AppSettings.EnableFitchOgLink)
                {
                    if (message.FitchOgLink?.Count > 0)
                    {
                        holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Visible;

                        var url = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "url").Value ?? "";
                        var title = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "title").Value ?? "";
                        var description = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "description").Value ?? "";
                        var image = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "image").Value ?? "";

                        if (!string.IsNullOrEmpty(url))
                        {
                            var prepareUrl = url.Replace("https://", "").Replace("http://", "").Split('/').FirstOrDefault();
                            holder.OgLinkMessageView.OgLinkUrl.Text = prepareUrl?.ToUpper();
                            holder.OgLinkMessageView.OgLinkTitle.Text = title;
                            holder.OgLinkMessageView.OgLinkDescription.Text = description;

                            GlideImageLoader.LoadImage(MainActivity, image, holder.OgLinkMessageView.OgLinkImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                        }
                        if (message.Position == "right")
                        {
                            holder.OgLinkMessageView.OgLinkTitle.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.OgLinkMessageView.OgLinkUrl.SetTextColor(Color.ParseColor("#efefef"));
                            holder.OgLinkMessageView.OgLinkDescription.SetTextColor(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        //Check if find website in text 
                        foreach (Match match in Regex.Matches(holder.SuperTextView.Text, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                        {
                            Console.WriteLine(match.Value);
                            message.FitchOgLink = await Methods.OgLink.FitchOgLink(match.Value);
                            break;
                        }

                        if (message.FitchOgLink?.Count > 0)
                        {
                            holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Visible;

                            var url = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "url").Value ?? "";
                            var title = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "title").Value ?? "";
                            var description = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "description").Value ?? "";
                            var image = message?.FitchOgLink?.FirstOrDefault(a => a.Key == "image").Value ?? "";

                            if (!string.IsNullOrEmpty(url))
                            {
                                var prepareUrl = url.Replace("https://", "").Replace("http://", "").Split('/').FirstOrDefault();
                                holder.OgLinkMessageView.OgLinkUrl.Text = prepareUrl?.ToUpper();
                                holder.OgLinkMessageView.OgLinkTitle.Text = title;
                                holder.OgLinkMessageView.OgLinkDescription.Text = description;

                                GlideImageLoader.LoadImage(MainActivity, image, holder.OgLinkMessageView.OgLinkImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                            }

                            if (message.Position == "right")
                            {
                                holder.OgLinkMessageView.OgLinkTitle.SetTextColor(Color.ParseColor("#ffffff"));
                                holder.OgLinkMessageView.OgLinkUrl.SetTextColor(Color.ParseColor("#efefef"));
                                holder.OgLinkMessageView.OgLinkDescription.SetTextColor(Color.ParseColor("#efefef"));
                            }
                        }
                        else
                        {
                            holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Gone;
                        }
                    }
                }
                else
                {
                    if (holder.OgLinkMessageView.OgLinkContainerLayout != null)
                        holder.OgLinkMessageView.OgLinkContainerLayout.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
                if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
                {
                    Methods.App.SendEmail(MainActivity, matchedText.Replace(" ", "").Replace("\n", ""));
                }
                else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
                {
                    string url = matchedText.Replace(" ", "").Replace("\n", "");
                    if (!matchedText.Contains("http"))
                    {
                        url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
                    }

                    //var intent = new Intent(Activity, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url);
                    //intent.PutExtra("Type", url);
                    //Activity.StartActivity(intent);
                    new IntentController(MainActivity).OpenBrowserFromApp(url);
                }
                else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
                {

                }
                else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
                {
                    var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                    string name = matchedText.Replace("@", "").Replace(" ", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);

                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(MainActivity, user.UserId, user);
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            var intent = new Intent(MainActivity, typeof(MyProfileActivity));
                            MainActivity.StartActivity(intent);
                        }
                        else
                        {
                            var intent = new Intent(MainActivity, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            MainActivity.StartActivity(intent);
                        }
                    }
                }
                else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
                {
                    Methods.App.SaveContacts(MainActivity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void LoadMapOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra message, bool update)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            await Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                holder.Time.Text = message.TimeText;

                if (!update)
                {
                    LatLng latLng = new LatLng(Convert.ToDouble(message.Lat), Convert.ToDouble(message.Lng));

                    var addresses = await WoWonderTools.ReverseGeocodeCurrentLocation(latLng).ConfigureAwait(false);
                    if (addresses != null)
                    {
                        MainActivity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var deviceAddress = addresses.GetAddressLine(0);

                                string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                                //imageUrlMap += "center=" + item.CurrentLatitude + "," + item.CurrentLongitude;
                                imageUrlMap += "center=" + deviceAddress;
                                imageUrlMap += "&zoom=13";
                                imageUrlMap += "&scale=2";
                                imageUrlMap += "&size=150x150";
                                imageUrlMap += "&maptype=roadmap";
                                imageUrlMap += "&key=" + MainActivity.GetText(Resource.String.google_maps_key);
                                imageUrlMap += "&format=png";
                                imageUrlMap += "&visual_refresh=true";
                                imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + deviceAddress;

                                FullGlideRequestBuilder.Load(imageUrlMap).Into(holder.ImageView);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }

                }

                //holder.LoadingProgressview.Indeterminate = false;
                holder.LoadingProgressview.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadImageOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra message, bool update)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                Console.WriteLine(position);
                var fileName = message.Media.Split('/').Last();

                if (message.Media.Contains("http"))
                    message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, fileName, message.Media, "image");

                holder.Time.Text = message.TimeText;

                if (!update)
                {
                    if (message.Media.Contains("http"))
                    {
                        message.BtnDownload = WoWonderTools.CheckAllowedDownloadMedia("image");
                        if (message.BtnDownload)
                        {
                            GlideImageLoader.LoadImage(MainActivity, message.Media, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                        }
                        else
                        {
                            GlideImageLoader.LoadImage(MainActivity, message.Media, holder.ImageView, ImageStyle.Blur, ImagePlaceholders.Drawable);
                        }
                    }
                    else
                    {
                        message.BtnDownload = true;

                        var file = Uri.FromFile(new File(message.Media));
                        // GlideImageLoader.LoadImage(MainActivity, file.ToString(), holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                        FullGlideRequestBuilder.Load(file.Path).Apply(OptionsRoundedCrop).Into(holder.ImageView);
                    }
                }

                if (message.SendFile)
                {
                    // holder.LoadingProgressview.Indeterminate = true;
                    holder.LoadingProgressview.Visibility = ViewStates.Visible;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }
                else
                {
                    //holder.LoadingProgressview.Indeterminate = false;
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }

                if (message.ErrorSendMessage)
                {
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;

                    if (!message.BtnDownload)
                    {
                        holder.LoadingProgressview.Visibility = ViewStates.Gone;
                        holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                        holder.TxtDownload.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        holder.TxtDownload.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadProductOfChatItem(Holders.ProductViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                Console.WriteLine(position);
                string imageUrl = !string.IsNullOrEmpty(message.Media) ? message.Media : message.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;
                holder.Time.Text = message.TimeText;

                holder.Title.Text = message.Product?.ProductClass?.Name;
                holder.Cat.Text = ListUtils.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == message.Product?.ProductClass?.Category)?.CategoriesName;

                var (currency, currencyIcon) = WoWonderTools.GetCurrency(message.Product?.ProductClass?.Currency);
                holder.Price.Text = currencyIcon + " " + message.Product?.ProductClass?.Price;
                Console.WriteLine(currency);

                if (imageUrl != null && (imageUrl.Contains("http://") || imageUrl.Contains("https://")))
                {
                    GlideImageLoader.LoadImage(MainActivity, imageUrl, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                }
                else
                {
                    var file = Uri.FromFile(new File(imageUrl));
                    FullGlideRequestBuilder.Load(file.Path).Apply(OptionsRoundedCrop).Into(holder.ImageView);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadAudioOfChatItem(Holders.SoundViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                Console.WriteLine(position);

                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                if (message.SendFile)
                {
                    //holder.LoadingProgressView.Indeterminate = true;
                    holder.LoadingProgressView.IndeterminateDrawable?.SetColorFilter(new PorterDuffColorFilter(Color.White, PorterDuff.Mode.Multiply));
                    holder.LoadingProgressView.Visibility = ViewStates.Visible;
                    holder.PlayButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    // holder.LoadingProgressView.Indeterminate = false;
                    holder.LoadingProgressView.Visibility = ViewStates.Gone;
                    holder.PlayButton.Visibility = ViewStates.Visible;
                }

                holder.MsgTimeTextView.Text = message.TimeText;

                var fileName = message.Media.Split('/').Last();

                if (message.Media.Contains("http"))
                    message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimSound, fileName, message.Media, "audio");

                if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                {
                    var duration = WoWonderTools.GetDuration(message.Media);
                    holder.DurationTextView.Text = message.MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                }
                else
                    holder.DurationTextView.Text = message.MediaDuration;

                if (message.MediaIsPlaying)
                {
                    holder.PlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                    if (message.ModelType == MessageModelType.LeftAudio)
                        holder.PlayButton.ImageTintList = ColorStateList.ValueOf(AppSettings.SetTabDarkTheme ? Color.ParseColor("#efefef") : Color.ParseColor("#444444"));
                    else
                        holder.PlayButton.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                }
                else
                {
                    holder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadContactOfChatItem(Holders.ContactViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                Console.WriteLine(position);
                holder.MsgTimeTextView.Text = message.TimeText;
                holder.MsgTimeTextView.Visibility = message.ShowTimeText ? ViewStates.Visible : ViewStates.Gone;

                if (!string.IsNullOrEmpty(message.ContactName))
                {
                    holder.UserContactNameTextView.Text = message.ContactName;
                    holder.UserNumberTextView.Text = message.ContactNumber;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadVideoOfChatItem(Holders.VideoViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.SetBackgroundColor(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                var fileName = message.Media.Split('/').Last();
                var fileNameWithoutExtension = fileName.Split('.').First();

                if (message.Media.Contains("http"))
                    message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimVideo, fileName, message.Media, "video");

                holder.MsgTimeTextView.Text = message.TimeText;
                holder.FilenameTextView.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 12) + ".mp4";

                if (message.Media.Contains("http"))
                {
                    message.BtnDownload = WoWonderTools.CheckAllowedDownloadMedia("video");
                    if (!message.BtnDownload)
                    {
                        GlideImageLoader.LoadImage(MainActivity, message.Media, holder.ImageView, ImageStyle.Blur, ImagePlaceholders.Drawable);
                    }
                }
                else
                {
                    message.BtnDownload = true;
                    if (!string.IsNullOrEmpty(message.ImageVideo))
                    {
                        FullGlideRequestBuilder.Load(message.ImageVideo).Apply(OptionsRoundedCrop).Into(holder.ImageView);
                    }
                    else
                    {
                        var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + Id, fileNameWithoutExtension + ".png");
                        if (videoImage == "File Dont Exists")
                        {
                            File file2 = new File(message.Media);
                            try
                            {
                                Uri photoUri = message.Media.Contains("http") ? Uri.Parse(message.Media) : FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file2);
                                Glide.With(MainActivity)
                                    .AsBitmap()
                                    .Apply(OptionsRoundedCrop)
                                    .Load(photoUri) // or URI/path
                                    .Into(new MySimpleTarget(this, holder.ImageView, position));  //image view to set thumbnail to 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                                Glide.With(MainActivity)
                                    .AsBitmap()
                                    .Apply(OptionsRoundedCrop)
                                    .Load(file2) // or URI/path
                                    .Into(new MySimpleTarget(this, holder.ImageView, position));  //image view to set thumbnail to 
                            }
                        }
                        else
                        {
                            File file = new File(videoImage);
                            try
                            {
                                Uri photoUri = FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file);
                                FullGlideRequestBuilder.Load(photoUri).Apply(OptionsRoundedCrop).Into(holder.ImageView);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                                FullGlideRequestBuilder.Load(file).Apply(OptionsRoundedCrop).Into(holder.ImageView);
                            }
                        }
                    }
                }

                if (message.SendFile)
                {
                    // holder.LoadingProgressview.Indeterminate = true;
                    holder.LoadingProgressview.Visibility = ViewStates.Visible;
                    holder.PlayButton.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }
                else
                {
                    //holder.LoadingProgressview.Indeterminate = false;
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.PlayButton.Visibility = ViewStates.Visible;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }

                if (message.ErrorSendMessage)
                {
                    holder.PlayButton.Visibility = ViewStates.Gone;
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;

                    if (!message.BtnDownload)
                    {
                        holder.PlayButton.Visibility = ViewStates.Gone;
                        holder.LoadingProgressview.Visibility = ViewStates.Gone;
                        holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                        holder.TxtDownload.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        holder.TxtDownload.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MySimpleTarget : CustomTarget
        {
            private readonly MessageAdapter MAdapter;
            private readonly ImageView Image;
            private readonly int Position;
            public MySimpleTarget(MessageAdapter adapter, ImageView view, int position)
            {
                try
                {
                    MAdapter = adapter;
                    Image = view;
                    Position = position;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                try
                {
                    if (MAdapter.DifferList?.Count > 0)
                    {
                        var item = MAdapter.DifferList[Position].MesData;
                        if (item != null)
                        {
                            var fileName = item.Media.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();

                            var pathImage = Methods.Path.FolderDiskVideo + MAdapter.Id + "/" + fileNameWithoutExtension + ".png";

                            var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + MAdapter.Id, fileNameWithoutExtension + ".png");
                            if (videoImage == "File Dont Exists")
                            {
                                if (resource is Bitmap bitmap)
                                {
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmap, fileNameWithoutExtension, Methods.Path.FolderDiskVideo + MAdapter.Id + "/");

                                    File file2 = new File(pathImage);
                                    var photoUri = FileProvider.GetUriForFile(MAdapter.MainActivity, MAdapter.MainActivity.PackageName + ".fileprovider", file2);

                                    Glide.With(MAdapter.MainActivity).Load(photoUri).Apply(MAdapter.OptionsRoundedCrop).Into(Image);

                                    item.ImageVideo = photoUri.ToString();
                                }
                            }
                            else
                            {
                                File file2 = new File(pathImage);
                                var photoUri = FileProvider.GetUriForFile(MAdapter.MainActivity, MAdapter.MainActivity.PackageName + ".fileprovider", file2);

                                Glide.With(MAdapter.MainActivity).Load(photoUri).Apply(MAdapter.OptionsRoundedCrop).Into(Image);

                                item.ImageVideo = photoUri.ToString();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnLoadCleared(Drawable p0) { }
        }

        private void LoadStickerOfChatItem(Holders.StickerViewHolder holder, int position, MessageDataExtra message, bool update)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                Console.WriteLine(position);
                //var fileName = message.Media.Split('/').Last();
                //message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskSticker, fileName, message.Media);

                holder.Time.Text = message.TimeText;
                if (!update)
                {
                    if (message.Media.Contains("http"))
                    {
                        FullGlideRequestBuilder.Load(message.Media).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.ImageView);
                    }
                    else
                    {
                        var file = Uri.FromFile(new File(message.Media));
                        FullGlideRequestBuilder.Load(file.Path).Apply(Options).Into(holder.ImageView);
                    }
                }

                //holder.LoadingProgressview.Indeterminate = false;
                holder.LoadingProgressview.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFileOfChatItem(Holders.FileViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                Console.WriteLine(position);
                var fileName = message.Media.Split('/').Last();
                var fileNameWithoutExtension = fileName.Split('.').First();
                var fileNameExtension = fileName.Split('.').Last();

                if (message.Media.Contains("http"))
                    message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimFile, fileName, message.Media, "file");

                holder.MsgTimeTextView.Text = message.TimeText;
                holder.FileNameTextView.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                holder.SizeFileTextView.Text = message.FileSize;

                if (fileNameExtension.Contains("rar") || fileNameExtension.Contains("RAR") || fileNameExtension.Contains("zip") || fileNameExtension.Contains("ZIP"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf1c6"); //ZipBox
                }
                else if (fileNameExtension.Contains("txt") || fileNameExtension.Contains("TXT"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf15c"); //NoteText
                }
                else if (fileNameExtension.Contains("docx") || fileNameExtension.Contains("DOCX") || fileNameExtension.Contains("doc") || fileNameExtension.Contains("DOC"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf1c2"); //FileWord
                }
                else if (fileNameExtension.Contains("pdf") || fileNameExtension.Contains("PDF"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf1c1"); //FilePdf
                }
                else if (fileNameExtension.Contains("apk") || fileNameExtension.Contains("APK"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf17b"); //Fileandroid
                }
                else
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf15b"); //file
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadGifOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                    {
                        //holder.BubbleLayout.SetBackgroundResource(AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient ? Resource.Drawable.chat_rounded_right_gradient_layout : Resource.Drawable.chat_rounded_right_layout);

                        if (AppSettings.ColorMessageTheme == ColorMessageTheme.Gradient)
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Drawable drawableByTheme = WoWonderTools.GetShapeDrawableByThemeColor(MainActivity, message.ChatColor);
                                    if (drawableByTheme != null)
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.Background = drawableByTheme; });
                                    else
                                        MainActivity?.RunOnUiThread(() => { holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor)); });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                    }
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.Fav);

                SetForwardedMessage(holder.ForwardLayout, message.Forward != null && message.Forward.Value == 1);

                if (message.Story?.StoryClass != null && message.StoryId != "0")
                {
                    ReplyStoryItems(holder.RepliedMessageView, message.Story?.StoryClass);

                    if (message.Position == "right")
                    {
                        holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                        holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                    }
                }
                else
                {
                    if (message.Reply?.ReplyClass != null && message.ReplyId != "0")
                    {
                        ReplyItems(holder.RepliedMessageView, Holders.GetTypeModel(message.Reply?.ReplyClass), message.Reply?.ReplyClass, position);

                        if (message.Position == "right")
                        {
                            holder.RepliedMessageView.TxtOwnerName.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtShortMessage.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.TxtMessageType.SetTextColor(Color.ParseColor("#efefef"));
                            holder.RepliedMessageView.ColorView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#efefef"));
                        }
                    }
                    else
                    {
                        holder.RepliedMessageView.RepliedMessageLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (AppSettings.EnableReactionMessageSystem)
                    ReactionItems(holder.ReactionMessageView, message);
                else
                    holder.ReactionMessageView.CountLikeSection.Visibility = ViewStates.Gone;

                Console.WriteLine(position);
                // G_fixed_height_small_url, // UrlGif - view  >>  mediaFileName
                // G_fixed_height_small_mp4, //MediaGif - sent >>  media

                string imageUrl = "";
                if (!string.IsNullOrEmpty(message.Stickers))
                    imageUrl = message.Stickers;
                else if (!string.IsNullOrEmpty(message.Media))
                    imageUrl = message.Media;
                else if (!string.IsNullOrEmpty(message.MediaFileName))
                    imageUrl = message.MediaFileName;

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    string[] fileName = imageUrl.Split(new[] { "/", "200.gif?cid=", "&rid=200" }, StringSplitOptions.RemoveEmptyEntries);
                    var lastFileName = fileName.Last();
                    var name = fileName[3] + lastFileName;

                    if (message.Media.Contains("http"))
                        message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskGif, name, imageUrl, "image");
                }

                if (message.Media != null && message.Media.Contains("http"))
                {
                    message.BtnDownload = WoWonderTools.CheckAllowedDownloadMedia("image");
                    if (message.BtnDownload)
                    {
                        GlideImageLoader.LoadImage(MainActivity, imageUrl, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                    }
                    else
                    {
                        GlideImageLoader.LoadImage(MainActivity, imageUrl, holder.ImageView, ImageStyle.Blur, ImagePlaceholders.Drawable);
                    }
                }
                else
                {
                    message.BtnDownload = true;

                    var file = Uri.FromFile(new File(message.Media));
                    Glide.With(MainActivity).Load(file.Path).Thumbnail(0.5f).Apply(OptionsRoundedCrop).Into(holder.ImageView);
                }


                if (message.SendFile)
                {
                    // holder.LoadingProgressview.Indeterminate = true;
                    holder.LoadingProgressview.Visibility = ViewStates.Visible;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }
                else
                {
                    //holder.LoadingProgressview.Indeterminate = false;
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                }

                if (message.ErrorSendMessage)
                {
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.TxtErrorLoading.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.TxtErrorLoading.Visibility = ViewStates.Gone;

                    if (!message.BtnDownload)
                    {
                        holder.LoadingProgressview.Visibility = ViewStates.Gone;
                        holder.TxtErrorLoading.Visibility = ViewStates.Gone;
                        holder.TxtDownload.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        holder.TxtDownload.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override int ItemCount => DifferList?.Count ?? 0;

        public AdapterModelsClassMessage GetItem(int position)
        {
            var item = DifferList[position];

            return item;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = DifferList[position];
                if (item == null)
                    return (int)MessageModelType.None;

                switch (item.TypeView)
                {
                    case MessageModelType.RightProduct:
                        return (int)MessageModelType.RightProduct;
                    case MessageModelType.LeftProduct:
                        return (int)MessageModelType.LeftProduct;
                    case MessageModelType.RightGif:
                        return (int)MessageModelType.RightGif;
                    case MessageModelType.LeftGif:
                        return (int)MessageModelType.LeftGif;
                    case MessageModelType.RightText:
                        return (int)MessageModelType.RightText;
                    case MessageModelType.LeftText:
                        return (int)MessageModelType.LeftText;
                    case MessageModelType.RightImage:
                        return (int)MessageModelType.RightImage;
                    case MessageModelType.LeftImage:
                        return (int)MessageModelType.LeftImage;
                    case MessageModelType.RightAudio:
                        return (int)MessageModelType.RightAudio;
                    case MessageModelType.LeftAudio:
                        return (int)MessageModelType.LeftAudio;
                    case MessageModelType.RightContact:
                        return (int)MessageModelType.RightContact;
                    case MessageModelType.LeftContact:
                        return (int)MessageModelType.LeftContact;
                    case MessageModelType.RightVideo:
                        return (int)MessageModelType.RightVideo;
                    case MessageModelType.LeftVideo:
                        return (int)MessageModelType.LeftVideo;
                    case MessageModelType.RightSticker:
                        return (int)MessageModelType.RightSticker;
                    case MessageModelType.LeftSticker:
                        return (int)MessageModelType.LeftSticker;
                    case MessageModelType.RightFile:
                        return (int)MessageModelType.RightFile;
                    case MessageModelType.LeftFile:
                        return (int)MessageModelType.LeftFile;
                    case MessageModelType.RightMap:
                        return (int)MessageModelType.RightMap;
                    case MessageModelType.LeftMap:
                        return (int)MessageModelType.LeftMap;
                    default:
                        return (int)MessageModelType.None;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (int)MessageModelType.None;
            }
        }

        void OnDownloadClick(Holders.MesClickEventArgs args) => DownloadItemClick?.Invoke(this, args);
        void OnErrorLoadingClick(Holders.MesClickEventArgs args) => ErrorLoadingItemClick?.Invoke(this, args);
        void OnClick(Holders.MesClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(Holders.MesClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = DifferList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                string imageUrl = "";

                switch (item.TypeView)
                {
                    case MessageModelType.LeftProduct:
                    case MessageModelType.RightProduct:
                        imageUrl = !string.IsNullOrEmpty(item.MesData.Media) ? item.MesData.Media : item.MesData.Product?.ProductClass?.Images?.FirstOrDefault()?.Image;
                        break;
                    case MessageModelType.RightGif:
                    case MessageModelType.LeftGif:
                        if (!string.IsNullOrEmpty(item.MesData.Stickers))
                            imageUrl = item.MesData.Stickers;
                        else if (!string.IsNullOrEmpty(item.MesData.Media))
                            imageUrl = item.MesData.Media;
                        else if (!string.IsNullOrEmpty(item.MesData.MediaFileName))
                            imageUrl = item.MesData.MediaFileName;

                        string[] fileName = imageUrl.Split(new[] { "/", "200.gif?cid=", "&rid=200" }, StringSplitOptions.RemoveEmptyEntries);
                        var lastFileName = fileName.Last();
                        var name = fileName[3] + lastFileName;
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDiskGif, name, imageUrl, "image") : item.MesData.Media;
                        break;
                    case MessageModelType.RightImage:
                    case MessageModelType.LeftImage:
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, item.MesData.Media.Split('/').Last(), item.MesData.Media, "image") : item.MesData.Media;
                        break;
                    case MessageModelType.RightVideo:
                    case MessageModelType.LeftVideo:
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDcimVideo, item.MesData.Media.Split('/').Last(), imageUrl, "video") : item.MesData.Media;
                        break;
                    case MessageModelType.RightSticker:
                    case MessageModelType.LeftSticker:
                        imageUrl = imageUrl.Contains("http") ? WoWonderTools.GetFile(Id, Methods.Path.FolderDiskSticker, item.MesData.Media.Split('/').Last(), item.MesData.Media, "sticker") : item.MesData.Media;
                        break;
                    case MessageModelType.RightMap:
                    case MessageModelType.LeftMap:
                        if (!string.IsNullOrEmpty(item.MesData.MessageMap) && item.MesData.MessageMap.Contains("https://maps.googleapis.com/maps/api/staticmap?"))
                            imageUrl = item.MesData.MessageMap;
                        break;
                }

                if (!string.IsNullOrEmpty(imageUrl))
                    d.Add(imageUrl);

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
            return GlideImageLoader.GetPreLoadRequestBuilder(MainActivity, p0.ToString(), ImageStyle.CenterCrop);
        }

    }
}