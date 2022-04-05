using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MaterialDialogsCore;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Adcolony.Sdk;
using Newtonsoft.Json;
using WoWonder.Activities.Tab.Adapter;
using WoWonder.Frameworks.Agora;
using WoWonder.Frameworks.Twilio;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Message;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace WoWonder.Activities.Tab.Fragment
{
    public class LastCallsFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.IListCallback
    {
        #region Variables Basic

        public LastCallsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private Classes.CallUser DataUser;
        private AdView BannerAd;
        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetRecyclerViewAdapters();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                Get_CallUser();
                base.OnResume();
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

        public override void OnDestroy()
        {
            try
            {
                BannerAd?.Destroy();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                LinearLayout adContainer = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                if (AppSettings.ShowFbBannerAds)
                    BannerAd = AdsFacebook.InitAdView(Activity, adContainer, MRecycler);
                else
                    AdsColony.InitBannerAd(Activity, adContainer, AdColonyAdSize.Banner, MRecycler);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new LastCallsAdapter(Activity) { MCallUser = new ObservableCollection<Classes.CallUser>() };
                MAdapter.CallClick += MAdapterOnCallClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events 

        private void MAdapterOnCallClick(object sender, LastCallsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        DataUser = item;

                        switch (AppSettings.EnableAudioCall)
                        {
                            case true when AppSettings.EnableVideoCall:
                                {
                                    var arrayAdapter = new List<string>();
                                    var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                    arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Voice_call));
                                    arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Video_call));

                                    dialogList.Title(GetText(Resource.String.Lbl_Call));
                                    //dialogList.Content(GetText(Resource.String.Lbl_Select_Type_Call));
                                    dialogList.Items(arrayAdapter);
                                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                                    dialogList.AlwaysCallSingleChoiceCallback();
                                    dialogList.ItemsCallback(this).Build().Show();
                                    break;
                                }
                            // Video Call On
                            case false when AppSettings.EnableVideoCall:
                                try
                                {
                                    Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                                    switch (AppSettings.UseLibrary)
                                    {
                                        case SystemCall.Agora:
                                            intentVideoCall = new Intent(Context, typeof(AgoraVideoCallActivity));
                                            intentVideoCall.PutExtra("type", "Agora_video_calling_start");
                                            break;
                                        case SystemCall.Twilio:
                                            intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                                            intentVideoCall.PutExtra("type", "Twilio_video_calling_start");
                                            break;
                                    }

                                    var callUserObject = new CallUserObject
                                    {
                                        UserId = item.UserId,
                                        Avatar = item.Avatar,
                                        Name = item.Name,
                                        Data = new CallUserObject.DataCallUser()
                                    };
                                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                                    StartActivity(intentVideoCall);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }

                                break;
                            // Audio Call On
                            case true when AppSettings.EnableVideoCall == false:
                                try
                                {
                                    Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                                    switch (AppSettings.UseLibrary)
                                    {
                                        case SystemCall.Agora:
                                            intentVideoCall = new Intent(Context, typeof(AgoraAudioCallActivity));
                                            intentVideoCall.PutExtra("type", "Agora_audio_calling_start");
                                            break;
                                        case SystemCall.Twilio:
                                            intentVideoCall = new Intent(Context, typeof(TwilioAudioCallActivity));
                                            intentVideoCall.PutExtra("type", "Twilio_audio_calling_start");
                                            break;
                                    }

                                    var callUserObject = new CallUserObject
                                    {
                                        UserId = item.UserId,
                                        Avatar = item.Avatar,
                                        Name = item.Name,
                                        Data = new CallUserObject.DataCallUser()
                                    };
                                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                                    StartActivity(intentVideoCall);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }

                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Call

        private void Get_CallUser()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.Get_CallUserList();
                if (localList?.Count > 0)
                {
                    var countList = MAdapter.MCallUser.Count;
                    if (countList > 0)
                    {
                        foreach (var item in from item in localList let check = MAdapter.MCallUser.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            MAdapter.MCallUser.Insert(0, item);
                        }
                    }
                    else
                    {
                        MAdapter.MCallUser = new ObservableCollection<Classes.CallUser>(localList.OrderBy(a => a.Id));
                    }

                    MAdapter.NotifyDataSetChanged();
                }


                ShowEmptyPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.MCallUser.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoCall);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == Context.GetText(Resource.String.Lbl_Voice_call))
                {
                    string timeNow = DateTime.Now.ToString("hh:mm");
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time = Convert.ToString(unixTimestamp);

                    Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                    switch (AppSettings.UseLibrary)
                    {
                        case SystemCall.Agora:
                            intentVideoCall = new Intent(Context, typeof(AgoraAudioCallActivity));
                            intentVideoCall.PutExtra("type", "Agora_audio_calling_start");
                            break;
                        case SystemCall.Twilio:
                            intentVideoCall = new Intent(Context, typeof(TwilioAudioCallActivity));
                            intentVideoCall.PutExtra("type", "Twilio_audio_calling_start");
                            break;
                    }

                    var callUserObject = new CallUserObject
                    {
                        UserId = DataUser.UserId,
                        Avatar = DataUser.Avatar,
                        Name = DataUser.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                    StartActivity(intentVideoCall);
                }
                else if (itemString == Context.GetText(Resource.String.Lbl_Video_call))
                {
                    string timeNow = DateTime.Now.ToString("hh:mm");
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time = Convert.ToString(unixTimestamp);

                    Intent intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                    switch (AppSettings.UseLibrary)
                    {
                        case SystemCall.Agora:
                            intentVideoCall = new Intent(Context, typeof(AgoraVideoCallActivity));
                            intentVideoCall.PutExtra("type", "Agora_video_calling_start");
                            break;
                        case SystemCall.Twilio:
                            intentVideoCall = new Intent(Context, typeof(TwilioVideoCallActivity));
                            intentVideoCall.PutExtra("type", "Twilio_video_calling_start");
                            break;
                    }

                    var callUserObject = new CallUserObject
                    {
                        UserId = DataUser.UserId,
                        Avatar = DataUser.Avatar,
                        Name = DataUser.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                    StartActivity(intentVideoCall);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {

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

        #endregion

    }
}