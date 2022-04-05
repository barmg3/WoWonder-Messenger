using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.SuggestedUsers;
using WoWonder.Activities.Tab;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Authentication
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic
        private LinearLayout ToolBarLayout;
        private AppCompatButton RegisterButton;
        private EditText EmailEditText,UsernameEditText, PasswordEditText, PasswordRepeatEditText, GenderEditText, PhoneEditText;
        private LinearLayout MainLinearLayout; 
        private TextView TxtTermsOfService, TxtPrivacy;
        private CheckBox ChkAgree;
        private View PhoneView;
        private ProgressBar ProgressBar;
        private string GenderStatus = "male";

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
                SetContentView(Resource.Layout.RegisterLayout);

                //Get Value And Set Toolbar
                InitComponent(); 
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
                ToolBarLayout = FindViewById<LinearLayout>(Resource.Id.toolbarLayout);
                EmailEditText = FindViewById<EditText>(Resource.Id.emailfield);
                UsernameEditText = FindViewById<EditText>(Resource.Id.usernamefield);
                PasswordEditText = FindViewById<EditText>(Resource.Id.passwordfield);
                PasswordRepeatEditText = FindViewById<EditText>(Resource.Id.passwordrepeatfield);
                GenderEditText = FindViewById<EditText>(Resource.Id.Genderfield);
                RegisterButton = FindViewById<AppCompatButton>(Resource.Id.registerButton);
                MainLinearLayout = FindViewById<LinearLayout>(Resource.Id.mainLinearLayout);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                PhoneEditText = FindViewById<EditText>(Resource.Id.Phonefield);
                PhoneView = FindViewById<View>(Resource.Id.PhoneView);
                 
                ProgressBar.Visibility = ViewStates.Gone; 
                RegisterButton.Visibility = ViewStates.Visible;
                  
                ChkAgree = FindViewById<CheckBox>(Resource.Id.termCheckBox);
                TxtTermsOfService = FindViewById<TextView>(Resource.Id.secTermTextView);
                TxtPrivacy = FindViewById<TextView>(Resource.Id.secPrivacyTextView);
                 
                GenderEditText.Visibility = AppSettings.ShowGenderOnRegister ? ViewStates.Visible : ViewStates.Gone;
                Methods.SetFocusable(GenderEditText);
                 
                var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                if (smsOrEmail == "sms")
                {
                    PhoneEditText.Visibility = ViewStates.Visible;
                    PhoneView.Visibility = ViewStates.Visible;
                }
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
                    ToolBarLayout.Click += ToolBarOnClick;
                    MainLinearLayout.Click += Main_LoginPage_Click;
                    RegisterButton.Click += RegisterButtonOnClick;
                    TxtTermsOfService.Click += TxtTermsOfServiceOnClick;
                    TxtPrivacy.Click += TxtPrivacyOnClick;
                    GenderEditText.Touch += GenderEditTextOnTouch;
                }
                else
                {
                    ToolBarLayout.Click -= ToolBarOnClick;
                    MainLinearLayout.Click -= Main_LoginPage_Click;
                    RegisterButton.Click -= RegisterButtonOnClick;
                    TxtTermsOfService.Click -= TxtTermsOfServiceOnClick;
                    TxtPrivacy.Click -= TxtPrivacyOnClick;
                    GenderEditText.Touch -= GenderEditTextOnTouch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GenderEditTextOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    arrayAdapter.AddRange(from item in ListUtils.SettingsSiteList?.Genders select item.Value);
                }
                else
                {
                    arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                    arrayAdapter.Add(GetText(Resource.String.Radio_Female));
                }

                dialogList.Title(GetText(Resource.String.Lbl_Gender));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Events

        private void TxtPrivacyOnClick(object sender, EventArgs eventArgs)
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

        private void TxtTermsOfServiceOnClick(object sender, EventArgs eventArgs)
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
     
        private void ToolBarOnClick(object sender, EventArgs eventArgs)
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

        private void Main_LoginPage_Click(object sender, EventArgs e)
        {
            try
            {
                if (ChkAgree.Checked)
                {
                    HideKeyboard();
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Error_Terms), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
               Methods.DisplayReportResultTrack(ex);
            }
        }

        private async void RegisterButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (ChkAgree.Checked)
                {
                    if (Methods.CheckConnectivity())
                    {
                        if (!string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) ||
                            !string.IsNullOrEmpty(PasswordEditText.Text) ||
                            !string.IsNullOrEmpty(PasswordRepeatEditText.Text) ||
                            !string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")))
                        {
                            HideKeyboard();

                            if (AppSettings.ShowGenderOnRegister && string.IsNullOrEmpty(GenderStatus))
                            {
                                ProgressBar.Visibility = ViewStates.Invisible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                            if (smsOrEmail == "sms" && string.IsNullOrEmpty(PhoneEditText.Text))
                            {
                                ProgressBar.Visibility = ViewStates.Invisible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(EmailEditText.Text.Replace(" ", ""));
                            if (!check)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this,
                                    GetText(Resource.String.Lbl_VerificationFailed),
                                    GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                            }
                            else
                            {
                                if (PasswordRepeatEditText.Text == PasswordEditText.Text)
                                {
                                    ProgressBar.Visibility = ViewStates.Visible;

                                    var (apiStatus, respond) = await RequestsAsync.Auth.CreateAccountAsync(UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text, PasswordRepeatEditText.Text, EmailEditText.Text.Replace(" ", ""),
                                        GenderStatus, PhoneEditText.Text,"", UserDetails.DeviceId);
                                    switch (apiStatus)
                                    {
                                        case 200:
                                        {
                                            if (respond is CreatAccountObject result)
                                            {
                                                SetDataLogin(result);
                                             
                                                if (AppSettings.ShowWalkTroutPage)
                                                {
                                                    Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                                    newIntent.PutExtra("class", "register");
                                                    StartActivity(newIntent);
                                                }
                                                else
                                                {
                                                    StartActivity(AppSettings.ShowSuggestedUsersOnRegister
                                                        ? new Intent(this, typeof(SuggestionsUsersActivity))
                                                        : new Intent(this, typeof(TabbedMainActivity)));
                                                }
                                            }

                                            ProgressBar.Visibility = ViewStates.Invisible;
                                            Finish();
                                            break;
                                        }
                                        case 220:
                                        {
                                            if (respond is AuthMessageObject messageObject)
                                            {
                                                switch (smsOrEmail)
                                                {
                                                    case "sms":
                                                    {
                                                        UserDetails.Username = UsernameEditText.Text;
                                                        UserDetails.FullName = UsernameEditText.Text;
                                                        UserDetails.Password = PasswordEditText.Text;
                                                        UserDetails.UserId = messageObject.UserId;
                                                        UserDetails.Status = "Pending";
                                                        UserDetails.Email = EmailEditText.Text;

                                                        //Insert user data to database
                                                        var user = new DataTables.LoginTb
                                                        {
                                                            UserId = UserDetails.UserId,
                                                            AccessToken = UserDetails.AccessToken,
                                                            Cookie = UserDetails.Cookie,
                                                            Username = UserDetails.Username,
                                                            Password = UserDetails.Password,
                                                            Status = "Pending",
                                                            Lang = "",
                                                            DeviceId = UserDetails.DeviceId,
                                                            Email = UserDetails.Email,
                                                        };

                                                        ListUtils.DataUserLoginList.Clear();
                                                        ListUtils.DataUserLoginList.Add(user);

                                                        var dbDatabase = new SqLiteDatabase();
                                                        dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                                

                                                        Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                                                        newIntent.PutExtra("TypeCode", "AccountSms");
                                                        StartActivity(newIntent);
                                                        break;
                                                    }
                                                    case "mail":
                                                    {
                                                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                                        dialog.Title(GetText(Resource.String.Lbl_ActivationSent));
                                                        dialog.Content(GetText(Resource.String.Lbl_ActivationDetails).Replace("@", EmailEditText.Text));
                                                        dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                                                        dialog.AlwaysCallSingleChoiceCallback();
                                                        dialog.Build().Show();
                                                        break;
                                                    }
                                                    default:
                                                        ProgressBar.Visibility = ViewStates.Invisible;
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), messageObject.Message, GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                }
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
                                                            GetText(Resource.String.Lbl_ErrorRegister_3),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "4":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_4),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "5":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_something_went_wrong),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "6":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_6),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "7":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_7),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "8":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_8),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "9":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_9),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "10":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_10),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "11":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security),
                                                            GetText(Resource.String.Lbl_ErrorRegister_11),
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    default:
                                                        Methods.DialogPopup.InvokeAndShowDialog(this,
                                                            GetText(Resource.String.Lbl_Security), errorText,
                                                            GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                }
                                            }

                                            ProgressBar.Visibility = ViewStates.Invisible;
                                            break;
                                        }
                                        case 404:
                                            ProgressBar.Visibility = ViewStates.Invisible;
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                            break;
                                    }
                                }
                                else
                                {
                                    ProgressBar.Visibility = ViewStates.Invisible;

                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_Error_Register_password),
                                        GetText(Resource.String.Lbl_Ok));
                                }
                            }
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Invisible;

                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                                GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Invisible;

                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                            GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning),
                        GetText(Resource.String.Lbl_You_can_not_access_your_disapproval),
                        GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Invisible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message,
                    GetText(Resource.String.Lbl_Ok));
            }
        }

        #endregion

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    Finish();
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    GenderEditText.Text = itemString;

                    var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                    GenderStatus = key ?? "male";
                }
                else
                {
                    if (itemString == GetText(Resource.String.Radio_Male))
                    {
                        GenderEditText.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                    }
                    else if (itemString == GetText(Resource.String.Radio_Female))
                    {
                        GenderEditText.Text = GetText(Resource.String.Radio_Female);
                        GenderStatus = "female";
                    }
                    else
                    {
                        GenderEditText.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                    }
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

        private void SetDataLogin(CreatAccountObject auth)
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
                UserDetails.Email = EmailEditText.Text;
                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UsernameEditText.Text,
                    Password = PasswordEditText.Text,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,

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
    }
}