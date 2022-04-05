using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;

namespace WoWonder.NiceArt
{
    public class NiceArtEditorView : RelativeLayout, INiceArt.IOnSaveBitmap, INiceArt.IOnImageChangedListener
    {
        private FilterImageView MImgSource;
        private BrushDrawingView MBrushDrawingView;
        private ImageFilterView MImageFilterView;
        public static readonly int ImgSrcId = 1, BrushSrcId = 2, GlFilterId = 3;
        private INiceArt.IOnSaveBitmap MOnSaveBitmap;

        protected NiceArtEditorView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public NiceArtEditorView(Context context) : base(context)
        {
            Init(context, null);
        }

        public NiceArtEditorView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public NiceArtEditorView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        public NiceArtEditorView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs);
        }

        public void Init(Context context, IAttributeSet attrs)
        {
            try
            {
                //Setup image attributes
                MImgSource = new FilterImageView(context) {Id = ImgSrcId};
                MImgSource.SetAdjustViewBounds(true);
                LayoutParams imgSrcParam = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                imgSrcParam.AddRule(LayoutRules.CenterInParent, ImgSrcId);
                if (attrs != null)
                {
                    //TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.NiceArtEditorView);
                    //Drawable imgSrcDrawable = a.GetDrawable(Resource.Styleable.NiceArtEditorView_photo_src);
                    //if (imgSrcDrawable != null)
                    //{
                    //    MImgSource.SetImageDrawable(imgSrcDrawable);
                    //}
                }

                //Setup brush view
                MBrushDrawingView = new BrushDrawingView(context) {Visibility = ViewStates.Gone, Id = BrushSrcId};

                //Align brush to the size of image view
                LayoutParams brushParam = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                brushParam.AddRule(LayoutRules.CenterInParent, ImgSrcId);
                brushParam.AddRule(LayoutRules.AlignTop, ImgSrcId);
                brushParam.AddRule(LayoutRules.AlignBottom, ImgSrcId);

                //Setup GLSurface attributes
                MImageFilterView = new ImageFilterView(context)
                {
                    Id = GlFilterId,
                    Visibility = ViewStates.Gone
                };

                //Align brush to the size of image view
                LayoutParams imgFilterParam = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                imgFilterParam.AddRule(LayoutRules.CenterInParent, ImgSrcId);
                imgFilterParam.AddRule(LayoutRules.AlignTop, ImgSrcId);
                imgFilterParam.AddRule(LayoutRules.AlignBottom, ImgSrcId);

                OnBitmapLoaded(MImgSource.GetBitmap());

                //Add image source
                AddView(MImgSource, imgSrcParam);

                //Add Gl FilterView
                AddView(MImageFilterView, imgFilterParam);

                //Add brush view
                AddView(MBrushDrawingView, brushParam);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public async void OnBitmapLoaded(Bitmap sourceBitmap)
        {
            try
            {
                await MImageFilterView.SetFilterEffect(PhotoFilter.None);
                MImageFilterView.SetSourceBitmap(sourceBitmap);
                Console.WriteLine("NiceArtEditorView", "onBitmapLoaded() called with: sourceBitmap = [" + sourceBitmap + "]");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Source image which you want to edit 
        /// </summary>
        /// <returns>source ImageView</returns>
        public ImageView GetSource()
        {
            try
            {
                return MImgSource;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;

            }
        }

        public BrushDrawingView GetBrushDrawingView()
        {
            try
            {
                return MBrushDrawingView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;

            }
        }

        public async void SaveFilter(INiceArt.IOnSaveBitmap onSaveBitmap)
        {
            try
            {
                MOnSaveBitmap = onSaveBitmap;
                if (MImageFilterView.Visibility == ViewStates.Visible)
                    await MImageFilterView.SaveBitmap(this);
                else
                {
                    onSaveBitmap.OnBitmapReady(MImgSource.GetBitmap(), SaveType.Normal);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnBitmapReady(Bitmap saveBitmap, SaveType type)
        {
            try
            {
                Console.WriteLine("NiceArtEditorView", "saveFilter: " + saveBitmap);
                MImgSource.SetImageBitmap(saveBitmap);
                MImageFilterView.Visibility = ViewStates.Gone;
                MOnSaveBitmap.OnBitmapReady(saveBitmap, type);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnFailure(string e)
        {
            try
            {
                OnFailure(e);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public async void SetFilterEffect(PhotoFilter filterType)
        {
            try
            {
                MImageFilterView.Visibility = ViewStates.Visible;
                MImageFilterView.SetSourceBitmap(MImgSource.GetBitmap());
                await MImageFilterView.SetFilterEffect(filterType);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public async void SetFilterEffect(CustomEffect customEffect)
        {
            try
            {
                MImageFilterView.Visibility = ViewStates.Visible;
                MImageFilterView.SetSourceBitmap(MImgSource.GetBitmap());
                await MImageFilterView.SetFilterEffect(customEffect);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        } 
    }
}