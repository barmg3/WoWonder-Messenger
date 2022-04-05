using Android.App;
using Android.Views;
using Android.Widget;
using System;
using Android.Content.Res;
using Android.Util;
using AndroidX.RecyclerView.Widget;
using Com.Adcolony.Sdk;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.Ads
{
    public static class AdsColony
    {
        private static int CountInterstitial = 1;
        private static int CountRewarded = 1;
         
        #region Banner

        public static void InitBannerAd(Activity context, LinearLayout adContainer, AdColonyAdSize adSize, RecyclerView mRecycler)
        {
            try
            {
                if (adContainer == null)
                    return;

                if (WoWonderTools.GetStatusAds() && AppSettings.ShowColonyBannerAds)
                {
                    //Remove previous ad view if present.
                    if (adContainer.ChildCount > 0)
                        adContainer.RemoveAllViews();

                    // Construct optional app options object to be sent with configure
                    AdColonyAppOptions appOptions = new AdColonyAppOptions()
                        .SetPrivacyFrameworkRequired(AdColonyAppOptions.Gdpr, true)
                        .SetPrivacyConsentString(AdColonyAppOptions.Gdpr, "1");

                    // Configure AdColony in your launching Activity's onCreate() method so that cached ads can
                    // be available as soon as possible.
                    AdColony.Configure(context, appOptions, AppSettings.AdsColonyAppId, AppSettings.AdsColonyBannerId);

                    var listener = new MyAdColonyAdViewListener(adContainer, mRecycler);

                    // Optional Ad specific options to be sent with request
                    AdColonyAdOptions adOptions = new AdColonyAdOptions();

                    //Request Ad
                    AdColony.RequestAdView(AppSettings.AdsColonyBannerId, listener, adSize, adOptions);
                }
                else
                {
                    adContainer.Visibility = ViewStates.Gone;
                    if (mRecycler != null) Methods.SetMargin(mRecycler, 0, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAdColonyAdViewListener : AdColonyAdViewListener
        {
            private readonly LinearLayout AdContainer;
            private readonly RecyclerView MRecycler;

            public MyAdColonyAdViewListener(LinearLayout adContainer, RecyclerView mRecycler)
            {
                AdContainer = adContainer;
                MRecycler = mRecycler;
            }

            public override void OnRequestFilled(AdColonyAdView adColonyAdView)
            {
                try
                {
                    Console.WriteLine("onRequestFilled");
                    AdContainer?.AddView(adColonyAdView);
                    AdContainer.Visibility = ViewStates.Visible;

                    Resources r = Application.Context.Resources;
                    int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, adColonyAdView.AdSize.Height, r.DisplayMetrics);
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 0, 0, 0, px);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnRequestNotFilled(AdColonyZone zone)
            {
                try
                {
                    base.OnRequestNotFilled(zone);
                    Console.WriteLine("onRequestNotFilled");
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 0, 0, 0, 0);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnOpened(AdColonyAdView ad)
            {
                base.OnOpened(ad);
                Console.WriteLine("onOpened");
            }

            public override void OnClosed(AdColonyAdView ad)
            {
                base.OnClosed(ad);
                Console.WriteLine("onClosed");
            }

            public override void OnClicked(AdColonyAdView ad)
            {
                base.OnClicked(ad);
                Console.WriteLine("onClicked");
            }

            public override void OnLeftApplication(AdColonyAdView ad)
            {
                base.OnLeftApplication(ad);
                Console.WriteLine("onLeftApplication");
            }
        }

        #endregion

        //============================================

        #region Interstitial

        public static void Ad_Interstitial(Activity context)
        {
            try
            {
                if (WoWonderTools.GetStatusAds() && AppSettings.ShowColonyInterstitialAds)
                {
                    if (CountInterstitial == AppSettings.ShowAdInterstitialCount)
                    {
                        CountInterstitial = 1;
                        InitInterstitialAd(context);
                    }
                    else
                        CountInterstitial++;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private static void InitInterstitialAd(Activity context)
        {
            try
            {
                // Construct optional app options object to be sent with configure
                AdColonyAppOptions appOptions = new AdColonyAppOptions()
                    .SetPrivacyFrameworkRequired(AdColonyAppOptions.Gdpr, true)
                    .SetPrivacyConsentString(AdColonyAppOptions.Gdpr, "1")
                    .SetUserID(UserDetails.UserId)
                    .SetKeepScreenOn(true);

                // Configure AdColony in your launching Activity's onCreate() method so that cached ads can
                // be available as soon as possible.
                AdColony.Configure(context, appOptions, AppSettings.AdsColonyAppId, AppSettings.AdsColonyInterstitialId);

                // Optional user metadata sent with the ad options in each request
                //AdColonyUserMetadata metadata = new AdColonyUserMetadata()
                //    .SetUserAge(18)
                //    .SetUserEducation(AdColonyUserMetadata.UserEducationBachelorsDegree)
                //    .SetUserGender(AdColonyUserMetadata.UserMale);

                // Set up listener for interstitial ad callbacks. You only need to implement the callbacks
                // that you care about. The only required callback is onRequestFilled, as this is the only
                // way to get an ad object.
                var listener = new MyAdColonyInterstitialListener();

                AdColony.RequestInterstitial(AppSettings.AdsColonyInterstitialId, listener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAdColonyInterstitialListener : AdColonyInterstitialListener
        {
            public override void OnRequestFilled(AdColonyInterstitial ad)
            {
                try
                {
                    // Ad passed back in request filled callback, ad can now be shown 
                    ad?.Show();
                    Console.WriteLine("onRequestFilled");
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnRequestNotFilled(AdColonyZone zone)
            {
                // Ad request was not filled 
                Console.WriteLine("onRequestNotFilled");
            }

            public override void OnOpened(AdColonyInterstitial ad)
            {
                // Ad opened, reset UI to reflect state change
                Console.WriteLine("onOpened");
            }

            public override void OnExpiring(AdColonyInterstitial ad)
            {
                try
                {
                    // Request a new ad if ad is expiring
                    AdColony.RequestInterstitial(AppSettings.AdsColonyInterstitialId, this);
                    Console.WriteLine("onExpiring");
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        #endregion

        //============================================

        #region Rewarded

        public static void Ad_Rewarded(Activity context)
        {
            try
            {
                if (WoWonderTools.GetStatusAds() && AppSettings.ShowColonyRewardAds)
                {
                    if (CountRewarded == AppSettings.ShowAdRewardedVideoCount)
                    {
                        CountRewarded = 1;
                        InitRewardedAd(context);
                    }
                    else
                        CountRewarded++;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private static void InitRewardedAd(Activity context)
        {
            try
            {
                // Construct optional app options object to be sent with configure
                AdColonyAppOptions appOptions = new AdColonyAppOptions()
                    .SetPrivacyFrameworkRequired(AdColonyAppOptions.Gdpr, true)
                    .SetPrivacyConsentString(AdColonyAppOptions.Gdpr, "1")
                    .SetUserID(UserDetails.UserId)
                    .SetKeepScreenOn(true);

                // Configure AdColony in your launching Activity's onCreate() method so that cached ads can
                // be available as soon as possible.
                AdColony.Configure(context, appOptions, AppSettings.AdsColonyAppId, AppSettings.AdsColonyRewardedId);

                // Optional user metadata sent with the ad options in each request
                //AdColonyUserMetadata metadata = new AdColonyUserMetadata()
                //    .SetUserAge(26)
                //    .SetUserEducation(AdColonyUserMetadata.UserEducationBachelorsDegree)
                //    .SetUserGender(AdColonyUserMetadata.UserMale);

                // Ad specific options to be sent with request
                var adOptions = new AdColonyAdOptions()
                    .EnableConfirmationDialog(false)
                    .EnableResultsDialog(false);
                //.SetUserMetadata(metadata);

                // Create and set a reward listener
                AdColony.SetRewardListener(new MyAdColonyRewardListener());

                // Set up listener for interstitial ad callbacks. You only need to implement the callbacks
                // that you care about. The only required callback is onRequestFilled, as this is the only
                // way to get an ad object.
                var listener = new MyAdColonyRewardedListener(adOptions);
                AdColony.RequestInterstitial(AppSettings.AdsColonyRewardedId, listener, adOptions);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyAdColonyRewardedListener : AdColonyInterstitialListener
        {
            private readonly AdColonyAdOptions AdOptions;
            public MyAdColonyRewardedListener(AdColonyAdOptions adOptions)
            {
                AdOptions = adOptions;
            }

            public override void OnRequestFilled(AdColonyInterstitial ad)
            {
                try
                {
                    // Ad passed back in request filled callback, ad can now be shown 
                    ad?.Show();

                    Console.WriteLine("onRequestFilled");
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnRequestNotFilled(AdColonyZone zone)
            {
                // Ad request was not filled
                Console.WriteLine("onRequestNotFilled");
            }

            public override void OnOpened(AdColonyInterstitial ad)
            {
                // Ad opened, reset UI to reflect state change
                Console.WriteLine("onOpened");
            }

            public override void OnExpiring(AdColonyInterstitial ad)
            {
                try
                {
                    // Request a new ad if ad is expiring
                    AdColony.RequestInterstitial(AppSettings.AdsColonyRewardedId, this, AdOptions);
                    Console.WriteLine("onExpiring");
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class MyAdColonyRewardListener : Java.Lang.Object, IAdColonyRewardListener
        {
            public MyAdColonyRewardListener()
            {

            }

            public void OnReward(AdColonyReward p0)
            {
                Console.WriteLine("onReward");
            }
        }

        #endregion
    }
}