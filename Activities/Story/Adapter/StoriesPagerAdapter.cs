using System;
using System.Collections.ObjectModel;
using Android.OS;
using Android.Runtime;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using Newtonsoft.Json;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Story;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace WoWonder.Activities.Story.Adapter
{
    public class StoriesPagerAdapter : FragmentStateAdapter
    { 
        private readonly int CountStory;
        private readonly int CurrentStoryPosition;
        private readonly ObservableCollection<StoryDataObject> DataStories;
        //private readonly Dictionary<int, Fragment> RegisteredFragments;

        public StoriesPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public StoriesPagerAdapter(Fragment fragment) : base(fragment)
        {
        }

        public StoriesPagerAdapter(FragmentActivity fragmentActivity) : base(fragmentActivity)
        {
        }

        public StoriesPagerAdapter(FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
        {
        }
           
        public StoriesPagerAdapter(FragmentActivity fragmentActivity, int size, ObservableCollection<StoryDataObject> dataStories) : base(fragmentActivity)
        {
            try
            {
                CountStory = size;
                DataStories = dataStories;
                //RegisteredFragments = new Dictionary<int, Fragment>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public StoriesPagerAdapter(FragmentActivity fragmentActivity, int size, ObservableCollection<StoryDataObject> dataStories, int currentStoryPosition) : base(fragmentActivity)
        {
            try
            {
                CountStory = size;
                DataStories = dataStories;
                CurrentStoryPosition = currentStoryPosition;
                //RegisteredFragments = new Dictionary<int, Fragment>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override int ItemCount => CountStory;

        public override Fragment CreateFragment(int position)
        {
            try
            { 
                Bundle bundle = new Bundle();
                bundle.PutInt("position", position);
                bundle.PutInt("currentStoryPosition", CurrentStoryPosition);

                StoryDataObject dataItem = DataStories[position]; 
                if (dataItem != null)
                    bundle.PutString("DataItem", JsonConvert.SerializeObject(dataItem));

                ViewStoryFragment viewStoryFragment = new ViewStoryFragment { Arguments = bundle };
                return viewStoryFragment;
            }
            catch (Exception a)
            {
                Methods.DisplayReportResultTrack(a);
                return null!;
            } 
        }
          
        //public Fragment GetRegisteredFragment(int position)
        //{
        //    return RegisteredFragments[position];
        //}


        //        public override Object InstantiateItem(ViewGroup container, int position)
        //        {
        //            try
        //            {
        //                Fragment fragment = (Fragment)base.InstantiateItem(container, position);

        //                RegisteredFragments.Add(position, fragment);
        //                return fragment;
        //            }
        //            catch (Exception e)
        //            {
        //                Methods.DisplayReportResultTrack(e);
        //                return null!;
        //            }
        //        }

        //        public override void DestroyItem(ViewGroup container, int position, Object @object)
        //        {
        //            try
        //            {  
        //                if (RegisteredFragments.Count > 0 && RegisteredFragments.ContainsKey(position))
        //                    RegisteredFragments.Remove(position);

        //                base.DestroyItem(container, position, @object);
        //            }
        //            catch (Exception e)
        //            {
        //                Methods.DisplayReportResultTrack(e); 
        //            }
        //        }



    }
}