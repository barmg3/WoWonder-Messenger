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
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;

namespace WoWonder.Activities.SettingsPreferences.Custom
{
    public class CustomCheckBoxPreference : CheckBoxPreference
    {
        private string LikedPref, CommentedPref, SharedPref, FollowedPref, LikedPagePref, VisitedPref, MentionedPref, JoinedGroupPref, AcceptedPref, ProfileWallPostPref, MemoryPref;

        protected CustomCheckBoxPreference(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomCheckBoxPreference(Context context) : base(context)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCheckboxPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomCheckBoxPreference(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCheckboxPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomCheckBoxPreference(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCheckboxPreference;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomCheckBoxPreference(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            try
            {
                LayoutResource = Resource.Layout.SettingCheckboxPreference;
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
                var check = holder.ItemView.FindViewById<CheckBox>(Resource.Id.check);
                check.Checked = Checked;
                check.CheckedChange += CheckOnCheckedChange;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CheckOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (Key == "checkBox_PlaySound_key")
                {
                    UserDetails.SoundControl = e.IsChecked;
                } 
                else if (Key == "checkBox_e_liked_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELiked = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELiked = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                LikedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELiked = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELiked = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                LikedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_liked", LikedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_commented_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ECommented = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ECommented = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                CommentedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ECommented = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ECommented = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                CommentedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_commented", CommentedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_shared_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EShared = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EShared = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                SharedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EShared = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EShared = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                SharedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_shared", SharedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_followed_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EFollowed = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EFollowed = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                FollowedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EFollowed = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EFollowed = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                FollowedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_followed", FollowedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_liked_page_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELikedPage = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELikedPage = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);


                                LikedPagePref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELikedPage = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELikedPage = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);


                                LikedPagePref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_liked_page", LikedPagePref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_visited_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EVisited = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EVisited = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                VisitedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EVisited = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EVisited = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                VisitedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_visited", VisitedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_mentioned_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMentioned = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMentioned = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                MentionedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMentioned = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMentioned = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                MentionedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_mentioned", MentionedPref},

                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_joined_group_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EJoinedGroup = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EJoinedGroup = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                JoinedGroupPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EJoinedGroup = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EJoinedGroup = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                JoinedGroupPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_joined_group", JoinedGroupPref},

                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_accepted_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EAccepted = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EAccepted = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                AcceptedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EAccepted = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EAccepted = "0"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                AcceptedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_accepted", AcceptedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_profile_wall_post_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EProfileWallPost = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EProfileWallPost = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                ProfileWallPostPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EProfileWallPost = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EProfileWallPost = "0"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                ProfileWallPostPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_profile_wall_post", ProfileWallPostPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
                else if (Key == "checkBox_e_memory_key")
                {
                    switch (e.IsChecked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMemory = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMemory = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                MemoryPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMemory = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMemory = "0"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                MemoryPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_memory", MemoryPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
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