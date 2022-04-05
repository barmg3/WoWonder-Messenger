using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Newtonsoft.Json;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using WoWonder.Activities.Authentication;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Message;
using Exception = Java.Lang.Exception;

namespace WoWonder.Activities
{
    [Activity(Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true, Theme = "@style/SplashScreenTheme", Exported = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreenActivity : AppCompatActivity  
    { 
        protected override async void OnCreate(Bundle savedInstanceState) 
        {
            try
            {
                if (Build.VERSION.SdkInt == BuildVersionCodes.S)
                    Androidx.Core.Splashscreen.SplashScreen.InstallSplashScreen(this);

                base.OnCreate(savedInstanceState);

                if (AppSettings.ShowSettingsFingerprintLock && UserDetails.FingerprintLock)
                {
                    var availability = await CrossFingerprint.Current.IsAvailableAsync();
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M && availability)
                    {
                        await Task.Run(() =>
                        {
                            try
                            {
                                CrossFingerprint.SetCurrentActivityResolver(() => this);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });

                        var request = new AuthenticationRequestConfiguration("Prove you have fingers!", "Because without it you can't have access");
                        var result = await CrossFingerprint.Current.AuthenticateAsync(request);
                        if (result.Authenticated)
                        {
                            // do secret stuff :)
                            Task startupWork = new Task(FirstRunExcite);
                            startupWork.Start(); 
                        }
                        else
                        {
                            // not allowed to do secret stuff :(   
                            Console.WriteLine(result.ErrorMessage);
                        }
                    }
                }
                else
                {
                    // not allowed to do secret stuff :(   
                    Task startupWork = new Task(FirstRunExcite);
                    startupWork.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void FirstRunExcite()
        {
            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(AppSettings.Lang))
                        {
                            LangController.SetApplicationLang(this, AppSettings.Lang);
                        }
                        else
                        {
#pragma warning disable 618
                            UserDetails.LangName = (int)Build.VERSION.SdkInt < 25 ? Resources?.Configuration?.Locale?.Language.ToLower() : Resources?.Configuration?.Locales?.Get(0)?.Language?.ToLower() ?? Resources?.Configuration?.Locale?.Language?.ToLower();
#pragma warning restore 618
                            LangController.SetApplicationLang(this, UserDetails.LangName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                 
                if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                { 
                    var userId = Intent?.GetStringExtra("UserID") ?? ""; 
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var type = Intent?.GetStringExtra("type") ?? ""; //SendMsgProduct , OpenChat , OpenChatApp , OpenChatPage
                        OpenChat(type);
                    }
                    else
                    {
                        switch (UserDetails.Status)
                        {
                            case "Active":
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                break;
                            default:
                                StartActivity(new Intent(this, typeof(LoginActivity)));
                                break;
                        }
                    }
                }
                else
                {
                    var userId = Intent?.GetStringExtra("UserID") ?? "";
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var type = Intent?.GetStringExtra("type") ?? ""; //SendMsgProduct , OpenChat , OpenChatApp , OpenChatPage
                        OpenChat(type);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(LoginActivity)));
                    } 
                } 

                OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out); 
                Finish();
            }
            catch (Exception e)
            { 
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">SendMsgProduct , OpenChat , OpenChatApp , OpenChatPage</param>
        private void OpenChat(string type)
        {
            try
            {
                Intent intent;

                switch (type)
                {
                    case "OpenChat":
                    {
                        intent = new Intent(this, typeof(ChatWindowActivity));

                        intent.PutExtra("Notifier", "Notifier");
                        intent.PutExtra("App", "Timeline");
                        intent.PutExtra("TypeChat", "user");
                        intent.PutExtra("ShowEmpty", "no");

                        //new ChatObject() { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name , Avatar = DataObject.UserData .Avatar}

                        var itemObject = JsonConvert.DeserializeObject<ChatObject>(Intent?.GetStringExtra("itemObject") ?? "");
                        if (itemObject != null)
                        {
                            intent.PutExtra("ChatId", itemObject.ChatId); 
                            intent.PutExtra("UserID", itemObject.UserId); // to_id  
                            intent.PutExtra("Name", itemObject.Name); 
                            intent.PutExtra("Avatar", itemObject.Avatar);  
                        }
                        StartActivity(intent);
                        break;
                    }
                    case "SendMsgProduct":
                    {
                        intent = new Intent(this, typeof(ChatWindowActivity));
                        intent.PutExtra("Notifier", "Notifier");
                        intent.PutExtra("App", "Timeline");
                        intent.PutExtra("TypeChat", "SendMsgProduct");
                        intent.PutExtra("ShowEmpty", "no");

                        //new ChatObject() { UserId = ProductData.Seller.UserId , Avatar = ProductData.Seller.Avatar, Name = ProductData.Seller.Name , LastMessage = new LastMessageUnion()
                        //{LastMessageClass = new MessageData() { ProductId = ProductData.Id , Product = new ProductUnion(){ProductClass = ProductData}} }} );

                        var itemObject = JsonConvert.DeserializeObject<ChatObject>(Intent?.GetStringExtra("itemObject") ?? "");
                        if (itemObject != null)
                        {
                            intent.PutExtra("ChatId", itemObject.ChatId);  
                            intent.PutExtra("UserID", itemObject.UserId); // to_id  
                            intent.PutExtra("Name", itemObject.Name);  
                            intent.PutExtra("Avatar", itemObject.Avatar);

                            if (itemObject.LastMessage.LastMessageClass?.Product != null)
                            { 
                                intent.PutExtra("ProductId", itemObject.LastMessage.LastMessageClass.ProductId);
                                intent.PutExtra("ProductClass", JsonConvert.SerializeObject(itemObject.LastMessage.LastMessageClass.Product?.ProductClass));
                            }
                        }
                        StartActivity(intent);
                        break;
                    }
                    case "OpenChatPage":
                    {
                        intent = new Intent(this, typeof(PageChatWindowActivity));
                        intent.PutExtra("Notifier", "Notifier");
                        intent.PutExtra("App", "Timeline");
                        intent.PutExtra("TypeChat", "");
                        intent.PutExtra("ShowEmpty", "no");

                        //new ChatObject(){UserId = UserId , PageId = PageId , PageName = PageData.PageName , Avatar = PageData.Avatar});
                        var itemObject = JsonConvert.DeserializeObject<ChatObject>(Intent?.GetStringExtra("itemObject") ?? "");
                        if (itemObject != null)
                        {
                            intent.PutExtra("ChatId", itemObject.ChatId);
                            intent.PutExtra("UserID", itemObject.UserId); // to_id  
                            intent.PutExtra("PageId", itemObject.PageId);  
                            intent.PutExtra("PageName", itemObject.PageName);  
                            intent.PutExtra("Avatar", itemObject.Avatar); 
                        }
                        StartActivity(intent);
                        break;
                    }
                }
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
    }
}