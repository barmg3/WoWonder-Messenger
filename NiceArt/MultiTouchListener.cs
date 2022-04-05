using System;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;

namespace WoWonder.NiceArt
{
    public class MultiTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        public static readonly int InvalidPointerId = -1;
        public readonly GestureDetector MGestureListener;
        public static bool IsRotateEnabled = true;
        public static bool IsTranslateEnabled = true;
        public static bool IsScaleEnabled = true;
        public static float MinimumScale = 0.5f;
        public static float MaximumScale = 10.0f;
        public int MActivePointerId = InvalidPointerId;
        public float MPrevX, MPrevY, MPrevRawX, MPrevRawY;
        public ScaleGestureDetector MScaleGestureDetector;

        public int[] Location = new int[2];
        public Rect OutRect;
        public View DeleteView;
        public ImageView PhotoEditImageView;
        public RelativeLayout ParentView;

        public INiceArt.IOnMultiTouchListener OnMultiTouchListener;
        public static INiceArt.IOnGestureControl MOnGestureControl;
        public static bool MIsTextPinchZoomable;
        public INiceArt.IOnNiceArtEditorListener MOnNiceArtEditorListener;
        public static ViewType MviewType;

        public MultiTouchListener(View deleteView, RelativeLayout parentView, ImageView photoEditImageView, bool isTextPinchZoomable, INiceArt.IOnNiceArtEditorListener onNiceArtEditorListener)
        {
            try
            {
                MIsTextPinchZoomable = isTextPinchZoomable;
                MScaleGestureDetector = new ScaleGestureDetector(new ScaleGestureListener());

                MGestureListener = new GestureDetector(Application.Context, new GestureListener());
                DeleteView = deleteView;
                ParentView = parentView;
                PhotoEditImageView = photoEditImageView;
                MOnNiceArtEditorListener = onNiceArtEditorListener;
                OutRect = deleteView != null ? new Rect(deleteView.Left, deleteView.Top, deleteView.Right, deleteView.Bottom) : new Rect(0, 0, 0, 0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static float AdjustAngle(float degrees)
        {
            try
            {
                if (degrees > 180.0f)
                {
                    degrees -= 360.0f;
                }
                else if (degrees < -180.0f)
                {
                    degrees += 360.0f;
                }

                return degrees;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        public static void Move(View view, TransformInfo info)
        {
            try
            {
                ComputeRenderOffset(view, info.PivotX, info.PivotY);
                AdjustTranslation(view, info.DeltaX, info.DeltaY);

                float scale = view.ScaleX * info.DeltaScale;
                scale = Math.Max(info.MinimumScale, Math.Min(info.MaximumScale, scale));
                view.ScaleX = scale;
                view.ScaleY = scale;

                float rotation = AdjustAngle(view.Rotation + info.DeltaAngle);
                view.Rotation = rotation;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static void AdjustTranslation(View view, float deltaX, float deltaY)
        {
            try
            {
                float[] deltaVector = { deltaX, deltaY };
                view.Matrix.MapVectors(deltaVector);
                view.TranslationX = view.TranslationX + deltaVector[0];
                view.TranslationY = view.TranslationY + deltaVector[1];
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static void ComputeRenderOffset(View view, float pivotX, float pivotY)
        {
            try
            {
                if (view.PivotX == pivotX && view.PivotY == pivotY)
                {
                    return;
                }

                float[] prevPoint = { 0.0f, 0.0f };
                view.Matrix.MapPoints(prevPoint);

                view.PivotX = pivotX;
                view.PivotY = pivotY;

                float[] currPoint = { 0.0f, 0.0f };
                view.Matrix.MapPoints(currPoint);

                float offsetX = currPoint[0] - prevPoint[0];
                float offsetY = currPoint[1] - prevPoint[1];

                view.TranslationX = view.TranslationX - offsetX;
                view.TranslationY = view.TranslationY - offsetY;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public bool OnTouch(View v, MotionEvent @event)
        {
            try
            {
                MScaleGestureDetector.OnTouchEvent(v, @event);
                MGestureListener.OnTouchEvent(@event);

                if (!IsTranslateEnabled)
                {
                    return true;
                }

                var action = @event.Action;

                int x = (int)@event.RawX;
                int y = (int)@event.RawY;

                switch (action & @event.ActionMasked)
                {
                    case MotionEventActions.Down:
                        MPrevX = @event.GetX();
                        MPrevY = @event.GetY();
                        MPrevRawX = @event.RawX;
                        MPrevRawY = @event.RawY;
                        MActivePointerId = @event.GetPointerId(0);
                        if (DeleteView != null)
                        {
                            DeleteView.Visibility = ViewStates.Visible;
                        }
                        v.BringToFront();
                        FireNiceArtEditorSdkListener(v, true);
                        break;

                    case MotionEventActions.Move:
                        int pointerIndexMove = @event.FindPointerIndex(MActivePointerId);
                        if (pointerIndexMove != -1)
                        {
                            float currX = @event.GetX(pointerIndexMove);
                            float currY = @event.GetY(pointerIndexMove);
                            if (!MScaleGestureDetector.IsInProgress())
                            {
                                AdjustTranslation(v, currX - MPrevX, currY - MPrevY);
                            }
                        }
                        break;

                    case MotionEventActions.Cancel:
                        MActivePointerId = InvalidPointerId;
                        break;

                    case MotionEventActions.Up:
                        MActivePointerId = InvalidPointerId;
                        if (DeleteView != null && IsViewInBounds(DeleteView, x, y))
                        {
                            OnMultiTouchListener?.OnRemoveViewListener(v);
                        }
                        else if (!IsViewInBounds(PhotoEditImageView, x, y))
                        {
                            //v.Animate().TranslationY(0).TranslationY(0);
                        }
                        if (DeleteView != null)
                        {
                            DeleteView.Visibility = ViewStates.Gone;
                        }
                        FireNiceArtEditorSdkListener(v, false);
                        break;

                    case MotionEventActions.PointerUp:

                        int pointerIndex = (int)(@event.Action & MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
                        int pointerId = @event.GetPointerId(pointerIndex);
                        if (pointerId == MActivePointerId)
                        {
                            int newPointerIndex = pointerId == 0 ? 1 : 0;
                            MPrevX = @event.GetX(newPointerIndex);
                            MPrevY = @event.GetY(newPointerIndex);
                            MActivePointerId = @event.GetPointerId(newPointerIndex);
                        }
                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public void FireNiceArtEditorSdkListener(View view, bool isStart)
        {
            try
            {
                if (view is TextView)
                {
                    if (OnMultiTouchListener != null)
                    {
                        if (MOnNiceArtEditorListener != null)
                        {
                            if (isStart)
                                MOnNiceArtEditorListener.OnStartViewChangeListener(ViewType.Text);
                            else
                                MOnNiceArtEditorListener.OnStopViewChangeListener(ViewType.Text);
                        }
                    }
                    else
                    {
                        if (MOnNiceArtEditorListener != null)
                        {
                            if (isStart)
                                MOnNiceArtEditorListener.OnStartViewChangeListener(ViewType.Emojis);
                            else
                                MOnNiceArtEditorListener.OnStopViewChangeListener(ViewType.Emojis);
                        }
                    }
                }
                else
                {
                    if (MOnNiceArtEditorListener != null)
                    {
                        if (isStart)
                            MOnNiceArtEditorListener.OnStartViewChangeListener(ViewType.Image);
                        else
                            MOnNiceArtEditorListener.OnStopViewChangeListener(ViewType.Image);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public bool IsViewInBounds(View view, int x, int y)
        {
            try
            {
                view.GetDrawingRect(OutRect);
                view.GetLocationOnScreen(Location);
                OutRect.Offset(Location[0], Location[1]);
                return OutRect.Contains(x, y);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public void SetOnMultiTouchListener(INiceArt.IOnMultiTouchListener onMultiTouchListener)
        {
            try
            {
                OnMultiTouchListener = onMultiTouchListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public class ScaleGestureListener : INiceArt.IOnScaleGestureListener
        {
            private float MPivotX;
            private float MPivotY;
            private readonly Vector2D MPrevSpanVector = new Vector2D();
             
            public bool OnScale(View view, ScaleGestureDetector detector)
            {
                try
                {
                    //Just Zoom >>
                    //=====================
                    //mScaleFactor *= detector.GetScaleFactor();
                    //mScaleFactor = Math.Max(0.1f, Math.Min(mScaleFactor, 10.0f));
                    //view.ScaleX = mScaleFactor;
                    //view.ScaleY = mScaleFactor;

                    //return true;

                    // Zoom and Rotate >>
                    //=====================
                    TransformInfo info = new TransformInfo();
                    info.DeltaScale = IsScaleEnabled ? detector.GetScaleFactor() : 1.0f;
                    info.DeltaAngle = IsRotateEnabled ? Vector2D.GetAngle(MPrevSpanVector, detector.GetCurrentSpanVector()) : 0.0f;
                    info.DeltaX = IsTranslateEnabled ? detector.GetFocusX() - MPivotX : 0.0f;
                    info.DeltaY = IsTranslateEnabled ? detector.GetFocusY() - MPivotY : 0.0f;
                    info.PivotX = MPivotX;
                    info.PivotY = MPivotY;
                    info.MinimumScale = MinimumScale;
                    info.MaximumScale = MaximumScale;
                    Move(view, info);
                    return !MIsTextPinchZoomable;

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;

                }
            }

            public bool OnScaleBegin(View view, ScaleGestureDetector detector)
            {
                try
                {
                    MPivotX = detector.GetFocusX();
                    MPivotY = detector.GetFocusY();
                    MPrevSpanVector.Set(detector.GetCurrentSpanVector());
                    return MIsTextPinchZoomable;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;

                }
            }

            public void OnScaleEnd(View view, ScaleGestureDetector detector)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class TransformInfo
        {
            public float DeltaX;
            public float DeltaY;
            public float DeltaScale;
            public float DeltaAngle;
            public float PivotX;
            public float PivotY;
            public float MinimumScale;
            public float MaximumScale;
        }

        public void SetOnGestureControl(INiceArt.IOnGestureControl onGestureControl, ViewType viewType)
        {
            try
            {
                MviewType = viewType;
                MOnGestureControl = onGestureControl;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            public override bool OnSingleTapUp(MotionEvent e)
            {
                try
                {
                    MOnGestureControl?.OnClick();
                    return true;
                }
                catch (Exception ex)
                {
                    Methods.DisplayReportResultTrack(ex);
                    return false;
                }
            }

            public override void OnLongPress(MotionEvent e)
            {
                try
                {
                    base.OnLongPress(e);
                    if (MviewType == ViewType.Text)
                    {
                        // mOnGestureControl?.OnLongClick(mviewType);
                    }
                }
                catch (Exception ex)
                {
                    Methods.DisplayReportResultTrack(ex);
                }
            }


            public override bool OnDown(MotionEvent e)
            {
                return true;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                return true;
            }

        }
    }
}