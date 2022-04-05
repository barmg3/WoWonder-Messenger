using System;
using Android.Views;
using Java.Lang;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;
using Exception = Java.Lang.Exception;
using Math = Java.Lang.Math;

namespace WoWonder.NiceArt
{
    public class ScaleGestureDetector
    {
        public static readonly string Tag = "ScaleGestureDetector";

        // This value is the threshold ratio between our previous combined pressure
        // and the current combined pressure.We will only fire an onScale Event if
        // the computed ratio between the current and previous Event pressures is
        // greater than this value. When pressure decreases rapidly between Events
        // the position values can often be imprecise, as it usually indicates
        // that the user is in the process of lifting a pointer off of the device.
        // Its value was tuned experimentally.

        public static readonly float PressureThreshold = 0.67f;
        public readonly INiceArt.IOnScaleGestureListener MListener;
        public bool MGestureInProgress;
        public MotionEvent MPrevEvent;
        public MotionEvent MCurrEvent;
        public Vector2D MCurrSpanVector;
        public float MFocusX;
        public float MFocusY;
        public float MPrevFingerDiffX;
        public float MPrevFingerDiffY;
        public float MCurrFingerDiffX;
        public float MCurrFingerDiffY;
        public float MCurrLen;
        public float MPrevLen;
        public float MScaleFactor;
        public float MCurrPressure;
        public float MPrevPressure;
        public long MTimeDelta;
        public bool MInvalidGesture;
        // Pointer IDs currently responsible for the two fingers controlling the gesture
        public int MActiveId0;
        public int MActiveId1;
        public bool MActive0MostRecent;

        public ScaleGestureDetector(INiceArt.IOnScaleGestureListener listener)
        {
            try
            {
                MListener = listener;
                MCurrSpanVector = new Vector2D();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public bool OnTouchEvent(View view, MotionEvent Event)
        {
            try
            {
                var action = Event.ActionMasked;
                if (action == MotionEventActions.Down)
                {
                    Reset(); // Start fresh
                }

                bool handled = true;
                if (MInvalidGesture)
                {
                    handled = false;
                }
                else if (!MGestureInProgress)
                {
                    switch (action)
                    {
                        case MotionEventActions.Down:
                            {
                                MActiveId0 = Event.GetPointerId(0);
                                MActive0MostRecent = true;
                            }
                            break;

                        case MotionEventActions.Up:
                            Reset();
                            break;

                        case MotionEventActions.PointerDown:
                            {
                                // We have a new multi-finger gesture
                                if (MPrevEvent != null) MPrevEvent.Recycle();
                                MPrevEvent = MotionEvent.Obtain(Event);
                                MTimeDelta = 0;

                                int index1 = Event.ActionIndex;
                                int index0 = Event.FindPointerIndex(MActiveId0);
                                MActiveId1 = Event.GetPointerId(index1);
                                if (index0 < 0 || index0 == index1)
                                {
                                    // Probably someone sending us a broken Event stream.
                                    index0 = FindNewActiveIndex(Event, MActiveId1, -1);
                                    MActiveId0 = Event.GetPointerId(index0);
                                }

                                MActive0MostRecent = false;

                                SetContext(view, Event);

                                MGestureInProgress = MListener.OnScaleBegin(view, this);
                                break;
                            }
                    }
                }
                else
                {
                    // Transform gesture in progress - attempt to handle it
                    switch (action)
                    {
                        case MotionEventActions.PointerDown:
                            {
                                // End the old gesture and begin a new one with the most recent two fingers.
                                MListener.OnScaleEnd(view, this);
                                int oldActive0 = MActiveId0;
                                int oldActive1 = MActiveId1;
                                Reset();

                                MPrevEvent = MotionEvent.Obtain(Event);
                                MActiveId0 = MActive0MostRecent ? oldActive0 : oldActive1;
                                MActiveId1 = Event.GetPointerId(Event.ActionIndex);
                                MActive0MostRecent = false;

                                int index0 = Event.FindPointerIndex(MActiveId0);
                                if (index0 < 0 || MActiveId0 == MActiveId1)
                                {
                                    // Probably someone sending us a broken Event stream.
                                    index0 = FindNewActiveIndex(Event, MActiveId1, -1);
                                    MActiveId0 = Event.GetPointerId(index0);
                                }

                                SetContext(view, Event);

                                MGestureInProgress = MListener.OnScaleBegin(view, this);
                            }
                            break;

                        case MotionEventActions.PointerUp:
                            {
                                int pointerCount = Event.PointerCount;
                                int actionIndex = Event.ActionIndex;
                                int actionId = Event.GetPointerId(actionIndex);

                                bool gestureEnded = false;
                                if (pointerCount > 2)
                                {
                                    if (actionId == MActiveId0)
                                    {
                                        int newIndex = FindNewActiveIndex(Event, MActiveId1, actionIndex);
                                        if (newIndex >= 0)
                                        {
                                            MListener.OnScaleEnd(view, this);
                                            MActiveId0 = Event.GetPointerId(newIndex);
                                            MActive0MostRecent = true;
                                            MPrevEvent = MotionEvent.Obtain(Event);
                                            SetContext(view, Event);
                                            MGestureInProgress = MListener.OnScaleBegin(view, this);
                                        }
                                        else
                                        {
                                            gestureEnded = true;
                                        }
                                    }
                                    else if (actionId == MActiveId1)
                                    {
                                        int newIndex = FindNewActiveIndex(Event, MActiveId0, actionIndex);
                                        if (newIndex >= 0)
                                        {
                                            MListener.OnScaleEnd(view, this);
                                            MActiveId1 = Event.GetPointerId(newIndex);
                                            MActive0MostRecent = false;
                                            MPrevEvent = MotionEvent.Obtain(Event);
                                            SetContext(view, Event);
                                            MGestureInProgress = MListener.OnScaleBegin(view, this);
                                        }
                                        else
                                        {
                                            gestureEnded = true;
                                        }
                                    }

                                    MPrevEvent.Recycle();
                                    MPrevEvent = MotionEvent.Obtain(Event);
                                    SetContext(view, Event);
                                }
                                else
                                {
                                    gestureEnded = true;
                                }

                                if (gestureEnded)
                                {
                                    // Gesture ended
                                    SetContext(view, Event);

                                    // Set focus point to the remaining finger
                                    int activeId = actionId == MActiveId0 ? MActiveId1 : MActiveId0;
                                    int index = Event.FindPointerIndex(activeId);
                                    MFocusX = Event.GetX(index);
                                    MFocusY = Event.GetY(index);

                                    MListener.OnScaleEnd(view, this);
                                    Reset();
                                    MActiveId0 = activeId;
                                    MActive0MostRecent = true;
                                }
                            }
                            break;

                        case MotionEventActions.Cancel:
                            MListener.OnScaleEnd(view, this);
                            Reset();
                            break;

                        case MotionEventActions.Up:
                            Reset();
                            break;

                        case MotionEventActions.Move:
                            {
                                SetContext(view, Event);

                                // Only accept the Event if our relative pressure is within
                                // a certain limit - this can help filter shaky data as a
                                // finger is lifted.
                                if (MCurrPressure / MPrevPressure > PressureThreshold)
                                {
                                    bool updatePrevious = MListener.OnScale(view, this);

                                    if (updatePrevious)
                                    {
                                        MPrevEvent.Recycle();
                                        MPrevEvent = MotionEvent.Obtain(Event);
                                    }
                                }
                            }
                            break;
                    }
                }

                return handled;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }


        public int FindNewActiveIndex(MotionEvent ev, int otherActiveId, int removedPointerIndex)
        {
            try
            {
                int pointerCount = ev.PointerCount;

                // It's ok if this isn't found and returns -1, it simply won't match.
                int otherActiveIndex = ev.FindPointerIndex(otherActiveId);

                // Pick a new id and update tracking state.
                for (int i = 0; i < pointerCount; i++)
                {
                    if (i != removedPointerIndex && i != otherActiveIndex)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }


        public void SetContext(View view, MotionEvent curr)
        {
            try
            {
                if (MCurrEvent != null)
                {
                    MCurrEvent.Recycle();
                }

                MCurrEvent = MotionEvent.Obtain(curr);

                MCurrLen = -1;
                MPrevLen = -1;
                MScaleFactor = -1;
                MCurrSpanVector.Set(0.0f, 0.0f);

                MotionEvent prev = MPrevEvent;

                int prevIndex0 = prev.FindPointerIndex(MActiveId0);
                int prevIndex1 = prev.FindPointerIndex(MActiveId1);
                int currIndex0 = curr.FindPointerIndex(MActiveId0);
                int currIndex1 = curr.FindPointerIndex(MActiveId1);

                if (prevIndex0 < 0 || prevIndex1 < 0 || currIndex0 < 0 || currIndex1 < 0)
                {
                    MInvalidGesture = true;
                    Console.WriteLine(Tag, "Invalid MotionEvent stream detected.", new Throwable());
                    if (MGestureInProgress)
                    {
                        MListener.OnScaleEnd(view, this);
                    }

                    return;
                }

                float px0 = prev.GetX(prevIndex0);
                float py0 = prev.GetY(prevIndex0);
                float px1 = prev.GetX(prevIndex1);
                float py1 = prev.GetY(prevIndex1);
                float cx0 = curr.GetX(currIndex0);
                float cy0 = curr.GetY(currIndex0);
                float cx1 = curr.GetX(currIndex1);
                float cy1 = curr.GetY(currIndex1);

                float pvx = px1 - px0;
                float pvy = py1 - py0;
                float cvx = cx1 - cx0;
                float cvy = cy1 - cy0;

                MCurrSpanVector.Set(cvx, cvy);

                MPrevFingerDiffX = pvx;
                MPrevFingerDiffY = pvy;
                MCurrFingerDiffX = cvx;
                MCurrFingerDiffY = cvy;

                MFocusX = cx0 + cvx * 0.5f;
                MFocusY = cy0 + cvy * 0.5f;
                MTimeDelta = curr.EventTime - prev.EventTime;

                MCurrPressure = curr.GetPressure(currIndex0) + curr.GetPressure(currIndex1);
                MPrevPressure = prev.GetPressure(prevIndex0) + prev.GetPressure(prevIndex1);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        public void Reset()
        {
            try
            {
                if (MPrevEvent != null)
                {
                    MPrevEvent.Recycle();
                    MPrevEvent = null!;
                }

                if (MCurrEvent != null)
                {
                    MCurrEvent.Recycle();
                    MCurrEvent = null!;
                }

                MGestureInProgress = false;
                MActiveId0 = -1;
                MActiveId1 = -1;
                MInvalidGesture = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Returns {@code true} if a two-finger scale gesture is in progress.
        /// </summary>
        /// <returns>{@code true} if a scale gesture is in progress, {@code false} otherwise.</returns>
        public bool IsInProgress()
        {
            try
            {
                return MGestureInProgress;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }


        /// <summary>
        /// Get the X coordinate of the current gesture's focal point.
        /// If a gesture is in progress, the focal point is directly between
        /// Get the X coordinate of the current gesture's focal point.
        /// If a gesture is in progress, the focal point is directly between
        /// the two pointers forming the gesture.
        /// If a gesture is ending, the focal point is the location of the
        /// remaining pointer on the screen.
        /// If {@link #isInProgress()} would return false, the result of this
        /// function is undefined.
        /// </summary>
        /// <returns>X coordinate of the focal point in pixels.</returns>
        public float GetFocusX()
        {
            try
            {
                return MFocusX;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        /// <summary>
        /// Get the Y coordinate of the current gesture's focal point.
        /// If a gesture is in progress, the focal point is directly between
        /// Get the Y coordinate of the current gesture's focal point.
        /// If a gesture is in progress, the focal point is directly between
        /// the two pointers forming the gesture.
        /// If a gesture is ending, the focal point is the location of the
        /// remaining pointer on the screen.
        /// If {@link #isInProgress()} would return false, the result of this
        /// function is undefined.
        /// </summary>
        /// <returns>Y coordinate of the focal point in pixels.</returns>
        /// 
        public float GetFocusY()
        {
            try
            {
                return MFocusY;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        public float GetCurrentSpan()
        {
            try
            {
                if (MCurrLen == -1)
                {
                    float cvx = MCurrFingerDiffX;
                    float cvy = MCurrFingerDiffY;
                    MCurrLen = (float)Math.Sqrt(cvx * cvx + cvy * cvy);
                }

                return MCurrLen;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        public Vector2D GetCurrentSpanVector()
        {
            try
            {
                return MCurrSpanVector;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;

            }
        }


        /// <summary>
        /// Return the current x distance between the two pointers forming the
        /// gesture in progress.
        /// </summary>
        /// <returns>Distance between pointers in pixels.</returns>
        public float GetCurrentSpanX()
        {
            try
            {
                return MCurrFingerDiffX;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        /// <summary>
        /// Return the current y distance between the two pointers forming the
        /// gesture in progress.
        /// </summary>
        /// <returns>Distance between pointers in pixels.</returns>
        public float GetCurrentSpanY()
        {
            try
            {
                return MCurrFingerDiffY;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        /// <summary>
        /// Return the previous distance between the two pointers forming the
        /// gesture in progress.
        /// </summary>
        /// <returns>Previous distance between pointers in pixels.</returns>
        public float GetPreviousSpan()
        {
            try
            {
                if (MPrevLen == -1)
                {
                    float pvx = MPrevFingerDiffX;
                    float pvy = MPrevFingerDiffY;
                    MPrevLen = (float)Math.Sqrt(pvx * pvx + pvy * pvy);
                }

                return MPrevLen;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }


        /// <summary>
        /// Return the previous x distance between the two pointers forming the
        /// gesture in progress.
        /// </summary>
        /// <returns>Previous distance between pointers in pixels.</returns>
        public float GetPreviousSpanX()
        {
            try
            {
                return MPrevFingerDiffX;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }


        /// <summary>
        /// Return the previous y distance between the two pointers forming the
        /// gesture in progress.
        /// </summary>
        /// <returns>Previous distance between pointers in pixels.</returns>
        public float GetPreviousSpanY()
        {
            try
            {
                return MPrevFingerDiffY;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        /// <summary>
        /// Return the scaling factor from the previous scale event to the current
        /// ({@link #getCurrentSpan()} / {@link #getPreviousSpan()}).
        /// </summary>
        /// <returns>The current scaling factor.</returns>
        public float GetScaleFactor()
        {
            try
            {
                if (MScaleFactor == -1)
                {
                    MScaleFactor = GetCurrentSpan() / GetPreviousSpan();
                }

                return MScaleFactor;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        /// <summary>
        /// Return the time difference in milliseconds between the previous
        /// accepted scaling event and the current scaling event.
        /// </summary>
        /// <returns>Time difference since the last scaling event in milliseconds</returns>
        public long GetTimeDelta()
        {
            try
            {
                return MTimeDelta;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }


        /// <summary>
        ///  Return the event time of the current event being processed
        /// </summary>
        /// <returns>Current event time in milliseconds</returns>
        public long GetEventTime()
        {
            try
            {
                return MCurrEvent.EventTime;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }
    }
}