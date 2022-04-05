using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;

namespace WoWonder.Activities.GroupChat.Adapter
{
    public class GroupRequestsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<GroupRequestsAdapterClickEventArgs> DeleteButtonItemClick;
        public event EventHandler<GroupRequestsAdapterClickEventArgs> AddButtonItemClick;
        public event EventHandler<GroupRequestsAdapterClickEventArgs> ItemClick;
        public event EventHandler<GroupRequestsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<GroupChatRequest> GroupList = new ObservableCollection<GroupChatRequest>();

        public GroupRequestsAdapter(Activity activity)
        {
            try
            {
                //HasStableIds = true;
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => GroupList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_GroupRequestsView, parent, false);
                var vh = new GroupRequestsAdapterViewHolder(itemView, DeleteButtonClick, AddButtonClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is GroupRequestsAdapterViewHolder holder)
                {
                    var item = GroupList[position];
                    if (item?.GroupTab != null)
                    {
                        var image = item.GroupTab.Avatar?.Replace(InitializeWoWonder.WebsiteUrl, "");
                        if (image != null && !image.Contains("http"))
                            item.GroupTab.Avatar = InitializeWoWonder.WebsiteUrl + "/" + image;

                        GlideImageLoader.LoadImage(ActivityContext, item.GroupTab.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        holder.Name.Text = item.GroupTab.GroupName + " (" + ActivityContext.GetText(Resource.String.Lbl_Group) + ")";
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public GroupChatRequest GetItem(int position)
        {
            return GroupList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(GroupList[position].UserId);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void AddButtonClick(GroupRequestsAdapterClickEventArgs args)
        {
            AddButtonItemClick?.Invoke(this, args);
        }
        private void DeleteButtonClick(GroupRequestsAdapterClickEventArgs args)
        {
            DeleteButtonItemClick?.Invoke(this, args);
        }

        private void Click(GroupRequestsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(GroupRequestsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

    }

    public class GroupRequestsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public GroupRequestsAdapterViewHolder(View itemView, Action<GroupRequestsAdapterClickEventArgs> deleteButtonClickListener, Action<GroupRequestsAdapterClickEventArgs> addButtonClickListener, Action<GroupRequestsAdapterClickEventArgs> clickListener, Action<GroupRequestsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                AddButton = MainView.FindViewById<CircleButton>(Resource.Id.Add_button);
                DeleteButton = MainView.FindViewById<CircleButton>(Resource.Id.delete_button);

                //Event
                AddButton.Click += (sender, e) => addButtonClickListener(new GroupRequestsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                DeleteButton.Click += (sender, e) => deleteButtonClickListener(new GroupRequestsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.Click += (sender, e) => clickListener(new GroupRequestsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GroupRequestsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public CircleButton AddButton { get; private set; }
        public CircleButton DeleteButton { get; private set; }

        #endregion
    }

    public class GroupRequestsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

}