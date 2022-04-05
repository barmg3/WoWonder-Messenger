using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Adapters
{
    public class GendersAdapter : RecyclerView.Adapter
    {
        public event EventHandler<GendersAdapterClickEventArgs> ItemClick;
        public event EventHandler<GendersAdapterClickEventArgs> ItemLongClick;

        private Activity ActivityContext;
        public ObservableCollection<Classes.Gender> GenderList = new ObservableCollection<Classes.Gender>();

        public GendersAdapter(Activity context)
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

        public override int ItemCount => GenderList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Categories_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_CategoriesView, parent, false);
                var vh = new GendersAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is GendersAdapterViewHolder holder)
                {
                    var item = GenderList[position];
                    if (item == null) return;

                    holder.Button.Text = item.GenderName;

                    if (item.GenderSelect)
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                        holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                    }
                    else
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        holder.Button.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#444444"));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public Classes.Gender GetItem(int position)
        {
            return GenderList[position];
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

        private void Click(GendersAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(GendersAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class GendersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public GendersAdapterViewHolder(View itemView, Action<GendersAdapterClickEventArgs> clickListener,
            Action<GendersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new GendersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GendersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                Button.SetTextColor(Color.ParseColor("#efefef"));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }



        public AppCompatButton Button { get; set; }

        #endregion
    }

    public class GendersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}