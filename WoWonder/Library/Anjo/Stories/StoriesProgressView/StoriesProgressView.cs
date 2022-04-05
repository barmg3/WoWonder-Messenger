﻿using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using LayoutDirection = Android.Views.LayoutDirection;
using Orientation = Android.Widget.Orientation;

namespace WoWonder.Library.Anjo.Stories.StoriesProgressView
{
    public class StoriesProgressView : LinearLayout
    {
        private readonly LayoutParams ProgressBarLayoutParam = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);
        private readonly LayoutParams SpaceLayoutParam = new LayoutParams(5, ViewGroup.LayoutParams.WrapContent);

        private readonly List<PausableProgressBar> ProgressBars = new List<PausableProgressBar>();

        private int StoriesCount = -1;
        //pointer of running animation
        private int Current = -1;
        private IStoriesListener StoriesListener;
        bool IsComplete;

        private bool IsSkipStart;
        private bool IsReverseStart;

        public interface IStoriesListener
        {
            void OnNext();

            void OnPrev();

            void OnComplete();
        }

        public interface IStoryStateListener
        {
            void OnPause();
            void OnResume();
        }

        protected StoriesProgressView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public StoriesProgressView(Context context) : base(context)
        {
            Init(context, null);
        }

        public StoriesProgressView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public StoriesProgressView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        public StoriesProgressView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs);
        }

        private void Init(Context context, IAttributeSet attrs)
        {
            try
            {
                Orientation = Orientation.Horizontal;
                if (attrs != null)
                {
                    TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.StoriesProgressView);
                    StoriesCount = typedArray.GetInt(Resource.Styleable.StoriesProgressView_progressCount, 0);
                    typedArray.Recycle();
                }
                BindViews();

                //isRtl
                if (AppSettings.FlowDirectionRightToLeft)
                {
                    LayoutDirection = LayoutDirection.Ltr;
                    Rotation = 180;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BindViews()
        {
            try
            {
                ProgressBars.Clear();
                RemoveAllViews();

                for (int i = 0; i < StoriesCount; i++)
                {
                    PausableProgressBar p = CreateProgressBar();
                    ProgressBars.Add(p);
                    AddView(p);
                    if (i + 1 < StoriesCount)
                    {
                        AddView(CreateSpace());
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private PausableProgressBar CreateProgressBar()
        {
            PausableProgressBar p = new PausableProgressBar(Context) { LayoutParameters = ProgressBarLayoutParam };
            return p;
        }

        private View CreateSpace()
        {
            View v = new View(Context) { LayoutParameters = SpaceLayoutParam };
            return v;
        }

        /// <summary>
        /// Set story count and create views
        /// </summary>
        /// <param name="storiesCount">story count</param>
        public void SetStoriesCount(int storiesCount)
        {
            try
            {
                StoriesCount = storiesCount;
                BindViews();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Set storiesListener
        /// </summary>
        /// <param name="storiesListener">StoriesListener</param>
        public void SetStoriesListener(IStoriesListener storiesListener)
        {
            try
            {
                StoriesListener = storiesListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Skip current story
        /// </summary>
        public void Skip()
        {
            try
            {
                if (IsSkipStart || IsReverseStart) return;
                if (IsComplete) return;
                if (Current < 0) return;
                PausableProgressBar p = ProgressBars[Current];
                IsSkipStart = true;
                p.SetMax();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Reverse current story
        /// </summary>
        public void Reverse()
        {
            try
            {
                if (IsSkipStart || IsReverseStart) return;
                if (IsComplete) return;
                if (Current < 0) return;
                PausableProgressBar p = ProgressBars[Current];
                IsReverseStart = true;
                p.SetMin();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Set a story's duration
        /// </summary>
        /// <param name="duration">millisecond</param>
        public void SetStoryDuration(long duration)
        {
            try
            {
                for (int i = 0; i < ProgressBars.Count; i++)
                {
                    ProgressBars[i].SetDuration(duration);
                    ProgressBars[i].SetCallback(Callback(i));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Set stories count and each story duration
        /// </summary>
        /// <param name="durations"></param>
        public void SetStoriesCountWithDurations(long[] durations)
        {
            try
            {
                StoriesCount = durations.Length;
                BindViews();
                for (int i = 0; i < ProgressBars.Count; i++)
                {
                    ProgressBars[i].SetDuration(durations[i]);
                    ProgressBars[i].SetCallback(Callback(i));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private PausableProgressBar.ICallback Callback(int index)
        {
            return new MyPassableProgressBar(this, index);
        }

        private class MyPassableProgressBar : PausableProgressBar.ICallback
        {
            private readonly int Index;
            private readonly StoriesProgressView ProgressView;

            public MyPassableProgressBar(StoriesProgressView progressView, int index)
            {
                ProgressView = progressView;
                Index = index;
            }
            public void OnStartProgress()
            {
                ProgressView.Current = Index;
            }

            public void OnFinishProgress()
            {
                try
                {
                    if (ProgressView.IsReverseStart)
                    {
                        ProgressView.StoriesListener?.OnPrev();
                        if (0 <= ProgressView.Current - 1)
                        {
                            PausableProgressBar p = ProgressView.ProgressBars[ProgressView.Current - 1];
                            p.SetMinWithoutCallback();
                            ProgressView.ProgressBars[--ProgressView.Current].StartProgress();
                        }
                        else
                        {
                            ProgressView.ProgressBars[ProgressView.Current].StartProgress();
                        }
                        ProgressView.IsReverseStart = false;
                        return;
                    }
                    int next = ProgressView.Current + 1;
                    if (next <= ProgressView.ProgressBars.Count - 1)
                    {
                        ProgressView.StoriesListener?.OnNext();
                        ProgressView.ProgressBars[next].StartProgress();
                    }
                    else
                    {
                        ProgressView.IsComplete = true;
                        ProgressView.StoriesListener?.OnComplete();
                    }
                    ProgressView.IsSkipStart = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        /// <summary>
        /// Start progress animation
        /// </summary>
        public void StartStories()
        {
            try
            {
                ProgressBars[0].StartProgress();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Start progress animation from specific progress
        /// </summary>
        /// <param name="from"></param>
        public void StartStories(int from)
        {
            try
            {
                for (int i = 0; i < from; i++)
                {
                    ProgressBars[i].SetMaxWithoutCallback();
                }
                ProgressBars[from].StartProgress();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Need to call when Activity or Fragment destroy
        /// </summary>
        public void Destroy()
        {
            try
            {
                IsComplete = false;
                foreach (var p in ProgressBars)
                {
                    p.Clear();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Pause story
        /// </summary>
        public void Pause()
        {
            try
            {
                if (Current < 0) return;
                ProgressBars[Current].PauseProgress();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Resume story
        /// </summary>
        public void Resume()
        {
            try
            {
                if (Current < 0) return;
                ProgressBars[Current].ResumeProgress();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}