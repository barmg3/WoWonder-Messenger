using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Widget;
using Bumptech.Glide;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Authentication;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Story.Service;
using WoWonder.Activities.Tab.Services;
using WoWonder.Frameworks.Floating;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SocketSystem;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Exception = System.Exception;
using File = Java.IO.File;
using Thread = System.Threading.Thread;


namespace WoWonder.Helpers.Controller
{
    internal static class ApiRequest
    {
        //############# DONT'T MODIFY HERE #############

        //Main API URLS
        //*********************************************************

        private static readonly string ApiGetSearchGif = "https://api.giphy.com/v1/gifs/search?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g&q=";
        private static readonly string ApiGeTrendingGif = "https://api.giphy.com/v1/gifs/trending?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g";
        private static readonly string ApiGetTimeZone = "http://ip-api.com/json/";
        //########################## Client ##########################
         
        public static async Task<string> GetTimeZoneAsync()
        {
            try
            {
                if (AppSettings.AutoCodeTimeZone)
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGetTimeZone);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TimeZoneObject>(json); 
                    if (data != null)
                    {
                        UserDetails.Lat = data.Lat.ToString(CultureInfo.InvariantCulture);
                        UserDetails.Lng = data.Lon.ToString(CultureInfo.InvariantCulture);

                        return data.Timezone;
                    }
                }

                return AppSettings.CodeTimeZone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return AppSettings.CodeTimeZone;
            } 
        }
        
        public static async Task<ObservableCollection<GifGiphyClass.Datum>> SearchGif(string searchKey, string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
                    return new ObservableCollection<GifGiphyClass.Datum>();
                }
                else
                { 
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGetSearchGif + searchKey + "&offset=" + offset);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (data.meta.Status == 200)
                        {
                            return new ObservableCollection<GifGiphyClass.Datum>(data.Data);
                        }
                        else
                        {
                            return new ObservableCollection<GifGiphyClass.Datum>();
                        }
                    }
                    else
                    {
                        return new ObservableCollection<GifGiphyClass.Datum>();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<GifGiphyClass.Datum>();
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> TrendingGif(string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
                    return new ObservableCollection<GifGiphyClass.Datum>();
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGeTrendingGif + "&offset=" + offset);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (data.meta.Status == 200)
                        {
                            return new ObservableCollection<GifGiphyClass.Datum>(data.Data);
                        }
                        else
                        {
                            return new ObservableCollection<GifGiphyClass.Datum>();
                        }
                    }
                    else
                    {
                        return new ObservableCollection<GifGiphyClass.Datum>();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<GifGiphyClass.Datum>();
            }
        }

        public static async Task<GetSiteSettingsObject.ConfigObject> GetSettings_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                //await SetLangUserAsync().ConfigureAwait(false);

                (var apiStatus, dynamic respond) = await Current.GetSettingsAsync();

                if (apiStatus != 200 || respond is not GetSiteSettingsObject result || result.Config == null)
                    return Methods.DisplayReportResult(context, respond);

                ListUtils.SettingsSiteList = result.Config;

                AppSettings.OneSignalAppId = result.Config.AndroidNPushId;
                OneSignalNotification.Instance.RegisterNotificationDevice(context);

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateSettings(result.Config);

                await Task.Factory.StartNew(() =>
                {
                    try
                    {   
                        //Products Categories
                        var listProducts = result.Config.ProductsCategories?.Select(cat => new Classes.Categories
                        {
                            CategoriesId = cat.Key,
                            CategoriesName = Methods.FunString.DecodeString(cat.Value),
                            CategoriesColor = "#ffffff",
                            SubList = new List<SubCategories>()
                        }).ToList();

                        ListUtils.ListCategoriesProducts.Clear();
                        if (listProducts?.Count > 0)
                            ListUtils.ListCategoriesProducts = new ObservableCollection<Classes.Categories>(listProducts);

                        if (result.Config.ProductsSubCategories?.SubCategoriesList?.Count > 0)
                        {
                            //Sub Categories Products
                            foreach (var sub in result.Config.ProductsSubCategories?.SubCategoriesList)
                            {
                                var subCategories = result.Config.ProductsSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                if (subCategories?.Count > 0)
                                {
                                    var cat = ListUtils.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                    if (cat != null)
                                    {
                                        foreach (var pairs in subCategories)
                                        {
                                            cat.SubList.Add(pairs);
                                        }
                                    }
                                }
                            }
                        }
                           
                        if (AppSettings.SetApisReportMode)
                        { 
                            if (ListUtils.ListCategoriesProducts.Count == 0)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(context, "ReportMode", "List Categories Products Not Found, Please check api get_site_settings ", "Close");
                            } 
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).ConfigureAwait(false);

                return result.Config;
            }
            else
            {
                ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                return null!;
            }
        }

        private static async Task SetLangUserAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Current.AccessToken) || !AppSettings.SetLangUser)
                    return;

                string lang = "english";
                if (UserDetails.LangName.Contains("en"))
                    lang = "english";
                else if (UserDetails.LangName.Contains("ar"))
                    lang = "arabic";
                else if (UserDetails.LangName.Contains("de"))
                    lang = "german";
                else if (UserDetails.LangName.Contains("el"))
                    lang = "greek";
                else if (UserDetails.LangName.Contains("es"))
                    lang = "spanish";
                else if (UserDetails.LangName.Contains("fr"))
                    lang = "french";
                else if (UserDetails.LangName.Contains("it"))
                    lang = "italian";
                else if (UserDetails.LangName.Contains("ja"))
                    lang = "japanese";
                else if (UserDetails.LangName.Contains("nl"))
                    lang = "dutch";
                else if (UserDetails.LangName.Contains("pt"))
                    lang = "portuguese";
                else if (UserDetails.LangName.Contains("ro"))
                    lang = "romanian";
                else if (UserDetails.LangName.Contains("ru"))
                    lang = "russian";
                else if (UserDetails.LangName.Contains("sq"))
                    lang = "albanian";
                else if (UserDetails.LangName.Contains("sr"))
                    lang = "serbian";
                else if (UserDetails.LangName.Contains("tr"))
                    lang = "turkish";
                //else
                //    lang = string.IsNullOrEmpty(UserDetails.LangName) ? AppSettings.Lang : "";

                await Task.Factory.StartNew(() =>
                {
                    if (lang != "")
                    {
                        Dictionary<string, string> dataPrivacy = new Dictionary<string, string>();

                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataPrivacy = new Dictionary<string, string>();

                            dataUser.Language = lang;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                        }

                        dataPrivacy.Add("language", lang);

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                        else
                            ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task Get_MyProfileData_Api()
        {
            if (Methods.CheckConnectivity())
            {
                var(apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId , "user_data,followers,following");
                if (apiStatus == 200)
                {
                    if (respond is GetUserDataObject result)
                    {
                        UserDetails.Avatar = result.UserData.Avatar;
                        UserDetails.Cover = result.UserData.Cover;
                        UserDetails.Username = result.UserData.Username;
                        UserDetails.FullName = result.UserData.Name;
                        UserDetails.Email = result.UserData.Email;

                        ListUtils.MyProfileList = new ObservableCollection<UserDataObject> {result.UserData};

                        await Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                SqLiteDatabase dbDatabase = new SqLiteDatabase();

                                // user_data
                                if (result.UserData != null)
                                {
                                    //Insert Or Update All data user_data
                                    dbDatabase.Insert_Or_Update_To_MyProfileTable(result.UserData);
                                }

                                if (result.Following.Count > 0)
                                {
                                    //Insert Or Update All data Groups
                                    dbDatabase.Insert_Or_Replace_MyContactTable(new ObservableCollection<UserDataObject>(result.Following));
                                }

                                if (result.Followers.Count > 0)
                                {
                                    //Insert Or Update All data Groups
                                    ListUtils.MyFollowersList = new ObservableCollection<UserDataObject>(result.Followers);
                                } 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            } 
                        }); 
                    }
                }
            }
        }

        public static async Task GetArchivedChats()
        {
            if (Methods.CheckConnectivity() && !string.IsNullOrEmpty(UserDetails.AccessToken) && AppSettings.EnableChatArchive)
            {
                var (apiStatus, respond) = await RequestsAsync.Message.GetArchivedChatsAsync().ConfigureAwait(false);
                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    //Methods.DisplayReportResult(respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.ArchiveList.FirstOrDefault(a => a.ChatId == item.ChatId) where check == null select WoWonderTools.FilterDataLastChatNewV(item))
                        {
                            ListUtils.ArchiveList?.Add(new Classes.LastChatArchive
                            {
                                ChatType = item.ChatType,
                                ChatId = item.ChatId,
                                UserId = item.UserId,
                                GroupId = item.GroupId,
                                PageId = item.PageId,
                                Name = item.Name,
                                IdLastMessage = item?.LastMessage.LastMessageClass?.Id ?? "",
                                LastChat = item,
                            });
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORUpdateORDelete_ListArchive(ListUtils.ArchiveList?.ToList());
                    }
                }
            }
        }

        public static async Task GetPinChats()
        {
            if (Methods.CheckConnectivity() && !string.IsNullOrEmpty(UserDetails.AccessToken) && AppSettings.EnableChatPin)
            {
                var (apiStatus, respond) = await RequestsAsync.Message.GetPinChatsAsync().ConfigureAwait(false);
                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    //Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.PinList.FirstOrDefault(a => a.ChatId == item.ChatId) where check == null select WoWonderTools.FilterDataLastChatNewV(item))
                        {
                            if (item.IsArchive)
                                continue;

                            ListUtils.PinList?.Add(new Classes.LastChatArchive
                            {
                                ChatType = item.ChatType,
                                ChatId = item.ChatId,
                                UserId = item.UserId,
                                GroupId = item.GroupId,
                                PageId = item.PageId,
                                Name = item.Name,
                                IdLastMessage = item?.LastMessage.LastMessageClass?.Id ?? "",
                                LastChat = item,
                            });
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertORUpdateORDelete_ListPin(ListUtils.PinList?.ToList());
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////
        private static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Delete");

                    context?.RunOnUiThread(() =>
                    {
                        try
                        { 
                            context?.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                            context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                            Methods.Path.DeleteAll_FolderUser();

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.DropAll();

                            Runtime.GetRuntime()?.RunFinalization();
                            Runtime.GetRuntime()?.Gc();
                            TrimCache(context);

                            ListUtils.ClearAllList();

                            UserDetails.ClearAllValueUserDetails();

                            dbDatabase.CheckTablesStatus();

                            context.StopService(new Intent(context, typeof(StoryService)));
                            context.StopService(new Intent(context, typeof(ChatHeadService)));
                            ChatJobInfo.StopJob(context);

                            MainSettings.SharedData?.Edit()?.Clear()?.Commit();
                            MainSettings.InAppReview?.Edit()?.Clear()?.Commit();
                            MainSettings.LastPosition?.Edit()?.Clear()?.Commit();

                            Intent intent = new Intent(context, typeof(LoginActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            context.StartActivity(intent);
                            context.FinishAffinity();
                            context.Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }); 

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Logout");

                    context?.RunOnUiThread(() =>
                    {
                        try
                        {
                            context?.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                            context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                            Methods.Path.DeleteAll_FolderUser();

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.DropAll();

                            Runtime.GetRuntime()?.RunFinalization();
                            Runtime.GetRuntime()?.Gc();
                            TrimCache(context);

                            ListUtils.ClearAllList();

                            UserDetails.ClearAllValueUserDetails();

                            dbDatabase.CheckTablesStatus();

                            context.StopService(new Intent(context, typeof(StoryService)));
                            context.StopService(new Intent(context, typeof(ChatHeadService)));
                            ChatJobInfo.StopJob(context);

                            MainSettings.SharedData?.Edit()?.Clear()?.Commit();
                            MainSettings.InAppReview?.Edit()?.Clear()?.Commit();
                            MainSettings.LastPosition?.Edit()?.Clear()?.Commit();

                            Intent intent = new Intent(context, typeof(LoginActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            context.StartActivity(intent);
                            context.FinishAffinity();
                            context.Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void TrimCache(Activity context)
        {
            try
            {
                File dir = context?.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context?.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                if (context?.IsDestroyed != false)
                    return;

                Glide.Get(context)?.ClearMemory();
                new Thread(() =>
                {
                    try
                    {
                        Glide.Get(context)?.ClearDiskCache();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                string[] children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private static void Reset()
        {
            try
            {
                MentionActivity.MAdapter = null!;

                
                Current.AccessToken = string.Empty;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    UserDetails.Socket?.DisconnectSocket();

                switch (type)
                {
                    case "Logout":
                    {
                        if (Methods.CheckConnectivity())
                        {
                            await RequestsAsync.Auth.DeleteTokenAsync();
                        }

                        break;
                    }
                    case "Delete":
                    {
                        Methods.Path.DeleteAll_FolderUser();

                        if (Methods.CheckConnectivity())
                        {
                            await RequestsAsync.Auth.DeleteUserAsync(UserDetails.Password);
                        }

                        break;
                    }
                }

                if (AppSettings.ShowGoogleLogin && LoginActivity.MGoogleSignInClient != null)
                    if (Auth.GoogleSignInApi != null)
                    {
                        LoginActivity.MGoogleSignInClient.SignOut();
                        LoginActivity.MGoogleSignInClient = null!;
                    }

                if (AppSettings.ShowFacebookLogin)
                {
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }
                }

                OneSignalNotification.Instance.UnRegisterNotificationDevice();
                AppNotificationsManager.Instance.CancelAllNotification();

                ListUtils.ClearAllList();
                Reset();

                UserDetails.ClearAllValueUserDetails();

                Methods.DeleteNoteOnSD();

                GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}