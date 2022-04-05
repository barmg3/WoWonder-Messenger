using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Newtonsoft.Json;
using WoWonder.Activities.Tab;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;

namespace WoWonder.Activities.Authentication
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/ProfileTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class FirstActivity : AppCompatActivity
    {
        #region Variables Basic

        private AppCompatButton ContinueButton, LogIntoButton;
        private DataTables.LoginTb LoginTb;
        private ImageView Imageplace;
        private TextView TermsAndConditionsText;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                
                // Create your application here
                SetContentView(Resource.Layout.FirstLayout);
                 
                //Get Value 
                InitComponent();

                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

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
                AddOrRemoveEvent(true);
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
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                TermsAndConditionsText = FindViewById<TextView>(Resource.Id.TermsText);
                ContinueButton = FindViewById<AppCompatButton>(Resource.Id.ContinueButton);
                LogIntoButton = FindViewById<AppCompatButton>(Resource.Id.LogIntoButton);
                Imageplace = FindViewById<ImageView>(Resource.Id.Imageplace);

                LoginTb = JsonConvert.DeserializeObject<DataTables.LoginTb>(Methods.ReadNoteOnSD());
                if (LoginTb != null && !string.IsNullOrEmpty(LoginTb.AccessToken) &&  !string.IsNullOrEmpty(LoginTb.Username))
                    ContinueButton.Text = GetString(Resource.String.Lbl_ContinueAs) + " " + LoginTb.Username;

                Glide.With(this).Load(Resource.Drawable.first_activity_image).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Into(Imageplace);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    TermsAndConditionsText.Click += TermsAndConditionsText_Click;
                    ContinueButton.Click += ContinueButtonOnClick;
                    LogIntoButton.Click += LogIntoButtonOnClick;
                }
                else
                {
                    TermsAndConditionsText.Click -= TermsAndConditionsText_Click;
                    ContinueButton.Click -= ContinueButtonOnClick;
                    LogIntoButton.Click -= LogIntoButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TermsAndConditionsText_Click(object sender, EventArgs e)
        {
            try
            {
                string url = InitializeWoWonder.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Events


        private void LogIntoButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ContinueButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    // Check Created My Folder Or Not 
                    Methods.Path.Chack_MyFolder();
                    CrossAppAuthentication();
                }
                else
                {
                    if (PermissionsController.CheckPermissionStorage())
                    {
                        // Check Created My Folder Or Not 
                        Methods.Path.Chack_MyFolder();
                        CrossAppAuthentication();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(100);
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        // Check Created My Folder Or Not 
                        Methods.Path.Chack_MyFolder();
                        CrossAppAuthentication();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void CrossAppAuthentication()
        {
            try
            {
                if (LoginTb != null && !string.IsNullOrEmpty(LoginTb.AccessToken) && !string.IsNullOrEmpty(LoginTb.Username))
                {
                    Current.AccessToken = LoginTb.AccessToken;

                    UserDetails.Username = LoginTb.Username;
                    UserDetails.FullName = LoginTb.Username;
                    UserDetails.Password = LoginTb.Password;
                    UserDetails.AccessToken = LoginTb.AccessToken;
                    UserDetails.UserId = LoginTb.UserId;
                    UserDetails.Status = "Pending";
                    UserDetails.Cookie = LoginTb.AccessToken;
                    UserDetails.Email = LoginTb.Email;

                    //Insert user data to database
                    var user = new DataTables.LoginTb
                    {
                        UserId = UserDetails.UserId,
                        AccessToken = UserDetails.AccessToken,
                        Cookie = UserDetails.Cookie,
                        Username = UserDetails.Username,
                        Password = UserDetails.Password,
                        Status = "Pending",
                        DeviceId = UserDetails.DeviceId,
                        Email = UserDetails.Email,
                    };
                    ListUtils.DataUserLoginList.Clear();
                    ListUtils.DataUserLoginList.Add(user);

                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                    
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api });

                    if (AppSettings.ShowWalkTroutPage)
                    {
                        Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                        newIntent.PutExtra("class", "login");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                    }

                    Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}