using Android.Content;
using Android.Runtime;
using System;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Library.Anjo.XRecordView
{
    public class RecordButton : AppCompatImageView, View.IOnTouchListener, View.IOnClickListener
    {
        private ScaleAnim ScaleAnim;
        private RecordView RecordView;
        private bool ListenForRecord = true;
        private IOnRecordClickListener OnRecordClickListener;

        public RecordButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public RecordButton(Context context) : base(context)
        {
            Init(context, null);
        }

        public RecordButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public RecordButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        private void Init(Context context, IAttributeSet attrs)
        {
            try
            {
                if (attrs != null)
                {
                    TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.RecordButton);

                    int imageResource = typedArray.GetResourceId(Resource.Styleable.RecordButton_mic_icon, -1);

                    if (imageResource != -1)
                    {
                        SetTheImageResource(imageResource);
                    }

                    typedArray.Recycle();
                }

                ScaleAnim = new ScaleAnim(this);

                SetOnTouchListener(this);
                SetOnClickListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void SetRecordView(RecordView recordView)
        {
            RecordView = recordView;
        }

        protected override void OnAttachedToWindow()
        {
            try
            {
                base.OnAttachedToWindow();
                SetClip(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void SetClip(View v)
        {
            try
            {
                if (v.Parent == null)
                {
                    return;
                }

                if (v is ViewGroup view)
                {
                    view.SetClipChildren(false);
                    view.SetClipToPadding(false);
                }

                if (v.Parent is View parent)
                {
                    SetClip(parent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        public void SetTheImageResource(int imageResource)
        {
            try
            {
                Drawable image = AppCompatResources.GetDrawable(Context, imageResource);
                SetImageDrawable(image);
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
                if (IsListenForRecord())
                {
                    switch (e.Action)
                    {

                        case MotionEventActions.Down:
                            RecordView.OnActionDown((RecordButton)v, e);
                            break;
                        case MotionEventActions.Move:
                            RecordView.OnActionMove((RecordButton)v, e);
                            break;
                        case MotionEventActions.Up:
                            RecordView.OnActionUp((RecordButton)v);
                            break;
                    }

                }
                return IsListenForRecord();

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return IsListenForRecord();
            }
        }

        public void StartScale()
        {
            try
            {
                ScaleAnim.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void StopScale()
        {
            try
            {
                ScaleAnim.Stop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void SetListenForRecord(bool listenForRecord)
        {
            ListenForRecord = listenForRecord;
        }

        public bool IsListenForRecord()
        {
            return ListenForRecord;
        }

        public void SetOnRecordClickListener(IOnRecordClickListener onRecordClickListener)
        {
            try
            {
                OnRecordClickListener = onRecordClickListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                OnRecordClickListener?.OnClick(v);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public interface IOnBasketAnimationEnd
    {
        void OnAnimationEnd();
    }
    public interface IOnRecordClickListener
    {
        void OnClick(View v);
    }

    public interface IOnRecordListener
    {
        void OnStartRecord();
        void OnCancelRecord();
        void OnFinishRecord(long recordTime);
        void OnLessThanSecond();
    }
}