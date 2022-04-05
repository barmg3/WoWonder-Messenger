using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Views.Animations;
using AndroidX.VectorDrawable.Graphics.Drawable;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Library.Anjo.XRecordView
{
    public class AnimationHelper
    {
        private readonly Context Context;
        private readonly AnimatedVectorDrawableCompat AnimatedVectorDrawable;
        private readonly ImageView BasketImg;
        private readonly ImageView SmallBlinkingMic;
        private AlphaAnimation AlphaAnimation;
        private IOnBasketAnimationEnd OnBasketAnimationEndListener;
        private bool IsBasketAnimating, IsStartRecorded;
        private float MicX, MicY;
        private AnimatorSet MicAnimation;
        private TranslateAnimation TranslateAnimation1, TranslateAnimation2;
        private Handler Handler1, Handler2;

        public AnimationHelper(Context context, ImageView basketImg, ImageView smallBlinkingMic)
        {
            try
            {
                Context = context;
                SmallBlinkingMic = smallBlinkingMic;
                BasketImg = basketImg;
                AnimatedVectorDrawable = AnimatedVectorDrawableCompat.Create(context, Resource.Drawable.recv_basket_animated);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AnimateBasket(float basketInitialY)
        {
            try
            {
                IsBasketAnimating = true;

                ClearAlphaAnimation(false);

                //save initial x,y values for mic icon
                if (MicX == 0)
                {
                    MicX = SmallBlinkingMic.GetX();
                    MicY = SmallBlinkingMic.GetY();
                }

                MicAnimation = (AnimatorSet)AnimatorInflaterCompat.LoadAnimator(Context, Resource.Animator.delete_mic_animation);
                MicAnimation.SetTarget(SmallBlinkingMic); // set the view you want to animate

                TranslateAnimation1 = new TranslateAnimation(0, 0, basketInitialY, basketInitialY - 90) { Duration = 250 };
                TranslateAnimation2 = new TranslateAnimation(0, 0, basketInitialY - 130, basketInitialY) { Duration = 350 };

                MicAnimation.Start();
                BasketImg.SetImageDrawable(AnimatedVectorDrawable);

                Handler1 = new Handler(Looper.MainLooper);
                Handler1.PostDelayed(() =>
                {
                    try
                    {
                        BasketImg.Visibility = ViewStates.Visible;
                        BasketImg.StartAnimation(TranslateAnimation1);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }, 350);

                TranslateAnimation1.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        AnimatedVectorDrawable.Start();
                        Handler2 = new Handler(Looper.MainLooper);
                        Handler2.PostDelayed(() =>
                        {
                            try
                            {
                                BasketImg.StartAnimation(TranslateAnimation2);
                                SmallBlinkingMic.Visibility = ViewStates.Invisible;
                                BasketImg.Visibility = ViewStates.Invisible;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }, 450);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };


                TranslateAnimation2.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        BasketImg.Visibility = ViewStates.Invisible;

                        IsBasketAnimating = false;

                        //if the user pressed the record button while the animation is running
                        // then do NOT call on Animation end
                        if (OnBasketAnimationEndListener != null && !IsStartRecorded)
                        {
                            OnBasketAnimationEndListener.OnAnimationEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// if the user started a new Record while the Animation is running
        /// then we want to stop the current animation and revert views back to default state
        /// </summary>
        public void ResetBasketAnimation()
        {
            try
            {
                if (IsBasketAnimating)
                {
                    TranslateAnimation1?.Reset();
                    TranslateAnimation1?.Cancel();

                    TranslateAnimation2?.Reset();
                    TranslateAnimation2?.Cancel();

                    MicAnimation.Cancel();

                    SmallBlinkingMic.ClearAnimation();
                    BasketImg.ClearAnimation();

                    Handler1?.RemoveCallbacksAndMessages(null);
                    Handler2?.RemoveCallbacksAndMessages(null);

                    BasketImg.Visibility = ViewStates.Invisible;
                    SmallBlinkingMic.SetX(MicX);
                    SmallBlinkingMic.SetY(MicY);
                    SmallBlinkingMic.Visibility = ViewStates.Gone;

                    IsBasketAnimating = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void ClearAlphaAnimation(bool hideView)
        {
            try
            {
                AlphaAnimation.Cancel();
                AlphaAnimation.Reset();
                SmallBlinkingMic.ClearAnimation();
                if (hideView)
                {
                    SmallBlinkingMic.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void AnimateSmallMicAlpha()
        {
            try
            {
                AlphaAnimation = new AlphaAnimation(0.0f, 1.0f)
                {
                    Duration = 500,
                    RepeatMode = RepeatMode.Reverse,
                    RepeatCount = Animation.Infinite
                };
                SmallBlinkingMic.StartAnimation(AlphaAnimation);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void MoveRecordButtonAndSlideToCancelBack(RecordButton recordBtn, FrameLayout slideToCancelLayout, float initialX, float difX)
        {
            try
            {
                ValueAnimator positionAnimator = ValueAnimator.OfFloat(recordBtn.GetX(), initialX);
                if (positionAnimator != null)
                {
                    positionAnimator.SetInterpolator(new AccelerateDecelerateInterpolator());
                    positionAnimator.Update += (sender, args) =>
                    {
                        try
                        {
                            float x = (float)args.Animation?.AnimatedValue;
                            recordBtn.SetX(x);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    };

                    recordBtn.StopScale();
                    positionAnimator.SetDuration(0);
                    positionAnimator.Start();
                }

                // if the move event was not called ,then the difX will still 0 and there is no need to move it back
                if (difX != 0)
                {
                    float x = initialX - difX;
                    slideToCancelLayout.Animate()
                        ?.X(x)
                        ?.SetDuration(0)
                        ?.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void ResetSmallMic()
        {
            try
            {
                if (SmallBlinkingMic != null)
                {
#pragma warning disable 618
                    SmallBlinkingMic.SetAlpha(1);
#pragma warning restore 618
                    SmallBlinkingMic.ScaleX = 1.0f;
                    SmallBlinkingMic.ScaleY = 1.0f;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void setOnBasketAnimationEndListener(IOnBasketAnimationEnd onBasketAnimationEndListener)
        {
            try
            {
                OnBasketAnimationEndListener = onBasketAnimationEndListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationEnd()
        {
            try
            {
                OnBasketAnimationEndListener?.OnAnimationEnd();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        //check if the user started a new Record by pressing the RecordButton
        public void SetStartRecorded(bool startRecorded)
        {
            try
            {
                IsStartRecorded = startRecorded;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}