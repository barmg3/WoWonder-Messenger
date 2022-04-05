using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Editor.Adapters
{
    public class FontTypeFaceAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;
        public LayoutInflater Inflater;
        public ObservableCollection<Typeface> MFontTypeFacesList = new ObservableCollection<Typeface>();


        public FontTypeFaceAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                GetFontTypeFace();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public FontTypeFaceAdapter(Activity context, ObservableCollection<Typeface> fontTypefaces)
        {
            try
            {
                ActivityContext = context;
                MFontTypeFacesList = fontTypefaces;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => MFontTypeFacesList?.Count ?? 0;

        public event EventHandler<FontTypeFaceAdapterClickEventArgs> ItemClick;
        public event EventHandler<FontTypeFaceAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager) 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> row_FontTypeFace
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_FontTypeFaceView, parent, false);
                var vh = new FontTypeFaceAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is FontTypeFaceAdapterViewHolder holder)
                {
                    var item = MFontTypeFacesList[position];
                    if (item != null)
                    {
                        holder.TxtFontTypeFace.Text = ActivityContext.GetText(Resource.String.text);
                        holder.TxtFontTypeFace.SetTypeface(item, TypefaceStyle.Normal);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public Typeface GetItem(int position)
        {
            return MFontTypeFacesList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public void OnClick(FontTypeFaceAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        public void OnLongClick(FontTypeFaceAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public void GetFontTypeFace()
        {
            try
            {
                var fontTxt0 = Typeface.CreateFromAsset(ActivityContext.Assets, "beyond_wonderland.ttf");
                var fontTxt1 = Typeface.CreateFromAsset(ActivityContext.Assets, "Bryndan-Write.ttf");
                var fontTxt2 = Typeface.CreateFromAsset(ActivityContext.Assets, "Norican-Regular.ttf");
                var fontTxt3 = Typeface.CreateFromAsset(ActivityContext.Assets, "BoutrosMBCDinkum-Medium.ttf");
                var fontTxt4 = Typeface.CreateFromAsset(ActivityContext.Assets, "Oswald-Heavy.ttf");
                var fontTxt5 = Typeface.CreateFromAsset(ActivityContext.Assets, "Roboto-Medium.ttf");
                var fontTxt6 = Typeface.CreateFromAsset(ActivityContext.Assets, "RobotoMono-Regular.ttf");
                var fontTxt8 = Typeface.CreateFromAsset(ActivityContext.Assets, "Hacen Sudan.ttf");
                var fontTxt9 = Typeface.CreateFromAsset(ActivityContext.Assets, "Harmattan-Regular.ttf");

                MFontTypeFacesList.Add(fontTxt0);
                MFontTypeFacesList.Add(fontTxt1);
                MFontTypeFacesList.Add(fontTxt2);
                MFontTypeFacesList.Add(fontTxt3);
                MFontTypeFacesList.Add(fontTxt4);
                MFontTypeFacesList.Add(fontTxt5);
                MFontTypeFacesList.Add(fontTxt6);
                MFontTypeFacesList.Add(fontTxt8);
                MFontTypeFacesList.Add(fontTxt9);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class FontTypeFaceAdapterViewHolder : RecyclerView.ViewHolder
    {
        public FontTypeFaceAdapterViewHolder(View itemView, Action<FontTypeFaceAdapterClickEventArgs> clickListener,
            Action<FontTypeFaceAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                TxtFontTypeFace = itemView.FindViewById<TextView>(Resource.Id.txt_Font);

                itemView.Click += (sender, e) => clickListener(new FontTypeFaceAdapterClickEventArgs
                { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new FontTypeFaceAdapterClickEventArgs
                { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public TextView TxtFontTypeFace { get; private set; }
        public View MainView { get; }
    }

    public class FontTypeFaceAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}