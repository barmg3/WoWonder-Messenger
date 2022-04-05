using System;
using Android.Views;
using AndroidX.CustomView.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Library.Anjo.Stories.DragView
{
    public class DragHelperCallback : ViewDragHelper.Callback
    {
        private readonly DragToClose DragToClose;
        private readonly View DraggableContainer;

        private int LastDraggingState;
        private int TopBorderDraggableContainer;

        public DragHelperCallback(DragToClose dragToClose, View draggableContainer)
        {
            try
            {
                DragToClose = dragToClose;
                DraggableContainer = draggableContainer;
                LastDraggingState = ViewDragHelper.StateIdle;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Checks dragging states and notifies them.
        /// </summary>
        /// <param name="state"></param>
        public override void OnViewDragStateChanged(int state)
        {
            try
            {
                // If no state change, don't do anything
                if (state == LastDraggingState)
                {
                    return;
                }
                // If last state was dragging or settling and current state is idle,
                // the view has stopped moving. If the top border of the container is
                // equal to the vertical draggable range, the view has being dragged out,
                // so close activity is called
                if ((LastDraggingState == ViewDragHelper.StateDragging || LastDraggingState == ViewDragHelper.StateSettling) && state == ViewDragHelper.StateIdle && TopBorderDraggableContainer == DragToClose.GetDraggableRange())
                {
                    DragToClose.CloseActivity();
                }
                // If the view has just started being dragged, notify event
                if (state == ViewDragHelper.StateDragging)
                {
                    DragToClose.OnStartDraggingView();
                }
                // Save current state
                LastDraggingState = state;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnViewDragStateChanged(state);
            }
        }

        /// <summary>
        /// Registers draggable container position and changes the transparency of the container based on the vertical position while the view is being vertical dragged.
        /// </summary>
        /// <param name="changedView"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
        {
            try
            {
                TopBorderDraggableContainer = top;
                DragToClose.ChangeDragViewViewAlpha();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnViewPositionChanged(changedView, left, top, dx, dy);
            }
        }


        /// <summary>
        /// Handles the settling of the draggable view when it is released.
        /// Dragging speed is more important than the place the view is released.
        /// If the speed is greater than SPEED_THRESHOLD_TO_CLOSE the view is settled to closed.
        /// Else if the top
        /// </summary>
        /// <param name="releasedChild"></param>
        /// <param name="xVel"></param>
        /// <param name="yVel"></param>
        public override void OnViewReleased(View releasedChild, float xVel, float yVel)
        {
            base.OnViewReleased(releasedChild, xVel, yVel);
            try
            {
                // If view is in its original position or out of range, don't do anything
                if (TopBorderDraggableContainer == 0 || TopBorderDraggableContainer >= DragToClose.GetDraggableRange())
                {
                    return;
                }
                bool settleToClosed = false;
                // Check speed
                if (yVel > DragToClose.SpeedThresholdToClose)
                {
                    settleToClosed = true;
                }
                else
                {
                    // Check position
                    int verticalDraggableThreshold = (int)(DragToClose.GetDraggableRange() * DragToClose.HeightThresholdToClose);
                    if (TopBorderDraggableContainer > verticalDraggableThreshold)
                    {
                        settleToClosed = true;
                    }
                }
                // If settle to closed -> moved view out of the screen
                int settleDestY = settleToClosed ? DragToClose.GetDraggableRange() : 0;
                DragToClose.SmoothScrollToY(settleDestY);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                throw;
            }
        }

        /// <summary>
        /// Sets the vertical draggable range.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override int GetViewVerticalDragRange(View child)
        {
            try
            {
                return DragToClose.GetDraggableRange();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.GetViewVerticalDragRange(child);
            }
        }

        /// <summary>
        /// Configures which is going to be the draggable container.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="pointerId"></param>
        /// <returns></returns>
        public override bool TryCaptureView(View child, int pointerId)
        {
            return child.Equals(DraggableContainer);
        }

        /// <summary>
        /// Defines clamped position for left border.
        /// DragToClose padding must be taken into consideration.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="left"></param>
        /// <param name="dx"></param>
        /// <returns></returns>
        public override int ClampViewPositionHorizontal(View child, int left, int dx)
        {
            try
            {
                return child.Left;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.ClampViewPositionHorizontal(child, left, dx);
            }
        }

        /// <summary>
        /// Defines clamped position for top border.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="top"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public override int ClampViewPositionVertical(View child, int top, int dy)
        {
            try
            {
                int topBound = DragToClose.PaddingTop; // Top limit
                int bottomBound = DragToClose.GetDraggableRange(); // Bottom limit
                return Math.Min(Math.Max(top, topBound), bottomBound);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.ClampViewPositionVertical(child, top, dy);
            }
        }
    }
}