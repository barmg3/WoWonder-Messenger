using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using WoWonderClient.JobWorker;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Story.Service
{ 
    [Service(Exported = false)]
    public class StoryService : Android.App.Service
    {
        #region Variables Basic

        public string ActionPostService;
        public static string ActionStory;
        private static string PagePost;
        private static StoryService Service;

        private TabbedMainActivity GlobalContextTabbed;
        public FileModel DataPost;

        #endregion

        #region General

        public static StoryService GetPostService()
        {
            return Service;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                Service = this;


                GlobalContextTabbed = TabbedMainActivity.GetInstance();
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                Create_Progress_Notification();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);

                ActionPostService = intent.Action;
                var data = intent.GetStringExtra("DataPost");
                PagePost = intent.GetStringExtra("PagePost") ?? "";

                switch (string.IsNullOrEmpty(data))
                {
                    case false:
                        {
                            DataPost = JsonConvert.DeserializeObject<FileModel>(data);

                            if (ActionPostService == ActionStory)
                            {
                                if (DataPost != null)
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { AddStory }); 
                                    UpdateNotification("Story");
                                }
                            }

                            break;
                        }
                }

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }

        public async Task AddStory()
        {
            try
            {
                var modelStory = GlobalContextTabbed.LastStoriesTab.MAdapter;

                string time = Methods.Time.TimeAgo(DateTime.Now, false);
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = unixTimestamp.ToString();

                var userData = ListUtils.MyProfileList?.FirstOrDefault();

                //just pass file_path and type video or image
                var (apiStatus, respond) = await RequestsAsync.Story.CreateStoryAsync(DataPost.StoryTitle, DataPost.StoryDescription, DataPost.StoryFilePath, DataPost.StoryFileType, DataPost.StoryThumbnail);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case CreateStoryObject result:
                                    {
                                        ToastUtils.ShowToast(GlobalContextTabbed, GlobalContextTabbed.GetText(Resource.String.Lbl_Story_added), ToastLength.Short);

                                        var check = modelStory.StoryList?.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                                        if (check != null)
                                        {
                                            switch (DataPost.StoryFileType)
                                            {
                                                case "image":
                                                    {
                                                        var item = new StoryDataObject.Story
                                                        {
                                                            UserId = UserDetails.UserId,
                                                            Id = result.StoryId,
                                                            Description = DataPost.StoryDescription,
                                                            Title = DataPost.StoryTitle,
                                                            TimeText = time,
                                                            IsOwner = true,
                                                            Expire = "",
                                                            Posted = time2,
                                                            Thumbnail = DataPost.StoryFilePath,
                                                            UserData = userData,
                                                            ViewCount = "0",
                                                            Images = new List<StoryDataObject.Image>(),
                                                            Videos = new List<StoryDataObject.Video>(),
                                                            Reaction = new Reaction()
                                                        };

                                                        check.DurationsList ??= new List<long> { };
                                                        check.DurationsList.Add(AppSettings.StoryImageDuration * 1000);

                                                        check.Stories.Add(item);
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        var item = new StoryDataObject.Story
                                                        {
                                                            UserId = UserDetails.UserId,
                                                            Id = result.StoryId,
                                                            Description = DataPost.StoryDescription,
                                                            Title = DataPost.StoryTitle,
                                                            TimeText = time,
                                                            IsOwner = true,
                                                            Expire = "",
                                                            Posted = time2,
                                                            Thumbnail = DataPost.StoryThumbnail,
                                                            UserData = userData,
                                                            ViewCount = "0",
                                                            Images = new List<StoryDataObject.Image>(),
                                                            Videos = new List<StoryDataObject.Video>
                                                    {
                                                        new StoryDataObject.Video
                                                        {
                                                            StoryId = result.StoryId,
                                                            Filename = DataPost.StoryFilePath,
                                                            Id = time2,
                                                            Expire = time2,
                                                            Type = "video",
                                                        }
                                                    },
                                                            Reaction = new Reaction()
                                                        };

                                                        check.DurationsList ??= new List<long> { };
                                                        if (AppSettings.ShowFullVideo)
                                                        {
                                                            var duration = WoWonderTools.GetDuration(DataPost.StoryFilePath);
                                                            check.DurationsList.Add(Long.ParseLong(duration));
                                                        }
                                                        else
                                                        {
                                                            check.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                        }
                                                         
                                                        check.Stories.Add(item);
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            switch (DataPost.StoryFileType)
                                            {
                                                case "image":
                                                    {
                                                        var item = new StoryDataObject
                                                        {
                                                            Type = "image",
                                                            Stories = new List<StoryDataObject.Story>
                                                    {
                                                        new StoryDataObject.Story
                                                        {
                                                            UserId = UserDetails.UserId,
                                                            Id = result.StoryId,
                                                            Description = DataPost.StoryDescription,
                                                            Title = DataPost.StoryTitle,
                                                            TimeText = time,
                                                            IsOwner = true,
                                                            Expire = "",
                                                            Posted = time2,
                                                            Thumbnail = DataPost.StoryFilePath,
                                                            UserData = userData,
                                                            ViewCount = "0",
                                                            Images = new List<StoryDataObject.Image>(),
                                                            Videos = new List<StoryDataObject.Video>(),
                                                            Reaction = new Reaction()
                                                        }

                                                    },
                                                            UserId = userData?.UserId,
                                                            Username = userData?.Username,
                                                            Email = userData?.Email,
                                                            FirstName = userData?.FirstName,
                                                            LastName = userData?.LastName,
                                                            Avatar = userData?.Avatar,
                                                            Cover = userData?.Cover,
                                                            BackgroundImage = userData?.BackgroundImage,
                                                            RelationshipId = userData?.RelationshipId,
                                                            Address = userData?.Address,
                                                            Working = userData?.Working,
                                                            Gender = userData?.Gender,
                                                            Facebook = userData?.Facebook,
                                                            Google = userData?.Google,
                                                            Twitter = userData?.Twitter,
                                                            Linkedin = userData?.Linkedin,
                                                            Website = userData?.Website,
                                                            Instagram = userData?.Instagram,
                                                            WebDeviceId = userData?.WebDeviceId,
                                                            Language = userData?.Language,
                                                            IpAddress = userData?.IpAddress,
                                                            PhoneNumber = userData?.PhoneNumber,
                                                            Timezone = userData?.Timezone,
                                                            Lat = userData?.Lat,
                                                            Lng = userData?.Lng,
                                                            About = userData?.About,
                                                            Birthday = userData?.Birthday,
                                                            Registered = userData?.Registered,
                                                            Lastseen = userData?.Lastseen,
                                                            LastLocationUpdate = userData?.LastLocationUpdate,
                                                            Balance = userData?.Balance,
                                                            Verified = userData?.Verified,
                                                            Status = userData?.Status,
                                                            Active = userData?.Active,
                                                            Admin = userData?.Admin,
                                                            IsPro = userData?.IsPro,
                                                            ProType = userData?.ProType,
                                                            School = userData?.School,
                                                            Name = userData?.Name,
                                                            AndroidMDeviceId = userData?.AndroidMDeviceId,
                                                            ECommented = userData?.ECommented,
                                                            AndroidNDeviceId = userData?.AndroidMDeviceId,
                                                            AvatarFull = userData?.AvatarFull,
                                                            BirthPrivacy = userData?.BirthPrivacy,
                                                            CanFollow = userData?.CanFollow,
                                                            ConfirmFollowers = userData?.ConfirmFollowers,
                                                            CountryId = userData?.CountryId,
                                                            EAccepted = userData?.EAccepted,
                                                            EFollowed = userData?.EFollowed,
                                                            EJoinedGroup = userData?.EJoinedGroup,
                                                            ELastNotif = userData?.ELastNotif,
                                                            ELiked = userData?.ELiked,
                                                            ELikedPage = userData?.ELikedPage,
                                                            EMentioned = userData?.EMentioned,
                                                            EProfileWallPost = userData?.EProfileWallPost,
                                                            ESentmeMsg = userData?.ESentmeMsg,
                                                            EShared = userData?.EShared,
                                                            EVisited = userData?.EVisited,
                                                            EWondered = userData?.EWondered,
                                                            EmailNotification = userData?.EmailNotification,
                                                            FollowPrivacy = userData?.FollowPrivacy,
                                                            FriendPrivacy = userData?.FriendPrivacy,
                                                            GenderText = userData?.GenderText,
                                                            InfoFile = userData?.InfoFile,
                                                            IosMDeviceId = userData?.IosMDeviceId,
                                                            IosNDeviceId = userData?.IosNDeviceId,
                                                            IsFollowing = userData?.IsFollowing,
                                                            IsFollowingMe = userData?.IsFollowingMe,
                                                            LastAvatarMod = userData?.LastAvatarMod,
                                                            LastCoverMod = userData?.LastCoverMod,
                                                            LastDataUpdate = userData?.LastDataUpdate,
                                                            LastFollowId = userData?.LastFollowId,
                                                            LastLoginData = userData?.LastLoginData,
                                                            LastseenStatus = userData?.LastseenStatus,
                                                            LastseenTimeText = userData?.LastseenTimeText,
                                                            LastseenUnixTime = userData?.LastseenUnixTime,
                                                            MessagePrivacy = userData?.MessagePrivacy,
                                                            NewEmail = userData?.NewEmail,
                                                            NewPhone = userData?.NewPhone,
                                                            NotificationsSound = userData?.NotificationsSound,
                                                            OrderPostsBy = userData?.OrderPostsBy,
                                                            PaypalEmail = userData?.PaypalEmail,
                                                            PostPrivacy = userData?.PostPrivacy,
                                                            Referrer = userData?.Referrer,
                                                            ShareMyData = userData?.ShareMyData,
                                                            ShareMyLocation = userData?.ShareMyLocation,
                                                            ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                                                            TwoFactor = userData?.TwoFactor,
                                                            TwoFactorVerified = userData?.TwoFactorVerified,
                                                            Url = userData?.Url,
                                                            VisitPrivacy = userData?.VisitPrivacy,
                                                            Vk = userData?.Vk,
                                                            Wallet = userData?.Wallet,
                                                            WorkingLink = userData?.WorkingLink,
                                                            Youtube = userData?.Youtube,
                                                            City = userData?.City,
                                                            Points = userData?.Points,
                                                            DailyPoints = userData?.DailyPoints,
                                                            PointDayExpire = userData?.PointDayExpire,
                                                            State = userData?.State,
                                                            Zip = userData?.Zip,
                                                            Details = new DetailsUnion
                                                            {
                                                                DetailsClass = new Details(),
                                                            },
                                                            ApiNotificationSettings = new NotificationSettingsUnion
                                                            {
                                                                NotificationSettingsClass = new NotificationSettings()
                                                            },
                                                        };

                                                        item.DurationsList ??= new List<long> { };
                                                        item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);

                                                        modelStory.StoryList?.Add(item);
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        var item = new StoryDataObject
                                                        {
                                                            Type = "video",
                                                            Stories = new List<StoryDataObject.Story>
                                                    {
                                                        new StoryDataObject.Story
                                                        {
                                                            UserId = UserDetails.UserId,
                                                            Id = result.StoryId,
                                                            Description = DataPost.StoryDescription,
                                                            Title = DataPost.StoryTitle,
                                                            TimeText = time,
                                                            IsOwner = true,
                                                            Expire = "",
                                                            Posted = time2,
                                                            Thumbnail = DataPost.StoryThumbnail,
                                                            UserData = userData,
                                                            ViewCount = "0",
                                                            Images = new List<StoryDataObject.Image>(),
                                                            Videos = new List<StoryDataObject.Video>
                                                            {
                                                                new StoryDataObject.Video
                                                                {
                                                                    StoryId = result.StoryId,
                                                                    Filename = DataPost.StoryFilePath,
                                                                    Id = time2,
                                                                    Expire = time2,
                                                                    Type = "video",
                                                                }
                                                            },
                                                            Reaction = new Reaction()
                                                        }
                                                    },
                                                            UserId = userData?.UserId,
                                                            Username = userData?.Username,
                                                            Email = userData?.Email,
                                                            FirstName = userData?.FirstName,
                                                            LastName = userData?.LastName,
                                                            Avatar = userData?.Avatar,
                                                            Cover = userData?.Cover,
                                                            BackgroundImage = userData?.BackgroundImage,
                                                            RelationshipId = userData?.RelationshipId,
                                                            Address = userData?.Address,
                                                            Working = userData?.Working,
                                                            Gender = userData?.Gender,
                                                            Facebook = userData?.Facebook,
                                                            Google = userData?.Google,
                                                            Twitter = userData?.Twitter,
                                                            Linkedin = userData?.Linkedin,
                                                            Website = userData?.Website,
                                                            Instagram = userData?.Instagram,
                                                            WebDeviceId = userData?.WebDeviceId,
                                                            Language = userData?.Language,
                                                            IpAddress = userData?.IpAddress,
                                                            PhoneNumber = userData?.PhoneNumber,
                                                            Timezone = userData?.Timezone,
                                                            Lat = userData?.Lat,
                                                            Lng = userData?.Lng,
                                                            About = userData?.About,
                                                            Birthday = userData?.Birthday,
                                                            Registered = userData?.Registered,
                                                            Lastseen = userData?.Lastseen,
                                                            LastLocationUpdate = userData?.LastLocationUpdate,
                                                            Balance = userData?.Balance,
                                                            Verified = userData?.Verified,
                                                            Status = userData?.Status,
                                                            Active = userData?.Active,
                                                            Admin = userData?.Admin,
                                                            IsPro = userData?.IsPro,
                                                            ProType = userData?.ProType,
                                                            School = userData?.School,
                                                            Name = userData?.Name,
                                                            AndroidMDeviceId = userData?.AndroidMDeviceId,
                                                            ECommented = userData?.ECommented,
                                                            AndroidNDeviceId = userData?.AndroidMDeviceId,
                                                            AvatarFull = userData?.AvatarFull,
                                                            BirthPrivacy = userData?.BirthPrivacy,
                                                            CanFollow = userData?.CanFollow,
                                                            ConfirmFollowers = userData?.ConfirmFollowers,
                                                            CountryId = userData?.CountryId,
                                                            EAccepted = userData?.EAccepted,
                                                            EFollowed = userData?.EFollowed,
                                                            EJoinedGroup = userData?.EJoinedGroup,
                                                            ELastNotif = userData?.ELastNotif,
                                                            ELiked = userData?.ELiked,
                                                            ELikedPage = userData?.ELikedPage,
                                                            EMentioned = userData?.EMentioned,
                                                            EProfileWallPost = userData?.EProfileWallPost,
                                                            ESentmeMsg = userData?.ESentmeMsg,
                                                            EShared = userData?.EShared,
                                                            EVisited = userData?.EVisited,
                                                            EWondered = userData?.EWondered,
                                                            EmailNotification = userData?.EmailNotification,
                                                            FollowPrivacy = userData?.FollowPrivacy,
                                                            FriendPrivacy = userData?.FriendPrivacy,
                                                            GenderText = userData?.GenderText,
                                                            InfoFile = userData?.InfoFile,
                                                            IosMDeviceId = userData?.IosMDeviceId,
                                                            IosNDeviceId = userData?.IosNDeviceId,
                                                            IsFollowing = userData?.IsFollowing,
                                                            IsFollowingMe = userData?.IsFollowingMe,
                                                            LastAvatarMod = userData?.LastAvatarMod,
                                                            LastCoverMod = userData?.LastCoverMod,
                                                            LastDataUpdate = userData?.LastDataUpdate,
                                                            LastFollowId = userData?.LastFollowId,
                                                            LastLoginData = userData?.LastLoginData,
                                                            LastseenStatus = userData?.LastseenStatus,
                                                            LastseenTimeText = userData?.LastseenTimeText,
                                                            LastseenUnixTime = userData?.LastseenUnixTime,
                                                            MessagePrivacy = userData?.MessagePrivacy,
                                                            NewEmail = userData?.NewEmail,
                                                            NewPhone = userData?.NewPhone,
                                                            NotificationsSound = userData?.NotificationsSound,
                                                            OrderPostsBy = userData?.OrderPostsBy,
                                                            PaypalEmail = userData?.PaypalEmail,
                                                            PostPrivacy = userData?.PostPrivacy,
                                                            Referrer = userData?.Referrer,
                                                            ShareMyData = userData?.ShareMyData,
                                                            ShareMyLocation = userData?.ShareMyLocation,
                                                            ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                                                            TwoFactor = userData?.TwoFactor,
                                                            TwoFactorVerified = userData?.TwoFactorVerified,
                                                            Url = userData?.Url,
                                                            VisitPrivacy = userData?.VisitPrivacy,
                                                            Vk = userData?.Vk,
                                                            Wallet = userData?.Wallet,
                                                            WorkingLink = userData?.WorkingLink,
                                                            Youtube = userData?.Youtube,
                                                            City = userData?.City,
                                                            Points = userData?.Points,
                                                            DailyPoints = userData?.DailyPoints,
                                                            State = userData?.State,
                                                            Zip = userData?.Zip,
                                                            Details = new DetailsUnion
                                                            {
                                                                DetailsClass = new Details(),
                                                            },
                                                            ApiNotificationSettings = new NotificationSettingsUnion
                                                            {
                                                                NotificationSettingsClass = new NotificationSettings()
                                                            },
                                                        };

                                                        item.DurationsList ??= new List<long> { };
                                                        if (AppSettings.ShowFullVideo)
                                                        {
                                                            var duration = WoWonderTools.GetDuration(DataPost.StoryFilePath);
                                                            item.DurationsList.Add(Long.ParseLong(duration));
                                                        }
                                                        else
                                                        {
                                                            item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                        }

                                                        modelStory.StoryList?.Add(item);
                                                        break;
                                                    }
                                            }
                                        }

                                        GlobalContextTabbed.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                modelStory.NotifyDataSetChanged();
                                                GlobalContextTabbed.LastStoriesTab.ShowEmptyPage();

                                                switch (UserDetails.SoundControl)
                                                {
                                                    case true:
                                                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");
                                                        break;
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(GlobalContextTabbed, respond);
                        break;
                }

                RemoveNotification();
            }
            catch (Exception e)
            {
                RemoveNotification();
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Notification

        private readonly string NotificationChannelId = "wowonder_ch_1";

        private NotificationManager MNotificationManager;
        private NotificationCompat.Builder NotificationBuilder;
        private RemoteViews NotificationView;
        private void Create_Progress_Notification()
        {
            try
            {
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                NotificationView = new RemoteViews(PackageName, Resource.Layout.ViewProgressNotification);

                Intent resultIntent = new Intent();
                PendingIntent resultPendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);
                NotificationBuilder = new NotificationCompat.Builder(this, NotificationChannelId);
                NotificationBuilder.SetSmallIcon(Resource.Mipmap.icon);
                NotificationBuilder.SetColor(ContextCompat.GetColor(this, Resource.Color.accent));
                NotificationBuilder.SetCustomContentView(NotificationView)
                    .SetOngoing(true)
                    .SetContentIntent(resultPendingIntent)
                    .SetDefaults(NotificationCompat.DefaultAll)
                    .SetPriority((int)NotificationPriority.High);

                NotificationBuilder.SetVibrate(new[] { 0L });
                NotificationBuilder.SetVisibility(NotificationCompat.VisibilityPublic);

                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.O:
                        {
                            var importance = NotificationImportance.High;
                            NotificationChannel notificationChannel = new NotificationChannel(NotificationChannelId, AppSettings.ApplicationName, importance);
                            notificationChannel.EnableLights(false);
                            notificationChannel.EnableVibration(false);
                            NotificationBuilder.SetChannelId(NotificationChannelId);

                            MNotificationManager?.CreateNotificationChannel(notificationChannel);
                            break;
                        }
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RemoveNotification()
        {
            try
            {
                NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager?.Cancel(2020);

                MNotificationManager?.CancelAll();
                StopSelf();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateNotification(string type)
        {
            try
            {
                switch (type)
                {
                    case "Story":
                        NotificationView.SetTextViewText(Resource.Id.title, GetString(Resource.String.Lbl_UploadingStory));
                        break;
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    } 
}