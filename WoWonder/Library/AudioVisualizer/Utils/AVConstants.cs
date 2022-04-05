 
using System;
using Android.Graphics;

namespace WoWonder.Library.AudioVisualizer.Utils
{
    public class AvConstants
	{
		public const float DefaultDensity = 0.25f;
		public static readonly Color DefaultColor = Color.Black;
		public const float DefaultStrokeWidth = 6.0f;
		public const int MaxAnimBatchCount = 4;



        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

	}

}