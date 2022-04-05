using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using MaterialDialogsCore;
using Newtonsoft.Json;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.DefaultUser;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
 
namespace WoWonder.Activities.DialogUserFragment
{
    public class DialogUser : AndroidX.Fragment.App.DialogFragment
    {
        public class OnUserUpEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }

        #region Variables Basic

        private TextView TxtUsername;
        private TextView TxtName;

        private CircleButton BtnSendMesseges;
        private CircleButton BtnAdd;

        private ImageView ImageUserprofile;

        public event EventHandler<OnUserUpEventArgs> OnUserUpComplete;

        public string Userid = "";
        public UserDataObject Item;
        private readonly SearchActivity ActivityContext;
        #endregion

        public DialogUser(SearchActivity activity ,string userid, UserDataObject item)
        {
            try
            {
                ActivityContext = activity;
                Userid = userid;
                Item = item;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //open Layout as a message
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                base.OnCreateView(inflater, container, savedInstanceState);
                 
                var view = inflater.Inflate(Resource.Layout.DialogUserFragment, container, false);

                // Get values
                TxtUsername = view.FindViewById<TextView>(Resource.Id.Txt_Username);
                TxtName = view.FindViewById<TextView>(Resource.Id.Txt_SecendreName);

                BtnSendMesseges = view.FindViewById<CircleButton>(Resource.Id.SendMesseges_button);

                BtnAdd = view.FindViewById<CircleButton>(Resource.Id.Add_button);
                BtnAdd.Tag = "Add";

                ImageUserprofile = view.FindViewById<ImageView>(Resource.Id.profileAvatar_image);

                //profile_picture
                GlideImageLoader.LoadImage(Activity,Item.Avatar, ImageUserprofile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                 
                TxtUsername.Text = Item.Name;
                TxtName.Text = "@" + Item.Username;
                 
                WoWonderTools.SetAddFriendConditionWithImage(Item, Item.IsFollowing, BtnAdd);
                 
                // Event
                BtnSendMesseges.Click += BtnSendMessegesOnClick;
                BtnAdd.Click += BtnAddOnClick;

                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        //animations
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            try
            {
                var dd = base.OnCreateDialog(savedInstanceState);

                dd.Window.RequestFeature(WindowFeatures.NoTitle); //Sets the title bar to invisible
                dd.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation; //set the animation

                return dd;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
         
        private void BtnSendMessegesOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                Dismiss();
                int x = Resource.Animation.slide_right;
                Console.WriteLine(x);

                if (Item.ChatColor == null)
                    Item.ChatColor = AppSettings.MainColor;

                var mainChatColor = Item.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(Item.ChatColor) : Item.ChatColor ?? AppSettings.MainColor;

                Intent intent = new Intent(Context, typeof(ChatWindowActivity));
                intent.PutExtra("ChatId", Userid);
                intent.PutExtra("UserID", Userid);
                intent.PutExtra("TypeChat", "Search");
                intent.PutExtra("ColorChat", mainChatColor);
                intent.PutExtra("UserItem", JsonConvert.SerializeObject(Item));
                StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnAddOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    Item.IsFollowing = Item.IsFollowing switch
                    {
                        null => "0",
                        _ => Item.IsFollowing
                    };

                    var dbDatabase = new SqLiteDatabase();
                    string isFollowing;
                    switch (Item.IsFollowing)
                    {
                        case "0": // Add Or request friends
                        case "no":
                        case "No":
                            if (Item.ConfirmFollowers == "1" || AppSettings.ConnectivitySystem == 0)
                            {
                                Item.IsFollowing = isFollowing = "2";
                                BtnAdd.Tag = "request";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(Item, "Update");
                            }
                            else
                            {
                                Item.IsFollowing = isFollowing = "1";
                                BtnAdd.Tag = "friends";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(Item, "Insert");
                            }

                            WoWonderTools.SetAddFriendConditionWithImage(Item, isFollowing, BtnAdd);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(Item.UserId) });

                            ToastUtils.ShowToast(ActivityContext, AppSettings.ConnectivitySystem == 1 ? GetText(Resource.String.Lbl_Sent_successfully_followed) : GetText(Resource.String.Lbl_Sent_successfully_FriendRequest), ToastLength.Short);


                            var local = ActivityContext.MAdapter?.UserList?.FirstOrDefault(a => a.UserId == Userid);
                            if (local != null)
                            {
                                local.IsFollowing = Item.IsFollowing;
                                ActivityContext.MAdapter?.NotifyItemChanged(ActivityContext.MAdapter.UserList.IndexOf(local));
                            }

                            break;
                        case "1": // Remove friends
                        case "yes":
                        case "Yes":
                            {
                                var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                                dialog.Content(ActivityContext.GetText(Resource.String.Lbl_confirmationUnFriend));
                                dialog.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Confirm)).OnPositive((materialDialog, action) =>
                                {
                                    try
                                    {
                                        Item.IsFollowing = isFollowing = "0";
                                        BtnAdd.Tag = "Add";

                                        dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(Item, "Delete");

                                        WoWonderTools.SetAddFriendConditionWithImage(Item, isFollowing, BtnAdd);

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(Item.UserId) });

                                        ToastUtils.ShowToast(ActivityContext, AppSettings.ConnectivitySystem == 1 ? GetText(Resource.String.Lbl_Sent_successfully_Unfollowed) : GetText(Resource.String.Lbl_Sent_successfully_FriendRequestCancelled), ToastLength.Short);


                                        var local = ActivityContext.MAdapter?.UserList?.FirstOrDefault(a => a.UserId == Userid);
                                        if (local != null)
                                        {
                                            local.IsFollowing = Item.IsFollowing;
                                            ActivityContext.MAdapter?.NotifyItemChanged(ActivityContext.MAdapter.UserList.IndexOf(local));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                                dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                                dialog.AlwaysCallSingleChoiceCallback();
                                dialog.Build().Show();
                            }
                            break;
                        case "2": // Remove request friends 
                            {
                                var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                                dialog.Content(ActivityContext.GetText(Resource.String.Lbl_confirmationUnFriend));
                                dialog.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Confirm)).OnPositive((materialDialog, action) =>
                                {
                                    try
                                    {
                                        Item.IsFollowing = isFollowing = "0";
                                        BtnAdd.Tag = "Add";

                                        dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(Item, "Delete");

                                        WoWonderTools.SetAddFriendConditionWithImage(Item, isFollowing, BtnAdd);

                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(Item.UserId) });

                                        ToastUtils.ShowToast(ActivityContext, AppSettings.ConnectivitySystem == 1 ? GetText(Resource.String.Lbl_Sent_successfully_Unfollowed) : GetText(Resource.String.Lbl_Sent_successfully_FriendRequestCancelled), ToastLength.Short);


                                        var local = ActivityContext.MAdapter?.UserList?.FirstOrDefault(a => a.UserId == Userid);
                                        if (local != null)
                                        {
                                            local.IsFollowing = Item.IsFollowing;
                                            ActivityContext.MAdapter?.NotifyItemChanged(ActivityContext.MAdapter.UserList.IndexOf(local));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                                dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                                dialog.AlwaysCallSingleChoiceCallback();
                                dialog.Build().Show();
                            }
                            break;
                    } 
                }
                else
                {
                    ToastUtils.ShowToast(ActivityContext, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }
 
        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}