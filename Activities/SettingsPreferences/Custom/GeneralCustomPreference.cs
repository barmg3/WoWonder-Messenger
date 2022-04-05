using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.SettingsPreferences.Custom
{
    public class GeneralCustomPreference : Preference
    { 
        protected GeneralCustomPreference(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public GeneralCustomPreference(Context context) : base(context)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingGeneralPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public GeneralCustomPreference(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingGeneralPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public GeneralCustomPreference(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingGeneralPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public GeneralCustomPreference(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingGeneralPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public override void OnBindViewHolder(PreferenceViewHolder holder)
        {
            try
            {
                base.OnBindViewHolder(holder);

                var title = holder.ItemView.FindViewById<TextView>(Resource.Id.title);
                title.Text = Title;

                var summary = holder.ItemView.FindViewById<TextView>(Resource.Id.summary);
                summary.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#CECECE"));
                if (!string.IsNullOrEmpty(Summary))
                {
                    summary.Text = Summary;
                    summary.Visibility = ViewStates.Visible;
                }
                else
                {
                    summary.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}