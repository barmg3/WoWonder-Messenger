using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using WoWonder.Library.AudioVisualizer.Utils;
using BaseVisualizer = WoWonder.Library.AudioVisualizer.Base.BaseVisualizer;

 
namespace WoWonder.Library.AudioVisualizer.mVisualizer
{ 
	public class BlastVisualizer : BaseVisualizer
	{

		private const int BlastMaxPoints = 1000;
		private const int BlastMinPoints = 3;

		private Path MSpikePath;
		private int MRadius;
		private int NPoints;


        public BlastVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public BlastVisualizer(Context context) : base(context)
        {
        }

        public BlastVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public BlastVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public BlastVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

		protected override void Init()
		{
			MRadius = -1;
			NPoints = (int)(BlastMaxPoints * MDensity);
			if (NPoints < BlastMinPoints)
			{
				NPoints = BlastMinPoints;
			}

			MSpikePath = new Path();
		}

		protected   override void OnDraw(Canvas canvas)
		{

			//first time initialization
			if (MRadius == -1)
			{
				MRadius = Height < Width ? Height : Width;
				MRadius = (int)(MRadius * 0.65 / 2);
			}

			//create the path and draw
			if (IsVisualizationEnabled && MRawAudioBytes != null)
			{

				if (MRawAudioBytes.Length == 0)
				{
					return;
				}

				MSpikePath.Rewind();

				double angle = 0;
				for (int i = 0; i < NPoints; i++, angle += (360.0f / NPoints))
				{
					int x = (int) Math.Ceiling((decimal) (i * (MRawAudioBytes.Length / NPoints)));
					int t = 0;
					if (x < 1024)
					{
						t = (unchecked((sbyte)(-Math.Abs(MRawAudioBytes[x]) + 128))) * (canvas.Height / 4) / 128;
					}

					float posX = (float)(Width / 2 + (MRadius + t) * Math.Cos(AvConstants.ConvertToRadians(angle)));

					float posY = (float)(Height / 2 + (MRadius + t) * Math.Sin(AvConstants.ConvertToRadians(angle)));

					if (i == 0)
					{
						MSpikePath.MoveTo(posX, posY);
					}
					else
					{
						MSpikePath.LineTo(posX, posY);
					}

				}
				MSpikePath.Close();

				canvas.DrawPath(MSpikePath, MPaint);

			}

			base.OnDraw(canvas);
		}

    }
}