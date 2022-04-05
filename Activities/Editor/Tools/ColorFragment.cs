using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.Editor.Adapters;
using WoWonder.Helpers.Model.Editor;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt;

namespace WoWonder.Activities.Editor.Tools
{
    public class ColorFragment : BottomSheetDialogFragment
    {
        //private readonly BottomSheetBehavior.BottomSheetCallback MBottomSheetBehaviorCallback = new MyBottomSheetCallBack();
        private readonly NiceArtEditor NiceArtEditor;

        private ColorPickerAdapter PickerAdapter;
        private readonly EditColorActivity ColorActivity;

        public ColorFragment(NiceArtEditor mNiceArtEditor, EditColorActivity colorActivity)
        {
            // Required empty public constructor
            NiceArtEditor = mNiceArtEditor;
            ColorActivity = colorActivity;
        }

        public override void SetupDialog(Dialog dialog, int style)
        {
            try
            {
                base.SetupDialog(dialog, style);
                var contentView = View.Inflate(Context, Resource.Layout.DialogStickerEmoji, null);
                dialog.SetContentView(contentView);
                //var @params = (CoordinatorLayout.LayoutParams)((View)contentView.Parent).LayoutParameters;
                //var behavior = @params.Behavior;

                //if (behavior != null && behavior.GetType() == typeof(BottomSheetBehavior))
                //    ((BottomSheetBehavior)behavior).SetBottomSheetCallback(MBottomSheetBehaviorCallback);

                var rvEmoji = contentView.FindViewById<RecyclerView>(Resource.Id.rvEmoji);

                var gridLayoutManager = new GridLayoutManager(Activity, 4);
                rvEmoji.SetLayoutManager(gridLayoutManager);
                PickerAdapter = new ColorPickerAdapter(Activity, ColorType.ColorNormal);
                PickerAdapter.ItemClick += PickerAdapterOnItemClick;
                rvEmoji.SetAdapter(PickerAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window.SetLayout(width, height);
                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Select Color Text
        private void PickerAdapterOnItemClick(object sender, ColorPickerAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = PickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        ColorActivity.MColorCode = item.ColorFirst;
                        ColorActivity.MAutoResizeEditText.SetTextColor(Color.ParseColor(item.ColorFirst));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //public class MyBottomSheetCallBack : BottomSheetBehavior.BottomSheetCallback
        //{
        //    public override void OnSlide(View bottomSheet, float slideOffset)
        //    {
        //        try
        //        {
        //            //Sliding
        //            if (slideOffset == StateHidden) Dispose();
        //        }
        //        catch (Exception e)
        //        {
        //            Methods.DisplayReportResultTrack(e);
        //        }
        //    }

        //    public override void OnStateChanged(View bottomSheet, int newState)
        //    {
        //        //State changed
        //    }
        //}
    }
}