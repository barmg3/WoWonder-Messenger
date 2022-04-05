using System;
using System.Collections.Generic;
using Android.Util;
using WoWonder.Helpers.Utils;

namespace WoWonder.Library.Anjo.SuperTextLibrary
{
    public class StTools
    {
        public enum StTypeText
        {
            ReadMore,
            ReadLess
        }

        public enum XAutoLinkMode
        {
            ModeHashTag,
            ModeMention,
            ModeUrl,
            ModePhone,
            ModeEmail,
            ModeCustom,
        }

        public class XAutoLinkItem
        {
            private readonly XAutoLinkMode AutoLinkMode;

            private readonly string MatchedText;
            private readonly Dictionary<string, string> UserId;

            private readonly int StartPoint;
            private readonly int EndPoint;

            public XAutoLinkItem(int startPoint, int endPoint, string matchedText, XAutoLinkMode autoLinkMode, Dictionary<string, string> userId)
            {
                StartPoint = startPoint;
                EndPoint = endPoint;
                MatchedText = matchedText;
                AutoLinkMode = autoLinkMode;
                UserId = userId;
            }

            public XAutoLinkMode GetAutoLinkMode()
            {
                return AutoLinkMode;
            }

            public string GetMatchedText()
            {
                return MatchedText;
            }

            public Dictionary<string, string> GetUserIdText()
            {
                return UserId;
            }

            public int GetStartPoint()
            {
                return StartPoint;
            }

            public int GetEndPoint()
            {
                return EndPoint;
            }
        }

        public interface IXAutoLinkOnClickListener
        {
            void AutoLinkTextClick(XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData);
        }

        private static class XRegexParser
        {
            public static readonly string PhonePattern = Patterns.Phone.InvokePattern();
            public static readonly string EmailPattern = Patterns.EmailAddress.InvokePattern();
            public static readonly string HashtagPattern = "(?:^|\\s|$)#[\\p{L}0-9_]*";
            public static readonly string MentionPattern = "(?:^|\\s|$|[.])@[\\p{L}0-9_]*";
            public static readonly string UrlPattern = "(^|[\\s.:;?\\-\\]<\\(])((https?://|www\\.|pic\\.)[-\\w;/?:@&=+$\\|\\_.!~*\\|'()\\[\\]%#,☺]+[\\w/#](\\(\\))?)(?=$|[\\s',\\|\\(\\).:;?\\-\\[\\]>\\)])";
        }

        public static class XUtils
        {
            private static bool IsValidRegex(string regex)
            {
                return regex != null && !string.IsNullOrEmpty(regex) && regex.Length > 2;
            }

            public static string GetRegexByAutoLinkMode(XAutoLinkMode anAutoLinkMode, string customRegex)
            {
                try
                {
                    switch (anAutoLinkMode)
                    {
                        case XAutoLinkMode.ModeHashTag:
                            return XRegexParser.HashtagPattern;
                        case XAutoLinkMode.ModeMention:
                            return XRegexParser.MentionPattern;
                        case XAutoLinkMode.ModePhone:
                            return XRegexParser.PhonePattern;
                        case XAutoLinkMode.ModeEmail:
                            return XRegexParser.EmailPattern;
                        case XAutoLinkMode.ModeUrl:
                            return XRegexParser.UrlPattern;
                        case XAutoLinkMode.ModeCustom when !IsValidRegex(customRegex):
                            Console.WriteLine("Your custom regex is null, returning URL_PATTERN");
                            return "";
                        case XAutoLinkMode.ModeCustom:
                            return customRegex;
                        default:
                            return "";
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return "";
                }
            }
        }

    }
}