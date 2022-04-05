using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient.Requests;

namespace WoWonder.Activities.SettingsPreferences.Custom
{
    public class CustomSwitchPreference : SwitchPreferenceCompat
    {
        string SConfirmRequestFollowsPref, SShowMyActivitiesPref, SOnlineUsersPref, SShareMyLocationPref;

        protected CustomSwitchPreference(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomSwitchPreference(Context context) : base(context)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCustomSwitch;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomSwitchPreference(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCustomSwitch;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomSwitchPreference(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCustomSwitch;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomSwitchPreference(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCustomSwitch;
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

                holder.ItemView.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#444444") : Color.ParseColor("#ffffff"));

                var title = holder.ItemView.FindViewById<TextView>(Resource.Id.title);
                title.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#444444"));
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
                var Switch = holder.ItemView.FindViewById<Switch>(Resource.Id.togglebutton);
                Switch.Checked = Checked;
                Switch.CheckedChange += SwitchOnCheckedChange;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            { 
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (Key == "FingerprintLock_key")
                {
                    UserDetails.FingerprintLock = e.IsChecked; 
                }
                else if (Key == "notifications_key")  
                {  
                    if (e.IsChecked)
                    {
                        OneSignalNotification.Instance.RegisterNotificationDevice(Context);
                    }
                    else
                    {
                        OneSignalNotification.Instance.UnRegisterNotificationDevice();
                    }
                }
                else if (Key == "chatheads_key")  
                {
                    UserDetails.ChatHead = e.IsChecked; 
                    SettingsPrefFragment.OpenChatHead();
                }
                else if (Key == "ConfirmRequestFollows_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                SConfirmRequestFollowsPref = "1";
                                if (dataUser != null)
                                {
                                    dataUser.ConfirmFollowers = "1";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                }

                                break;
                            }
                        default:
                            {
                                SConfirmRequestFollowsPref = "0";
                                if (dataUser != null)
                                {
                                    dataUser.ConfirmFollowers = "0";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"confirm_followers", SConfirmRequestFollowsPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "ShowMyActivities_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser != null)
                                {
                                    dataUser.ShowActivitiesPrivacy = "1";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                SShowMyActivitiesPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser != null)
                                {
                                    dataUser.ShowActivitiesPrivacy = "0";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                SShowMyActivitiesPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"show_activities_privacy", SShowMyActivitiesPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }

                }
                else if (Key == "onlineUser_key")
                {
                    switch (e.IsChecked)
                    {
                        //Online >> value = 0
                        case true:
                            {
                                SOnlineUsersPref = "0";

                                if (dataUser != null)
                                {
                                    dataUser.Status = "0";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                break;
                            }
                        //Offline >> value = 1
                        default:
                            {
                                SOnlineUsersPref = "1";

                                if (dataUser != null)
                                {
                                    dataUser.Status = "1";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"status", SOnlineUsersPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "ShareMyLocation_key")
                {
                    switch (e.IsChecked)
                    {
                        //Yes >> value = 1
                        case true:
                            {
                                if (dataUser != null)
                                {
                                    dataUser.ShareMyLocation = "1";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                SShareMyLocationPref = "1";
                                break;
                            }
                        //No >> value = 0
                        default:
                            {
                                if (dataUser != null)
                                {
                                    dataUser.ShareMyLocation = "0";
                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                }

                                SShareMyLocationPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"share_my_location", SShareMyLocationPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}