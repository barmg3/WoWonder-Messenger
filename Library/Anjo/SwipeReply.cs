using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.Graphics.Drawables;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;
using Math = System.Math;

namespace WoWonder.Library.Anjo
{
    public class SwipeReply : ItemTouchHelper.Callback
    {
        private Drawable ImageDrawable;
        private Drawable ShareRound;
        private RecyclerView.ViewHolder CurrentItemViewHolder;
        private View MView;
        private float DX = 0f;
        private float ReplyButtonProgress = 0f;
        private long LastReplyButtonAnimationTime = 0;
        private bool SwipeBack = false;
        private bool IsVibrate = false;
        private bool StartTracking = false;
        private float Density;
        private readonly Context Context;
        private readonly ISwipeControllerActions SwipeControllerActions;

        public interface ISwipeControllerActions
        {
            void ShowReplyUi(int position);
        }

        public SwipeReply(Context context, ISwipeControllerActions swipeControllerActions)
        {
            try
            {
                Context = context;
                SwipeControllerActions = swipeControllerActions;
                Density = 1.0F;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            MView = viewHolder.ItemView;
            ImageDrawable = Context.GetDrawable(Resource.Drawable.icon_chat_reply);
            ShareRound = Context.GetDrawable(Resource.Drawable.recv_bg_mic);
            return MakeMovementFlags(ItemTouchHelper.ActionStateIdle, ItemTouchHelper.Right);
        }

        public override bool OnMove(RecyclerView p0, RecyclerView.ViewHolder p1, RecyclerView.ViewHolder p2)
        {
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder p0, int p1)
        {

        }

        public override int ConvertToAbsoluteDirection(int flags, int layoutDirection)
        {
            try
            {
                if (SwipeBack)
                {
                    SwipeBack = false;
                    return 0;
                }
                return base.ConvertToAbsoluteDirection(flags, layoutDirection);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.ConvertToAbsoluteDirection(flags, layoutDirection);
            }
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            try
            {
                if (actionState == ItemTouchHelper.ActionStateSwipe)
                {
                    SetTouchListener(recyclerView, viewHolder);
                }

                if (MView.TranslationX < ConvertTodp(130) || dX < DX)
                {
                    base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                    DX = dX;
                    StartTracking = true;
                }
                CurrentItemViewHolder = viewHolder;
                DrawReplyButton(c);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetTouchListener(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            try
            {
                recyclerView.SetOnTouchListener(new MyOnTouchListener(this, viewHolder));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyOnTouchListener : Java.Lang.Object, View.IOnTouchListener
        {
            private readonly SwipeReply SwipeReply;
            private readonly RecyclerView.ViewHolder ViewHolder;
            public MyOnTouchListener(SwipeReply reply, RecyclerView.ViewHolder viewHolder)
            {
                try
                {
                    SwipeReply = reply;
                    ViewHolder = viewHolder;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                try
                {
                    SwipeReply.SwipeBack = e.Action == MotionEventActions.Cancel || e.Action == MotionEventActions.Up;
                    if (SwipeReply.SwipeBack)
                    {
                        if (Math.Abs(SwipeReply.MView.TranslationX) >= SwipeReply.ConvertTodp(100))
                        {
                            SwipeReply.SwipeControllerActions.ShowReplyUi(ViewHolder.BindingAdapterPosition);
                        }
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                    return false;
                }
            }
        }

        private void DrawReplyButton(Canvas canvas)
        {
            try
            {
                if (CurrentItemViewHolder == null)
                {
                    return;
                }
                float translationX = MView.TranslationX;
                long newTime = Methods.Time.CurrentTimeMillis();
                long dt = Math.Min(17, newTime - LastReplyButtonAnimationTime);
                LastReplyButtonAnimationTime = newTime;
                bool showing = translationX >= ConvertTodp(30);
                if (showing)
                {
                    if (ReplyButtonProgress < 1.0f)
                    {
                        ReplyButtonProgress += dt / 180.0f;
                        if (ReplyButtonProgress > 1.0f)
                        {
                            ReplyButtonProgress = 1.0f;
                        }
                        else
                        {
                            MView.Invalidate();
                        }
                    }
                }
                else if (translationX <= 0.0f)
                {
                    ReplyButtonProgress = 0f;
                    StartTracking = false;
                    IsVibrate = false;
                }
                else
                {
                    if (ReplyButtonProgress > 0.0f)
                    {
                        ReplyButtonProgress -= dt / 180.0f;
                        if (ReplyButtonProgress < 0.1f)
                        {
                            ReplyButtonProgress = 0f;
                        }
                        else
                        {
                            MView.Invalidate();
                        }
                    }
                }
                int alpha;
                float scale;
                if (showing)
                {
                    scale = ReplyButtonProgress <= 0.8F ? 1.2F * (ReplyButtonProgress / 0.8F) : 1.2F - 0.2F * ((ReplyButtonProgress - 0.8F) / 0.2F);
                    alpha = (int)Math.Min(255.0F, (float)255 * (ReplyButtonProgress / 0.8F));
                }
                else
                {
                    scale = ReplyButtonProgress;
                    alpha = (int)Math.Min(255.0F, (float)255 * ReplyButtonProgress);
                }
                ShareRound.SetAlpha(alpha);
                ImageDrawable.SetAlpha(alpha);
                if (StartTracking)
                {
                    if (!IsVibrate && MView.TranslationX >= ConvertTodp(100))
                    {
                        MView.PerformHapticFeedback(FeedbackConstants.KeyboardTap, FeedbackFlags.IgnoreGlobalSetting);
                        IsVibrate = true;
                    }
                }

                int x;
                if (MView.TranslationX > (float)ConvertTodp(130))
                {
                    x = ConvertTodp(130) / 2;
                }
                else
                {
                    x = (int)(MView.TranslationX / (float)2);
                }

                float y;
                y = (float)(MView.Top + MView.MeasuredHeight / 2);
                ShareRound.SetTint(Color.ParseColor(AppSettings.MainColor));
                ShareRound.SetBounds((int)((float)x - (float)ConvertTodp(18) * scale), (int)(y - (float)ConvertTodp(18) * scale), (int)((float)x + (float)ConvertTodp(18) * scale), (int)(y + (float)ConvertTodp(18) * scale));

                ShareRound.Draw(canvas);
                ImageDrawable.SetBounds((int)((float)x - (float)ConvertTodp(12) * scale), (int)(y - (float)ConvertTodp(11) * scale), (int)((float)x + (float)ConvertTodp(12) * scale), (int)(y + (float)ConvertTodp(10) * scale));

                ImageDrawable.Draw(canvas);
                ShareRound.SetAlpha(255);
                ImageDrawable.SetAlpha(255);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private int ConvertTodp(int pixel)
        {
            return Dp((float)pixel, Context);
        }

        public int Dp(float value, Context context)
        {
            if (Density == 1.0F)
            {
                CheckDisplaySize(context);
            }

            return value == 0.0F ? 0 : (int)Math.Ceiling((double)(Density * value));
        }

        private void CheckDisplaySize(Context context)
        {
            try
            {
                Density = context.Resources.DisplayMetrics.Density;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}