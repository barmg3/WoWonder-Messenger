using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Auth.Api.Credentials;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Tasks;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Newtonsoft.Json;
using Org.Json;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Tab;
using WoWonder.Activities.WalkTroutPage; 
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.SocialLogins;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;

namespace WoWonder.Activities.Authentication
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, IOnCompleteListener, IOnFailureListener, IActivityResultCallback
    {
        #region Variables Basic

        private LinearLayout ToolBarLayout;
        private AppCompatButton MButtonViewSignIn, RegisterButton, MGoogleSignIn;
        private EditText UsernameEditText, PasswordEditText;
        private LinearLayout MainLinearLayout;
        private TextView TopTittle, ForgetPass, TermsOfService, Privacy; 
        private ProgressBar ProgressBar; 
        private LoginButton BtnFbLogin;
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;
        public static GoogleSignInClient MGoogleSignInClient; 
        private string TimeZone = AppSettings.CodeTimeZone;
        private bool IsActiveUser = true;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.LoginLayout);

                InitializeWoWonder.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnTrustFailureOnWebException);

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();

                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.GetSettings_Api(this), GetTimezone});

                if (AppSettings.EnableSmartLockForPasswords)
                    BuildClients(null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();

                if (!MIsResolving && AppSettings.EnableSmartLockForPasswords)
                {
                    RequestCredentials(false);
                    LoadHintClicked();
                }
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

        protected override void OnDestroy()
        {
            try
            {
                ProfileTracker?.StopTracking();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
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
                //Get values
                ToolBarLayout = FindViewById<LinearLayout>(Resource.Id.toolbarLayout);
                UsernameEditText = FindViewById<EditText>(Resource.Id.usernamefield);
                PasswordEditText = FindViewById<EditText>(Resource.Id.passwordfield);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                MButtonViewSignIn = FindViewById<AppCompatButton>(Resource.Id.loginButton);
                RegisterButton = FindViewById<AppCompatButton>(Resource.Id.signUpButton);
                MainLinearLayout = FindViewById<LinearLayout>(Resource.Id.mainLinearLayout);
                TopTittle = FindViewById<TextView>(Resource.Id.titile);
                TopTittle.Text = AppSettings.ApplicationName;
                
                ForgetPass = FindViewById<TextView>(Resource.Id.forgetpassButton);
                TermsOfService = FindViewById<TextView>(Resource.Id.secTermTextView);
                Privacy = FindViewById<TextView>(Resource.Id.secPrivacyTextView);

                ProgressBar.Visibility = ViewStates.Invisible;

                if (!AppSettings.EnableRegisterSystem)
                    RegisterButton.Visibility = ViewStates.Gone;
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
                    ToolBarLayout.Click += ToolBarLayout_Click;
                    ForgetPass.Click += ForgetPassOnClick; 
                    TermsOfService.Click += TermsOfServiceOnClick;
                    Privacy.Click += PrivacyOnClick;
                    MainLinearLayout.Click += Main_LoginPage_Click;
                    RegisterButton.Click += RegisterButton_Click;
                    MButtonViewSignIn.Click += BtnLoginOnClick;

                }
                else
                {
                    //Close Event
                    ToolBarLayout.Click -= ToolBarLayout_Click;
                    ForgetPass.Click -= ForgetPassOnClick; 
                    TermsOfService.Click -= TermsOfServiceOnClick;
                    Privacy.Click -= PrivacyOnClick;
                    MainLinearLayout.Click -= Main_LoginPage_Click;
                    RegisterButton.Click -= RegisterButton_Click;
                    MButtonViewSignIn.Click -= BtnLoginOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ToolBarLayout_Click(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.StartTracking();
                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;

                    BtnFbLogin = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    BtnFbLogin.Visibility = ViewStates.Visible;
                    BtnFbLogin.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    BtnFbLogin.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    BtnFbLogin = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    BtnFbLogin.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    MGoogleSignIn = FindViewById<AppCompatButton>(Resource.Id.Googlelogin_button);
                    MGoogleSignIn.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    MGoogleSignIn = FindViewById<AppCompatButton>(Resource.Id.Googlelogin_button);
                    MGoogleSignIn.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Login With Facebook
        private void ProfileTrackerOnMOnProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                    //var FbFirstName = e.MProfile.FirstName;
                    //var FbLastName = e.MProfile.LastName;
                    //var FbName = e.MProfile.Name;
                    //var FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
               Methods.DisplayReportResultTrack(ex);
            }
        }
         
        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception ex)
            {
               Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Events

        public override void OnBackPressed()
        {
            try
            {
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }

        //Click Button Login
        private async void BtnLoginOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                        GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
                else
                {
                    if (!string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        HideKeyboard();

                        ProgressBar.Visibility = ViewStates.Visible;
                        MButtonViewSignIn.Visibility = ViewStates.Gone;

                        if (string.IsNullOrEmpty(TimeZone))
                           await GetTimezone();

                        await AuthApi(UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text);

                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                            GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task AuthApi(string email, string password)
        {
            var (apiStatus, respond) = await RequestsAsync.Auth.AuthAsync(email, password, TimeZone, UserDetails.DeviceId);
            switch (apiStatus)
            {
                case 200 when respond is AuthObject auth:
                    {
                        var emailValidation = ListUtils.SettingsSiteList?.EmailValidation ?? "0";
                        if (emailValidation == "1")
                        {
                            IsActiveUser = await CheckIsActiveUser(auth.UserId);
                        }

                        if (IsActiveUser)
                        {
                            if (AppSettings.EnableSmartLockForPasswords)
                            {
                                // Save Google Sign In to SmartLock
                                Credential credential = new Credential.Builder(email)
                                    .SetName(email)
                                    .SetPassword(password)
                                    .Build();

                                SaveCredential(credential);
                            }
                             
                            SetDataLogin(auth);

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

                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible;
                            FinishAffinity();
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                case 200:
                    {
                        if (respond is AuthMessageObject messageObject)
                        {
                            UserDetails.Username = UsernameEditText.Text;
                            UserDetails.FullName = UsernameEditText.Text;
                            UserDetails.Password = PasswordEditText.Text;
                            UserDetails.UserId = messageObject.UserId;
                            UserDetails.Status = "Pending";
                            UserDetails.Email = UsernameEditText.Text;

                            //Insert user data to database
                            var user = new DataTables.LoginTb
                            {
                                UserId = UserDetails.UserId,
                                AccessToken = "",
                                Cookie = "",
                                Username = UsernameEditText.Text,
                                Password = PasswordEditText.Text,
                                Status = "Pending",
                                Lang = "",
                                DeviceId = UserDetails.DeviceId,
                            };
                            ListUtils.DataUserLoginList.Add(user);

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdateLogin_Credentials(user);


                            Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                            newIntent.PutExtra("TypeCode", "TwoFactor");
                            StartActivity(newIntent);
                        }

                        break;
                    }
                case 400:
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            switch (errorId)
                            {
                                case "3":
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_ErrorLogin_3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "4":
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "5":
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_ErrorLogin_5), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security), errorText,
                                        GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                        break;
                    }
                case 404:
                    ProgressBar.Visibility = ViewStates.Gone;
                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    break;
            } 
        }

        private void PrivacyOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                string url = InitializeWoWonder.WebsiteUrl + "/terms/privacy-policy";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TermsOfServiceOnClick(object sender, EventArgs eventArgs)
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
        private void ForgetPassOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                StartActivity(typeof(ForgetPasswordActivity)); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this ,typeof(RegisterActivity)));
                OverridePendingTransition(Resource.Animation.abc_grow_fade_in_from_bottom,Resource.Animation.abc_shrink_fade_out_from_bottom);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Main_LoginPage_Click(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result
         
        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                Log.Debug("Login_Activity", "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);

                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task);
                }
                else if (requestCode == RcCredentialsRead)
                {
                    if (resultCode == Result.Ok)
                    {
                        var extra = data.GetParcelableExtra(Credential.ExtraKey);
                        if (extra != null && extra is Credential credential)
                        {
                            HandleCredential(credential, OnlyPasswords);
                        }
                    }
                }
                else if (requestCode == RcCredentialsSave)
                {
                    MIsResolving = false;
                    if (resultCode == Result.Ok)
                    {
                        //Saved
                    }
                    else
                    {
                        //Credential save failed
                    }
                }
                else if (requestCode == RcCredentialsHint)
                {
                    MIsResolving = false;
                    if (resultCode == Result.Ok)
                    {
                        var extra = data.GetParcelableExtra(Credential.ExtraKey);
                        if (extra != null && extra is Credential credential)
                        {
                            OnlyPasswords = true;
                            HandleCredential(credential, OnlyPasswords);
                        }
                    }
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int)resultCode, data);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                MButtonViewSignIn.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                var data = json.ToString();
                var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                 
                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            if (respond is AuthObject auth)
                            {
                                if (AppSettings.EnableSmartLockForPasswords)
                                {
                                    // Save Google Sign In to SmartLock
                                    Credential credential = new Credential.Builder(result.Email)
                                        .SetAccountType(IdentityProviders.Facebook)
                                        .SetName(result.Name)
                                        //.SetPassword(auth.AccessToken)
                                        .Build();

                                    SaveCredential(credential);
                                }


                                SetDataLogin(auth);

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

                            break;
                        }
                        case 400:
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;

                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));

                            }

                            break;
                        }
                        case 404:
                        {
                            var error = respond.ToString();
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                            break;
                        }
                    }

                    ProgressBar.Visibility = ViewStates.Gone;
                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #region Google
         
        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    MButtonViewSignIn.Visibility = ViewStates.Gone;

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                     
                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        switch (apiStatus)
                        {
                            case 200:
                            {
                                if (respond is AuthObject auth)
                                {
                                    if (AppSettings.EnableSmartLockForPasswords)
                                    {
                                        // Save Google Sign In to SmartLock
                                        Credential credential = new Credential.Builder(acct.Email)
                                            .SetAccountType(IdentityProviders.Google)
                                            .SetName(acct.DisplayName)
                                            .SetProfilePictureUri(acct.PhotoUrl)
                                            .Build();

                                        SaveCredential(credential);
                                    }
                                           
                                    SetDataLogin(auth);

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

                                break;
                            }
                            case 400:
                            {
                                if (respond is ErrorObject error)
                                {
                                    var errorText = error.Error.ErrorText;

                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                }

                                break;
                            }
                            case 404:
                            {
                                var error = respond.ToString();
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                                break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible; 
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #endregion


        #region Cross App Authentication

        private static readonly int RcCredentialsSave = 1;
        private static readonly int RcCredentialsRead = 2;
        private static readonly int RcCredentialsHint = 3;

        private bool OnlyPasswords;
        private bool MIsResolving;
        private Credential MCredential;
        private string CredentialType;

        private CredentialsClient MCredentialsClient;
        private GoogleSignInClient mSignInClient;

        private void BuildClients(string accountName)
        {
            try
            {
                var gsoBuilder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                    .RequestIdToken(AppSettings.ClientId)
                    .RequestScopes(new Scope(Scopes.Profile))
                    .RequestScopes(new Scope(Scopes.PlusMe))
                    .RequestScopes(new Scope(Scopes.DriveAppfolder))
                    .RequestServerAuthCode(AppSettings.ClientId)
                    .RequestProfile().RequestEmail();

                if (accountName != null)
                    gsoBuilder.SetAccountName(accountName);

                MCredentialsClient = Credentials.GetClient(this, CredentialsOptions.Default);
                mSignInClient = GoogleSignIn.GetClient(this, gsoBuilder.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadHintClicked()
        {
            try
            {
                HintRequest hintRequest = new HintRequest.Builder()
                    .SetHintPickerConfig(new CredentialPickerConfig.Builder()
                        .SetShowCancelButton(true)
                        .Build())
                    .SetIdTokenRequested(false)
                    .SetEmailAddressIdentifierSupported(true)
                    .SetAccountTypes(IdentityProviders.Google)
                    .Build();

                PendingIntent intent = MCredentialsClient.GetHintPickerIntent(hintRequest);
                StartIntentSenderForResult(intent.IntentSender, RcCredentialsHint, null, 0, 0, 0);
                MIsResolving = true;
            }
            catch (Exception e)
            {
                //Could not start hint picker Intent
                MIsResolving = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RequestCredentials(bool onlyPasswords)
        {
            try
            {
                OnlyPasswords = onlyPasswords;

                CredentialRequest.Builder crBuilder = new CredentialRequest.Builder()
                    .SetPasswordLoginSupported(true);

                if (!onlyPasswords)
                {
                    crBuilder.SetAccountTypes(IdentityProviders.Google);
                }

                CredentialType = "Request";

                MCredentialsClient.Request(crBuilder.Build()).AddOnCompleteListener(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void HandleCredential(Credential credential, bool onlyPasswords)
        {
            try
            {
                // See "Handle successful credential requests"  
                MCredential = credential;

                //Log.d(TAG, "handleCredential:" + credential.getAccountType() + ":" + credential.getId());
                if (IdentityProviders.Google.Equals(credential.AccountType))
                {
                    // Google account, rebuild GoogleApiClient to set account name and then try
                    BuildClients(credential.Id);
                    GoogleSilentSignIn();
                }
                else if (!string.IsNullOrEmpty(credential?.Id) && !string.IsNullOrEmpty(credential?.Password))
                {
                    // Email/password account
                    Console.WriteLine("Signed in as {0}", credential.Id);

                    //ContinueButton.Text = GetString(Resource.String.Lbl_ContinueAs) + " " + credential.Id;
                    //ContinueButton.Visibility = ViewStates.Visible;

                    if (onlyPasswords)
                    {
                        //send api auth  
                        HideKeyboard();

                        //ToggleVisibility(true);

                        await AuthApi(credential.Id, credential.Password);
                    }
                }
                else
                {
                    //ContinueButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ResolveResult(ResolvableApiException rae, int requestCode)
        {
            try
            {
                if (!MIsResolving)
                {
                    try
                    {
                        rae.StartResolutionForResult(this, requestCode);
                        MIsResolving = true;
                    }
                    catch (IntentSender.SendIntentException e)
                    {
                        MIsResolving = false;
                        //Failed to send Credentials intent
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SaveCredential(Credential credential)
        {
            try
            {
                if (credential == null)
                {
                    //Log.w(TAG, "Ignoring null credential.");
                    return;
                }

                CredentialType = "Save";
                MCredentialsClient.Save(credential).AddOnCompleteListener(this, this).AddOnFailureListener(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void OnGoogleRevokeClicked()
        {
            if (MCredential != null)
            {
                await MCredentialsClient.DeleteAsync(MCredential);
            }
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            try
            {
                if (CredentialType == "Request")
                {
                    if (task.IsSuccessful && task.Result is CredentialRequestResponse credential)
                    {
                        // Auto sign-in success
                        HandleCredential(credential.Credential, OnlyPasswords);
                        return;
                    }
                }
                else if (CredentialType == "Save")
                {
                    if (task.IsSuccessful)
                    {
                        return;
                    }
                }

                var ee = task.Exception;
                if (ee is ResolvableApiException rae)
                {
                    // Getting credential needs to show some UI, start resolution 
                    if (CredentialType == "Request")
                        ResolveResult(rae, RcCredentialsRead);

                    else if (CredentialType == "Save")
                        ResolveResult(rae, RcCredentialsSave);
                }
                else
                {
                    Console.WriteLine("request: not handling exception {0}", ee);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {

        }

        private async void GoogleSilentSignIn()
        {
            try
            {
                // Try silent sign-in with Google Sign In API
                GoogleSignInAccount silentSignIn = await mSignInClient.SilentSignInAsync();
                if (silentSignIn != null)
                {
                    SetContentGoogle(silentSignIn);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task GetTimezone()
        {
            try
            {
                TimeZone = await ApiRequest.GetTimeZoneAsync();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task<bool> CheckIsActiveUser(string userId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.IsActiveUserAsync(userId);
                switch (apiStatus)
                {
                    case 200 when respond is MessageObject auth:
                        Console.WriteLine(auth);
                        return true;
                    case 400:
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            switch (errorId)
                            {
                                case "5":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ThisUserNotActive), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "4":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_UserNotFound), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        break;
                    }
                    case 404:
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        break;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }
         
        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = UsernameEditText.Text;
                UserDetails.FullName = UsernameEditText.Text;
                UserDetails.Password = PasswordEditText.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = UsernameEditText.Text;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UsernameEditText.Text,
                    Password = UsernameEditText.Text,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };
                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                 
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnActivityResult(Object result)
        {
            try
            {
                // handle the result in your own way 
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}