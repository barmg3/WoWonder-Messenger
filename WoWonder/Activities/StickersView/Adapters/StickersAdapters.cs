using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using WoWonder.Activities.ChatWindow.Adapters;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.StickersView.Adapters
{
    public class StickersAdapters : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider 
    {
        public event EventHandler<StickersAdaptersViewHolderClickEventArgs> ItemClick;
        public event EventHandler<StickersAdaptersViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<StickersModel> StickersList = new ObservableCollection<StickersModel>();
        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }
        private IconAdapter IconAdapter;
        private EmptySuggestionMessagesAdapter TagsAdapter;
        private readonly string TypePage;
        private readonly RequestOptions Options;

        public StickersAdapters(Activity context, string typePage)
        {
            try
            {
                HasStableIds = true;
                TypePage = typePage;
                ActivityContext = context;
                RecycledViewPool = new RecyclerView.RecycledViewPool();
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

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int)StickersModel.ItemTypeShop.MySticker:
                    {
                        //Setup your layout here >> Style_MyStickersView
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MyStickersView, parent, false);
                        var vh = new MyStickerAdaptersViewHolder(itemView, OnClick, OnLongClick);
                        return vh;
                    }
                    case (int)StickersModel.ItemTypeShop.ShopSticker:
                    {
                        //Setup your layout here >> Style_ShopStickersView
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ShopStickersView, parent, false);
                        var vh = new ShopStickersAdaptersViewHolder(itemView, OnClick, OnLongClick);
                        return vh;
                    } 
                    case (int)StickersModel.ItemTypeShop.IconSticker:
                    {
                        //Setup your layout here >> Style_StickerView
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_StickerView, parent, false);
                        var vh = new IconStickerAdapterViewHolder(itemView, OnClick, OnLongClick);
                        return vh;
                    }
                    case (int)StickersModel.ItemTypeShop.RecommendedTag:
                    case (int)StickersModel.ItemTypeShop.TrendingSticker:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                        var vh = new TemplateRecyclerViewHolder(itemView, OnClick, OnLongClick);
                        RecycledViewPool = new RecyclerView.RecycledViewPool();
                        vh.MRecycler.SetRecycledViewPool(RecycledViewPool);
                        return vh;
                    }
                    default:
                        return null!;
                }
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
                var item = StickersList[position];
                if (item != null)
                {
                    switch (item.ItemType)
                    { 
                        case StickersModel.ItemTypeShop.TrendingSticker:
                        {
                            if (viewHolder is TemplateRecyclerViewHolder holder)
                            {
                                if (IconAdapter == null)
                                {
                                    IconAdapter = new IconAdapter(item.ListSticker);
                                     
                                    LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                    holder.MRecycler.SetLayoutManager(layoutManager);
                                    holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                    holder.MRecycler.NestedScrollingEnabled = false;

                                    var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                    var preLoader = new RecyclerViewPreloader<string>(ActivityContext, IconAdapter, sizeProvider, 10);
                                    holder.MRecycler.AddOnScrollListener(preLoader);
                                    holder.MRecycler.SetAdapter(IconAdapter);
                                    IconAdapter.ItemClick += IconAdapterOnItemClick;

                                    holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Trending);
                                    holder.MoreText.Visibility = ViewStates.Invisible;
                                } 
                            }
                            break;
                        }
                        case StickersModel.ItemTypeShop.RecommendedTag:
                        {
                            if (viewHolder is TemplateRecyclerViewHolder holder)
                            {
                                if (TagsAdapter == null)
                                {
                                    TagsAdapter = new EmptySuggestionMessagesAdapter(ActivityContext)
                                    {
                                        SuggestionMessagesList = new JavaList<EmptySuggestionMessagesAdapter.SuggestionMessages>()
                                    };
                                     
                                    LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                    holder.MRecycler.SetLayoutManager(layoutManager);
                                    holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                    holder.MRecycler.NestedScrollingEnabled = false;
                                    holder.MRecycler.SetAdapter(TagsAdapter);
                                    TagsAdapter.OnItemClick += TagsAdapterOnOnItemClick;

                                    holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_RecommendedTag);
                                    holder.MoreText.Visibility = ViewStates.Invisible;
                                }

                                if (item.ListTags.Count > 0)
                                {
                                    TagsAdapter.SuggestionMessagesList = new JavaList<EmptySuggestionMessagesAdapter.SuggestionMessages>();

                                    foreach (var tag in item.ListTags)
                                    {
                                        TagsAdapter.SuggestionMessagesList.Add(new EmptySuggestionMessagesAdapter.SuggestionMessages()
                                        {
                                            Message = tag,
                                            RealMessage = tag,
                                        });
                                    }
                                    
                                    TagsAdapter.NotifyDataSetChanged();
                                }
                            }
                            break;
                        }
                        case StickersModel.ItemTypeShop.ShopSticker:
                            {
                                if (viewHolder is ShopStickersAdaptersViewHolder holder)
                                {
                                    var url = item.ListSticker.FirstOrDefault();
                                    Glide.With(ActivityContext)
                                        .Load(url)
                                        .Apply(Options)
                                        .Error(Resource.Drawable.ImagePlacholder_circle_grey)
                                        .Placeholder(Resource.Drawable.ImagePlacholder_circle_grey)
                                        .Into(holder.StickerImage);

                                    var color = Methods.FunString.RandomColor();
                                    holder.MainLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(color.Item2)); 
                                    holder.InfoLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(color.Item1)); 

                                    holder.Title.Text = item.Name; 
                                }
                                break;
                            } 
                        case StickersModel.ItemTypeShop.MySticker:
                            {
                                if (viewHolder is MyStickerAdaptersViewHolder holder)
                                {
                                    holder.Title.Text = item.Name;

                                    holder.SubTitle.Text = item.Count + " " + Application.Context.GetText(Resource.String.Lbl_SendStickerFile);

                                    if (item.Visibility)
                                    {
                                        holder.Add.Text = Application.Context.GetText(Resource.String.Lbl_Remove);
                                        holder.Add.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#888888"));
                                    }
                                    else
                                    {
                                        holder.Add.Text = Application.Context.GetText(Resource.String.Lbl_Add);
                                        holder.Add.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                    }

                                    if (item.ListSticker.Count > 0)
                                    {
                                        holder.Stickers.SetAdapter(new IconAdapter(item.ListSticker));
                                    }
                                }
                                break;
                            }
                        case StickersModel.ItemTypeShop.IconSticker:
                            {
                                if (viewHolder is IconStickerAdapterViewHolder holder)
                                {
                                    var url = item.ListSticker.FirstOrDefault();
                                    Glide.With(ActivityContext)
                                        .Load(url)
                                        .Apply(Options)
                                        .Error(Resource.Drawable.ImagePlacholder_circle_grey)
                                        .Placeholder(Resource.Drawable.ImagePlacholder_circle_grey)
                                        .Into(holder.Image);
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TagsAdapterOnOnItemClick(object sender, AdapterClickEvents e)
        {
            try
            {
                var item = TagsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    BrowseStickersFragment.GetInstance()?.StartSearch(item.Message);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        private void IconAdapterOnItemClick(object sender, IconAdapterClickEventArgs e)
        {
            try
            {
                var item = IconAdapter.GetItem(e.Position);
                if (item != null)
                {
                    new StickerItemClickListener(TypePage).StickerAdapterOnOnItemClick(item);
                    BrowseStickersFragment.GetInstance().Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => StickersList?.Count ?? 0;
         
        public StickersModel GetItem(int position)
        {
            return StickersList[position];
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
                var item = StickersList[position];
                if (item != null)
                {
                    return item.ItemType switch
                    {
                        StickersModel.ItemTypeShop.ShopSticker => (int)StickersModel.ItemTypeShop.ShopSticker,
                        StickersModel.ItemTypeShop.MySticker => (int)StickersModel.ItemTypeShop.MySticker,
                        StickersModel.ItemTypeShop.TrendingSticker => (int)StickersModel.ItemTypeShop.TrendingSticker,
                        StickersModel.ItemTypeShop.RecommendedTag => (int)StickersModel.ItemTypeShop.RecommendedTag,
                        StickersModel.ItemTypeShop.IconSticker => (int)StickersModel.ItemTypeShop.IconSticker,
                        _ => (int)StickersModel.ItemTypeShop.ShopSticker,
                    };
                }

                return (int)StickersModel.ItemTypeShop.ShopSticker;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)StickersModel.ItemTypeShop.ShopSticker;
            }
        }

        void OnClick(StickersAdaptersViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(StickersAdaptersViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StickersList[p0];
              
                if (item?.ListSticker?.Count > 0)
                    d.AddRange(item.ListSticker);
                 
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

    public class MyStickerAdaptersViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; private set; }

        public TextView Title { get; private set; }
        public TextView SubTitle { get; private set; }
        public AppCompatButton Add { get; private set; }
        public RecyclerView Stickers { get; private set; }

        #endregion

        public MyStickerAdaptersViewHolder(View itemView, Action<StickersAdaptersViewHolderClickEventArgs> clickListener, Action<StickersAdaptersViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Title = itemView.FindViewById<TextView>(Resource.Id.title);
                SubTitle = itemView.FindViewById<TextView>(Resource.Id.subtitle);
                Add = itemView.FindViewById<AppCompatButton>(Resource.Id.add);
                Stickers = itemView.FindViewById<RecyclerView>(Resource.Id.rv);
                Stickers?.SetLayoutManager(new GridLayoutManager(itemView.Context, 4));

                //Create an Event
                Add.Click += (sender, e) => clickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
     
    public class ShopStickersAdaptersViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; private set; }

        public LinearLayout MainLayout { get; private set; }
        public LinearLayout InfoLayout { get; private set; }
        public TextView Title { get; private set; }
        public ImageView IconView { get; private set; }
        public ImageView StickerImage { get; private set; }

        #endregion

        public ShopStickersAdaptersViewHolder(View itemView, Action<StickersAdaptersViewHolderClickEventArgs> clickListener, Action<StickersAdaptersViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainLayout = itemView.FindViewById<LinearLayout>(Resource.Id.MainLayout);
                InfoLayout = itemView.FindViewById<LinearLayout>(Resource.Id.InfoLayout);
                Title = itemView.FindViewById<TextView>(Resource.Id.name);
                IconView = itemView.FindViewById<ImageView>(Resource.Id.iconView);
                StickerImage = itemView.FindViewById<ImageView>(Resource.Id.stickerImage);
                 
                //Create an Event
                itemView.Click += (sender, e) => clickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
     
    public class IconStickerAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; private set; }

        public ImageView Image { get; private set; }

        #endregion

        public IconStickerAdapterViewHolder(View itemView, Action<StickersAdaptersViewHolderClickEventArgs> clickListener, Action<StickersAdaptersViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = itemView.FindViewById<ImageView>(Resource.Id.stickerImage);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
     
    public class TemplateRecyclerViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public TextView TitleText { get; private set; }
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public TemplateRecyclerViewHolder(View itemView, Action<StickersAdaptersViewHolderClickEventArgs> clickListener, Action<StickersAdaptersViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainView = itemView;
                MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                TitleText = MainView.FindViewById<TextView>(Resource.Id.headText);
                MoreText = MainView.FindViewById<TextView>(Resource.Id.moreText);

                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StickersAdaptersViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
     
    public class StickersAdaptersViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}