using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Com.Aghajari.Emojiview.Sticker;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.StickersView.Page
{
    public class WoWonderStickers : Object, IStickerCategory
    {
        private readonly string Number;
        public WoWonderStickers(string number)
        {
            Number = number;
        }

        public void BindView(View view)
        {

        }

        public View GetEmptyView(ViewGroup viewGroup)
        {
            return null!;
        }

        public Sticker[] GetStickers()
        {
            List<Sticker> ss = new List<Sticker>();
            switch (Number)
            {
                case "1":
                    ss.AddRange(StickersModel.Locally.StickerList1.Select(icon => new Sticker(icon)));
                    break;
                case "2":
                    ss.AddRange(StickersModel.Locally.StickerList2.Select(icon => new Sticker(icon)));
                    break;
                case "3":
                    ss.AddRange(StickersModel.Locally.StickerList3.Select(icon => new Sticker(icon)));
                    break;
                case "4":
                    ss.AddRange(StickersModel.Locally.StickerList4.Select(icon => new Sticker(icon)));
                    break;
                case "5":
                    ss.AddRange(StickersModel.Locally.StickerList5.Select(icon => new Sticker(icon)));
                    break;
                case "6":
                    ss.AddRange(StickersModel.Locally.StickerList6.Select(icon => new Sticker(icon)));
                    break;
                case "7":
                    ss.AddRange(StickersModel.Locally.StickerList7.Select(icon => new Sticker(icon)));
                    break;
                case "8":
                    ss.AddRange(StickersModel.Locally.StickerList8.Select(icon => new Sticker(icon)));
                    break;
                case "9":
                    ss.AddRange(StickersModel.Locally.StickerList9.Select(icon => new Sticker(icon)));
                    break;
                case "10":
                    ss.AddRange(StickersModel.Locally.StickerList10.Select(icon => new Sticker(icon)));
                    break;
                case "11":
                    ss.AddRange(StickersModel.Locally.StickerList11.Select(icon => new Sticker(icon)));
                    break;
                default:
                    break;
            }

            return ss.ToArray();
        }

        public View GetView(ViewGroup viewGroup)
        {
            return null!;
        }

        public bool UseCustomView()
        {
            return false;
        }

        public Object CategoryData
        {
            get
            {
                return Number switch
                {
                    "1" => Resource.Drawable.Sticker1,
                    "2" => Resource.Drawable.sticker2,
                    "3" => Resource.Drawable.Sticker3,
                    "4" => Resource.Drawable.Sticker4,
                    "5" => Resource.Drawable.Sticker5,
                    "6" => Resource.Drawable.Sticker6,
                    "7" => Resource.Drawable.Sticker7,
                    "8" => Resource.Drawable.Sticker8,
                    "9" => Resource.Drawable.Sticker9,
                    "10" => Resource.Drawable.Sticker10,
                    "11" => Resource.Drawable.sticker11,
                    _ => Resource.Drawable.sticker11,
                };
            }
        }
    }
}