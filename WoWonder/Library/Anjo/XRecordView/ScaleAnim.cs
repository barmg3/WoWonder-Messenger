using Android.Views;
using System;
using Android.Animation;
using Android.Views.Animations;
using WoWonder.Helpers.Utils;

namespace WoWonder.Library.Anjo.XRecordView
{
    public class ScaleAnim
    {
        private readonly View View;
        public ScaleAnim(View view)
        {
            View = view;
        }

        public void Start()
        {
            try
            {
                AnimatorSet set = new AnimatorSet();
                ObjectAnimator scaleY = ObjectAnimator.OfFloat(View, "scaleY", 2.0f);

                ObjectAnimator scaleX = ObjectAnimator.OfFloat(View, "scaleX", 2.0f);
                set.SetDuration(150);
                set.SetInterpolator(new AccelerateDecelerateInterpolator());
                set.PlayTogether(scaleY, scaleX);
                set.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void Stop()
        {
            try
            {
                AnimatorSet set = new AnimatorSet();
                ObjectAnimator scaleY = ObjectAnimator.OfFloat(View, "scaleY", 1.0f);
                //        scaleY.setDuration(250);
                //        scaleY.setInterpolator(new DecelerateInterpolator());


                ObjectAnimator scaleX = ObjectAnimator.OfFloat(View, "scaleX", 1.0f);
                //        scaleX.setDuration(250);
                //        scaleX.setInterpolator(new DecelerateInterpolator());


                set.SetDuration(150);
                set.SetInterpolator(new AccelerateDecelerateInterpolator());
                set.PlayTogether(scaleY, scaleX);
                set.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }
    }
}