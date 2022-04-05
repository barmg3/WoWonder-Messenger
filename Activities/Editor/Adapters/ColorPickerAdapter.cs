using System;
using System.Collections.ObjectModel;
using AmulyaKhare.TextDrawableLib;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Model.Editor;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Utils;

namespace WoWonder.Activities.Editor.Adapters
{
    public class ColorPickerAdapter : RecyclerView.Adapter
    {
        private readonly bool StyleShapeColor;
        private readonly Activity ActivityContext;
        private LayoutInflater Inflater;
        public ObservableCollection<ColorPicker> MColorPickerList = new ObservableCollection<ColorPicker>();

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="colorType">ColorNormal,ColorGradient,ColorNormalAndGradient</param>
        /// <param name="styleShapeColor">true >> Round , false >> RoundRect </param>
        public ColorPickerAdapter(Activity context, ColorType colorType, bool styleShapeColor = true)
        {
            try
            {
                ActivityContext = context;
                StyleShapeColor = styleShapeColor;

                MColorPickerList = GetDefaultColors(context, colorType);
                Inflater = LayoutInflater.From(context);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="colorPickerColors"></param>
        /// <param name="styleShapeColor">true >> Round , false >> RoundRect </param>
        public ColorPickerAdapter(Activity context, ObservableCollection<ColorPicker> colorPickerColors,
            bool styleShapeColor = true)
        {
            try
            {
                ActivityContext = context;
                StyleShapeColor = styleShapeColor;

                Inflater = LayoutInflater.From(context);
                MColorPickerList = colorPickerColors;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => MColorPickerList?.Count ?? 0;

        public event EventHandler<ColorPickerAdapterClickEventArgs> ItemClick;
        public event EventHandler<ColorPickerAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> color_picker_item_list
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ColorPickerView, parent, false);
                var vh = new ColorPickerAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is ColorPickerAdapterViewHolder holder)
                {
                    var item = MColorPickerList[position];
                    if (item != null)
                    {
                        TextDrawable drawable;

                        if (StyleShapeColor)
                        {
                            drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound("", Color.ParseColor(item.ColorFirst));
                            holder.ColorPickerView.SetImageDrawable(drawable);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.ColorSecond))
                            {
                                var width = 150;
                                var height = 150;
                                int[] color = { Color.ParseColor(item.ColorFirst), Color.ParseColor(item.ColorSecond) };

                                var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, width, height, false, true);
                                holder.ColorPickerView.SetImageDrawable(gradient);
                            }
                            else
                            {
                                drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRoundRect("", Color.ParseColor(item.ColorFirst), 10);
                                holder.ColorPickerView.SetImageDrawable(drawable);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<ColorPicker> GetDefaultColors(Activity context, ColorType colorType)
        {
            try
            {
                var colorPickerColors = new ObservableCollection<ColorPicker>();

                if (colorType == ColorType.ColorNormal)
                {
                    colorPickerColors.Add(new ColorPicker { Id = 1, ColorFirst = "#1abc9c", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 2, ColorFirst = "#2ecc71", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 3, ColorFirst = "#3498db", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 4, ColorFirst = "#9b59b6", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 5, ColorFirst = "#34495e", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 6, ColorFirst = "#f1c40f", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 7, ColorFirst = "#e67e22", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 8, ColorFirst = "#e74c3c", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 9, ColorFirst = "#ecf0f1", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 10, ColorFirst = "#95a5a6", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 11, ColorFirst = "#D81B60", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 12, ColorFirst = "#3D5AFE", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 13, ColorFirst = "#795548", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 14, ColorFirst = "#000000", ColorSecond = "" });
                }
                else if (colorType == ColorType.ColorGradient)
                {
                    colorPickerColors.Add(new ColorPicker { Id = 15, ColorFirst = "#ef5350", ColorSecond = "#b71c1c" });
                    colorPickerColors.Add(new ColorPicker { Id = 16, ColorFirst = "#EC407A", ColorSecond = "#880E4F" });
                    colorPickerColors.Add(new ColorPicker { Id = 17, ColorFirst = "#AB47BC", ColorSecond = "#4A148C" });
                    colorPickerColors.Add(new ColorPicker { Id = 18, ColorFirst = "#5C6BC0", ColorSecond = "#1A237E" });
                    colorPickerColors.Add(new ColorPicker { Id = 19, ColorFirst = "#29B6F6", ColorSecond = "#01579B" });
                    colorPickerColors.Add(new ColorPicker { Id = 20, ColorFirst = "#26A69A", ColorSecond = "#004D40" });
                    colorPickerColors.Add(new ColorPicker { Id = 21, ColorFirst = "#9CCC65", ColorSecond = "#33691E" });
                    colorPickerColors.Add(new ColorPicker { Id = 22, ColorFirst = "#FFEE58", ColorSecond = "#F57F17" });
                    colorPickerColors.Add(new ColorPicker { Id = 23, ColorFirst = "#FF7043", ColorSecond = "#BF360C" });
                    colorPickerColors.Add(new ColorPicker { Id = 24, ColorFirst = "#BDBDBD", ColorSecond = "#424242" });
                    colorPickerColors.Add(new ColorPicker { Id = 25, ColorFirst = "#78909C", ColorSecond = "#263238" });
                    colorPickerColors.Add(new ColorPicker { Id = 26, ColorFirst = "#6ec052", ColorSecond = "#28c4f3" });
                    colorPickerColors.Add(new ColorPicker { Id = 27, ColorFirst = "#fcca5b", ColorSecond = "#ed4955" });
                    colorPickerColors.Add(new ColorPicker { Id = 28, ColorFirst = "#3033c6", ColorSecond = "#fb0049" });
                }
                else if (colorType == ColorType.ColorNormalAndGradient)
                {
                    colorPickerColors.Add(new ColorPicker { Id = 1, ColorFirst = "#1abc9c", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 2, ColorFirst = "#2ecc71", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 3, ColorFirst = "#3498db", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 4, ColorFirst = "#9b59b6", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 5, ColorFirst = "#34495e", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 6, ColorFirst = "#f1c40f", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 7, ColorFirst = "#e67e22", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 8, ColorFirst = "#e74c3c", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 9, ColorFirst = "#ecf0f1", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 10, ColorFirst = "#95a5a6", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 11, ColorFirst = "#D81B60", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 12, ColorFirst = "#3D5AFE", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 13, ColorFirst = "#795548", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 14, ColorFirst = "#000000", ColorSecond = "" });
                    colorPickerColors.Add(new ColorPicker { Id = 15, ColorFirst = "#ef5350", ColorSecond = "#b71c1c" });
                    colorPickerColors.Add(new ColorPicker { Id = 16, ColorFirst = "#EC407A", ColorSecond = "#880E4F" });
                    colorPickerColors.Add(new ColorPicker { Id = 17, ColorFirst = "#AB47BC", ColorSecond = "#4A148C" });
                    colorPickerColors.Add(new ColorPicker { Id = 18, ColorFirst = "#5C6BC0", ColorSecond = "#1A237E" });
                    colorPickerColors.Add(new ColorPicker { Id = 19, ColorFirst = "#29B6F6", ColorSecond = "#01579B" });
                    colorPickerColors.Add(new ColorPicker { Id = 20, ColorFirst = "#26A69A", ColorSecond = "#004D40" });
                    colorPickerColors.Add(new ColorPicker { Id = 21, ColorFirst = "#9CCC65", ColorSecond = "#33691E" });
                    colorPickerColors.Add(new ColorPicker { Id = 22, ColorFirst = "#FFEE58", ColorSecond = "#F57F17" });
                    colorPickerColors.Add(new ColorPicker { Id = 23, ColorFirst = "#FF7043", ColorSecond = "#BF360C" });
                    colorPickerColors.Add(new ColorPicker { Id = 24, ColorFirst = "#BDBDBD", ColorSecond = "#424242" });
                    colorPickerColors.Add(new ColorPicker { Id = 25, ColorFirst = "#78909C", ColorSecond = "#263238" });
                    colorPickerColors.Add(new ColorPicker { Id = 26, ColorFirst = "#6ec052", ColorSecond = "#28c4f3" });
                    colorPickerColors.Add(new ColorPicker { Id = 27, ColorFirst = "#fcca5b", ColorSecond = "#ed4955" });
                    colorPickerColors.Add(new ColorPicker { Id = 28, ColorFirst = "#3033c6", ColorSecond = "#fb0049" });
                }

                return colorPickerColors;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public ColorPicker GetItem(int position)
        {
            return MColorPickerList[position];
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

        public void OnClick(ColorPickerAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        public void OnLongClick(ColorPickerAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class ColorPickerAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ColorPickerAdapterViewHolder(View itemView, Action<ColorPickerAdapterClickEventArgs> clickListener,
            Action<ColorPickerAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ColorPickerView = itemView.FindViewById<ImageView>(Resource.Id.color_picker_view);

                itemView.Click += (sender, e) => clickListener(new ColorPickerAdapterClickEventArgs
                { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ColorPickerAdapterClickEventArgs
                { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }
        public ImageView ColorPickerView { get; }

        #endregion
    }

    public class ColorPickerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}