using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.CustomView.Widget;
using Java.Lang;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;
using Math = System.Math;

namespace WoWonder.Library.Anjo.Stories.DragView
{
    public class DragToClose : FrameLayout, View.IOnClickListener
    {
        // Sensitivity detecting the start of a drag (larger values are more sensitive)
        public static readonly float DragSensitivity = 1.0f;
        // If the view is dragged with a higher speed than the threshold, the view is
        // closed automatically
        public static readonly float SpeedThresholdToClose = 800.0f;
        // If dragging finishes below this threshold the view returns to its original position,
        // if the threshold is exceeded, the view is closed automatically
        public static readonly float HeightThresholdToClose = 0.5f;

        // Attributes
        private int DraggableContainerId;
        private int DraggableViewId;
        private bool FinishActivity;
        private bool CloseOnClick;

        private View DraggableContainer;
        private View DraggableView;
        private int DraggableContainerTop;
        private int DraggableContainerLeft;

        private ViewDragHelper DragHelper;
        private IDragListener Listener;

        private int VerticalDraggableRange;

        public interface IDragListener
        {
            //Invoked when the view has just started to be dragged.
            void OnStartDraggingView();
            //Invoked when the view has  dragging.
            void OnDraggingView(float offset);
            //Invoked when the view has being dragged out of the screen and just before calling activity.finish().
            void OnViewClosed();
        }

        protected DragToClose(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public DragToClose(Context context) : base(context)
        {
            Init(context, null);
        }

        public DragToClose(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public DragToClose(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        public DragToClose(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs);
        }

        /// <summary>
        /// Initializes XML attributes.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attrs"></param>
        private void Init(Context context, IAttributeSet attrs)
        {
            TypedArray array = null!;
            try
            {
                if (attrs != null)
                {
                    array = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.DragToClose, 0, 0);

                    DraggableViewId = array.GetResourceId(Resource.Styleable.DragToClose_draggableView, -1);
                    DraggableContainerId = array.GetResourceId(Resource.Styleable.DragToClose_draggableContainer, -1);
                    FinishActivity = array.GetBoolean(Resource.Styleable.DragToClose_finishActivity, true);
                    CloseOnClick = array.GetBoolean(Resource.Styleable.DragToClose_closeOnClick, false);
                    if (DraggableViewId == -1 || DraggableContainerId == -1)
                    {
                        throw new IllegalArgumentException("draggableView and draggableContainer attributes are required.");
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            finally
            {
                array?.Recycle();
            }
        }

        /// <summary>
        /// Configures draggable view and initializes DragViewHelper.
        /// </summary>
        protected override void OnFinishInflate()
        {
            try
            {
                base.OnFinishInflate();
                InitViews();
                InitViewDragHelper();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Gets the height of the DragToClose view and configures the vertical draggable threshold base on it.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="oldw"></param>
        /// <param name="oldh"></param>
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            try
            {
                base.OnSizeChanged(w, h, oldw, oldh);
                VerticalDraggableRange = h;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Intercepts only touch events over the draggable view.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            try
            {
                bool handled = false;
                if (Enabled)
                {
                    handled = DragHelper.ShouldInterceptTouchEvent(ev)
                        && DragHelper.IsViewUnder(DraggableView, (int)ev.GetX(), (int)ev.GetY());
                }
                else
                {
                    DragHelper.Cancel();
                }
                return handled || base.OnInterceptTouchEvent(ev);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.OnInterceptTouchEvent(ev);
            }
        }

        /// <summary>
        /// Dispatches touch event to the draggable view.
        /// The touch is realized only if is over the draggable view.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnTouchEvent(MotionEvent e)
        {
            try
            {
                DragHelper.ProcessTouchEvent(e);
                return IsViewTouched(DraggableView, (int)e.GetX(), (int)e.GetY());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return base.OnTouchEvent(e);
            }
        }

        public override void ComputeScroll()
        {
            try
            {
                if (DragHelper.ContinueSettling(true))
                {
                    ViewCompat.PostInvalidateOnAnimation(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        /// <summary>
        /// Returns the draggable view id.
        /// </summary>
        /// <returns></returns>
        public int GetDraggableViewId()
        {
            return DraggableViewId;
        }

        /// <summary>
        /// Sets the draggable view id.
        /// </summary>
        /// <param name="draggableViewId"></param>
        public void SetDraggableViewId(int draggableViewId)
        {
            try
            {
                DraggableViewId = draggableViewId;
                Invalidate();
                RequestLayout();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Returns the draggable container id.
        /// </summary>
        /// <returns></returns>
        public int GetDraggableContainerId()
        {
            return DraggableContainerId;
        }

        /// <summary>
        /// Sets the draggable container id.
        /// </summary>
        /// <param name="draggableContainerId"></param>
        public void SetDraggableContainerId(int draggableContainerId)
        {
            try
            {
                DraggableContainerId = draggableContainerId;
                Invalidate();
                RequestLayout();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Checks whether finish activity is activated or not.
        /// </summary>
        /// <returns></returns>
        public bool IsFinishActivity()
        {
            return FinishActivity;
        }

        /// <summary>
        /// Sets finish activity attribute. If true, the activity is closed when the view is dragged out. Default: true.
        /// </summary>
        /// <param name="finishActivity"></param>
        public void SetFinishActivity(bool finishActivity)
        {
            FinishActivity = finishActivity;
        }

        /// <summary>
        /// Checks whether close on click is activated or not.
        /// </summary>
        /// <returns></returns>
        public bool IsCloseOnClick()
        {
            return CloseOnClick;
        }

        /// <summary>
        /// Sets close on click attribute. If true, the draggable container is slid down when the draggable view is clicked.
        /// Default: false.
        /// </summary>
        /// <param name="closeOnClick"></param>
        public void SetCloseOnClick(bool closeOnClick)
        {
            try
            {
                if (closeOnClick)
                {
                    initOnClickListener(DraggableView);
                }
                else
                {
                    DraggableView.SetOnClickListener(null);
                }
                CloseOnClick = closeOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        ///  Sets drag listener.
        /// </summary>
        /// <param name="listener"></param>
        public void SetDragListener(IDragListener listener)
        {
            Listener = listener;
        }

        /// <summary>
        /// Slides down draggable container out of the DragToClose view.
        /// </summary>
        public void CloseDraggableContainer()
        {
            SlideViewTo(DraggableContainer, PaddingLeft + DraggableContainerLeft, GetDraggableRange());
        }

        /// <summary>
        /// Slides up draggable container to its original position.
        /// </summary>
        public void OpenDraggableContainer()
        {
            SlideViewTo(DraggableContainer, PaddingLeft + DraggableContainerLeft, PaddingTop + DraggableContainerTop);
        }

        /// <summary>
        /// Invoked when the view has just started to be dragged.
        /// </summary>
        public void OnStartDraggingView()
        {
            try
            {
                Listener?.OnStartDraggingView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Returns the draggable range.
        /// </summary>
        /// <returns></returns>
        public int GetDraggableRange()
        {
            return VerticalDraggableRange;
        }

        /// <summary>
        /// Notifies the listener that the view has been closed and finishes the activity (if need be).
        /// </summary>
        public void CloseActivity()
        {
            try
            {
                Listener?.OnViewClosed();

                if (FinishActivity)
                {
                    Activity activity = (Activity)Context;
                    activity?.Finish();
                    activity?.OverridePendingTransition(0, Resource.Animation.fade_out);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Modify dragged view alpha based on the vertical position while the view is being vertical dragged.
        /// </summary>
        public void ChangeDragViewViewAlpha()
        {
            try
            {
                //  draggableContainer.setAlpha(1 - getVerticalDragOffset());
                Alpha = 1 - GetVerticalDragOffset();
                Listener?.OnDraggingView(1 - GetVerticalDragOffset());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Drags the draggable container to given position.
        /// </summary>
        /// <param name="settleDestY"></param>
        public void SmoothScrollToY(int settleDestY)
        {
            try
            {
                if (DragHelper.SettleCapturedViewAt(PaddingLeft, settleDestY))
                {
                    ViewCompat.PostInvalidateOnAnimation(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        /// <summary>
        /// Initializes views.
        /// </summary>
        private void InitViews()
        {
            try
            {
                DraggableContainer = FindViewById<View>(DraggableContainerId);
                if (DraggableContainer == null)
                {
                    throw new IllegalArgumentException("draggableContainer not found!");
                }
                DraggableContainerTop = DraggableContainer.Top;
                DraggableContainerLeft = DraggableContainer.Left;
                DraggableView = FindViewById<View>(DraggableViewId);
                if (DraggableView == null)
                {
                    throw new IllegalArgumentException("draggableView not found!");
                }
                if (CloseOnClick)
                {
                    initOnClickListener(DraggableView);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Initializes on OnClickListener (if need be).
        /// </summary>
        /// <param name="clickableView"></param>
        private void initOnClickListener(View clickableView)
        {
            try
            {
                clickableView.SetOnClickListener(this);
            }
            catch (Exception a)
            {
                Methods.DisplayReportResultTrack(a);
            }
        }
        public void OnClick(View v)
        {
            try
            {
                CloseDraggableContainer();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Initializes ViewDragHelper.
        /// </summary>
        private void InitViewDragHelper()
        {
            try
            {
                DragHelper = ViewDragHelper.Create(this, DragSensitivity, new DragHelperCallback(this, DraggableContainer));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Determines if position (x, y) is below given view.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsViewTouched(View view, int x, int y)
        {
            try
            {
                int[] viewLocation = new int[2];
                view.GetLocationOnScreen(viewLocation);
                int[] parentLocation = new int[2];
                GetLocationOnScreen(parentLocation);
                int screenX = parentLocation[0] + x;
                int screenY = parentLocation[1] + y;
                return screenX >= viewLocation[0]
                       && screenX < viewLocation[0] + view.Width
                       && screenY >= viewLocation[1] && screenY < viewLocation[1] + view.Height;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }


        /// <summary>
        /// Calculate the dragged view top position normalized between 1 and 0.
        /// </summary>
        /// <returns></returns>
        private float GetVerticalDragOffset()
        {
            return (float)Math.Abs(DraggableContainer.Top) / (float)Height;
        }

        /// <summary>
        /// Slides down a view.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        private void SlideViewTo(View view, int left, int top)
        {
            try
            {
                DragHelper.SmoothSlideViewTo(view, left, top);
                Invalidate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


    }
}