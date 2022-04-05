using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Com.Onesignal.Shortcutbadger;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Utils;
using TaskStackBuilder = Android.App.TaskStackBuilder;

namespace WoWonder.SocketSystem
{
	public class AppNotificationsManager
	{
		private static volatile AppNotificationsManager InstanceRenamed;

		private NotificationManager MNotificationManager;
		private readonly int Index = 21828;

		public static AppNotificationsManager Instance
		{
			get
			{
                AppNotificationsManager localInstance = InstanceRenamed;
				if (localInstance == null)
				{
					lock (typeof(AppNotificationsManager))
					{
						localInstance = InstanceRenamed;
						if (localInstance == null)
						{
							InstanceRenamed = localInstance = new AppNotificationsManager();
						}
					}
				}
				return localInstance;
            }
		}

		public void ShowUserNotification(string type, string conversationId, string username, string message, string id, string ChatId, string avatar, string color, int counterUnreadMessages = 1)
		{
			try
            {
                Context mContext = Application.Context;

                //Toast.MakeText(mContext, "ShowUserNotification", ToastLength.Short)?.Show();
				string channelId;

				//this for default activity
				Intent messagingIntent = new Intent(mContext, typeof(TabbedMainActivity));
				messagingIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
				messagingIntent.AddFlags(ActivityFlags.SingleTop);
				messagingIntent.SetAction(Intent.ActionView);
				messagingIntent.PutExtra("ChatId", ChatId);
				messagingIntent.PutExtra("userId", id);
                messagingIntent.PutExtra("PageId", id);
                messagingIntent.PutExtra("GroupId", id);
                messagingIntent.PutExtra("type", type);
				messagingIntent.PutExtra("Notifier", "Chat");

				TaskStackBuilder stackBuilder = TaskStackBuilder.Create(mContext);
				// Adds the back stack
				stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(ChatWindowActivity)));

				// Adds the Intent to the top of the stack
				stackBuilder.AddNextIntent(messagingIntent);
				// Gets a PendingIntent containing the entire back stack
				PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);

				NotificationCompat.Builder mNotifyBuilder;

				NotificationCompat.InboxStyle inboxStyle = new NotificationCompat.InboxStyle();

				MNotificationManager = (NotificationManager)mContext.GetSystemService(Context.NotificationService);
				if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
				{
					channelId = username; // The id of the channel.
					var name = AppSettings.ApplicationName; // The user-visible name of the channel.
					var mChannel = new NotificationChannel(channelId, name, NotificationImportance.High);
					mNotifyBuilder = new NotificationCompat.Builder(mContext, channelId).SetVisibility(NotificationCompat.VisibilityPublic).SetColor(ContextCompat.GetColor(mContext, Resource.Color.accent)).SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default).SetContentIntent(resultPendingIntent).SetChannelId(channelId).SetCategory(NotificationCompat.CategoryMessage);

					MNotificationManager.CreateNotificationChannel(mChannel);
				}
				else
				{
					channelId = username; // The id of the channel.
					mNotifyBuilder = new NotificationCompat.Builder(mContext, channelId).SetVisibility(NotificationCompat.VisibilityPublic).SetColor(ContextCompat.GetColor(mContext, Resource.Color.accent)).SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default).SetContentIntent(resultPendingIntent).SetPriority((int)NotificationPriority.High).SetCategory(NotificationCompat.CategoryMessage);
				}

				//if more message
				if (ListUtils.MessageUnreadList != null)
				{
					if (ListUtils.MessageUnreadList.Count > 0)
					{
						if (ListUtils.MessageUnreadList.Count == 1)
						{
							NotificationCompat.Action action = new NotificationCompat.Action.Builder(Resource.Drawable.icon_chat_reply, mContext.GetString(Resource.String.Lbl_Reply), resultPendingIntent).Build();
							mNotifyBuilder.AddAction(action);

							inboxStyle.SetBigContentTitle(username);

							mNotifyBuilder.SetContentTitle(username);
							if (message != null)
							{
								mNotifyBuilder.SetContentText(message);
							}

							inboxStyle.SetSummaryText(counterUnreadMessages + " " + mContext.GetString(Resource.String.new_messages_notify));

							var list = ListUtils.MessageUnreadList.Where(a => a.Sender == id)?.ToList();
							if (list?.Count > 0)
								foreach (var m in list)
								{
									inboxStyle.AddLine(m.Message);
								}
						}
						else
						{
							inboxStyle.SetBigContentTitle(AppSettings.ApplicationName);

							mNotifyBuilder.SetContentTitle(username);
							if (message != null)
							{
								mNotifyBuilder.SetContentText(message);
							}
							inboxStyle.SetSummaryText(counterUnreadMessages + " " + mContext.GetString(Resource.String.messages_from_notify) + " " + counterUnreadMessages + " " + mContext.GetString(Resource.String.chats_notify));

							var list = ListUtils.MessageUnreadList.Where(a => a.Sender == id)?.ToList();
							if (list?.Count > 0)
								foreach (var m in list)
								{
									inboxStyle.AddLine(m.Message);
								}
						}
					}
					else
					{
						NotificationCompat.Action action = new NotificationCompat.Action.Builder(Resource.Drawable.icon_chat_reply, mContext.GetString(Resource.String.Lbl_Reply), resultPendingIntent).Build();
						mNotifyBuilder.AddAction(action);

						inboxStyle.SetBigContentTitle(username);

						mNotifyBuilder.SetContentTitle(username);

						mNotifyBuilder.SetContentText(message);
						inboxStyle.SetSummaryText(counterUnreadMessages + " " + mContext.GetString(Resource.String.new_messages_notify));
						inboxStyle.AddLine(message);
					}
				}
				else
				{
					NotificationCompat.Action action = new NotificationCompat.Action.Builder(Resource.Drawable.icon_chat_reply, mContext.GetString(Resource.String.Lbl_Reply), resultPendingIntent).Build();
					mNotifyBuilder.AddAction(action);

					inboxStyle.SetBigContentTitle(username);

					mNotifyBuilder.SetContentTitle(username);

					mNotifyBuilder.SetContentText(message);
					inboxStyle.SetSummaryText(counterUnreadMessages + " " + mContext.GetString(Resource.String.new_messages_notify));
					inboxStyle.AddLine(message);

				}

				mNotifyBuilder.SetStyle(inboxStyle);
				Drawable drawable = ContextCompat.GetDrawable(mContext, Resource.Drawable.no_profile_image);

				if (!string.IsNullOrEmpty(avatar))
				{
					var url = avatar;
					if (!string.IsNullOrEmpty(url))
					{
						var bit = BitmapUtil.GetImageBitmapFromUrl(url);
						if (bit != null)
							mNotifyBuilder.SetLargeIcon(bit);
					}
				}
				else
				{
					Bitmap bitmap = ConvertToBitmap(drawable, 150, 150);
					mNotifyBuilder.SetLargeIcon(bitmap);
				}

				if (MainSettings.SharedData?.GetBoolean("checkBox_PlaySound_key", true) ?? true)
				{
					mNotifyBuilder.SetDefaults(NotificationCompat.DefaultSound);
				}

				//if (MainSettings.SharedData.GetBoolean("checkBox_vibrate_notifications_key", true))
				//{
				//	long[] vibrate = new long[] { 2000, 2000, 2000, 2000, 2000 };
				//  mNotifyBuilder.SetVibrate(vibrate);
				//}

				int defaultVibrate = 0;
				defaultVibrate |= NotificationCompat.DefaultVibrate;
				mNotifyBuilder.SetDefaults(defaultVibrate);

				if (color != null)
				{
					mNotifyBuilder.SetLights(Color.ParseColor(color), 1500, 1500);
				}
				else
				{
					int defaults = 0;
					defaults |= NotificationCompat.DefaultLights;
					mNotifyBuilder.SetDefaults(defaults);
				}

				mNotifyBuilder.SetAutoCancel(true);

				MNotificationManager?.Notify(id, Index, mNotifyBuilder.Build());

				SetupBadger(counterUnreadMessages);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private Bitmap ConvertToBitmap(Drawable drawable, int widthPixels, int heightPixels)
		{
			Bitmap mutableBitmap = Bitmap.CreateBitmap(widthPixels, heightPixels, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(mutableBitmap);
			drawable.SetBounds(0, 0, widthPixels, heightPixels);
			drawable.Draw(canvas);

			return mutableBitmap;
		}

		NotificationCompat.Builder MNotifyBuilder;
		public void ShowUpDownNotification(string userName, string messageId, string userId, string chatId)
		{
			try
			{
                Context mContext = Application.Context;
				string channelId;

				//this for default activity
				Intent messagingIntent = new Intent(mContext, typeof(ChatWindowActivity));

				messagingIntent.PutExtra("UserID", userId);
				messagingIntent.PutExtra("ChatId", chatId);
				//messagingIntent.PutExtra("TypeChat", "LastMessenger");
				//messagingIntent.PutExtra("ShowEmpty", "no");
				//messagingIntent.PutExtra("ColorChat", mainChatColor);
				//messagingIntent.PutExtra("UserItem", JsonConvert.SerializeObject(item.LastMessagesUser));

				TaskStackBuilder stackBuilder = TaskStackBuilder.Create(mContext);
				stackBuilder?.AddNextIntentWithParentStack(messagingIntent);

				PendingIntent resultPendingIntent = stackBuilder?.GetPendingIntent(0, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);

				MNotificationManager = (NotificationManager)mContext.GetSystemService(Context.NotificationService);
				if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
				{
					channelId = userId; // The id of the channel.
					var name = AppSettings.ApplicationName; // The user-visible name of the channel.
					var mChannel = new NotificationChannel(channelId, name, NotificationImportance.Low);
					MNotifyBuilder = new NotificationCompat.Builder(mContext, channelId).SetVisibility(NotificationCompat.VisibilityPublic).SetColor(ContextCompat.GetColor(mContext, Resource.Color.accent)).SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default).SetChannelId(channelId).SetContentIntent(resultPendingIntent).SetCategory(NotificationCompat.CategoryProgress);

					MNotificationManager?.CreateNotificationChannel(mChannel);
				}
				else
				{
					channelId = userId; // The id of the channel.
					MNotifyBuilder = new NotificationCompat.Builder(mContext, channelId).SetVisibility(NotificationCompat.VisibilityPublic).SetColor(ContextCompat.GetColor(mContext, Resource.Color.accent)).SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default).SetPriority((int)NotificationPriority.Low).SetContentIntent(resultPendingIntent).SetCategory(NotificationCompat.CategoryProgress);
				}

				MNotifyBuilder.SetAutoCancel(true)
						.SetOngoing(false)
						.SetDefaults(NotificationCompat.DefaultLights)
						.SetSound(null, 0);

				if (!string.IsNullOrEmpty(userName))
				{
					MNotifyBuilder.SetContentTitle(mContext.GetText(Resource.String.Lbl_SendingFileTo) + " " + userName);
				}
				else
				{
					MNotifyBuilder.SetContentTitle(mContext.GetText(Resource.String.Lbl_SendingFile));
				}

				MNotifyBuilder.SetProgress(0, 0, true);
				MNotificationManager?.Notify(messageId, Index, MNotifyBuilder.Build());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		/// <summary>
		/// method to cancel  All notification
		/// </summary>
		public void CancelAllNotification()
		{
			try
			{
				MNotificationManager?.CancelAll();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		/// <summary>
		/// method to cancel a specific notification
		/// </summary>
		/// <param name="tag"> </param>
		public void CancelNotification(string tag)
		{
			try
			{
				MNotificationManager?.Cancel(tag, Index);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void UpdateUpDownNotification(string messageId, int progress)
		{
			try
			{
				if (MNotifyBuilder != null)
				{
					MNotifyBuilder.SetContentText(progress + "%");

					MNotifyBuilder.SetProgress(100, progress, false);
					MNotificationManager?.Notify(messageId, Index, MNotifyBuilder.Build());
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		/// <summary>
		/// method to set badger counter for the app
		/// </summary>
		public void SetupBadger(int messageBadgeCounter = 0)
		{ 
			try
			{
                Context mContext = Application.Context;
				string deviceName = Build.Manufacturer;
				string[] devicesName = new string[] { "Sony", "Samsung", "LG", "HTC", "Xiaomi", "ASUS", "ADW", "NOVA", "Huawei", "ZUK", "APEX", "OPPO", "ZTE", "EverythingMe" };

				if (devicesName.Any(device => deviceName != null && deviceName.Equals(device.ToLower())))
				{
					try
					{
						try
						{
							ShortcutBadger.ApplyCount(mContext.ApplicationContext, messageBadgeCounter);
						}
						catch (Exception e)
						{
							Console.WriteLine(" ShortcutBadger Exception " + e.Message);
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(" ShortcutBadger Exception " + e.Message);
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}