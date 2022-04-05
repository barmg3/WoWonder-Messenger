using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using AndroidX.Core.Content;
using FloatingView.Lib;
using Newtonsoft.Json;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace WoWonder.Frameworks.Floating
{
    public class FloatingObject
    {
        public string ChatId { set; get; }
        public string PageId { set; get; }
        public string GroupId { set; get; }
        public string UserId { set; get; }
        public string Avatar { set; get; }
        public string ChatType { set; get; }
        public string ChatColor { set; get; }
        public string Name { set; get; }
        public string LastSeen { set; get; }
        public string LastSeenUnixTime { set; get; }
        public string MessageCount { set; get; }
    }

    public class InitFloating
    {
        private static Activity ActivityContext;
        public static readonly int ChatHeadDataRequestCode = 5599;
        public static FloatingObject FloatingObject;

        public InitFloating()
        {
            try
            {
                UserDetails.ChatHead = CanDrawOverlays(Application.Context);
                ActivityContext = MainApplication.GetInstance()?.Activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FloatingShow(FloatingObject userData)
        {
            try
            {
                if (!UserDetails.ChatHead)
                    return;

                FloatingObject = userData;

                if (CanDrawOverlays(Application.Context))
                {
                    StartFloatingViewService(Application.Context, userData);
                    return;
                }

                //Intent intent = new Intent(Settings.ActionManageOverlayPermission, Uri.Parse("package:" + ActivityContext.PackageName));
                //ActivityContext.StartActivityForResult(intent, ChatHeadDataRequestCode);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool CheckPermission()
        {
            try
            {
                if (CanDrawOverlays(Application.Context))
                    return true;

                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public void OpenManagePermission()
        {
            try
            {
                if (CanDrawOverlays(Application.Context))
                    return;

                Intent intent = new Intent(Settings.ActionManageOverlayPermission, Uri.Parse("package:" + Application.Context.PackageName));
                ActivityContext.StartActivityForResult(intent, ChatHeadDataRequestCode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartFloatingViewService(Context context, FloatingObject userData)
        {
            try
            {
                // *** You must follow these rules when obtain the cutout(FloatingViewManager.findCutoutSafeArea) ***
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                    {
                        // 1. 'windowLayoutInDisplayCutoutMode' do not be set to 'never'
                        if (ActivityContext?.Window.Attributes.LayoutInDisplayCutoutMode == LayoutInDisplayCutoutMode.Never)
                        {
                            //ToastUtils.ShowToast(Application.Context, "windowLayoutInDisplayCutoutMode' do not be set to 'never" ,ToastLength.Short);
                            //throw new Exception("'windowLayoutInDisplayCutoutMode' do not be set to 'never'");
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                if (ChatHeadService.RunService)
                    return;

                // launch service 
                Intent intent = new Intent(context, typeof(ChatHeadService));

                if (ActivityContext != null)
                    intent.PutExtra(ChatHeadService.ExtraCutoutSafeArea, FloatingViewManager.FindCutoutSafeArea(ActivityContext));

                intent.PutExtra("UserData", JsonConvert.SerializeObject(userData));
                ContextCompat.StartForegroundService(context, intent);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static bool CanDrawOverlays(Context context)
        {
            try
            {
                if (!AppSettings.ShowChatHeads)
                    return false;

                switch (Build.VERSION.SdkInt)
                {
                    case < BuildVersionCodes.M:
                        return true;
                    case >= BuildVersionCodes.OMr1:
                        return Settings.CanDrawOverlays(context);
                }

                if (Settings.CanDrawOverlays(context)) return true;
                try
                {
                    var mgr = (IWindowManager)context.GetSystemService(Context.WindowService);
                    if (mgr == null) return false; //getSystemService might return null 
                    View viewToAdd = new View(context);
                    var paramsParams = new WindowManagerLayoutParams(0, 0, Build.VERSION.SdkInt >= BuildVersionCodes.O ? WindowManagerTypes.ApplicationOverlay : WindowManagerTypes.SystemAlert, WindowManagerFlags.NotTouchable | WindowManagerFlags.NotFocusable, Format.Transparent);
                    viewToAdd.LayoutParameters = paramsParams;
                    mgr.AddView(viewToAdd, paramsParams);
                    mgr.RemoveView(viewToAdd);
                    return true;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

    }
}