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
	/// Custom view to create blob visualizer
	/// <para>
	/// Created by gk
	/// </para>
	/// </summary>

	public class BlobVisualizer : BaseVisualizer
	{

		private const int BlobMaxPoints = 60;
		private const int BlobMinPoints = 3;

		private Path MBlobPath;
		private int MRadius;

		private int NPoints;

		private PointF[] MBezierPoints;
		private BezierSpline MBezierSpline;

		private float MAngleOffset;
		private float MChangeFactor;


        public BlobVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public BlobVisualizer(Context context) : base(context)
        {
        }

        public BlobVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public BlobVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public BlobVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

		protected override void Init()
		{
			MRadius = -1;
			NPoints = (int)(MDensity * BlobMaxPoints);
			if (NPoints < BlobMinPoints)
			{
				NPoints = BlobMinPoints;
			}

			MAngleOffset = (360.0f / NPoints);

			UpdateChangeFactor(MAnimSpeed, false);

			MBlobPath = new Path();

			//initialize mBezierPoints, 2 extra for the smoothing first and last point
			MBezierPoints = new PointF[NPoints + 2];
			for (int i = 0; i < MBezierPoints.Length; i++)
			{
				MBezierPoints[i] = new PointF();
			}

			MBezierSpline = new BezierSpline(MBezierPoints.Length);
		}

		public override AnimSpeed AnimationSpeed
		{
			set
			{
				base.AnimationSpeed = value;
				UpdateChangeFactor(value, true);
			}
		}

		private void UpdateChangeFactor(AnimSpeed animSpeed, bool useHeight)
		{
			int height = 1;
			if (useHeight)
			{
				height = Height > 0 ? Height : 1000;
			}

			if (animSpeed == AnimSpeed.Slow)
			{
				MChangeFactor = height * 0.003f;
			}
			else if (animSpeed == AnimSpeed.Medium)
			{
				MChangeFactor = height * 0.006f;
			}
			else
			{
				MChangeFactor = height * 0.01f;
			}
		}

		protected   override void OnDraw(Canvas canvas)
		{

			double angle = 0;
			//first time initialization
			if (MRadius == -1)
			{
				MRadius = Height < Width ? Height : Width;
				MRadius = (int)(MRadius * 0.65 / 2);

				MChangeFactor = Height * MChangeFactor;

				//initialize bezier points
				for (int i = 0; i < NPoints; i++, angle += MAngleOffset)
				{
					float posX = (float)(Width / 2 + (MRadius) * Math.Cos(AvConstants.ConvertToRadians(angle)));

					float posY = (float)(Height / 2 + (MRadius) * Math.Sin(AvConstants.ConvertToRadians(angle)));

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

				MBlobPath.Rewind();

				//find the destination bezier point for a batch
				for (int i = 0; i < NPoints; i++, angle += MAngleOffset)
				{

					int x = (int) Math.Ceiling((decimal) ((i + 1) * (MRawAudioBytes.Length / NPoints)));
					int t = 0;
					if (x < 1024)
					{
						t = (unchecked((sbyte)(-Math.Abs(MRawAudioBytes[x]) + 128))) * (canvas.Height / 4) / 128;
					}

					float posX = (float)(Width / 2 + (MRadius + t) * Math.Cos(AvConstants.ConvertToRadians(angle)));

					float posY = (float)(Height / 2 + (MRadius + t) * Math.Sin(AvConstants.ConvertToRadians(angle)));

					//calculate the new x based on change
					if (posX - MBezierPoints[i].X > 0)
					{
						MBezierPoints[i].X += MChangeFactor;
					}
					else
					{
						MBezierPoints[i].X -= MChangeFactor;
					}

					//calculate the new y based on change
					if (posY - MBezierPoints[i].Y > 0)
					{
						MBezierPoints[i].Y += MChangeFactor;
					}
					else
					{
						MBezierPoints[i].Y -= MChangeFactor;
					}
				}
				//set the first and last point as first
				MBezierPoints[NPoints].Set(MBezierPoints[0].X, MBezierPoints[0].Y);
				MBezierPoints[NPoints + 1].Set(MBezierPoints[0].X, MBezierPoints[0].Y);

				//update the control points
				MBezierSpline.UpdateCurveControlPoints(MBezierPoints);
				PointF[] firstCp = MBezierSpline.FirstControlPoints;
				PointF[] secondCp = MBezierSpline.SecondControlPoints;

				//create the path
				MBlobPath.MoveTo(MBezierPoints[0].X, MBezierPoints[0].Y);
				for (int i = 0; i < firstCp.Length; i++)
				{
					MBlobPath.CubicTo(firstCp[i].X, firstCp[i].Y, secondCp[i].X, secondCp[i].Y, MBezierPoints[i + 1].X, MBezierPoints[i + 1].Y);
				}
				//add an extra line to center cover the gap generated by last cubicTo
				if (MPaintStyle == PaintStyle.Fill)
				{
					MBlobPath.LineTo(Width / 2, Height / 2);
				}

				canvas.DrawPath(MBlobPath, MPaint);

			}

			base.OnDraw(canvas);
		}

    }
}