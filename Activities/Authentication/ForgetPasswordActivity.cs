using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Authentication
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ForgetPasswordActivity : AppCompatActivity
    {
        #region Variables Basic

        private AppCompatButton BtnSend;
        private EditText EmailEditext;
        private ProgressBar ProgressBar;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                // Set our view from the "ForgetPasswordLayout" layout resource
                SetContentView(Resource.Layout.ForgetPasswordLayout);

                EmailEditext = FindViewById<EditText>(Resource.Id.emailfield);
                BtnSend = FindViewById<AppCompatButton>(Resource.Id.send);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                ProgressBar.Visibility = ViewStates.Invisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                //Add Event
                BtnSend.Click += BtnSendOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();

                //Close Event
                BtnSend.Click -= BtnSendOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void BtnSendOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!string.IsNullOrEmpty(EmailEditext.Text))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditext.Text);
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this,
                                GetText(Resource.String.Lbl_VerificationFailed),
                                GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Visible;
                            var (apiStatus, respond) = await RequestsAsync.Auth.ResetPasswordEmailAsync(EmailEditext.Text);
                            switch (apiStatus)
                            {
                                case 200:
                                {
                                    if (respond is StatusObject result)
                                    {
                                        Console.WriteLine(result);
                                        ProgressBar.Visibility = ViewStates.Invisible;
                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                            GetText(Resource.String.Lbl_CheckYourEmail),
                                            GetText(Resource.String.Lbl_WeSentEmailTo).Replace("@", EmailEditext.Text),
                                            GetText(Resource.String.Lbl_Ok));
                                    }

                                    break;
                                }
                                case 400:
                                {
                                    if (respond is ErrorObject error)
                                    {
                                        var errortext = error.Error.ErrorText;
                                        Methods.DialogPopup.InvokeAndShowDialog(this,GetText(Resource.String.Lbl_Security), errortext,GetText(Resource.String.Lbl_Ok));
                                    }

                                    ProgressBar.Visibility = ViewStates.Invisible;
                                    break;
                                }
                                case 404:
                                    ProgressBar.Visibility = ViewStates.Invisible;
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_Error_Login), GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Invisible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed),
                            GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Invisible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed),
                    exception.ToString(), GetText(Resource.String.Lbl_Ok));
            }
        }
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
    }
}