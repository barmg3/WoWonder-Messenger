using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Util;
using AndroidX.AppCompat.Content.Res;
using IO.SuperCharge.ShimmerLayoutLib;
using WoWonder.Helpers.Utils;

namespace WoWonder.Library.Anjo.XRecordView
{
    public class RecordView : RelativeLayout
    {
        public static int DefaultCancelBounds = 8; //8dp
        private ImageView SmallBlinkingMic, BasketImg;
        private Chronometer CounterTime;
        private TextView SlideToCancel;
        private ShimmerLayout SlideToCancelLayout;
        private ImageView Arrow;
        private float InitialX, BasketInitialY, DifX;
        private float CancelBounds = DefaultCancelBounds;
        private long StartTime, ElapsedTime;
        private new readonly Context Context;
        private IOnRecordListener RecordListener;
        private bool IsSwiped, IsLessThanSecondAllowed;
        private bool IsSoundEnabled = true;
        private int RecordStart = Resource.Raw.record_start;
        private int RecordFinished = Resource.Raw.record_finished;
        private int RecordError = Resource.Raw.record_error;
        private MediaPlayer Player;
        private AnimationHelper AnimationHelper;

        public RecordView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public RecordView(Context context) : base(context)
        {
            Context = context;
            Init(context, null, -1, -1);
        }

        public RecordView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Context = context;
            Init(context, attrs, -1, -1);
        }

        public RecordView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Context = context;
            Init(context, attrs, defStyleAttr, -1);
        }

        public RecordView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Context = context;
            Init(context, attrs, defStyleAttr, defStyleRes);
        }

        private void Init(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
        {
            try
            {
                View view = Inflate(context, Resource.Layout.RecordViewLayout, null);
                AddView(view);

                ViewGroup viewGroup = (ViewGroup)view.Parent;
                viewGroup.SetClipChildren(false);

                Arrow = view.FindViewById<ImageView>(Resource.Id.arrow);
                SlideToCancel = view.FindViewById<TextView>(Resource.Id.slide_to_cancel);
                SmallBlinkingMic = view.FindViewById<ImageView>(Resource.Id.glowing_mic);
                CounterTime = view.FindViewById<Chronometer>(Resource.Id.counter_tv);
                BasketImg = view.FindViewById<ImageView>(Resource.Id.basket_img);
                SlideToCancelLayout = view.FindViewById<ShimmerLayout>(Resource.Id.shimmer_layout);

                HideViews(true);

                if (attrs != null)
                {
                    TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.RecordView, defStyleAttr, defStyleRes);

                    int slideArrowResource = typedArray.GetResourceId(Resource.Styleable.RecordView_slide_to_cancel_arrow, -1);
                    string slideToCancelText = typedArray.GetString(Resource.Styleable.RecordView_slide_to_cancel_text);
                    int slideMarginRight = (int)typedArray.GetDimension(Resource.Styleable.RecordView_slide_to_cancel_margin_right, 30);
                    Color counterTimeColor = typedArray.GetColor(Resource.Styleable.RecordView_counter_time_color, -1);
                    Color arrowColor = typedArray.GetColor(Resource.Styleable.RecordView_slide_to_cancel_arrow_color, -1);

                    int cancelBounds = typedArray.GetDimensionPixelSize(Resource.Styleable.RecordView_slide_to_cancel_bounds, -1);

                    if (cancelBounds != -1)
                        SetCancelBounds(cancelBounds, false);//don't convert it to pixels since it's already in pixels

                    if (slideArrowResource != -1)
                    {
                        Drawable slideArrow = AppCompatResources.GetDrawable(((View)this).Context, slideArrowResource);
                        Arrow.SetImageDrawable(slideArrow);
                    }

                    if (slideToCancelText != null)
                        SlideToCancel.Text = slideToCancelText;

                    if (counterTimeColor != -1)
                        SetCounterTimeColor(counterTimeColor);

                    if (arrowColor != -1)
                        SetSlideToCancelArrowColor(arrowColor);

                    SetMarginRight(slideMarginRight, true);

                    typedArray.Recycle();
                }

                AnimationHelper = new AnimationHelper(context, BasketImg, SmallBlinkingMic);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        private void HideViews(bool hideSmallMic)
        {
            try
            {
                SlideToCancelLayout.Visibility = ViewStates.Gone;
                CounterTime.Visibility = ViewStates.Gone;
                if (hideSmallMic)
                    SmallBlinkingMic.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        private void ShowViews()
        {
            try
            {
                SlideToCancelLayout.Visibility = ViewStates.Visible;
                SmallBlinkingMic.Visibility = ViewStates.Visible;
                CounterTime.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        private bool isLessThanOneSecond(long time)
        {
            return time <= 1000;
        }

        private void PlaySound(int soundRes)
        {
            try
            {
                if (IsSoundEnabled)
                {
                    if (soundRes == 0)
                        return;

                    Player = new MediaPlayer();
                    AssetFileDescriptor afd = Context.Resources?.OpenRawResourceFd(soundRes);
                    if (afd == null) return;
                    Player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                    afd.Close();
                    Player.Prepare();
                    Player.Start();

                    Player.Completion += (sender, args) =>
                    {
                        try
                        {
                            Player.Release();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);

                        }
                    };
                    Player.Looping = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnActionDown(RecordButton recordBtn, MotionEvent motionEvent)
        {
            try
            {
                RecordListener?.OnStartRecord();

                AnimationHelper.SetStartRecorded(true);
                AnimationHelper.ResetBasketAnimation();
                AnimationHelper.ResetSmallMic();

                recordBtn.StartScale();
                SlideToCancelLayout.StartShimmerAnimation();

                InitialX = recordBtn.GetX();

                BasketInitialY = BasketImg.GetY() + 90;

                PlaySound(RecordStart);

                ShowViews();

                AnimationHelper.AnimateSmallMicAlpha();
                CounterTime.Base = SystemClock.ElapsedRealtime();
                StartTime = Methods.Time.CurrentTimeMillis();
                CounterTime.Start();
                IsSwiped = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnActionMove(RecordButton recordBtn, MotionEvent motionEvent)
        {
            try
            {
                long time = Methods.Time.CurrentTimeMillis() - StartTime;

                if (!IsSwiped)
                {
                    //Swipe To Cancel
                    if (SlideToCancelLayout.GetX() != 0 && SlideToCancelLayout.GetX() <= CounterTime.Right + CancelBounds)
                    {
                        //if the time was less than one second then do not start basket animation
                        if (isLessThanOneSecond(time))
                        {
                            HideViews(true);
                            AnimationHelper.ClearAlphaAnimation(false);
                            AnimationHelper.OnAnimationEnd();
                        }
                        else
                        {
                            HideViews(false);
                            AnimationHelper.AnimateBasket(BasketInitialY);
                        }

                        AnimationHelper.MoveRecordButtonAndSlideToCancelBack(recordBtn, SlideToCancelLayout, InitialX, DifX);

                        CounterTime.Stop();
                        SlideToCancelLayout.StopShimmerAnimation();
                        IsSwiped = true;

                        AnimationHelper.SetStartRecorded(false);

                        RecordListener?.OnCancelRecord();
                    }
                    else
                    {
                        //if statement is to Prevent Swiping out of bounds
                        if (motionEvent.RawX < InitialX)
                        {
                            recordBtn.Animate()
                                ?.X(motionEvent.RawX)
                                ?.SetDuration(0)
                                ?.Start();

                            if (DifX == 0)
                                DifX = InitialX - SlideToCancelLayout.GetX();

                            SlideToCancelLayout.Animate()
                                ?.X(motionEvent.RawX - DifX)
                                ?.SetDuration(0)
                                ?.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnActionUp(RecordButton recordBtn)
        {
            try
            {
                ElapsedTime = Methods.Time.CurrentTimeMillis() - StartTime;

                if (!IsLessThanSecondAllowed && isLessThanOneSecond(ElapsedTime) && !IsSwiped)
                {
                    RecordListener?.OnLessThanSecond();

                    AnimationHelper.SetStartRecorded(false);
                    PlaySound(RecordError);
                }
                else
                {
                    if (!IsSwiped)
                        RecordListener?.OnFinishRecord(ElapsedTime);

                    AnimationHelper.SetStartRecorded(false);

                    if (!IsSwiped)
                        PlaySound(RecordFinished);
                }

                //if user has swiped then do not hide SmallMic since it will be hidden after swipe Animation
                HideViews(!IsSwiped);

                if (!IsSwiped)
                    AnimationHelper.ClearAlphaAnimation(true);

                AnimationHelper?.MoveRecordButtonAndSlideToCancelBack(recordBtn, SlideToCancelLayout, InitialX, DifX);
                CounterTime.Stop();
                SlideToCancelLayout.StopShimmerAnimation();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        private void SetMarginRight(int marginRight, bool convertToDp)
        {
            try
            {
                LayoutParams layoutParams = (LayoutParams)SlideToCancelLayout.LayoutParameters;
                if (layoutParams != null)
                {
                    if (convertToDp)
                    {
                        layoutParams.RightMargin = (int)DpUtil.ToPixel(marginRight, Context);
                    }
                    else
                        layoutParams.RightMargin = marginRight;

                    SlideToCancelLayout.LayoutParameters = layoutParams;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public void SetOnRecordListener(IOnRecordListener recrodListener)
        {
            RecordListener = recrodListener;
        }

        public void SetOnBasketAnimationEndListener(IOnBasketAnimationEnd onBasketAnimationEndListener)
        {
            AnimationHelper?.setOnBasketAnimationEndListener(onBasketAnimationEndListener);
        }

        public void SetSoundEnabled(bool isEnabled)
        {
            IsSoundEnabled = isEnabled;
        }

        public void SetLessThanSecondAllowed(bool isAllowed)
        {
            IsLessThanSecondAllowed = isAllowed;
        }

        public void SetSlideToCancelText(string text)
        {
            SlideToCancel.Text = text;
        }

        public void SetSlideToCancelTextColor(Color color)
        {
            SlideToCancel.SetTextColor(color);
        }

        public void SetSmallMicColor(Color color)
        {
            SmallBlinkingMic.SetColorFilter(color);
        }

        public void SetSmallMicIcon(int icon)
        {
            SmallBlinkingMic.SetImageResource(icon);
        }

        public void SetSlideMarginRight(int marginRight)
        {
            SetMarginRight(marginRight, true);
        }


        public void SetCustomSounds(int startSound, int finishedSound, int errorSound)
        {
            //0 means do not play sound
            RecordStart = startSound;
            RecordFinished = finishedSound;
            RecordError = errorSound;
        }

        public float GetCancelBounds()
        {
            return CancelBounds;
        }

        public void SetCancelBounds(float cancelBounds)
        {
            SetCancelBounds(cancelBounds, true);
        }

        //set Chronometer color
        public void SetCounterTimeColor(Color color)
        {
            CounterTime?.SetTextColor(color);
        }

        public void SetSlideToCancelArrowColor(Color color)
        {
            Arrow?.SetColorFilter(color);
        }

        private void SetCancelBounds(float cancelBounds, bool convertDpToPixel)
        {
            float bounds = convertDpToPixel ? DpUtil.ToPixel(cancelBounds, Context) : cancelBounds;
            CancelBounds = bounds;
        }
    }

    public static class DpUtil
    {
        public static float ToPixel(float dp, Context context)
        {
            try
            {
                Resources resources = context.Resources;
                DisplayMetrics metrics = resources?.DisplayMetrics;
                float px = dp * ((float)metrics.DensityDpi / (float)DisplayMetricsDensity.Default);
                return px;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return dp;
            }
        }

        public static float ToDp(float px, Context context)
        {
            try
            {
                Resources resources = context.Resources;
                DisplayMetrics metrics = resources?.DisplayMetrics;
                float dp = px / ((float)metrics.DensityDpi / (float)DisplayMetricsDensity.Default);
                return dp;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return px;
            }
        }
    }
}