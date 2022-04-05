using Android.Content;
using Android.Views;
using System;

namespace WoWonder.Helpers.Utils
{
    public class ViewSwipeTouchListener : Java.Lang.Object, View.IOnTouchListener
    {

        private readonly GestureDetector GestureDetector;
        private readonly Context Context;
        private readonly IOnSwipeListener OnSwipe;
        private readonly View MainView;
        public ViewSwipeTouchListener(Context ctx, View mainView, IOnSwipeListener listener)
        {
            try
            {
                GestureDetector = new GestureDetector(ctx, new GestureListener(mainView, listener));
                mainView?.SetOnTouchListener(this);
                Context = ctx;
                OnSwipe = listener;
                MainView = mainView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return GestureDetector.OnTouchEvent(e);
        }

        public interface IOnSwipeListener
        {
            void Swipe(View v, SwipeType type);
        }

        public enum SwipeType
        {
            Right,
            Top,
            Bottom,
            Left,
        }

        private class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            private static readonly int SwipeThreshold = 100;
            private static readonly int SwipeVelocityThreshold = 100;
            private readonly IOnSwipeListener OnSwipe;
            private readonly View MainView;

            public GestureListener(View v, IOnSwipeListener listener)
            {
                MainView = v;
                OnSwipe = listener;
            }

            public override bool OnDown(MotionEvent e)
            {
                return true;
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                try
                {
                    bool result = false;
                    float diffY = e2?.GetY() - e1?.GetY() ?? 0;
                    float diffX = e2?.GetX() - e1?.GetX() ?? 0;
                    if (Math.Abs(diffX) > Math.Abs(diffY))
                    {
                        if (Math.Abs(diffX) > SwipeThreshold && Math.Abs(velocityX) > SwipeVelocityThreshold)
                        {
                            if (diffX > 0)
                            {
                                //Swiped Right
                                OnSwipe?.Swipe(MainView, SwipeType.Right);
                            }
                            else
                            {
                                //Swiped Left
                                OnSwipe?.Swipe(MainView, SwipeType.Left);
                            }
                            result = true;
                        }
                    }
                    else if (Math.Abs(diffY) > SwipeThreshold && Math.Abs(velocityY) > SwipeVelocityThreshold)
                    {
                        if (diffY > 0)
                        {
                            //Swiped Down
                            OnSwipe?.Swipe(MainView, SwipeType.Bottom);
                        }
                        else
                        {
                            //Swiped Up
                            OnSwipe?.Swipe(MainView, SwipeType.Top);
                        }
                        result = true;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return base.OnFling(e1, e2, velocityX, velocityY);
                }
            }
        }
    }
}