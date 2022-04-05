using Android.Views;
using Android.Widget;
using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.CardView.Widget;
using WoWonder.Helpers.Model;

namespace WoWonder.Helpers.Utils
{
    public static class ToastUtils
    {
        private static TextView ToastText;
        private static Toast PostToast;

        public static void ShowToast(Context context, string content, ToastLength length)
        {
            try
            {
                if (AppSettings.ToastTheme == ToastTheme.Custom)
                {
                    LayoutInflater layoutInflater = (LayoutInflater)context?.GetSystemService(Context.LayoutInflaterService);

                    View toastLayout = layoutInflater?.Inflate(Resource.Layout.toast_row, null);
                    CardView cardToastRoot = toastLayout?.FindViewById<CardView>(Resource.Id.ll_toast_root);
                    cardToastRoot?.SetCardBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#FFFEFE"));

                    ToastText = toastLayout?.FindViewById<TextView>(Resource.Id.tv_toast);

                    PostToast = new Toast(context) { Duration = length };

                    if (Build.VERSION.SdkInt < BuildVersionCodes.R)
                    {
#pragma warning disable 618
                        PostToast.View = toastLayout;
#pragma warning restore 618
                        ToastText.Text = content;
                    }
                    else
                    {
                        PostToast.SetText(content);
                    }

                    PostToast.Show();
                }
                else
                {
                    Toast.MakeText(context, content, length)?.Show();
                }
            }
            catch (Exception exception)
            {
                Toast.MakeText(context, content, length)?.Show();
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}