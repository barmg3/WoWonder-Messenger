﻿using System;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM; 
using Android.Util;
using Android.Widget;
using Com.Google.Android.Play.Core.Appupdate;
using Com.Google.Android.Play.Core.Install.Model;
using Com.Google.Android.Play.Core.Tasks;

namespace WoWonder.Helpers.Utils
{
    public class UpdateManagerApp
    {
        public const int AppUpdateTypeSupported = AppUpdateType.Immediate;

        public static void CheckUpdateApp(Activity mainActivity, int updateRequest, Intent intent)
        {
            try
            {
                // Creates instance of the manager.
                var appUpdateManager = AppUpdateManagerFactory.Create(mainActivity);

                // Returns an intent object that you use to check for an update.
                var appUpdateInfoTask = appUpdateManager.AppUpdateInfo;
                // Checks that the platform will allow the specified type of update.
                appUpdateInfoTask.AddOnSuccessListener(new AppUpdateSuccessListener(appUpdateManager, mainActivity, updateRequest, intent));
                appUpdateInfoTask.AddOnFailureListener(new AppUpdateOnFailureListener());

                Console.WriteLine(appUpdateInfoTask);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class AppUpdateSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            private readonly IAppUpdateManager AppUpdateManager;
            private readonly Activity MainActivity;
            private readonly int UpdateRequest;
            private readonly Intent Intent;

            public AppUpdateSuccessListener(IAppUpdateManager appUpdateManager, Activity mainActivity, int updateRequest, Intent intent)
            {
                try
                {
                    AppUpdateManager = appUpdateManager;
                    MainActivity = mainActivity;
                    UpdateRequest = updateRequest;
                    Intent = intent;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnSuccess(Java.Lang.Object p0)
            {
                try
                {
                    if (!(p0 is AppUpdateInfo info))
                        return;

                    Log.Debug("AVAILABLE VERSION CODE", $"{info.AvailableVersionCode()}");

                    PackageInfo packageInfo = MainActivity?.PackageManager?.GetPackageInfo(MainActivity.PackageName, 0);
                    string versionName = packageInfo?.VersionName;

                    var availability = info.UpdateAvailability();
                    if (availability.Equals(UpdateAvailability.UpdateAvailable) || availability.Equals(UpdateAvailability.DeveloperTriggeredUpdateInProgress))
                    {
                        var dialog = new MaterialDialog.Builder(MainActivity).Theme(MaterialDialogsTheme.Light)
                            .Title(MainActivity.GetText(Resource.String.Lbl_ThereIsNewUpdate))
                            .CustomView(Resource.Layout.DialogCheckUpdateApp, true)
                            .PositiveText(MainActivity.GetText(Resource.String.Lbl_Update)).OnPositive((materialDialog, action) =>
                            {
                                try
                                {
                                    switch (availability)
                                    {
                                        case UpdateAvailability.UpdateAvailable or UpdateAvailability.DeveloperTriggeredUpdateInProgress when info.IsUpdateTypeAllowed(AppUpdateType.Immediate):
                                            // Start an update
                                            AppUpdateManager.StartUpdateFlowForResult(info, AppUpdateType.Immediate, MainActivity, UpdateRequest);

                                            //#if DEBUG
                                            //if (AppUpdateManager is FakeAppUpdateManager fakeAppUpdate && fakeAppUpdate.IsImmediateFlowVisible)
                                            //{
                                            //    fakeAppUpdate.UserAcceptsUpdate();
                                            //    fakeAppUpdate.DownloadStarts();
                                            //    fakeAppUpdate.DownloadCompletes();
                                            //    LaunchRestartDialog(AppUpdateManager);
                                            //}
                                            //#endif
                                            break;
                                        case UpdateAvailability.UpdateNotAvailable:
                                        case UpdateAvailability.Unknown:
                                            Log.Debug("UPDATE NOT AVAILABLE", $"{info.AvailableVersionCode()}");
                                            // You can start your activityonresult method when update is not available when using immediate update
                                            MainActivity.StartActivityForResult(Intent, 400); // You can use any random result code
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            })
                            .NegativeText(MainActivity.GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
                            .Build();

                        var txtNewVersion = dialog.CustomView.FindViewById<TextView>(Resource.Id.tv_new_version);
                        txtNewVersion.Text = MainActivity.GetText(Resource.String.Lbl_DiscoverNewVersion) + " V" + info.AvailableVersionCode();
                        var txtVersion = dialog.CustomView.FindViewById<TextView>(Resource.Id.tv_version);
                        txtVersion.Text = MainActivity.GetText(Resource.String.Lbl_Current) + " V" + versionName;
                        dialog.Show();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            // The restart dialog was only used to test the fakeappupdatemanager
            //private void LaunchRestartDialog(IAppUpdateManager appUpdateManager)
            //{
            //    try
            //    {
            //        AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity);
            //        AlertDialog alert = dialog.Create();
            //        alert.SetMessage("Application successfully updated! You need to restart the app in order to use this new features");
            //        alert.SetCancelable(false);
            //        alert.SetButton((int)DialogButtonType.Positive, "Restart", (o, args) =>
            //        {
            //            appUpdateManager.CompleteUpdate();
            //            // You can start your activityonresult method when update is not available when using immediate update when testing with fakeappupdate manager
            //            //_mainActivity.StartActivityForResult(_intent, 400);
            //        });
            //        alert.Show();
            //    }
            //    catch (Exception e)
            //    {
            //        Methods.DisplayReportResultTrack(e); 
            //    } 
            //} 
        }

        private class AppUpdateOnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            public void OnFailure(Java.Lang.Exception p0)
            {
                try
                {
                    Methods.DisplayReportResultTrack(p0);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}