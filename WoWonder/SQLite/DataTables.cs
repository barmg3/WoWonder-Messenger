using SQLite;
using WoWonder.Helpers.Model;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;

namespace WoWonder.SQLite
{
    public class DataTables
    {
        [Table("LoginTb")]
        public class LoginTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdLogin { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string AccessToken { get; set; }
            public string Cookie { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Lang { get; set; }
            public string DeviceId { get; set; }
        }

        [Table("SettingsTb")]
        public class SettingsTb : GetSiteSettingsObject.ConfigObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdSettings { get; set; }
             
            public new string CurrencyArray { get; set; }
            public new string CurrencySymbolArray  { get; set; }
            public new string PageCategories  { get; set; }
            public new string GroupCategories  { get; set; }
            public new string BlogCategories  { get; set; }
            public new string ProductsCategories  { get; set; }
            public new string JobCategories { get; set; }
            public new string Genders { get; set; }
            public new string Family  { get; set; }
            public new string MovieCategory { get; set; }
            public new string PostColors  { get; set; }
            public new string Fields { get; set; } 
            public new string PageSubCategories { get; set; } 
            public new string GroupSubCategories  { get; set; } 
            public new string ProductsSubCategories { get; set; } 
            public new string PageCustomFields { get; set; } 
            public new string GroupCustomFields { get; set; } 
            public new string ProductCustomFields { get; set; } 
            public new string PostReactionsTypes  { get; set; } 
            public new string ProPackages { get; set; } 
            public new string ProPackagesTypes { get; set; }  
        }

        [Table("MyContactsTb")]
        public class MyContactsTb : UserDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMyFollowing { get; set; }

            public new string ApiNotificationSettings { get; set; }
            public new string Details { get; set; }
        }
         
        [Table("MyProfileTb")]
        public class MyProfileTb : UserDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMyProfile { get; set; }

            public new string ApiNotificationSettings { get; set; }
            public new string Details { get; set; }
        }

        [Table("SearchFilterTb")]
        public class SearchFilterTb
        {
            [PrimaryKey, AutoIncrement]
            public int  AutoIdSearchFilter { get; set; }

            public string Gender { get; set; }
            public string Country { get; set; }
            public string Status { get; set; }
            public string Verified { get; set; }
            public string ProfilePicture { get; set; }
            public string FilterByAge { get; set; }
            public string AgeFrom { get; set; }
            public string AgeTo { get; set; }
        }

        [Table("NearByFilterTb")]
        public class NearByFilterTb
        { 
            [PrimaryKey, AutoIncrement]
            public int AutoIdNearByFilter { get; set; }
            
            public int DistanceValue { get; set; }
            public string Gender { get; set; }
            public int Status { get; set; }
            public string Relationship { get; set; }
        }

        [Table("FilterLastChatTb")]
        public class FilterLastChatTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdLastChatFilter { get; set; }

            public string Status { get; set; }
            public string Type { get; set; }
        }

        [Table("LastUsersTb")] //New Version
        public class LastUsersTb : ChatObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdLastUsers { get; set; }

            public new string ApiNotificationSettings { get; set; }
            public new string Details { get; set; }
            public new string UserData { get; set; }
            public new string LastMessage { get; set; }
            public new string Parts { get; set; }
            public new string Mute { get; set; }
        }

        [Table("MessageTb")]
        public class MessageTb : MessageDataExtra
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMessage { get; set; }

            public new string Product { get; set; }
            public new string MessageUser { get; set; }
            public new string UserData { get; set; }
            public new string ToData { get; set; }
            public new string Reaction { get; set; }
            public new string Reply { get; set; }
            public new string Story { get; set; }

            //not Important
            public new string MediaPlayer { get; set; }
            public new string MediaTimer { get; set; }
            public new string FitchOgLink { get; set; }
        }

        [Table("CallUserTb")]
        public class CallUserTb
        {
            [PrimaryKey, AutoIncrement] public int IdCallUser { get; set; }

            public string VideoCall { get; set; }

            public string UserId { get; set; }
            public string Avatar { get; set; }
            public string Name { get; set; }

            //Data
            public string CallId { get; set; }
            public string AccessToken { get; set; }
            public string AccessToken2 { get; set; }
            public string FromId { get; set; }
            public string ToId { get; set; }
            public string Active { get; set; }
            public string Called { get; set; }
            public string Time { get; set; }
            public string Declined { get; set; }
            public string Url { get; set; }
            public string Status { get; set; }
            public string RoomName { get; set; }
            public string Type { get; set; }

            //Style       
            public string TypeIcon { get; set; }
            public string TypeColor { get; set; }
        }


        [Table("MuteTb")]
        public class MuteTb : Classes.OptionLastChat
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMute { get; set; }

        }

        [Table("PinTb")]
        public class PinTb : Classes.OptionLastChat
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdPin { get; set; }

        }

        [Table("ArchiveTb")]
        public class ArchiveTb : Classes.LastChatArchive
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdArchive { get; set; }

            public new string LastChat { get; set; }
            public new string LastChatPage { get; set; }
        }

        [Table("StickersTb")]
        public class StickersTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdStickers { get; set; }

            public string PackageId { get; set; }
            public string Name { get; set; }
            public string Count { get; set; }
            public bool Visibility { get; set; }

            public string StickerList { get; set; }
        }

    }
}