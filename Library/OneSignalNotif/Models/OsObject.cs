using System.Collections.Generic;
using Newtonsoft.Json;

namespace WoWonder.Library.OneSignalNotif.Models
{
    public class OsObject
    {
        public class OsNotificationObject
        {
            [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
            public OsFromObject From { get; set; }

            [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
            public OsFromObject To { get; set; }

            public class OsFromObject
            {
                [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
                public string UserId { get; set; }

                [JsonProperty("pushToken", NullValueHandling = NullValueHandling.Ignore)]
                public string PushToken { get; set; }

                [JsonProperty("isPushDisabled", NullValueHandling = NullValueHandling.Ignore)]
                public bool? IsPushDisabled { get; set; }

                [JsonProperty("isSubscribed", NullValueHandling = NullValueHandling.Ignore)]
                public bool? IsSubscribed { get; set; }
            }
        }

        //==================================================================

        public class OsNotificationReceivedObject
        {
            [JsonProperty("notification", NullValueHandling = NullValueHandling.Ignore)]
            public OsNotificationObject Notification { get; set; }

            [JsonProperty("isComplete", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsComplete { get; set; }

            public class OsNotificationObject
            {
                [JsonProperty("androidNotificationId", NullValueHandling = NullValueHandling.Ignore)]
                public long? AndroidNotificationId { get; set; }

                //[JsonProperty("groupedNotifications", NullValueHandling = NullValueHandling.Ignore)]
                //public List<object> GroupedNotifications { get; set; }

                [JsonProperty("notificationId", NullValueHandling = NullValueHandling.Ignore)]
                public string NotificationId { get; set; }

                [JsonProperty("templateName", NullValueHandling = NullValueHandling.Ignore)]
                public string TemplateName { get; set; }

                [JsonProperty("templateId", NullValueHandling = NullValueHandling.Ignore)]
                public string TemplateId { get; set; }

                [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
                public string Title { get; set; }

                [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
                public string Body { get; set; }

                [JsonProperty("lockScreenVisibility", NullValueHandling = NullValueHandling.Ignore)]
                public long? LockScreenVisibility { get; set; }

                [JsonProperty("fromProjectNumber", NullValueHandling = NullValueHandling.Ignore)]
                public string FromProjectNumber { get; set; }

                [JsonProperty("priority", NullValueHandling = NullValueHandling.Ignore)]
                public long? Priority { get; set; }

                [JsonProperty("additionalData", NullValueHandling = NullValueHandling.Ignore)]
                public Dictionary<string, object> AdditionalData { get; set; }

                [JsonProperty("rawPayload", NullValueHandling = NullValueHandling.Ignore)]
                public string RawPayload { get; set; }
            }
        }


    }
}