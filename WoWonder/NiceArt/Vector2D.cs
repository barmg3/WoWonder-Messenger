using System;
using Android.Graphics;
using WoWonder.Helpers.Utils;

namespace WoWonder.NiceArt
{
    public class Vector2D : PointF
    {
        public static float GetAngle(Vector2D vector1, Vector2D vector2)
        {
            try
            {
                vector1.Normalize();
                vector2.Normalize();
                double degrees = 180.0 / Math.PI * (Math.Atan2(vector2.Y, vector2.X) - Math.Atan2(vector1.Y, vector1.X));
                return (float)degrees;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        public void Normalize()
        {
            try
            {
                float length = (float)Math.Sqrt(X * X + Y * Y);
                X /= length;
                Y /= length;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }
    }
}