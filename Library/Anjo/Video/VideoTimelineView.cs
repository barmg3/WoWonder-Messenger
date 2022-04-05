using Android.Content;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Util;
using Java.IO;
using WoWonder.Helpers.Utils;
using WoWonder.Library.RangeSlider;
using Exception = System.Exception;
using Math = System.Math;
using Object = System.Object;
using String = System.String;
using Uri = Android.Net.Uri;

namespace WoWonder.Library.Anjo.Video
{
    public class VideoTimelineView : View
    {
        private long VideoLength;
        private float ProgressLeft;
        private float ProgressRight = 1;
        private Paint Paint;
        private Paint Paint2;
        private bool PressedLeft;
        private bool PressedRight;
        private float PressDx;
        private MediaMetadataRetriever MediaMetadataRetriever;
        private IVideoTimelineViewListener VideoTimelineViewListener;
        private List<Bitmap> Frames = new List<Bitmap>();
        private AsyncTask<int, int, Bitmap> CurrentTask;
        private static Object Sync = new Object();
        private long FrameTimeOffset;
        private int FrameWidth;
        private int FrameHeight;
        private int FramesToLoad;
        private float MaxProgressDiff = 1.0f;
        private float MinProgressDiff = 0.0f;
        private bool IsRoundFrames;
        private Rect Rect1;
        private Rect Rect2;

        public interface IVideoTimelineViewListener
        { 
            void OnLeftProgressChanged(float progress);

            void OnRightProgressChanged(float progress);

            void DidStartDragging();

            void DidStopDragging();
        }

        protected VideoTimelineView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public VideoTimelineView(Context context) : base(context)
        {
            Init();
        }

        public VideoTimelineView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public VideoTimelineView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public VideoTimelineView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }


        private void Init()
        { 
            try
            {
                Paint = new Paint(PaintFlags.AntiAlias);
                Paint.Color = Color.ParseColor(AppSettings.MainColor);
                Paint2 = new Paint();
                var color = Color.ParseColor("#7f000000");
                Paint2.Color = new Color(color.R , color.G , color.B , color.A);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public float GetLeftProgress()
        {
            return ProgressLeft;
        }

        public float GetRightProgress()
        {
            return ProgressRight;
        }

        public void SetMinProgressDiff(float value)
        {
            MinProgressDiff = value;
        }

        public void SetMaxProgressDiff(float value)
        {
            MaxProgressDiff = value;
            if (ProgressRight - ProgressLeft > MaxProgressDiff)
            {
                ProgressRight = ProgressLeft + MaxProgressDiff;
                Invalidate();
            }
        }

        public void SetRoundFrames(bool value)
        {
            IsRoundFrames = value;
            if (IsRoundFrames)
            {
                Rect1 = new Rect(PixelUtil.Dp(Context, 14), PixelUtil.Dp(Context, 14), PixelUtil.Dp(Context, 14 + 28), PixelUtil.Dp(Context, 14 + 28));
                Rect2 = new Rect();
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

                int width = MeasuredWidth - PixelUtil.Dp(Context, 32);
                int startX = (int)(width * ProgressLeft) + PixelUtil.Dp(Context, 16);
                int endX = (int)(width * ProgressRight) + PixelUtil.Dp(Context, 16);

                if (e.Action == MotionEventActions.Down)
                {
                    Parent.RequestDisallowInterceptTouchEvent(true);
                    if (MediaMetadataRetriever == null)
                    {
                        return false;
                    }

                    int additionWidth = PixelUtil.Dp(Context, 12);
                    if (startX - additionWidth <= x && x <= startX + additionWidth && y >= 0 &&
                        y <= MeasuredHeight)
                    {
                        if (VideoTimelineViewListener != null)
                        {
                            VideoTimelineViewListener.DidStartDragging();
                        }

                        PressedLeft = true;
                        PressDx = (int)(x - startX);
                        Invalidate();
                        return true;
                    }
                    else if (endX - additionWidth <= x && x <= endX + additionWidth && y >= 0 &&
                             y <= MeasuredHeight)
                    {
                        if (VideoTimelineViewListener != null)
                        {
                            VideoTimelineViewListener.DidStartDragging();
                        }

                        PressedRight = true;
                        PressDx = (int)(x - endX);
                        Invalidate();
                        return true;
                    }
                }
                else if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel)
                {
                    if (PressedLeft)
                    {
                        if (VideoTimelineViewListener != null)
                        {
                            VideoTimelineViewListener.DidStopDragging();
                        }

                        PressedLeft = false;
                        return true;
                    }
                    else if (PressedRight)
                    {
                        if (VideoTimelineViewListener != null)
                        {
                            VideoTimelineViewListener.DidStopDragging();
                        }

                        PressedRight = false;
                        return true;
                    }
                }
                else if (e.Action == MotionEventActions.Move)
                {
                    if (PressedLeft)
                    {
                        startX = (int)(x - PressDx);
                        if (startX < PixelUtil.Dp(Context, 16))
                        {
                            startX = PixelUtil.Dp(Context, 16);
                        }
                        else if (startX > endX)
                        {
                            startX = endX;
                        }

                        ProgressLeft = (float)(startX - PixelUtil.Dp(Context, 16)) / (float)width;
                        if (ProgressRight - ProgressLeft > MaxProgressDiff)
                        {
                            ProgressRight = ProgressLeft + MaxProgressDiff;
                        }
                        else if (MinProgressDiff != 0 && ProgressRight - ProgressLeft < MinProgressDiff)
                        {
                            ProgressLeft = ProgressRight - MinProgressDiff;
                            if (ProgressLeft < 0)
                            {
                                ProgressLeft = 0;
                            }
                        }

                        if (VideoTimelineViewListener != null)
                        {
                            VideoTimelineViewListener.OnLeftProgressChanged(ProgressLeft);
                        }

                        Invalidate();
                        return true;
                    }
                    else if (PressedRight)
                    {
                        endX = (int)(x - PressDx);
                        if (endX < startX)
                        {
                            endX = startX;
                        }
                        else if (endX > width + PixelUtil.Dp(Context, 16))
                        {
                            endX = width + PixelUtil.Dp(Context, 16);
                        }

                        ProgressRight = (float)(endX - PixelUtil.Dp(Context, 16)) / (float)width;
                        if (ProgressRight - ProgressLeft > MaxProgressDiff)
                        {
                            ProgressLeft = ProgressRight - MaxProgressDiff;
                        }
                        else if (MinProgressDiff != 0 && ProgressRight - ProgressLeft < MinProgressDiff)
                        {
                            ProgressRight = ProgressLeft + MinProgressDiff;
                            if (ProgressRight > 1.0f)
                            {
                                ProgressRight = 1.0f;
                            }
                        }

                        if (VideoTimelineViewListener != null)
                        {
                            VideoTimelineViewListener.OnRightProgressChanged(ProgressRight);
                        }

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


        public void SetColor(Color color)
        {
            Paint.Color = (color);
        }

        public void SetMaxDuration(int videoLength)
        {
            VideoLength = videoLength;
        }

        public void SetVideoPath(string path)
        {
            Destroy();
            MediaMetadataRetriever = new MediaMetadataRetriever();
            ProgressLeft = 0.0f;
            ProgressRight = 1.0f;
            try
            {
                var file = Uri.FromFile(new File(path));
                MediaMetadataRetriever.SetDataSource(Context, file);
                String duration = MediaMetadataRetriever.ExtractMetadata(MetadataKey.Duration);
                VideoLength = 10;// Long.parseLong(duration);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            Invalidate();
        }

        public void SetVideoTimelineViewListener(IVideoTimelineViewListener videoTimelineViewListener)
        {
            VideoTimelineViewListener = videoTimelineViewListener;
        }

        private void ReloadFrames(int frameNum)
        {
            try
            {
                if (MediaMetadataRetriever == null)
                {
                    return;
                }
                if (frameNum == 0)
                {
                    if (IsRoundFrames)
                    {
                        FrameHeight = FrameWidth = PixelUtil.Dp(Context, 56);
                        FramesToLoad = (int)Math.Ceiling((MeasuredWidth - PixelUtil.Dp(Context, 16)) / (FrameHeight / 2.0f));
                    }
                    else
                    {
                        FrameHeight = PixelUtil.Dp(Context, 40);
                        FramesToLoad = (MeasuredWidth - PixelUtil.Dp(Context, 16)) / FrameHeight;
                        FrameWidth = (int)Math.Ceiling((float)(MeasuredWidth - PixelUtil.Dp(Context, 16)) / (float)FramesToLoad);
                    }
                    FrameTimeOffset = VideoLength / FramesToLoad;
                }
                CurrentTask = new AsyncTaskAnonymousInnerClass(this);
                CurrentTask.Execute(frameNum);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private class AsyncTaskAnonymousInnerClass : AsyncTask<int, int, Bitmap>
        {
            private readonly VideoTimelineView OuterInstance;

            private int FrameNum;

            public AsyncTaskAnonymousInnerClass(VideoTimelineView outerInstance)
            {
                OuterInstance = outerInstance;
            }
             
            protected override Bitmap RunInBackground(params int[] objects)
            {
                try
                {
                    FrameNum = objects?.First() ?? 0;
                    Bitmap bitmap = OuterInstance.MediaMetadataRetriever.GetFrameAtTime(OuterInstance.FrameTimeOffset * FrameNum * 1000, Option.ClosestSync);
                    if (bitmap != null)
                    {
                        Bitmap result = Bitmap.CreateBitmap(OuterInstance.FrameWidth, OuterInstance.FrameHeight, Bitmap.Config.Argb8888);
                        if (result != null)
                        {
                            Canvas canvas = new Canvas(result);
                            float scaleX = (float)OuterInstance.FrameWidth / (float)bitmap.Width;
                            float scaleY = (float)OuterInstance.FrameHeight / (float)bitmap.Height;
                            float scale = scaleX > scaleY ? scaleX : scaleY;
                            int w = (int)(bitmap.Width * scale);
                            int h = (int)(bitmap.Height * scale);
                            Rect srcRect = new Rect(0, 0, bitmap.Width, bitmap.Height);
                            Rect destRect = new Rect((OuterInstance.FrameWidth - w) / 2, (OuterInstance.FrameHeight - h) / 2, w, h);
                            canvas.DrawBitmap(bitmap, srcRect, destRect, null);
                        }

                        bitmap.Recycle();
                        bitmap = result;
                    }

                    return bitmap;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            }

            protected override void OnPostExecute(Bitmap result)
            {
                try
                {
                    base.OnPostExecute(result);

                    OuterInstance.Frames.Add(result);
                    OuterInstance.Invalidate();
                    if (FrameNum < OuterInstance.FramesToLoad)
                    {
                        OuterInstance.ReloadFrames(FrameNum + 1);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
             
        }

        public void Destroy()
        {
            try
            {
                lock (Sync)
                {
                    try
                    {
                        if (MediaMetadataRetriever != null)
                        {
                            MediaMetadataRetriever.Release();
                            MediaMetadataRetriever = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
                foreach (var bitmap in Frames)
                {
                    bitmap?.Recycle();
                }
                Frames.Clear();
                if (CurrentTask != null)
                {
                    CurrentTask.Dispose();
                    CurrentTask = null;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearFrames()
        {
            for (int a = 0; a < Frames.Count; a++)
            {
                Bitmap bitmap = Frames[a];
                if (bitmap != null)
                {
                    bitmap.Recycle();
                }
            }
            Frames.Clear();
            if (CurrentTask != null)
            {
                CurrentTask.Dispose();
                CurrentTask = null;
            }
            Invalidate();
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            int width = MeasuredWidth - PixelUtil.Dp(Context, 36);
            int startX = (int)(width * ProgressLeft) + PixelUtil.Dp(Context, 16);
            int endX = (int)(width * ProgressRight) + PixelUtil.Dp(Context, 16);

            canvas.Save();
            canvas.ClipRect(PixelUtil.Dp(Context, 16), 0, width + PixelUtil.Dp(Context, 20), MeasuredHeight);
            if (Frames.Count == 0 && CurrentTask == null)
            {
                ReloadFrames(0);
            }
            else
            {
                int offset = 0;
                foreach (var bitmap in Frames)
                {
                    if (bitmap != null)
                    {
                        int x = PixelUtil.Dp(Context, 16) + offset * (IsRoundFrames ? FrameWidth / 2 : FrameWidth);
                        int y = PixelUtil.Dp(Context, 2);
                        if (IsRoundFrames)
                        {
                            Rect2.Set(x, y, x + PixelUtil.Dp(Context, 28), y + PixelUtil.Dp(Context, 28));
                            canvas.DrawBitmap(bitmap, Rect1, Rect2, null);
                        }
                        else
                        {
                            canvas.DrawBitmap(bitmap, x, y, null);
                        }
                    }
                    offset++;
                }
            }

            int top = PixelUtil.Dp(Context, 2);

            canvas.DrawRect(PixelUtil.Dp(Context, 16), top, startX, MeasuredHeight - top, Paint2);
            canvas.DrawRect(endX + PixelUtil.Dp(Context, 4), top, PixelUtil.Dp(Context, 16) + width + PixelUtil.Dp(Context, 4), MeasuredHeight - top, Paint2);

            canvas.DrawRect(startX, 0, startX + PixelUtil.Dp(Context, 2), MeasuredHeight, Paint);
            canvas.DrawRect(endX + PixelUtil.Dp(Context, 2), 0, endX + PixelUtil.Dp(Context, 4), MeasuredHeight, Paint);
            canvas.DrawRect(startX + PixelUtil.Dp(Context, 2), 0, endX + PixelUtil.Dp(Context, 4), top, Paint);
            canvas.DrawRect(startX + PixelUtil.Dp(Context, 2), MeasuredHeight - top, endX + PixelUtil.Dp(Context, 4), MeasuredHeight, Paint);
            canvas.Restore();

            canvas.DrawCircle(startX, MeasuredHeight / 2, PixelUtil.Dp(Context, 7), Paint);
            canvas.DrawCircle(endX + PixelUtil.Dp(Context, 4), MeasuredHeight / 2, PixelUtil.Dp(Context, 7), Paint);
        }
    }
}