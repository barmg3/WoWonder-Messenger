using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using AndroidX.Core.Content;
using WoWonder.Activities.DefaultUser;
using WoWonder.Helpers.Controller;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.SQLite;

namespace WoWonder.Helpers.Utils
{
    public class TextSanitizer : Java.Lang.Object, StTools.IXAutoLinkOnClickListener 
    {
        private readonly SuperTextView SuperTextView;
        private readonly Activity Activity; 

        public TextSanitizer(SuperTextView linkTextView, Activity activity)
        {
            try
            {
                SuperTextView = linkTextView;
                Activity = activity; 
                SuperTextView.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Load(string text, string position = "Left")
        {
            try
            {
                SuperTextView.AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention, StTools.XAutoLinkMode.ModeCustom });
                if (position == "Right" || position == "right")
                {
                    SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModePhone_color)));
                    SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeEmail_color)));
                    SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeHashtag_color)));
                    SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color)));
                    SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeMention_color)));
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color)));
                    SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));
                }
                else
                {
                    SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModePhone_color)));
                    SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModeEmail_color)));
                    SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModeHashtag_color)));
                    SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModeUrl_color)));
                    SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModeMention_color)));
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModeUrl_color)));
                    SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));
                }

                var textt = text.Split('/');
                if (textt.Length > 1)
                {
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.left_ModeUrl_color)));
                    SuperTextView.SetCustomRegex(@"\b(" + textt.LastOrDefault() + @")\b");
                }

                string laststring = text.Replace(" /", " ");
                if (!string.IsNullOrEmpty(laststring))
                    SuperTextView.SetText(laststring, TextView.BufferType.Spannable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
                if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
                {
                    Methods.App.SendEmail(Activity, matchedText.Replace(" ", "").Replace("\n", ""));
                }
                else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
                {
                    string url = matchedText.Replace(" ", "").Replace("\n", "");
                    if (!matchedText.Contains("http"))
                    {
                        url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
                    }

                    //var intent = new Intent(Activity, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url);
                    //intent.PutExtra("Type", url);
                    //Activity.StartActivity(intent);
                    new IntentController(Activity).OpenBrowserFromApp(url);
                }
                else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
                {
                    
                }
                else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
                {
                    var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                    string name = matchedText.Replace("@", "").Replace(" ", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);

                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(Activity, user.UserId, user);
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            var intent = new Intent(Activity, typeof(MyProfileActivity));
                            Activity.StartActivity(intent);

                        }
                        else
                        {
                            var intent = new Intent(Activity, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            Activity.StartActivity(intent);
                        }
                    }
                }
                else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
                {
                    Methods.App.SaveContacts(Activity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }
}