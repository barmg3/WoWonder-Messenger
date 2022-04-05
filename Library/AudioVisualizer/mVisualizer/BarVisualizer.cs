using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using WoWonder.Library.AudioVisualizer.Model;
using WoWonder.Library.AudioVisualizer.Utils;
using BaseVisualizer = WoWonder.Library.AudioVisualizer.Base.BaseVisualizer;

 
namespace WoWonder.Library.AudioVisualizer.mVisualizer
{ 
	public class BarVisualizer : BaseVisualizer
	{

		private const int BarMaxPoints = 120;
		private const int BarMinPoints = 3;

		private int MMaxBatchCount;

		private int NPoints;

		private float[] MSrcY, MDestY;

		private float MBarWidth;
		private Rect MClipBounds;

		private int NBatchCount;

		private Random MRandom;


        public BarVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public BarVisualizer(Context context) : base(context)
        {
        }

        public BarVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public BarVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public BarVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

		protected override void Init()
		{
			NPoints = (int)(BarMaxPoints * MDensity);
			if (NPoints < BarMinPoints)
			{
				NPoints = BarMinPoints;
			}

			MBarWidth = -1;
			NBatchCount = 0;

			AnimationSpeed = MAnimSpeed;

			MRandom = new Random();

			MClipBounds = new Rect();

			MSrcY = new float[NPoints];
			MDestY = new float[NPoints];

		}

		public override AnimSpeed AnimationSpeed
		{
			set
			{
				base.AnimationSpeed = value;
				MMaxBatchCount = AvConstants.MaxAnimBatchCount - (int)MAnimSpeed;
			}
		}

		protected override void OnDraw(Canvas canvas)
		{

			if (MBarWidth == -1)
			{

				canvas.GetClipBounds(MClipBounds);

				MBarWidth = canvas.Width / NPoints;

				//initialize points
				for (int i = 0; i < MSrcY.Length; i++)
				{
					float posY;
					if (MPositionGravity == PositionGravity.Top)
					{
						posY = MClipBounds.Top;
					}
					else
					{
						posY = MClipBounds.Bottom;
					}

					MSrcY[i] = posY;
					MDestY[i] = posY;
				}
			}

			//create the path and draw
			if (IsVisualizationEnabled && MRawAudioBytes != null)
			{

				if (MRawAudioBytes.Length == 0)
				{
					return;
				}

				//find the destination bezier point for a batch
				if (NBatchCount == 0)
				{
					float randPosY = MDestY[MRandom.Next(NPoints)];
					for (int i = 0; i < MSrcY.Length; i++)
					{

						int x = (int) Math.Ceiling((decimal) ((i + 1) * (MRawAudioBytes.Length / NPoints)));
						int t = 0;
						if (x < 1024)
						{
							t = canvas.Height + (unchecked((sbyte)(Math.Abs(MRawAudioBytes[x]) + 128))) * canvas.Height / 128;
						}

						float posY;
						if (MPositionGravity == PositionGravity.Top)
						{
							posY = MClipBounds.Bottom - t;
						}
						else
						{
							posY = MClipBounds.Top + t;
						}

						//change the source and destination y
						MSrcY[i] = MDestY[i];
						MDestY[i] = posY;
					}

					MDestY[MSrcY.Length - 1] = randPosY;
				}

				//increment batch count
				NBatchCount++;

				//calculate bar position and draw
				for (int i = 0; i < MSrcY.Length; i++)
				{
					float barY = MSrcY[i] + (((float)(NBatchCount) / MMaxBatchCount) * (MDestY[i] - MSrcY[i]));
					float barX = (i * MBarWidth) + (MBarWidth / 2);
					canvas.DrawLine(barX, canvas.Height, barX, barY, MPaint);
				}

				//reset the batch count
				if (NBatchCount == MMaxBatchCount)
				{
					NBatchCount = 0;
				}

			}

			base.OnDraw(canvas);
		}

    }
}