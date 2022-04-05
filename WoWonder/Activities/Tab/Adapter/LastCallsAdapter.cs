 using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Tab.Adapter
{
    public class LastCallsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<LastCallsAdapterClickEventArgs> ItemClick;
        public event EventHandler<LastCallsAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<LastCallsAdapterClickEventArgs> CallClick;

        public ObservableCollection<Classes.CallUser> MCallUser = new ObservableCollection<Classes.CallUser>();

        private readonly Activity ActivityContext;

        public LastCallsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Last_Calls_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LastCallsView, parent, false);
                var holder = new LastCallsAdapterViewHolder(itemView, OnClick, OnLongClick, CallOnClick);
                return holder;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is LastCallsAdapterViewHolder holder)
                {
                    var item = MCallUser[position];
                    if (item != null)
                    {
                        Initialize(holder, item);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Initialize(LastCallsAdapterViewHolder holder, Classes.CallUser item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                string name = Methods.FunString.DecodeString(item.Name);
                holder.TxtUsername.Text = name;

                switch (item.TypeIcon)
                {
                    case "Accept":
                        Glide.With(ActivityContext).Load(Resource.Drawable.social_ic_phone_calls).Into(holder.IconCall);
                        break;
                    case "Cancel":
                        Glide.With(ActivityContext).Load(Resource.Drawable.social_ic_phone_call_arrow).Into(holder.IconCall);
                        break;
                    case "Declined":
                        Glide.With(ActivityContext).Load(Resource.Drawable.social_ic_forward_call).Into(holder.IconCall);
                        break;
                }

                switch (item.Time)
                {
                    case "Declined call":
                        holder.TxtLastTimecall.Text = ActivityContext.GetText(Resource.String.Lbl_NotAnswered_call);
                        break;
                    case "Missed call":
                        holder.TxtLastTimecall.Text = ActivityContext.GetText(Resource.String.Lbl_Missed_call);
                        break;
                    case "Answered call":
                        holder.TxtLastTimecall.Text = ActivityContext.GetText(Resource.String.Lbl_Answered_call);
                        break;
                    default:
                        holder.TxtLastTimecall.Text = item.Time;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Function Call
        public void Insert(Classes.CallUser call)
        {
            try
            {
                var check = MCallUser.FirstOrDefault(a => a.Id == call.Id);
                if (check == null)
                {
                    MCallUser.Insert(0, call);


                    var instance = TabbedMainActivity.GetInstance();
                    instance?.RunOnUiThread(() =>
                    {
                        try
                        {
                            NotifyItemInserted(0);
                            instance.LastCallsTab?.MRecycler?.ScrollToPosition(0);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => MCallUser?.Count ?? 0;

        public Classes.CallUser GetItem(int position)
        {
            return MCallUser[position];
        }

        public override long GetItemId(int position)
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

        void OnClick(LastCallsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(LastCallsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void CallOnClick(LastCallsAdapterClickEventArgs args) => CallClick?.Invoke(this, args);

    }

    public class LastCallsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public TextView TxtUsername { get; private set; }
        public TextView TxtLastTimecall { get; private set; }

        public ImageView IconCall { get; private set; }
        public ImageView ImageAvatar { get; private set; }
        public CircleImageView ImageLastseen { get; private set; }

        #endregion

        public LastCallsAdapterViewHolder(View itemView, Action<LastCallsAdapterClickEventArgs> clickListener, Action<LastCallsAdapterClickEventArgs> longClickListener, Action<LastCallsAdapterClickEventArgs> callclickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values  
                TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_name);

                TxtLastTimecall = (TextView)MainView.FindViewById(Resource.Id.Txt_Lasttimecalls);
                ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.Img_Avatar);
                IconCall = (ImageView)MainView.FindViewById(Resource.Id.IconCall);
                ImageLastseen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);
                ImageLastseen.Visibility = ViewStates.Gone;

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LastCallsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LastCallsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                IconCall.Click += (sender, e) => callclickListener(new LastCallsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class LastCallsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}