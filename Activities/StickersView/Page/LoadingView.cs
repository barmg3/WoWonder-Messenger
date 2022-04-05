using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Com.Aghajari.Emojiview.Utils;
using Com.Aghajari.Emojiview.View;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.StickersView.Page
{
    public class LoadingView : AXEmojiLayout
    {
        private ProgressBar ProgressBar;
        protected LoadingView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LoadingView(Context context) : base(context)
        {
            Init(context);
        }

        public LoadingView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public LoadingView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                ProgressBar = new ProgressBar(context);
                AddView(ProgressBar, new LayoutParams(0, 0, Utils.DpToPx(context, 44), Utils.DpToPx(context, 44)));
                ProgressBar.Indeterminate = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            try
            {
                base.OnSizeChanged(w, h, oldw, oldh);

                var layoutParams = (LayoutParams)ProgressBar.LayoutParameters;
                if (layoutParams != null)
                {
                    layoutParams.LeftMargin = w / 2 - Utils.DpToPx(Context, 22);
                    layoutParams.TopMargin = h / 2 - Utils.DpToPx(Context, 44);
                }

                RequestLayout();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }
    }
}