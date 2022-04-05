using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager2.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.Story.Adapter;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Stories;
using WoWonder.Library.Anjo.Stories.DragView;
using WoWonderClient.Classes.Story;
using Exception = System.Exception;

namespace WoWonder.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/DragTransparentBlack", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class StoryDetailsActivity : AppCompatActivity
    {
        #region Variables Basic
        private readonly string KeySelectedPage = "KEY_SELECTED_PAGE";

        public ViewPager2 Pager;
        public DragToClose DragToClose;
        private StoriesPagerAdapter MAdapter;
        private int SelectedPage, StoriesCount;
        private ObservableCollection<StoryDataObject> DataStories;
        private string UserId;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this, true);

                if (savedInstanceState != null)
                {
                    SelectedPage = savedInstanceState.GetInt(KeySelectedPage);
                }

                // Create your application here
                SetContentView(Resource.Layout.StoryDetailsLayout);

                if (Intent != null)
                {
                    UserId = Intent.GetStringExtra("UserId") ?? "";
                    StoriesCount = Intent.GetIntExtra("StoriesCount", 0);
                    SelectedPage = Intent.GetIntExtra("IndexItem", 0);
                    DataStories = JsonConvert.DeserializeObject<ObservableCollection<StoryDataObject>>(Intent?.GetStringExtra("DataItem") ?? "");
                }

                //Get Value And Set Toolbar
                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            try
            {
                base.OnSaveInstanceState(outState);
                outState.PutInt(KeySelectedPage, Pager.CurrentItem);
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
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                DestroyBasic();

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                Pager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                DragToClose = FindViewById<DragToClose>(Resource.Id.drag_to_close);

                MAdapter = SelectedPage > 0 ? new StoriesPagerAdapter(this, StoriesCount, DataStories, SelectedPage) : new StoriesPagerAdapter(this, StoriesCount, DataStories);

                //Pager.CurrentItem = MAdapter.ItemCount;
                //Pager.OffscreenPageLimit = 0;

                Pager.Orientation = ViewPager2.OrientationHorizontal;
                Pager.SetPageTransformer(new CustomViewPageTransformer(TransformType.Flow));
                Pager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                Pager.Adapter = MAdapter;
                Pager.Adapter.NotifyDataSetChanged();

                Pager.SetCurrentItem(SelectedPage, false);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                Pager = null!;
                DragToClose = null!;
                MAdapter = null!;
                SelectedPage = 0;
                DataStories = null!;
                UserId = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                var instance = ViewStoryFragment.GetInstance();
                switch (requestCode)
                {
                    case 5326 when resultCode == Result.Ok:
                        instance?.StoriesProgress?.Resume();
                        instance?.StoryStateListener?.OnResume();
                        break;
                    case 5320 when resultCode == Result.Ok:
                        {
                            var isDelete = data.GetBooleanExtra("isDelete", false);
                            if (isDelete)
                            {
                                Finish();
                            }
                            else
                            {
                                instance?.StoriesProgress?.Resume();
                                instance?.StoryStateListener?.OnResume();
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly StoryDetailsActivity Activity;

            public MyOnPageChangeCallback(StoryDetailsActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                try
                {
                    base.OnPageScrolled(position, positionOffset, positionOffsetPixels);

                    var instance = ViewStoryFragment.GetInstance();
                    instance?.StoryStateListener?.OnPause();
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }
    }
}