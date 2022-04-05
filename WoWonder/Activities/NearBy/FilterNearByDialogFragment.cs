using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.BottomSheet;
using WoWonder.Adapters;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using Exception = System.Exception;

namespace WoWonder.Activities.NearBy
{
    public class FilterNearByDialogFragment : BottomSheetDialogFragment, SeekBar.IOnSeekBarChangeListener, IDialogInterfaceOnShowListener
    {
        #region Variables Basic

        private PeopleNearByActivity ContextNearBy;
        private TextView TxtDistanceCount;
        private TextView ResetTextView;
        private SeekBar DistanceBar;
        private GendersAdapter GenderAdapter;
        private AppCompatButton ButtonOffline, ButtonOnline, BothStatusButton, BtnApply;
        private AppCompatButton BtnSingle, BtnRelationship, BtnMarried, BtnEngaged, BtnRelationAll;
        private ImageButton ButtonBack;
        private RecyclerView GenderRecycler;
        private int DistanceCount, Status;
        private string Gender, RelationshipId;

        #endregion

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            ContextNearBy = (PeopleNearByActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetNearByFilter, container, false);
                // View view = inflater.Inflate(Resource.Layout.BottomSheetNearByFilter, container, false); 
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

                Dialog.SetOnShowListener(this);

                InitComponent(view);
                SetRecyclerViewAdapters();

                ButtonBack.Click += IconBackOnClick;

                // status buttons click
                ButtonOffline.Click += ButtonOfflineOnClick;
                ButtonOnline.Click += ButtonOnlineOnClick;
                BothStatusButton.Click += BothStatusButtonOnClick;

                // relationship buttons click
                BtnSingle.Click += BtnSingle_Click;
                BtnRelationship.Click += BtnRelationship_Click;
                BtnMarried.Click += BtnMarried_Click;
                BtnEngaged.Click += BtnEngaged_Click;
                BtnRelationAll.Click += BtnRelationAll_Click;

                BtnApply.Click += BtnApplyOnClick;
                ResetTextView.Click += ResetTextViewOnClick;

                GetFilter();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnRelationAll_Click(object sender, EventArgs e)
        {
            RelationshipId = "5";

            SetRelationship();
        }

        private void BtnEngaged_Click(object sender, EventArgs e)
        {
            RelationshipId = "4";

            SetRelationship();
        }

        private void BtnMarried_Click(object sender, EventArgs e)
        {
            RelationshipId = "3";

            SetRelationship();
        }

        private void BtnRelationship_Click(object sender, EventArgs e)
        {
            RelationshipId = "2";

            SetRelationship();
        }

        private void BtnSingle_Click(object sender, EventArgs e)
        {
            RelationshipId = "1";

            SetRelationship();
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ButtonBack = view.FindViewById<ImageButton>(Resource.Id.ib_back);

                TxtDistanceCount = view.FindViewById<TextView>(Resource.Id.Distancenumber);
                DistanceBar = view.FindViewById<SeekBar>(Resource.Id.distanceSeeker);

                GenderRecycler = view.FindViewById<RecyclerView>(Resource.Id.GenderRecyler);

                ButtonOffline = view.FindViewById<AppCompatButton>(Resource.Id.btn_status_offline);
                ButtonOnline = view.FindViewById<AppCompatButton>(Resource.Id.btn_status_online);
                BothStatusButton = view.FindViewById<AppCompatButton>(Resource.Id.btn_status_all);

                BtnSingle = view.FindViewById<AppCompatButton>(Resource.Id.btn_single);
                BtnRelationship = view.FindViewById<AppCompatButton>(Resource.Id.btn_relationship);
                BtnMarried = view.FindViewById<AppCompatButton>(Resource.Id.btn_married);
                BtnEngaged = view.FindViewById<AppCompatButton>(Resource.Id.btn_engaged);
                BtnRelationAll = view.FindViewById<AppCompatButton>(Resource.Id.btn_relation_all);

                BtnApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);
                ResetTextView = view.FindViewById<TextView>(Resource.Id.Resetbutton);

                DistanceBar.Max = 300;
                DistanceBar.SetOnSeekBarChangeListener(this);

                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.N:
                        DistanceBar.SetProgress(string.IsNullOrEmpty(UserDetails.NearByDistanceCount) ? 300 : Convert.ToInt32(UserDetails.NearByDistanceCount), true);
                        break;
                    // For API < 24 
                    default:
                        DistanceBar.Progress = string.IsNullOrEmpty(UserDetails.NearByDistanceCount) ? 300 : Convert.ToInt32(UserDetails.NearByDistanceCount);
                        break;
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
                GenderRecycler.HasFixedSize = true;
                GenderRecycler.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                GenderAdapter = new GendersAdapter(Activity)
                {
                    GenderList = new ObservableCollection<Classes.Gender>()
                };
                GenderRecycler.SetAdapter(GenderAdapter);
                GenderRecycler.NestedScrollingEnabled = false;
                GenderAdapter.NotifyDataSetChanged();
                GenderRecycler.Visibility = ViewStates.Visible;
                GenderAdapter.ItemClick += GenderAdapterOnItemClick;

                GenderAdapter.GenderList.Add(new Classes.Gender
                {
                    GenderId = "all",
                    GenderName = Activity.GetText(Resource.String.Lbl_All),
                    GenderColor = AppSettings.MainColor,
                    GenderSelect = false
                });

                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                        {
                            foreach (var (key, value) in ListUtils.SettingsSiteList?.Genders)
                            {
                                GenderAdapter.GenderList.Add(new Classes.Gender
                                {
                                    GenderId = key,
                                    GenderName = value,
                                    GenderColor = AppSettings.SetTabDarkTheme ? "#ffffff" : "#B0B6C3",
                                    GenderSelect = false
                                });
                            }

                            break;
                        }
                    default:
                        GenderAdapter.GenderList.Add(new Classes.Gender
                        {
                            GenderId = "male",
                            GenderName = Activity.GetText(Resource.String.Radio_Male),
                            GenderColor = AppSettings.SetTabDarkTheme ? "#ffffff" : "#B0B6C3",
                            GenderSelect = false
                        });
                        GenderAdapter.GenderList.Add(new Classes.Gender
                        {
                            GenderId = "female",
                            GenderName = Activity.GetText(Resource.String.Radio_Female),
                            GenderColor = AppSettings.SetTabDarkTheme ? "#ffffff" : "#B0B6C3",
                            GenderSelect = false
                        });
                        break;
                }

                GenderAdapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void GenderAdapterOnItemClick(object sender, GendersAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = GenderAdapter.GetItem(position);
                            if (item != null)
                            {
                                var check = GenderAdapter.GenderList.Where(a => a.GenderSelect).ToList();
                                switch (check.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var all in check)
                                                all.GenderSelect = false;
                                            break;
                                        }
                                }

                                item.GenderSelect = true;
                                GenderAdapter.NotifyDataSetChanged();

                                Gender = item.GenderId;
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.NearByGender = Gender;
                UserDetails.NearByDistanceCount = DistanceCount.ToString();
                UserDetails.NearByStatus = Status.ToString();
                UserDetails.NearByRelationship = RelationshipId;

                var dbDatabase = new SqLiteDatabase();
                var newSettingsFilter = new DataTables.NearByFilterTb
                {
                    DistanceValue = DistanceCount,
                    Gender = Gender,
                    Status = Status,
                    Relationship = RelationshipId
                };
                dbDatabase.InsertOrUpdate_NearByFilter(newSettingsFilter);


                ContextNearBy.MAdapter.UserList.Clear();
                ContextNearBy.MAdapter.NotifyDataSetChanged();

                ContextNearBy.SwipeRefreshLayout.Refreshing = true;
                ContextNearBy.SwipeRefreshLayout.Enabled = true;

                ContextNearBy.MainScrollEvent.IsLoading = false;

                ContextNearBy.MRecycler.Visibility = ViewStates.Visible;
                ContextNearBy.EmptyStateLayout.Visibility = ViewStates.Gone;

                Task.Factory.StartNew(() => ContextNearBy.StartApiService());

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Reset Value
        private void ResetTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var newSettingsFilter = new DataTables.NearByFilterTb
                {
                    DistanceValue = 0,
                    Gender = "all",
                    Status = 2,
                    Relationship = "5",
                };
                dbDatabase.InsertOrUpdate_NearByFilter(newSettingsFilter);


                Gender = "all";
                DistanceCount = 0;
                Status = 2;
                RelationshipId = "5";

                UserDetails.NearByGender = Gender;
                UserDetails.NearByDistanceCount = DistanceCount.ToString();
                UserDetails.NearByStatus = Status.ToString();
                UserDetails.NearByRelationship = RelationshipId;


                //////////////////////////// Status ////////////////////////////// 
                SetStatus();

                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.N:
                        DistanceBar.SetProgress(300, true);
                        break;
                    // For API < 24 
                    default:
                        DistanceBar.Progress = 300;
                        break;
                }

                TxtDistanceCount.Text = DistanceCount + " " + GetText(Resource.String.Lbl_km);

                // Relationship
                SetRelationship();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Select Status >> Both (2)
        private void BothStatusButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                //follow_button_profile_friends >> Un click
                //follow_button_profile_friends_pressed >> click
                Status = 2;

                SetStatus();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Select Status >> Online (1)
        private void ButtonOnlineOnClick(object sender, EventArgs e)
        {
            try
            {
                //follow_button_profile_friends >> Un click
                //follow_button_profile_friends_pressed >> click
                Status = 1;

                SetStatus();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Select Status >> Offline (0)
        private void ButtonOfflineOnClick(object sender, EventArgs e)
        {
            try
            {
                //follow_button_profile_friends >> Un click
                //follow_button_profile_friends_pressed >> click
                Status = 0;

                SetStatus();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region SeekBar

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            try
            {
                TxtDistanceCount.Text = progress + " " + GetText(Resource.String.Lbl_km);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {

        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                DistanceCount = seekBar.Progress;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void SetDistanceCount()
        {
            TxtDistanceCount.Text = DistanceCount + " " + GetText(Resource.String.Lbl_km);

            switch (Build.VERSION.SdkInt)
            {
                case >= BuildVersionCodes.N:
                    DistanceBar.SetProgress(DistanceCount == 0 ? 300 : DistanceCount, true);
                    break;
                // For API < 24 
                default:
                    DistanceBar.Progress = DistanceCount == 0 ? 300 : DistanceCount;
                    break;
            }
        }

        private void SetStatus()
        {
            switch (Status)
            {
                case 0:
                    ButtonOffline.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    ButtonOffline.SetTextColor(Color.ParseColor("#ffffff"));

                    BothStatusButton.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BothStatusButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    ButtonOnline.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    ButtonOnline.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    Status = 0;
                    break;
                case 1:
                    ButtonOnline.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    ButtonOnline.SetTextColor(Color.ParseColor("#ffffff"));

                    BothStatusButton.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BothStatusButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    ButtonOffline.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    ButtonOffline.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    Status = 1;
                    break;
                case 2:
                    BothStatusButton.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BothStatusButton.SetTextColor(Color.ParseColor("#ffffff"));

                    ButtonOnline.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    ButtonOnline.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    ButtonOffline.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    ButtonOffline.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    Status = 2;
                    break;
            }
        }

        private void SetRelationship()
        {
            switch (RelationshipId)
            {
                case "1":
                    BtnSingle.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnSingle.SetTextColor(Color.ParseColor("#ffffff"));

                    BtnRelationship.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationship.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnMarried.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnMarried.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnEngaged.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnEngaged.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnRelationAll.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationAll.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    RelationshipId = "1";
                    break;
                case "2":
                    BtnRelationship.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnRelationship.SetTextColor(Color.ParseColor("#ffffff"));

                    BtnSingle.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnSingle.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnMarried.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnMarried.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnEngaged.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnEngaged.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnRelationAll.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationAll.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    RelationshipId = "2";
                    break;
                case "3":
                    BtnMarried.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnMarried.SetTextColor(Color.ParseColor("#ffffff"));

                    BtnRelationship.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationship.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnSingle.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnSingle.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnEngaged.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnEngaged.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnRelationAll.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationAll.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    RelationshipId = "3";
                    break;
                case "4":
                    BtnEngaged.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnEngaged.SetTextColor(Color.ParseColor("#ffffff"));

                    BtnRelationship.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationship.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnMarried.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnMarried.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnSingle.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnSingle.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnRelationAll.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationAll.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    RelationshipId = "4";
                    break;
                case "5":
                    BtnRelationAll.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnRelationAll.SetTextColor(Color.ParseColor("#ffffff"));

                    BtnRelationship.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnRelationship.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnMarried.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnMarried.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnEngaged.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnEngaged.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    BtnSingle.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnSingle.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                    RelationshipId = "5";
                    break;
            }
        }

        private void GetFilter()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();

                var data = dbDatabase.GetNearByFilterById();
                if (data != null)
                {
                    Gender = data.Gender;
                    DistanceCount = data.DistanceValue;
                    Status = data.Status;
                    RelationshipId = data.Relationship;

                    //////////////////////////// Distance //////////////////////////////
                    SetDistanceCount();

                    //////////////////////////// Status //////////////////////////////
                    //Select Status >> Both (2)
                    //Select Status >> Online (1)
                    //Select Status >> Offline (0)
                    SetStatus();

                    //////////////////////////// Relationship //////////////////////////////
                    SetRelationship();
                }
                else
                {
                    var newSettingsFilter = new DataTables.NearByFilterTb
                    {
                        DistanceValue = 0,
                        Gender = "all",
                        Status = 2,
                        Relationship = "5",
                    };
                    dbDatabase.InsertOrUpdate_NearByFilter(newSettingsFilter);

                    Gender = "all";
                    DistanceCount = 0;
                    Status = 2;
                    RelationshipId = "5";

                    //////////////////////////// Status ////////////////////////////// 
                    SetStatus();

                    switch (Build.VERSION.SdkInt)
                    {
                        case >= BuildVersionCodes.N:
                            DistanceBar.SetProgress(300, true);
                            break;
                        // For API < 24 
                        default:
                            DistanceBar.Progress = 300;
                            break;
                    }

                    //TxtDistanceCount.Text = "300 " + GetText(Resource.String.Lbl_km);

                    SetRelationship();
                }


            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnShow(IDialogInterface dialog)
        {
            var d = dialog as BottomSheetDialog;
            var bottomSheet = d.FindViewById<View>(Resource.Id.design_bottom_sheet) as FrameLayout;
            var bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
            var layoutParams = bottomSheet.LayoutParameters;

            if (layoutParams != null)
                layoutParams.Height = Resources.DisplayMetrics.HeightPixels;
            bottomSheet.LayoutParameters = layoutParams;
            bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
        }
    }
}