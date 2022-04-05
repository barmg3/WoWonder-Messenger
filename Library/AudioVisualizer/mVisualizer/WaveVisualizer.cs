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
 
	/// <summary>
	/// Custom view to create wave visualizer
	/// <para>
	/// Created by gk
	/// </para>
	/// </summary>

	public class WaveVisualizer : BaseVisualizer
	{

		private const int WaveMaxPoints = 54;
		private const int WaveMinPoints = 3;

		private int MMaxBatchCount;

		private Path MWavePath;

		private int NPoints;

		private PointF[] MBezierPoints, MBezierControlPoints1, MBezierControlPoints2;

		private float[] MSrcY, MDestY;

		private float MWidthOffset;
		private Rect MClipBounds;

		private int NBatchCount;

		private Random MRandom;


        public WaveVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public WaveVisualizer(Context context) : base(context)
        {
        }

        public WaveVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public WaveVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public WaveVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

		protected override void Init()
		{
			NPoints = (int)(WaveMaxPoints * MDensity);
			if (NPoints < WaveMinPoints)
			{
				NPoints = WaveMinPoints;
			}

			MWidthOffset = -1;
			NBatchCount = 0;

			AnimationSpeed = MAnimSpeed;

			MRandom = new Random();

			MClipBounds = new Rect();

			MWavePath = new Path();

			MSrcY = new float[NPoints + 1];
			MDestY = new float[NPoints + 1];

			//initialize mBezierPoints
			MBezierPoints = new PointF[NPoints + 1];
			MBezierControlPoints1 = new PointF[NPoints + 1];
			MBezierControlPoints2 = new PointF[NPoints + 1];
			for (int i = 0; i < MBezierPoints.Length; i++)
			{
				MBezierPoints[i] = new PointF();
				MBezierControlPoints1[i] = new PointF();
				MBezierControlPoints2[i] = new PointF();
			}

		}

		public override AnimSpeed AnimationSpeed
		{
			set
			{
				base.AnimationSpeed = value;
				MMaxBatchCount = AvConstants.MaxAnimBatchCount - (int)MAnimSpeed;
			}
		}

		protected   override void OnDraw(Canvas canvas)
		{

			if (MWidthOffset == -1)
			{

				canvas.GetClipBounds(MClipBounds);

				MWidthOffset = canvas.Width / NPoints;

				//initialize bezier points
				for (int i = 0; i < MBezierPoints.Length; i++)
				{
					float posX = MClipBounds.Left + (i * MWidthOffset);

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
					MBezierPoints[i].Set(posX, posY);
				}
			}

			//create the path and draw
			if (IsVisualizationEnabled && MRawAudioBytes != null)
			{

				if (MRawAudioBytes.Length == 0)
				{
					return;
				}

				MWavePath.Rewind();

				//find the destination bezier point for a batch
				if (NBatchCount == 0)
				{

					float randPosY = MDestY[MRandom.Next(NPoints)];
					for (int i = 0; i < MBezierPoints.Length; i++)
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

					MDestY[MBezierPoints.Length - 1] = randPosY;
				}

				//increment batch count
				NBatchCount++;

				//for smoothing animation
				for (int i = 0; i < MBezierPoints.Length; i++)
				{
					MBezierPoints[i].Y = MSrcY[i] + (((float)(NBatchCount) / MMaxBatchCount) * (MDestY[i] - MSrcY[i]));
				}

				//reset the batch count
				if (NBatchCount == MMaxBatchCount)
				{
					NBatchCount = 0;
				}

				//calculate the bezier curve control points
				for (int i = 1; i < MBezierPoints.Length; i++)
				{
					MBezierControlPoints1[i].Set((MBezierPoints[i].X + MBezierPoints[i - 1].X) / 2, MBezierPoints[i - 1].Y);
					MBezierControlPoints2[i].Set((MBezierPoints[i].X + MBezierPoints[i - 1].X) / 2, MBezierPoints[i].Y);
				}

				//create the path
				MWavePath.MoveTo(MBezierPoints[0].X, MBezierPoints[0].Y);
				for (int i = 1; i < MBezierPoints.Length; i++)
				{
					MWavePath.CubicTo(MBezierControlPoints1[i].X, MBezierControlPoints1[i].Y, MBezierControlPoints2[i].X, MBezierControlPoints2[i].Y, MBezierPoints[i].X, MBezierPoints[i].Y);
				}

				//add last 3 line to close the view
				//mWavePath.lineTo(mClipBounds.right, mBezierPoints[0].Y);
				if (MPaintStyle == PaintStyle.Fill)
				{
					MWavePath.LineTo(MClipBounds.Right, MClipBounds.Bottom);
					MWavePath.LineTo(MClipBounds.Left, MClipBounds.Bottom);
					MWavePath.Close();
				}

				canvas.DrawPath(MWavePath, MPaint);
			}

			base.OnDraw(canvas);
		}

    }
}