using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.ChatWindow.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.ChatWindow
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MessageInfoActivity : BaseActivity
    {
        #region Variables Basic

        private MessageAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private TextView TxtRead, TxtTimeRead, TxtDelivered, TxtTimeDelivered;
        private string UserId = "", MainChatColor;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);

                MainChatColor = Intent?.GetStringExtra("MainChatColor") ?? "";
                SetTheme(MainChatColor);

                // Create your application here
                SetContentView(Resource.Layout.InfoMessagesLayout);

                UserId = Intent?.GetStringExtra("UserId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadData();
                AdsGoogle.Ad_RewardedVideo(this);
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

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                TxtRead = (TextView)FindViewById(Resource.Id.tvRead);
                TxtTimeRead = (TextView)FindViewById(Resource.Id.timeRead);
                TxtDelivered = (TextView)FindViewById(Resource.Id.tvDelivered);
                TxtTimeDelivered = (TextView)FindViewById(Resource.Id.timeDelivered);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_MessageInfo);

                    toolbar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));
                }
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
                MAdapter = new MessageAdapter(this, UserId, false)
                {
                    DifferList = new ObservableCollection<AdapterModelsClassMessage>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        #endregion

        #region Load Data 

        private void LoadData()
        {
            try
            {
                var item = JsonConvert.DeserializeObject<MessageDataExtra>(Intent?.GetStringExtra("SelectedItem") ?? "");
                if (item != null)
                {
                    MAdapter.DifferList.Add(new AdapterModelsClassMessage
                    {
                        MesData = item,
                        TypeView = item.ModelType
                    });
                    MAdapter.NotifyDataSetChanged();

                    DateTime dateTime = Methods.Time.UnixTimeStampToDateTime(int.Parse(item.Time));
                    TxtTimeDelivered.Text = dateTime.ToLongDateString() + ", " + dateTime.ToShortTimeString();

                    if (item.Seen != "0")
                    {
                        DateTime dateTimeSeen = Methods.Time.UnixTimeStampToDateTime(int.Parse(item.Seen));
                        TxtTimeRead.Text = dateTimeSeen.ToLongDateString() + ", " + dateTimeSeen.ToShortTimeString();
                    }
                    else
                    {
                        TxtTimeRead.Text = "---";
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetTheme(string color)
        {
            try
            {
                if (color.Contains("b582af"))
                {
                    SetTheme(Resource.Style.Chatththemeb582af);
                }
                else if (color.Contains("a84849") || color.Contains("a52729"))
                {
                    SetTheme(Resource.Style.Chatththemea84849);
                }
                else if (color.Contains("f9c270"))
                {
                    SetTheme(Resource.Style.Chatththemef9c270);
                }
                else if (color.Contains("70a0e0"))
                {
                    SetTheme(Resource.Style.Chatththeme70a0e0);
                }
                else if (color.Contains("56c4c5"))
                {
                    SetTheme(Resource.Style.Chatththeme56c4c5);
                }
                else if (color.Contains("f33d4c"))
                {
                    SetTheme(Resource.Style.Chatththemef33d4c);
                }
                else if (color.Contains("a1ce79"))
                {
                    SetTheme(Resource.Style.Chatththemea1ce79);
                }
                else if (color.Contains("a085e2"))
                {
                    SetTheme(Resource.Style.Chatththemea085e2);
                }
                else if (color.Contains("ed9e6a"))
                {
                    SetTheme(Resource.Style.Chatththemeed9e6a);
                }
                else if (color.Contains("2b87ce"))
                {
                    SetTheme(Resource.Style.Chatththeme2b87ce);
                }
                else if (color.Contains("f2812b"))
                {
                    SetTheme(Resource.Style.Chatththemef2812b);
                }
                else if (color.Contains("0ba05d"))
                {
                    SetTheme(Resource.Style.Chatththeme0ba05d);
                }
                else if (color.Contains("0e71ea"))
                {
                    SetTheme(Resource.Style.Chatththeme0e71ea);
                }
                else if (color.Contains("aa2294"))
                {
                    SetTheme(Resource.Style.Chatththemeaa2294);
                }
                else if (color.Contains("f9a722"))
                {
                    SetTheme(Resource.Style.Chatththemef9a722);
                }
                else if (color.Contains("008484"))
                {
                    SetTheme(Resource.Style.Chatththeme008484);
                }
                else if (color.Contains("5462a5"))
                {
                    SetTheme(Resource.Style.Chatththeme5462a5);
                }
                else if (color.Contains("fc9cde"))
                {
                    SetTheme(Resource.Style.Chatththemefc9cde);
                }
                else if (color.Contains("fc9cde"))
                {
                    SetTheme(Resource.Style.Chatththemefc9cde);
                }
                else if (color.Contains("51bcbc"))
                {
                    SetTheme(Resource.Style.Chatththeme51bcbc);
                }
                else if (color.Contains("c9605e"))
                {
                    SetTheme(Resource.Style.Chatththemec9605e);
                }
                else if (color.Contains("01a5a5"))
                {
                    SetTheme(Resource.Style.Chatththeme01a5a5);
                }
                else if (color.Contains("056bba"))
                {
                    SetTheme(Resource.Style.Chatththeme056bba);
                }
                else
                {
                    //Default Color >> AppSettings.MainColor
                    SetTheme(Resource.Style.Chatththemedefault);
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