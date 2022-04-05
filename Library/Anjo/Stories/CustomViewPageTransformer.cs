using System;
using Android.Views;
using AndroidX.ViewPager2.Widget;

namespace WoWonder.Library.Anjo.Stories
{
    public class CustomViewPageTransformer : Java.Lang.Object, ViewPager2.IPageTransformer
    {
        private static readonly float MinScaleDepth = 0.75f;
        private static readonly float MinScaleZoom = 0.85f;
        private static readonly float MinAlphaZoom = 0.5f;
        private static readonly float ScaleFactorSlide = 0.85f;
        private static readonly float MinAlphaSlide = 0.35f;
        private readonly TransformType TransformType;

        public CustomViewPageTransformer(TransformType transformType)
        {
            TransformType = transformType;
        }

        public void TransformPage(View page, float position)
        {
            float alpha = 1;
            float scale = 1;
            float translationX = 0;

            switch (TransformType)
            {
                case TransformType.Flow:
                    page.RotationY = position * -30f;
                    return;
                case TransformType.SlideOver:
                    if (position < 0 && position > -1)
                    {
                        scale = Math.Abs(Math.Abs(position) - 1) * (1.0f - ScaleFactorSlide) + ScaleFactorSlide;
                        alpha = Math.Max(MinAlphaSlide, 1 - Math.Abs(position));
                        int pageWidth = page.Width;
                        float translateValue = position * -pageWidth;
                        if (translateValue > -pageWidth)
                        {
                            translationX = translateValue;
                        }
                        else
                        {
                            translationX = 0;
                        }
                    }
                    else
                    {
                        alpha = 1;
                        scale = 1;
                        translationX = 0;
                    }
                    break;
                case TransformType.Depth:
                    if (position > 0 && position < 1)
                    {
                        alpha = 1 - position;
                        scale = MinScaleDepth + (1 - MinScaleDepth) * (1 - Math.Abs(position));
                        translationX = page.Width * -position;
                    }
                    else
                    {
                        alpha = 1;
                        scale = 1;
                        translationX = 0;
                    }
                    break;
                case TransformType.Zoom:
                    if (position >= -1 && position <= 1)
                    {
                        scale = Math.Max(MinScaleZoom, 1 - Math.Abs(position));
                        alpha = MinAlphaZoom + (scale - MinScaleZoom) / (1 - MinScaleZoom) * (1 - MinAlphaZoom);
                        float vMargin = page.Height * (1 - scale) / 2;
                        float hMargin = page.Width * (1 - scale) / 2;
                        if (position < 0)
                        {
                            translationX = hMargin - vMargin / 2;
                        }
                        else
                        {
                            translationX = -hMargin + vMargin / 2;
                        }
                    }
                    else
                    {
                        alpha = 1;
                        scale = 1;
                        translationX = 0;
                    }
                    break;
                case TransformType.Fade:
                    switch (position)
                    {
                        case <= -1.0F:
                        case >= 1.0F:
                            page.Alpha = 0.0F;
                            page.Clickable = false;
                            break;
                        case 0.0F:
                            page.Alpha = 1.0F;
                            page.Clickable = true;
                            break;
                        default:
                            page.Alpha = 1.0F - Math.Abs(position);
                            break;
                    }
                    break;
                default:
                    return;
            }

            page.Alpha = alpha;
            page.TranslationX = translationX;
            page.ScaleX = scale;
            page.ScaleY = scale;
        }
    }

    public enum TransformType
    {
        Flow,
        Depth,
        Zoom,
        SlideOver,
        Fade
    }
}