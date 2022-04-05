using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Browser.CustomTabs; 
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share.Abstractions;

namespace WoWonder.Library.Anjo.Share
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class ShareImplementation : IShare
    {
        /// <summary>
        /// Open a browser to a specific url
        /// </summary>
        /// <param name="url">Url to open</param>
        /// <param name="options">Platform specific options</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public Task<bool> OpenBrowser(Activity Activity, string url, BrowserOptions options = null)
        {
            try
            {
                if (options == null)
                    options = new BrowserOptions();

                if (Activity == null)
                {
                    var intent = new Intent(Intent.ActionView);
                    intent.SetData(Android.Net.Uri.Parse(url));

                    intent.SetFlags(ActivityFlags.ClearTop);
                    intent.SetFlags(ActivityFlags.NewTask);
                    Application.Context.StartActivity(intent);
                }
                else
                {
                    var tabsBuilder = new CustomTabsIntent.Builder();
                    tabsBuilder.SetShowTitle(options?.ChromeShowTitle ?? false);

                    //var toolbarColor = options?.ChromeToolbarColor;
                    //if (toolbarColor != null)
                    //    tabsBuilder.SetToolbarColor(toolbarColor.ToNativeColor());

                    var intent = tabsBuilder.Build();
                    intent.LaunchUrl(Activity, Android.Net.Uri.Parse(url));
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                return Task.FromResult(false);
            }
        }


        /// <summary>
        /// Share a message with compatible services
        /// </summary>
        /// <param name="message">Message to share</param>
        /// <param name="options">Platform specific options</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public Task<bool> Share(ShareMessage message, ShareOptions options = null)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                var items = new List<string>();
                if (message.Text != null)
                    items.Add(message.Text);
                if (message.Url != null)
                    items.Add(message.Url);

                var intent = new Intent(Intent.ActionSend);
                intent.SetType("text/plain");
                intent.PutExtra(Intent.ExtraText, string.Join(Environment.NewLine, items));
                if (message.Title != null)
                    intent.PutExtra(Intent.ExtraSubject, message.Title);

                var chooserIntent = Intent.CreateChooser(intent, options?.ChooserTitle);
                chooserIntent.SetFlags(ActivityFlags.ClearTop);
                chooserIntent.SetFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(chooserIntent);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Sets text of the clipboard
        /// </summary>
        /// <param name="text">Text to set</param>
        /// <param name="label">Label to display (not required, Android only)</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public Task<bool> SetClipboardText(string text, string label = null)
        {
            try
            {
                var sdk = (int)Android.OS.Build.VERSION.SdkInt;
                if (sdk < (int)Android.OS.BuildVersionCodes.Honeycomb)
                {
                    var clipboard = (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);
                    clipboard.Text = text;
                }
                else
                {
                    var clipboard = (ClipboardManager)Application.Context.GetSystemService(Context.ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText(label ?? string.Empty, text);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                return Task.FromResult(false);
            }
        }

		/// <summary>
		/// Checks if the url can be opened
		/// </summary>
		/// <param name="url">Url to check</param>
		/// <returns>True if it can</returns>
		public bool CanOpenUrl(Activity Activity ,string url)
		{
			try
			{
				var context = Activity ?? Application.Context;
				var intent = new Intent(Intent.ActionView);
				intent.SetData(Android.Net.Uri.Parse(url));

				intent.SetFlags(ActivityFlags.ClearTop);
				intent.SetFlags(ActivityFlags.NewTask);
				return intent.ResolveActivity(context.PackageManager) != null!;
			}
			catch (Exception ex)
			{
                Methods.DisplayReportResultTrack(ex);
				return false;
			}
		}

		/// <summary>
		/// Gets if cliboard is supported
		/// </summary>
		public bool SupportsClipboard => true;
    }
}