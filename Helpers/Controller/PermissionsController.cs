﻿using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.Controller
{
    public class PermissionsController
    {
        private readonly Activity Context;

        public PermissionsController(Activity activity)
        {
            try
            {
                Context = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static bool CheckPermissionStorage()
        {
            //if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            //{
            //    return Android.OS.Environment.IsExternalStorageManager;
            //}

            if ((int)Build.VERSION.SdkInt < 23)
                return true;

            if (Application.Context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                Application.Context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                return true; 

            return false;
        }

        /// <summary>
        /// Handle Permission Request
        /// </summary>
        /// <param name="idPermissions"> 100 >> Storage  101 >> ReadContacts && ReadPhoneNumbers  102 >> RecordAudio  103 >> Camera  104 >> SendSms  105 >> Location  106 >> GetAccounts && UseCredentials >> Social Logins  107 >> AccessWifiState && Internet  108 >> Storage && Camera</param>
        public void RequestPermission(int idPermissions)
        {
            switch ((int)Build.VERSION.SdkInt)
            {
                // Check if we're running on Android 5.0 or higher
                case >= 23:
                    switch (idPermissions)
                    {
                        case 100:
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                Context.RequestPermissions(new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 100);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else
                            {
                                Context.RequestPermissions(new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 100);
                            }
                            break;

                        case 101:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.ReadContacts,
                                Manifest.Permission.ReadPhoneNumbers
                            }, 101);
                            break;

                        case 102:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.RecordAudio,
                                Manifest.Permission.ModifyAudioSettings
                            }, 102);
                            break;

                        case 103:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.Camera
                            }, 103);
                            break;

                        case 104:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.SendSms,
                                Manifest.Permission.BroadcastSms
                            }, 104);
                            break;

                        case 105:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.AccessFineLocation,
                                Manifest.Permission.AccessCoarseLocation
                            }, 105);
                            break;

                        case 106:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.GetAccounts,
                                Manifest.Permission.UseCredentials
                            }, 106);
                            break;

                        case 107:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.AccessWifiState,
                                Manifest.Permission.Internet
                            }, 107);
                            break;
                        case 108:
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                Context.RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 108);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else
                            {
                                Context.RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 108);
                            }
                            break;
                        case 109:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.ReadProfile,
                                Manifest.Permission.ReadPhoneNumbers,
                                Manifest.Permission.ReadPhoneState
                            }, 109);
                            break;
                        case 110:
                            Context.RequestPermissions(new[]
                            {
                                Manifest.Permission.WakeLock
                            }, 110);
                            break;
                        case 111:

                            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                Context.RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                    Manifest.Permission.RecordAudio,
                                    Manifest.Permission.ModifyAudioSettings
                                }, 111);

                                //if (!Android.OS.Environment.IsExternalStorageManager)
                                //    OpenDialog();
                            }
                            else
                            {
                                Context.RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                    Manifest.Permission.RecordAudio,
                                    Manifest.Permission.ModifyAudioSettings
                                }, 111);
                            }
                            break;
                    }

                    break;
            }
        }

        //private void OpenDialog()
        //{
        //    try
        //    {
        //        var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
        //        dialog.Title(Resource.String.Lbl_Security).TitleColorRes(Resource.Color.primary);
        //        dialog.Content(Resource.String.Lbl_AllowGrantPermission);
        //        dialog.PositiveText(Resource.String.Lbl_GrantPermission).OnPositive((materialDialog, action) =>
        //        {
        //            try
        //            {
        //                Intent intent = new Intent(Settings.ActionManageAllFilesAccessPermission);
        //                intent.AddCategory(Intent.CategoryDefault);
        //                intent.SetData(Android.Net.Uri.Parse("package:" + Context.PackageName));
        //                Context.StartActivity(intent);
        //            }
        //            catch (Exception e)
        //            {
        //                 Methods.DisplayReportResultTrack(e); 
        //                Intent intent = new Intent();
        //                intent.SetAction(Settings.ActionManageAllFilesAccessPermission);
        //                Context.StartActivity(intent);
        //            }
        //        });
        //        dialog.NegativeText(Resource.String.Lbl_Cancel).OnNegative(new MyMaterialDialog());
        //        dialog.AlwaysCallSingleChoiceCallback();
        //        dialog.Build().Show();
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}
    }
}