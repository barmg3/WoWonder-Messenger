using System;
using AutoMapper;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;

namespace WoWonder.Helpers.Utils
{
    public static class ClassMapper
    {
        public static IMapper Mapper;
        public static void SetMappers()
        {
            try
            {
                var configuration = new MapperConfiguration(cfg =>
                {
                    try
                    {
                        cfg.AllowNullCollections = true;

                        cfg.CreateMap<MessageDataExtra, MessageData>();
                        cfg.CreateMap<MessageData, MessageDataExtra>();

                        cfg.CreateMap<GetSiteSettingsObject.ConfigObject, DataTables.SettingsTb>().ForMember(x => x.AutoIdSettings, opt => opt.Ignore());
                        cfg.CreateMap<UserDataObject, DataTables.MyContactsTb>().ForMember(x => x.AutoIdMyFollowing, opt => opt.Ignore());
                        cfg.CreateMap<UserDataObject, DataTables.MyProfileTb>().ForMember(x => x.AutoIdMyProfile, opt => opt.Ignore());
                        cfg.CreateMap<ChatObject, DataTables.LastUsersTb>().ForMember(x => x.AutoIdLastUsers, opt => opt.Ignore());
                        cfg.CreateMap<MessageDataExtra, DataTables.MessageTb>().ForMember(x => x.AutoIdMessage, opt => opt.Ignore());
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                // only during development, validate your mappings; remove it before release
               //configuration.AssertConfigurationIsValid();

                Mapper = configuration.CreateMapper();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
} 