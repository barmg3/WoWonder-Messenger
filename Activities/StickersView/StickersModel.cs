using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Android.Widget;
using Newtonsoft.Json;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.StickersView 
{
    public class StickersModel 
    { 
        public string PackageId { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
        public bool Visibility { get; set; }

        public ItemTypeShop ItemType { get; set; }
        public List<string> ListSticker { get; set; }
        public List<string> ListTags { get; set; }

        public enum ItemTypeShop
        {
            ShopSticker = 0,
            MySticker = 1,
            TrendingSticker = 2,
            RecommendedTag = 3,
            IconSticker = 4,
        }

        /// <summary>
        /// Get your stickers from here  >>> https://chatsticker.com/
        /// Get your stickers from here  >>> https://store.line.me/
        /// </summary>
        public class Locally
        {
            #region rappit

            public static string Sticker1 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719608.png";
            public static string Sticker2 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719613.png";
            public static string Sticker3 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719615.png";
            public static string Sticker4 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719626.png";
            public static string Sticker5 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719628.png";
            public static string Sticker6 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719631.png";
            public static string Sticker7 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719633.png";
            public static string Sticker8 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1165013/android/stickers/6719647.png";
            public static string Sticker9 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274057.png";
            public static string Sticker10 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274058.png";
            public static string Sticker11 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274071.png";
            public static string Sticker12 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274075.png";
            public static string Sticker13 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274081.png";
            public static string Sticker14 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274088.png";
            public static string Sticker15 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1253330/android/stickers/10274089.png";

            public static JavaList<string> StickerList1 = new JavaList<string> {
                Sticker1,
                Sticker2,
                Sticker3,
                Sticker4,
                Sticker5,
                Sticker6,
                Sticker7,
                Sticker8,
                Sticker9,
                Sticker10,
                Sticker11,
                Sticker12,
                Sticker13,
                Sticker14,
                Sticker15
            };

            #endregion

            #region Water Drop

            public static string Sticker16 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382400.png";
            public static string Sticker17 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382401.png";
            public static string Sticker18 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382405.png";
            public static string Sticker19 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382403.png";
            public static string Sticker20 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382414.png";
            public static string Sticker21 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382409.png";
            public static string Sticker22 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382412.png";
            public static string Sticker23 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382417.png";
            public static string Sticker24 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382430.png";
            public static string Sticker25 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382428.png";
            public static string Sticker26 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382425.png";
            public static string Sticker27 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382433.png";
            public static string Sticker28 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382437.png";
            public static string Sticker29 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382435.png";
            public static string Sticker30 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382419.png";
            public static string Sticker31 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1156666/android/stickers/6382422.png";

            public static JavaList<string> StickerList2 = new JavaList<string> {
                Sticker16,
                Sticker17,
                Sticker18,
                Sticker19,
                Sticker20,
                Sticker21,
                Sticker22,
                Sticker23,
                Sticker24,
                Sticker25,
                Sticker26,
                Sticker27,
                Sticker28,
                Sticker29,
                Sticker30,
                Sticker31
            };

            #endregion

            #region Monster

            public static string Sticker32 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913417.png";
            public static string Sticker33 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913416.png";
            public static string Sticker34 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913421.png";
            public static string Sticker35 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913424.png";
            public static string Sticker36 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913426.png";
            public static string Sticker37 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913427.png";
            public static string Sticker38 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913430.png";
            public static string Sticker39 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913436.png";
            public static string Sticker40 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913432.png";
            public static string Sticker41 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913420.png";
            public static string Sticker42 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913418.png";
            public static string Sticker43 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913422.png";
            public static string Sticker44 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913438.png";
            public static string Sticker45 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913429.png";
            public static string Sticker46 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913431.png";
            public static string Sticker47 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913439.png";
            public static string Sticker48 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/5957/android/stickers/9913435.png";

            public static JavaList<string> StickerList3 = new JavaList<string> {
                Sticker32,
                Sticker33,
                Sticker34,
                Sticker35,
                Sticker36,
                Sticker37,
                Sticker38,
                Sticker39,
                Sticker40,
                Sticker41,
                Sticker42,
                Sticker43,
                Sticker44,
                Sticker45,
                Sticker46,
                Sticker47,
                Sticker48
            };

            #endregion

            #region NINJA Nyankko

            public static string Sticker49 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116876.png";
            public static string Sticker50 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116877.png";
            public static string Sticker51 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116878.png";
            public static string Sticker52 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116879.png";
            public static string Sticker53 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116880.png";
            public static string Sticker54 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116881.png";
            public static string Sticker55 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116883.png";
            public static string Sticker56 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116884.png";
            public static string Sticker57 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116887.png";
            public static string Sticker58 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116888.png";
            public static string Sticker59 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116889.png";
            public static string Sticker60 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116890.png";
            public static string Sticker61 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116891.png";
            public static string Sticker62 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116892.png";
            public static string Sticker63 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116893.png";
            public static string Sticker64 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116894.png";
            public static string Sticker65 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116895.png";
            public static string Sticker66 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116896.png";
            public static string Sticker67 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116897.png";
            public static string Sticker68 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116898.png";
            public static string Sticker69 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116899.png";
            public static string Sticker70 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116900.png";
            public static string Sticker71 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116901.png";
            public static string Sticker72 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116902.png";
            public static string Sticker73 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116903.png";
            public static string Sticker74 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116904.png";
            public static string Sticker75 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116905.png";
            public static string Sticker76 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116906.png";
            public static string Sticker77 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116907.png";
            public static string Sticker78 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116908.png";
            public static string Sticker79 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116909.png";
            public static string Sticker80 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116910.png";
            public static string Sticker81 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116911.png;";
            public static string Sticker82 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1001463/android/stickers/116912.png";

            public static JavaList<string> StickerList4 = new JavaList<string> {
                Sticker49,
                Sticker50,
                Sticker51,
                Sticker52,
                Sticker53,
                Sticker54,
                Sticker55,
                Sticker56,
                Sticker57,
                Sticker58,
                Sticker59,
                Sticker60,
                Sticker61,
                Sticker62,
                Sticker63,
                Sticker64,
                Sticker65,
                Sticker66,
                Sticker67,
                Sticker68,
                Sticker69,
                Sticker70,
                Sticker71,
                Sticker72,
                Sticker73,
                Sticker74,
                Sticker75,
                Sticker76,
                Sticker77,
                Sticker78,
                Sticker79,
                Sticker80,
                Sticker81,
                Sticker82
            };

            #endregion

            #region So Much Love

            public static string Sticker83 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561130.png";
            public static string Sticker84 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561131.png";
            public static string Sticker85 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561132.png";
            public static string Sticker86 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561133.png";
            public static string Sticker87 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561134.png";
            public static string Sticker88 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561135.png";
            public static string Sticker89 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561136.png";
            public static string Sticker90 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561137.png";
            public static string Sticker91 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561138.png";
            public static string Sticker92 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561139.png";
            public static string Sticker93 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561140.png";
            public static string Sticker94 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561141.png";
            public static string Sticker95 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561142.png";
            public static string Sticker96 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561143.png";
            public static string Sticker97 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561144.png";
            public static string Sticker98 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561145.png";
            public static string Sticker99 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561146.png";
            public static string Sticker100 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561147.png";
            public static string Sticker101 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561148.png";
            public static string Sticker102 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561149.png";
            public static string Sticker103 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561150.png";
            public static string Sticker104 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561151.png";
            public static string Sticker105 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561152.png";
            public static string Sticker106 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561155.png";
            public static string Sticker107 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561156.png";
            public static string Sticker108 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561157.png";
            public static string Sticker109 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561158.png";
            public static string Sticker110 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561159.png";
            public static string Sticker111 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561160.png";
            public static string Sticker112 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561161.png";
            public static string Sticker113 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561162.png";
            public static string Sticker114 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561163.png";
            public static string Sticker115 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561164.png";
            public static string Sticker116 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561165.png";
            public static string Sticker117 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561166.png";
            public static string Sticker118 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1210792/android/stickers/8561167.png";

            public static JavaList<string> StickerList5 = new JavaList<string> {
                Sticker83,
                Sticker84,
                Sticker85,
                Sticker86,
                Sticker87,
                Sticker88,
                Sticker89,
                Sticker90,
                Sticker91,
                Sticker92,
                Sticker93,
                Sticker94,
                Sticker95,
                Sticker96,
                Sticker97,
                Sticker98,
                Sticker99,
                Sticker100,
                Sticker101,
                Sticker102,
                Sticker103,
                Sticker104,
                Sticker105,
                Sticker106,
                Sticker107,
                Sticker108,
                Sticker109,
                Sticker110,
                Sticker111,
                Sticker112,
                Sticker113,
                Sticker114,
                Sticker115,
                Sticker116,
                Sticker117,
                Sticker118
            };

            #endregion

            #region Sukkara chan

            public static string Sticker119 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599523.png";
            public static string Sticker120 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599524.png";
            public static string Sticker121 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599525.png";
            public static string Sticker122 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599526.png";
            public static string Sticker123 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599527.png";
            public static string Sticker124 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599528.png";
            public static string Sticker125 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599529.png";
            public static string Sticker126 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599530.png";
            public static string Sticker127 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599531.png";
            public static string Sticker128 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599532.png";
            public static string Sticker129 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599533.png";
            public static string Sticker130 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599536.png";
            public static string Sticker131 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599537.png";
            public static string Sticker132 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599538.png";
            public static string Sticker133 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599539.png";
            public static string Sticker134 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599540.png";
            public static string Sticker135 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599541.png";
            public static string Sticker136 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599542.png";
            public static string Sticker137 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599543.png";
            public static string Sticker138 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599544.png";
            public static string Sticker139 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599545.png";
            public static string Sticker140 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599546.png";
            public static string Sticker141 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599547.png";
            public static string Sticker142 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599548.png";
            public static string Sticker143 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599549.png";
            public static string Sticker144 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599550.png";
            public static string Sticker145 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599551.png";
            public static string Sticker146 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599552.png";
            public static string Sticker147 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599553.png";
            public static string Sticker148 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599554.png";
            public static string Sticker149 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599555.png";
            public static string Sticker150 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599556.png";
            public static string Sticker151 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599557.png";
            public static string Sticker152 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599558.png";
            public static string Sticker153 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599559.png";
            public static string Sticker154 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1062824/android/stickers/2599560.png";

            public static JavaList<string> StickerList6 = new JavaList<string> {
                Sticker119,
                Sticker120,
                Sticker121,
                Sticker122,
                Sticker123,
                Sticker124,
                Sticker125,
                Sticker126,
                Sticker127,
                Sticker128,
                Sticker129,
                Sticker130,
                Sticker131,
                Sticker132,
                Sticker133,
                Sticker134,
                Sticker135,
                Sticker136,
                Sticker137,
                Sticker138,
                Sticker139,
                Sticker140,
                Sticker141,
                Sticker142,
                Sticker143,
                Sticker144,
                Sticker145,
                Sticker146,
                Sticker147,
                Sticker148,
                Sticker149,
                Sticker150,
                Sticker151,
                Sticker152,
                Sticker153,
                Sticker154
            };

            #endregion

            #region Flower Hijab

            public static string Sticker155 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214293.png";
            public static string Sticker156 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214294.png";
            public static string Sticker157 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214295.png";
            public static string Sticker158 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214296.png";
            public static string Sticker159 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214297.png";
            public static string Sticker160 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214298.png";
            public static string Sticker161 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214299.png";
            public static string Sticker162 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214300.png";
            public static string Sticker163 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214301.png";
            public static string Sticker164 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214302.png";
            public static string Sticker165 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214303.png";
            public static string Sticker166 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214304.png";
            public static string Sticker167 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214305.png";
            public static string Sticker168 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214306.png";
            public static string Sticker169 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214307.png";
            public static string Sticker170 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214308.png";
            public static string Sticker171 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214309.png";
            public static string Sticker172 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214310.png";
            public static string Sticker173 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214311.png";
            public static string Sticker174 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214312.png";
            public static string Sticker175 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214313.png";
            public static string Sticker176 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214314.png";
            public static string Sticker177 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214315.png";
            public static string Sticker178 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214316.png";
            public static string Sticker179 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214317.png";
            public static string Sticker180 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214318.png";
            public static string Sticker181 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214319.png";
            public static string Sticker182 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214320.png";
            public static string Sticker183 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214321.png";
            public static string Sticker184 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214322.png";
            public static string Sticker185 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214323.png";
            public static string Sticker186 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214324.png";
            public static string Sticker187 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214325.png";
            public static string Sticker188 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214326.png";
            public static string Sticker189 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214327.png";
            public static string Sticker190 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214328.png";
            public static string Sticker191 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214329.png";
            public static string Sticker192 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214330.png";
            public static string Sticker193 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214331.png";
            public static string Sticker194 = "https://sdl-stickershop.line.naver.jp/products/0/0/2/1003801/android/stickers/214332.png";

            public static JavaList<string> StickerList7 = new JavaList<string> {
                Sticker155,
                Sticker156,
                Sticker157,
                Sticker158,
                Sticker159,
                Sticker160,
                Sticker161,
                Sticker162,
                Sticker163,
                Sticker164,
                Sticker165,
                Sticker166,
                Sticker167,
                Sticker168,
                Sticker169,
                Sticker170,
                Sticker171,
                Sticker172,
                Sticker173,
                Sticker175,
                Sticker176,
                Sticker177,
                Sticker178,
                Sticker179,
                Sticker180,
                Sticker181,
                Sticker182,
                Sticker183,
                Sticker184,
                Sticker185,
                Sticker186,
                Sticker187,
                Sticker188,
                Sticker189,
                Sticker190,
                Sticker191,
                Sticker192,
                Sticker193,
                Sticker194
            };

            #endregion

            #region Trendy boy

            public static string Sticker195 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349166.png";
            public static string Sticker196 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349167.png";
            public static string Sticker197 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349168.png";
            public static string Sticker198 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349169.png";
            public static string Sticker199 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349170.png";
            public static string Sticker200 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349171.png";
            public static string Sticker201 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349172.png";
            public static string Sticker202 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349173.png";
            public static string Sticker203 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349174.png";
            public static string Sticker204 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349175.png";
            public static string Sticker205 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349176.png";
            public static string Sticker206 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349177.png";
            public static string Sticker207 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349178.png";
            public static string Sticker208 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349179.png";
            public static string Sticker209 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349180.png";
            public static string Sticker210 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349181.png";
            public static string Sticker211 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349182.png";
            public static string Sticker212 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349183.png";
            public static string Sticker213 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349184.png";
            public static string Sticker214 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349185.png";
            public static string Sticker215 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349186.png";
            public static string Sticker216 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349187.png";
            public static string Sticker217 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349188.png";
            public static string Sticker218 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349189.png";
            public static string Sticker219 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349190.png";
            public static string Sticker220 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349191.png";
            public static string Sticker221 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349192.png";
            public static string Sticker222 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349193.png";
            public static string Sticker223 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349194.png";
            public static string Sticker224 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349195.png";
            public static string Sticker225 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349196.png";
            public static string Sticker226 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349197.png";
            public static string Sticker227 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349198.png";
            public static string Sticker228 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349199.png";
            public static string Sticker229 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349200.png";
            public static string Sticker230 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349201.png";
            public static string Sticker231 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349202.png";
            public static string Sticker232 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349203.png";
            public static string Sticker233 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349204.png";
            public static string Sticker234 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1331246/android/stickers/13349205.png";

            public static JavaList<string> StickerList8 = new JavaList<string> {
                Sticker195,
                Sticker196,
                Sticker197,
                Sticker198,
                Sticker199,
                Sticker200,
                Sticker201,
                Sticker202,
                Sticker203,
                Sticker204,
                Sticker205,
                Sticker206,
                Sticker207,
                Sticker208,
                Sticker209,
                Sticker210,
                Sticker211,
                Sticker212,
                Sticker213,
                Sticker214,
                Sticker215,
                Sticker216,
                Sticker217,
                Sticker218,
                Sticker219,
                Sticker220,
                Sticker221,
                Sticker222,
                Sticker223,
                Sticker224,
                Sticker225,
                Sticker226,
                Sticker227,
                Sticker228,
                Sticker229,
                Sticker230,
                Sticker231,
                Sticker232,
                Sticker233,
                Sticker234
            };

            #endregion

            #region The stickman and the cat

            public static string Sticker235 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779268.png";
            public static string Sticker236 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779269.png";
            public static string Sticker237 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779270.png";
            public static string Sticker238 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779271.png";
            public static string Sticker239 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779272.png";
            public static string Sticker240 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779273.png";
            public static string Sticker241 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779274.png";
            public static string Sticker242 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779275.png";
            public static string Sticker243 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779276.png";
            public static string Sticker244 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779277.png";
            public static string Sticker245 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779278.png";
            public static string Sticker246 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779279.png";
            public static string Sticker247 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779280.png";
            public static string Sticker248 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779281.png";
            public static string Sticker249 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779282.png";
            public static string Sticker250 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779283.png";
            public static string Sticker251 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779284.png";
            public static string Sticker252 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779285.png";
            public static string Sticker253 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779286.png";
            public static string Sticker254 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779287.png";
            public static string Sticker255 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779288.png";
            public static string Sticker256 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779289.png";
            public static string Sticker257 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779290.png";
            public static string Sticker258 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779291.png";
            public static string Sticker259 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779292.png";
            public static string Sticker260 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779293.png";
            public static string Sticker261 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779294.png";
            public static string Sticker262 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779295.png";
            public static string Sticker263 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779296.png";
            public static string Sticker264 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779297.png";
            public static string Sticker265 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779298.png";
            public static string Sticker266 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779299.png";
            public static string Sticker267 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779300.png";
            public static string Sticker268 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779301.png";
            public static string Sticker269 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779302.png";
            public static string Sticker270 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779303.png";
            public static string Sticker271 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779304.png";
            public static string Sticker272 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779305.png";
            public static string Sticker273 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779306.png";
            public static string Sticker274 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/1191351/android/stickers/7779307.png";

            public static JavaList<string> StickerList9 = new JavaList<string> {
                Sticker235,
                Sticker236,
                Sticker237,
                Sticker238,
                Sticker239,
                Sticker240,
                Sticker241,
                Sticker242,
                Sticker243,
                Sticker244,
                Sticker245,
                Sticker246,
                Sticker247,
                Sticker248,
                Sticker249,
                Sticker250,
                Sticker251,
                Sticker252,
                Sticker253,
                Sticker254,
                Sticker255,
                Sticker256,
                Sticker257,
                Sticker258,
                Sticker259,
                Sticker260,
                Sticker261,
                Sticker262,
                Sticker263,
                Sticker264,
                Sticker265,
                Sticker266,
                Sticker267,
                Sticker268,
                Sticker269,
                Sticker270,
                Sticker271,
                Sticker272,
                Sticker273,
                Sticker274
            };

            #endregion

            #region Chip 'n' Dale Animated

            public static string Sticker275 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867901.png";
            public static string Sticker276 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867902.png";
            public static string Sticker277 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867903.png";
            public static string Sticker278 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867904.png";
            public static string Sticker279 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867905.png";
            public static string Sticker280 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867906.png";
            public static string Sticker281 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867907.png";
            public static string Sticker282 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867908.png";
            public static string Sticker283 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867909.png";
            public static string Sticker284 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867910.png";
            public static string Sticker285 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867911.png";
            public static string Sticker286 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867912.png";
            public static string Sticker287 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867913.png";
            public static string Sticker288 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867915.png";
            public static string Sticker289 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867916.png";
            public static string Sticker290 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867917.png";
            public static string Sticker291 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867918.png";
            public static string Sticker292 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867919.png";
            public static string Sticker293 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867920.png";
            public static string Sticker294 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867921.png";
            public static string Sticker295 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867922.png";
            public static string Sticker296 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867923.png";
            public static string Sticker297 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867924.png";
            public static string Sticker298 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867925.png";
            public static string Sticker299 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867926.png";
            public static string Sticker300 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867927.png";
            public static string Sticker301 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867928.png";
            public static string Sticker302 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867929.png";
            public static string Sticker303 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867930.png";
            public static string Sticker304 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867931.png";
            public static string Sticker305 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867932.png";
            public static string Sticker306 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867933.png";
            public static string Sticker307 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867934.png";
            public static string Sticker308 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867935.png";
            public static string Sticker309 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867936.png";
            public static string Sticker310 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867937.png";
            public static string Sticker311 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867938.png";
            public static string Sticker312 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867939.png";
            public static string Sticker313 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867941.png";
            public static string Sticker314 = "https://sdl-stickershop.line.naver.jp/products/0/0/1/3296/android/stickers/1867942.png";

            public static JavaList<string> StickerList10 = new JavaList<string> {
                Sticker275,
                Sticker276,
                Sticker277,
                Sticker278,
                Sticker279,
                Sticker280,
                Sticker281,
                Sticker282,
                Sticker283,
                Sticker284,
                Sticker285,
                Sticker286,
                Sticker287,
                Sticker288,
                Sticker289,
                Sticker290,
                Sticker291,
                Sticker292,
                Sticker293,
                Sticker294,
                Sticker295,
                Sticker296,
                Sticker297,
                Sticker298,
                Sticker299,
                Sticker300,
                Sticker301,
                Sticker302,
                Sticker303,
                Sticker304,
                Sticker305,
                Sticker306,
                Sticker307,
                Sticker308,
                Sticker309,
                Sticker310,
                Sticker311,
                Sticker312,
                Sticker313,
                Sticker314
            };

            #endregion

            #region Others

            public static string Sticker315 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688564/ANDROID/sticker.png";
            public static string Sticker316 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688568/ANDROID/sticker.png";
            public static string Sticker317 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688570/ANDROID/sticker.png";
            public static string Sticker318 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688571/ANDROID/sticker.png";
            public static string Sticker319 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688578/ANDROID/sticker.png";
            public static string Sticker320 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688575/ANDROID/sticker.png";
            public static string Sticker321 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688576/ANDROID/sticker.png";
            public static string Sticker322 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688580/ANDROID/sticker.png";
            public static string Sticker323 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/22108/ANDROID/sticker.png";
            public static string Sticker324 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/22112/ANDROID/sticker.png";
            public static string Sticker325 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/22111/ANDROID/sticker.png";
            public static string Sticker326 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/13456187/ANDROID/sticker.png";
            public static string Sticker327 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/13456184/ANDROID/sticker.png";
            public static string Sticker328 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/13456186/ANDROID/sticker.png";
            public static string Sticker329 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/13456188/ANDROID/sticker.png";
            public static string Sticker330 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/13456182/ANDROID/sticker.png";
            public static string Sticker331 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/23944390/ANDROID/sticker.png";
            public static string Sticker332 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/23944368/ANDROID/sticker.png";
            public static string Sticker333 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135256/ANDROID/sticker.png";
            public static string Sticker334 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135257/ANDROID/sticker.png";
            public static string Sticker335 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135258/ANDROID/sticker.png";
            public static string Sticker336 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135259/ANDROID/sticker.png";
            public static string Sticker337 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135260/ANDROID/sticker.png";
            public static string Sticker338 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135261/ANDROID/sticker.png";
            public static string Sticker339 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135262/ANDROID/sticker.png";
            public static string Sticker340 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135263/ANDROID/sticker.png";
            public static string Sticker341 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135264/ANDROID/sticker.png";
            public static string Sticker342 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135265/ANDROID/sticker.png";
            public static string Sticker343 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135266/ANDROID/sticker.png";
            public static string Sticker344 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135267/ANDROID/sticker.png";
            public static string Sticker345 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/29135268/ANDROID/sticker.png";
            public static string Sticker346 = "https://stickershop.line-scdn.net/stickershop/v1/product/1041829/LINEStorePC/main.png";
            public static string Sticker347 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688558/ANDROID/sticker.png";
            public static string Sticker348 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688559/ANDROID/sticker.png";
            public static string Sticker349 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688562/ANDROID/sticker.png";
            public static string Sticker350 = "https://stickershop.line-scdn.net/stickershop/v1/sticker/68688563/ANDROID/sticker.png";

            public static JavaList<string> StickerList11 = new JavaList<string> {
                Sticker315,
                Sticker316,
                Sticker317,
                Sticker318,
                Sticker319,
                Sticker320,
                Sticker321,
                Sticker322,
                Sticker323,
                Sticker324,
                Sticker325,
                Sticker326,
                Sticker327,
                Sticker328,
                Sticker329,
                Sticker330,
                Sticker331,
                Sticker332,
                Sticker333,
                Sticker334,
                Sticker335,
                Sticker336,
                Sticker337,
                Sticker338,
                Sticker339,
                Sticker340,
                Sticker341,
                Sticker342,
                Sticker343,
                Sticker344,
                Sticker345,
                Sticker346,
                Sticker347,
                Sticker348,
                Sticker349,
                Sticker350,
            };

            #endregion

        }

        //############################# Stickers #############################

        public class ApiStipop
        {
            private static string Search = "https://messenger.stipop.io/v1/search?userId=2022";
            private static string TrendingSearch = "https://messenger.stipop.io/v1/search/keyword?userId=2022";
            private static string StickerPackInfo = "https://messenger.stipop.io/v1/package/";
            private static string TrendingStickerPack = "https://messenger.stipop.io/v1/package?userId=2022";
            private static string NewStickerPacks = "https://messenger.stipop.io/v1/package/new?userId=2022";
             
            public static async Task<StipopClasses.StipopStickerObject> ApiGetSearch(string keyword, string offset = "1")
            {
                try
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return null;
                    }

                    var client = new HttpClient(); 
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(Search + UserDetails.UserId + "&lang=en&pageNumber=" + offset + "&limit=10&premium=N&animated=Y&q=" + keyword),
                        Headers =
                        {
                            {"apikey", AppSettings.StickersApikey},
                        }
                    };

                    var response = await client.SendAsync(request); 
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<StipopClasses.StipopStickerObject>(json);
                    if (data != null)
                    {
                        if (data.Header.Status == "success" && data.Body.StickerList.Count > 0)
                        {
                            return data;
                        }
                    }
                   
                    return null;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            } 

            public static async Task<ObservableCollection<StipopClasses.KeywordListObject>> ApiGetTrendingSearch()
            {
                try
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return null;
                    }

                    var client = new HttpClient();

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(TrendingSearch + UserDetails.UserId),
                        Headers =
                        {
                            {"apikey", AppSettings.StickersApikey},
                        }
                    };

                    var response = await client.SendAsync(request); 
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<StipopClasses.StipopStickerObject>(json);
                    if (data != null)
                    {
                        if (data.Header.Status == "success")
                        {
                            return new ObservableCollection<StipopClasses.KeywordListObject>(data.Body.KeywordList);
                        }
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            } 

            public static async Task<StipopClasses.PackageObject> ApiGetStickerPackInfo(string packageId)
            {
                try
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return null;
                    }

                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(StickerPackInfo + packageId + "?userId=2022" + UserDetails.UserId + "&premium=N&animated=Y"),
                        Headers =
                        {
                            {"apikey", AppSettings.StickersApikey},
                        }
                    };

                    var response = await client.SendAsync(request); 
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<StipopClasses.StipopStickerObject>(json);
                    if (data != null)
                    {
                        if (data.Header.Status == "success")
                        {
                            return data.Body.Package;
                        }
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            } 
        
            public static async Task<ObservableCollection<StipopClasses.PackageObject>> ApiGetTrendingStickerPack(string keyword)
            {
                try
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return null;
                    }

                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(TrendingStickerPack + UserDetails.UserId + "&lang=en&pageNumber=1&limit=10&premium=N&animated=Y&q=" + keyword),
                        Headers =
                        {
                            {"apikey", AppSettings.StickersApikey},
                        }
                    };

                    var response = await client.SendAsync(request); 
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<StipopClasses.StipopStickerObject>(json);
                    if (data != null)
                    {
                        if (data.Header.Status == "success")
                        {
                            return new ObservableCollection<StipopClasses.PackageObject>(data.Body.PackageList);
                        }
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            } 
        
            public static async Task<ObservableCollection<StipopClasses.PackageObject>> ApiGetNewStickerPacks(string offset = "1")
            {
                try
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return null;
                    }

                    var client = new HttpClient();
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(NewStickerPacks + UserDetails.UserId + "&pageNumber="+ offset + "&lang=en&countryCode=US&limit=10&premium=N&animated=Y"),
                        Headers =
                        {
                            {"apikey", AppSettings.StickersApikey},
                        }
                    };

                    var response = await client.SendAsync(request); 
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<StipopClasses.StipopStickerObject>(json);
                    if (data != null)
                    {
                        if (data.Header.Status == "success")
                        {
                            return new ObservableCollection<StipopClasses.PackageObject>(data.Body.PackageList);
                        }
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null;
                }
            } 
        }
         
        public class StipopClasses
        { 
            public class StipopStickerObject
            {
                [JsonProperty("header", NullValueHandling = NullValueHandling.Ignore)]
                public Header Header { get; set; }

                [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
                public Body Body { get; set; }
            }

            public class Header
            {
                [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
                public string Code { get; set; }

                [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
                public string Status { get; set; }

                [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
                public string Message { get; set; }
            }

            public class Body
            {
                //Search
                [JsonProperty("stickerList", NullValueHandling = NullValueHandling.Ignore)]
                public List<StickerDataObject> StickerList { get; set; }

                [JsonProperty("pageMap", NullValueHandling = NullValueHandling.Ignore)]
                public PageMapObject PageMap { get; set; }

                //=================================================
                //TrendingSearch
                [JsonProperty("keywordList", NullValueHandling = NullValueHandling.Ignore)]
                public List<KeywordListObject> KeywordList { get; set; }

                //=================================================
                //StickerPackInfo
                [JsonProperty("package", NullValueHandling = NullValueHandling.Ignore)]
                public PackageObject Package { get; set; }


                //=================================================
                //Trending Sticker
                [JsonProperty("packageList", NullValueHandling = NullValueHandling.Ignore)]
                public List<PackageObject> PackageList { get; set; } 
            }

            public class PageMapObject
            {
                [JsonProperty("pageNumber", NullValueHandling = NullValueHandling.Ignore)]
                public long? PageNumber { get; set; }

                [JsonProperty("onePageCountRow", NullValueHandling = NullValueHandling.Ignore)]
                public long? OnePageCountRow { get; set; }

                [JsonProperty("totalCount", NullValueHandling = NullValueHandling.Ignore)]
                public long? TotalCount { get; set; }

                [JsonProperty("pageCount", NullValueHandling = NullValueHandling.Ignore)]
                public long? PageCount { get; set; }

                [JsonProperty("groupCount", NullValueHandling = NullValueHandling.Ignore)]
                public long? GroupCount { get; set; }

                [JsonProperty("groupNumber", NullValueHandling = NullValueHandling.Ignore)]
                public long? GroupNumber { get; set; }

                [JsonProperty("pageGroupCount", NullValueHandling = NullValueHandling.Ignore)]
                public long? PageGroupCount { get; set; }

                [JsonProperty("startPage", NullValueHandling = NullValueHandling.Ignore)]
                public long? StartPage { get; set; }

                [JsonProperty("endPage", NullValueHandling = NullValueHandling.Ignore)]
                public long? EndPage { get; set; }

                [JsonProperty("startRow", NullValueHandling = NullValueHandling.Ignore)]
                public long? StartRow { get; set; }

                [JsonProperty("endRow", NullValueHandling = NullValueHandling.Ignore)]
                public long? EndRow { get; set; }

                [JsonProperty("modNum", NullValueHandling = NullValueHandling.Ignore)]
                public long? ModNum { get; set; }

                [JsonProperty("listStartNumber", NullValueHandling = NullValueHandling.Ignore)]
                public long? ListStartNumber { get; set; }
            }

            public class StickerDataObject
            {
                [JsonProperty("stickerId", NullValueHandling = NullValueHandling.Ignore)]
                public string StickerId { get; set; }

                [JsonProperty("packageId", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageId { get; set; }

                [JsonProperty("artistName", NullValueHandling = NullValueHandling.Ignore)]
                public string ArtistName { get; set; }

                [JsonProperty("keyword", NullValueHandling = NullValueHandling.Ignore)]
                public string Keyword { get; set; }

                [JsonProperty("keywordIt", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordIt { get; set; }

                [JsonProperty("keywordPt", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordPt { get; set; }

                [JsonProperty("keywordRu", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordRu { get; set; }

                [JsonProperty("keywordEs", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordEs { get; set; }

                [JsonProperty("keywordFr", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordFr { get; set; }

                [JsonProperty("keywordKr", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordKr { get; set; }

                [JsonProperty("keywordZhCn", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordZhCn { get; set; }

                [JsonProperty("keywordZhTw", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordZhTw { get; set; }

                [JsonProperty("keywordVi", NullValueHandling = NullValueHandling.Ignore)]
                public string KeywordVi { get; set; }

                [JsonProperty("stickerImg", NullValueHandling = NullValueHandling.Ignore)]
                public string StickerImg { get; set; }

                [JsonProperty("favoriteYN", NullValueHandling = NullValueHandling.Ignore)]
                public string FavoriteYn { get; set; }
            }
             
            public class KeywordListObject
            {
                [JsonProperty("keyword", NullValueHandling = NullValueHandling.Ignore)]
                public string Keyword { get; set; }
            }

            public class PackageObject
            {
                [JsonProperty("packageId", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageId { get; set; }

                [JsonProperty("artistName", NullValueHandling = NullValueHandling.Ignore)]
                public string ArtistName { get; set; }

                [JsonProperty("packageName", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageName { get; set; }

                [JsonProperty("packageImg", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageImg { get; set; }

                [JsonProperty("packageAnimated", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageAnimated { get; set; }

                [JsonProperty("packageCategory", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageCategory { get; set; }

                [JsonProperty("packageKeywords", NullValueHandling = NullValueHandling.Ignore)]
                public string PackageKeywords { get; set; }

                [JsonProperty("isNew", NullValueHandling = NullValueHandling.Ignore)]
                public string IsNew { get; set; }

                [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
                public string Language { get; set; }

                [JsonProperty("isDownload", NullValueHandling = NullValueHandling.Ignore)]
                public string IsDownload { get; set; }

                [JsonProperty("isWish", NullValueHandling = NullValueHandling.Ignore)]
                public string IsWish { get; set; }

                [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
                public List<StickerDataObject> Stickers { get; set; }
            }

        }


    }
}