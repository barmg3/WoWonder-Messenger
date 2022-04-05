using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Library.Anjo.Stories.StoriesProgressView
{
    public class PausableProgressBar : FrameLayout
    {
        private static readonly int DefaultProgressDuration = 2000;

        private View FrontProgressView;
        private View MaxProgressView;

        private new PassableScaleAnimation Animation;
        private long Duration = DefaultProgressDuration;
        private ICallback Callback;

        public interface ICallback
        {
            void OnStartProgress();
            void OnFinishProgress();
        }

        protected PausableProgressBar(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public PausableProgressBar(Context context) : base(context)
        {
            Init(context);
        }

        public PausableProgressBar(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public PausableProgressBar(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        public PausableProgressBar(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                LayoutInflater.From(context)?.Inflate(Resource.Layout.pausable_progress, this);
                FrontProgressView = FindViewById(Resource.Id.front_progress);
                MaxProgressView = FindViewById(Resource.Id.max_progress); // work around 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDuration(long duration)
        {
            try
            {
                Duration = duration;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetCallback(ICallback callback)
        {
            try
            {
                Callback = callback;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetMax()
        {
            try
            {
                FinishProgress(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetMin()
        {
            try
            {
                FinishProgress(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetMinWithoutCallback()
        {
            try
            {
                MaxProgressView.SetBackgroundResource(Resource.Color.progress_secondary);

                MaxProgressView.Visibility = ViewStates.Visible;
                if (Animation != null)
                {
                    Animation.SetAnimationListener(null);
                    Animation.Cancel();
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetMaxWithoutCallback()
        {
            try
            {
                MaxProgressView.SetBackgroundResource(Resource.Color.progress_max_active);

                MaxProgressView.Visibility = ViewStates.Visible;
                if (Animation != null)
                {
                    Animation.SetAnimationListener(null);
                    Animation.Cancel();
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void FinishProgress(bool isMax)
        {
            try
            {
                if (isMax) MaxProgressView.SetBackgroundResource(Resource.Color.progress_max_active);
                MaxProgressView.Visibility = isMax ? ViewStates.Visible : ViewStates.Gone;
                if (Animation != null)
                {
                    Animation.SetAnimationListener(null);
                    Animation.Cancel();
                    Callback?.OnFinishProgress();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartProgress()
        {
            try
            {
                MaxProgressView.Visibility = ViewStates.Gone;

                Animation = new PassableScaleAnimation(0, 1, 1, 1, Dimension.Absolute, 0, Dimension.RelativeToSelf, 0)
                {
                    Duration = Duration,
                    Interpolator = new LinearInterpolator()
                };
                Animation.SetAnimationListener(new MyAnimationListener(this));
                Animation.FillAfter = true;
                FrontProgressView.StartAnimation(Animation);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public void PauseProgress()
        {
            try
            {
                Animation?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ResumeProgress()
        {
            try
            {

                Animation?.Resume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Clear()
        {
            try
            {
                if (Animation != null)
                {
                    Animation.SetAnimationListener(null);
                    Animation.Cancel();
                    Animation = null!;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAnimationListener : Java.Lang.Object, Animation.IAnimationListener
        {
            private readonly PausableProgressBar ProgressBar;
            public MyAnimationListener(PausableProgressBar progressBar)
            {
                ProgressBar = progressBar;
            }

            public void OnAnimationEnd(Animation animation)
            {
                ProgressBar.Callback?.OnFinishProgress();
            }

            public void OnAnimationRepeat(Animation animation)
            {

            }

            public void OnAnimationStart(Animation animation)
            {
                try
                {
                    ProgressBar.FrontProgressView.Visibility = ViewStates.Visible;
                    ProgressBar.Callback?.OnStartProgress();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        private class PassableScaleAnimation : ScaleAnimation
        {
            private long MElapsedAtPause = 0;
            private bool MPaused = false;

            protected PassableScaleAnimation(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            public PassableScaleAnimation(Context context, IAttributeSet attrs) : base(context, attrs)
            {
            }

            public PassableScaleAnimation(float fromX, float toX, float fromY, float toY) : base(fromX, toX, fromY, toY)
            {
            }

            public PassableScaleAnimation(float fromX, float toX, float fromY, float toY, float pivotX, float pivotY) : base(fromX, toX, fromY, toY, pivotX, pivotY)
            {
            }

            public PassableScaleAnimation(float fromX, float toX, float fromY, float toY, Dimension pivotXType, float pivotXValue, Dimension pivotYType, float pivotYValue) : base(fromX, toX, fromY, toY, pivotXType, pivotXValue, pivotYType, pivotYValue)
            {
            }

            public override bool GetTransformation(long currentTime, Transformation outTransformation, float scale)
            {
                try
                {
                    if (MPaused && MElapsedAtPause == 0)
                    {
                        MElapsedAtPause = currentTime - StartTime;
                    }
                    if (MPaused)
                    {
                        StartTime = currentTime - MElapsedAtPause;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                return base.GetTransformation(currentTime, outTransformation, scale);
            }

            /// <summary>
            /// pause animation
            /// </summary>
            public void Pause()
            {
                try
                {
                    if (MPaused) return;
                    MElapsedAtPause = 0;
                    MPaused = true;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            /// <summary>
            /// resume animation
            /// </summary>
            public void Resume()
            {
                try
                {
                    MPaused = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

        }
    }
}