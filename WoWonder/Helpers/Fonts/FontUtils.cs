﻿using System;
using Android.App;
using Android.Graphics;
using Android.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.Fonts
{
    public static class FontUtils
    {
        //Changes the TextView To IconFrameWork Fonts
        public static void SetTextViewIcon(FontsIconFrameWork type, TextView textViewUi, string iconUnicode)
        {
            try
            {
                switch (type)
                {
                    case FontsIconFrameWork.IonIcons:
                    {
                        var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                        textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                        if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                        //return font;
                        break;
                    }
                    case FontsIconFrameWork.FontAwesomeSolid:
                    {
                        var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-solid-900.ttf");
                        textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                        if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                        //return font;
                        break;
                    }
                    case FontsIconFrameWork.FontAwesomeRegular:
                    {
                        var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-regular-400.ttf");
                        textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                        if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                        //return font;
                        break;
                    }
                    case FontsIconFrameWork.FontAwesomeBrands:
                    {
                        var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-brands-400.ttf");
                        textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                        if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                        //return font;
                        break;
                    }
                    case FontsIconFrameWork.FontAwesomeLight:
                    {
                        var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "fa-light-300.ttf");
                        textViewUi.SetTypeface(font, TypefaceStyle.Normal);
                        if (!string.IsNullOrEmpty(iconUnicode)) textViewUi.Text = iconUnicode;
                        //return font;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                //return null!;
            }
        }
    }
}