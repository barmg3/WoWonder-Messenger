using Android.Content;
using Android.Runtime;
using Android.Views;
using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using AndroidX.AppCompat.Content.Res;
using WoWonder.Helpers.Utils;
using WoWonder.Library.RangeSlider;

namespace WoWonder.Library.Anjo.Video
{
    public class VideoSeekBarView : View
    {
        private static Drawable ThumbDrawable1;
        private static Drawable ThumbDrawablePressed1;
        private static Paint InnerPaint1 = new Paint();
        private static int ThumbWidth;
        private static int ThumbHeight;
        private int ThumbDx = 0;
        private float Progress = 0;
        private new bool Pressed = false;
        public ISeekBarDelegate BarDelegate;

        public interface ISeekBarDelegate
        {
            public void OnSeekBarDrag(float progress);
        }

        protected VideoSeekBarView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public VideoSeekBarView(Context context) : base(context)
        {
            Init(context);
        }

        public VideoSeekBarView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public VideoSeekBarView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        public VideoSeekBarView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                if (ThumbDrawable1 == null)
                {
                    ThumbDrawable1 = AppCompatResources.GetDrawable(context, Resource.Drawable.playback);
                    ThumbDrawablePressed1 = AppCompatResources.GetDrawable(context, Resource.Drawable.playback_active);
                    InnerPaint1.Color = Color.ParseColor("#99999999");
                    ThumbWidth = ThumbDrawable1.IntrinsicWidth;
                    ThumbHeight = ThumbDrawable1.IntrinsicHeight;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override bool OnTouchEvent(MotionEvent e)
        {
            try
            {
                if (e == null)
                {
                    return false;
                }
                float x = e.GetX();
                float y = e.GetY();
                float thumbX = (int)((MeasuredWidth - ThumbWidth) * Progress);
                if (e.Action == MotionEventActions.Down)
                {
                    int additionWidth = (MeasuredHeight - ThumbWidth) / 2;
                    if (thumbX - additionWidth <= x && x <= thumbX + ThumbWidth + additionWidth && y >= 0 && y <= MeasuredHeight)
                    {
                        Pressed = true;
                        ThumbDx = (int)(x - thumbX);
                        Parent.RequestDisallowInterceptTouchEvent(true);
                        Invalidate();
                        return true;
                    }
                }
                else if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel)
                {
                    if (Pressed)
                    {
                        if (e.Action == MotionEventActions.Up && BarDelegate != null)
                        {
                            BarDelegate.OnSeekBarDrag(thumbX / (float)(MeasuredWidth - ThumbWidth));
                        }
                        Pressed = false;
                        Invalidate();
                        return true;
                    }
                }
                else if (e.Action == MotionEventActions.Move)
                {
                    if (Pressed)
                    {
                        thumbX = (int)(x - ThumbDx);
                        if (thumbX < 0)
                        {
                            thumbX = 0;
                        }
                        else if (thumbX > MeasuredWidth - ThumbWidth)
                        {
                            thumbX = MeasuredWidth - ThumbWidth;
                        }
                        Progress = thumbX / (MeasuredWidth - ThumbWidth);
                        Invalidate();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return base.OnTouchEvent(e);

            }
        }

        public void SetProgress(float progress)
        {
            if (progress < 0)
            {
                progress = 0;
            }
            else if (progress > 1)
            {
                progress = 1;
            }
            Progress = progress;
            Invalidate();
        }

        public float GetProgress()
        {
            return Progress;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            try
            {
                Drawable thumb = null;
                if (!Pressed)
                {
                    thumb = ThumbDrawable1;
                }
                else
                {
                    thumb = ThumbDrawablePressed1;
                }
                int y = (MeasuredHeight - ThumbHeight) / 2;
                int thumbX = (int)((MeasuredWidth - ThumbWidth) * Progress);
                canvas.DrawRect(ThumbWidth / 2, MeasuredHeight / 2 - PixelUtil.Dp(Context, 1), MeasuredWidth - ThumbWidth / 2, MeasuredHeight / 2 + PixelUtil.Dp(Context, 1), InnerPaint1);
                thumb.SetBounds(thumbX, y, thumbX + ThumbWidth, y + ThumbHeight);
                thumb.Draw(canvas);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}