using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using WoWonder.Library.AudioVisualizer.Model;
using BaseVisualizer = WoWonder.Library.AudioVisualizer.Base.BaseVisualizer;

namespace WoWonder.Library.AudioVisualizer.mVisualizer
{  
	public class HiFiVisualizer : BaseVisualizer
	{
		private const int BarMaxPoints = 240;
		private const int BarMinPoints = 30;
		private const float PerRadius = .65f;
		private int MRadius;
		private int MPoints;
		private int[] MHeights;
		private Path MPath; //outward path
		private Path MPath1; //inward path
		/// <summary>
		/// This is the distance from center to bezier control point.
		/// We can calculate the bezier control points of each segment this distance and its angle;
		/// </summary>
		private int MBezierControlPointLen;


        public HiFiVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public HiFiVisualizer(Context context) : base(context)
        {
        }

        public HiFiVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public HiFiVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public HiFiVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

		protected override void Init()
		{
			MRadius = -1;
			MPath = new Path();
			MPath1 = new Path();
			MPaint.SetStyle(Paint.Style.Stroke);  
			MPaint.AntiAlias = true;
			MPaint.StrokeWidth = 1.0f;
			MPoints = (int)(BarMaxPoints * MDensity);
			if (MPoints < BarMinPoints)
			{
				MPoints = BarMinPoints;
			}
			MHeights = new int[MPoints];
		}

		/// <summary>
		/// you cannot change the style of paint;
		/// the paintStyle fixed at Paint.Style.STROKE:
		/// </summary>
		/// <param name="paintStyle"> style of the visualizer. </param>
		[Obsolete()]
		public override PaintStyle PaintStyle
		{
			set
			{
    
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
			if (MRadius == -1)
			{
				MRadius = (int)(Math.Min(Width, Height) / 2 * PerRadius);
				MBezierControlPointLen = (int)(MRadius / Math.Cos(Math.PI / MPoints));
			}
			UpdateData();
			MPath.Reset();
			MPath1.Reset();
			// start the outward path from the last point
			float cxL = (float)(Width / 2 + Math.Cos((360 - 360 / MPoints) * Math.PI / 180) * (MRadius + MHeights[MPoints - 1]));
			float cyL = (float)(Height / 2 - Math.Sin((360 - 360 / MPoints) * Math.PI / 180) * (MRadius + MHeights[MPoints - 1]));
			MPath.MoveTo(cxL, cyL);
			// start the inward path from the last point
			float cxL1 = (float)(Width / 2 + Math.Cos((360 - 360 / MPoints) * Math.PI / 180) * (MRadius - MHeights[MPoints - 1]));
			float cyL1 = (float)(Height / 2 - Math.Sin((360 - 360 / MPoints) * Math.PI / 180) * (MRadius - MHeights[MPoints - 1]));
			MPath1.MoveTo(cxL1, cyL1);
			for (int i = 0; i < 360; i = i + 360 / MPoints)
			{
				// outward
				// the next point of path
				float cx = (float)(Width / 2 + Math.Cos(i * Math.PI / 180) * (MRadius + MHeights[i * MPoints / 360]));
				float cy = (float)(Height / 2 - Math.Sin(i * Math.PI / 180) * (MRadius + MHeights[i * MPoints / 360]));
				//second bezier control point
				float bx = (float)(Width / 2 + Math.Cos((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen + MHeights[i * MPoints / 360]));
				float by = (float)(Height / 2 - Math.Sin((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen + MHeights[i * MPoints / 360]));
				int lastPoint = i == 0 ? MPoints - 1 : i * MPoints / 360 - 1;
				//fist bezier control point
				float ax = (float)(Width / 2 + Math.Cos((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen + MHeights[lastPoint]));
				float ay = (float)(Height / 2 - Math.Sin((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen + MHeights[lastPoint]));
				MPath.CubicTo(ax, ay, bx, by, cx, cy);
				// inward
				float cx1 = (float)(Width / 2 + Math.Cos(i * Math.PI / 180) * (MRadius - MHeights[i * MPoints / 360]));
				float cy1 = (float)(Height / 2 - Math.Sin(i * Math.PI / 180) * (MRadius - MHeights[i * MPoints / 360]));
				float bx1 = (float)(Width / 2 + Math.Cos((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen - MHeights[i * MPoints / 360]));
				float by1 = (float)(Height / 2 - Math.Sin((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen - MHeights[i * MPoints / 360]));
				float ax1 = (float)(Width / 2 + Math.Cos((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen - MHeights[lastPoint]));
				float ay1 = (float)(Height / 2 - Math.Sin((i - (180 / MPoints)) * Math.PI / 180) * (MBezierControlPointLen - MHeights[lastPoint]));
				MPath1.CubicTo(ax1, ay1, bx1, by1, cx1, cy1);
				canvas.DrawLine(cx, cy, cx1, cy1, MPaint);
			}
			canvas.DrawPath(MPath, MPaint);
			canvas.DrawPath(MPath1, MPaint);
		}

		private void UpdateData()
		{
			if (IsVisualizationEnabled && MRawAudioBytes != null)
			{
				if (MRawAudioBytes.Length == 0)
				{
					return;
				}
				for (int i = 0; i < MHeights.Length; i++)
				{
					int x = (int) Math.Ceiling((decimal) ((i + 1) * (MRawAudioBytes.Length / MPoints)));
					int t = 0;
					if (x < 1024)
					{
						t = (unchecked((sbyte)(Math.Abs(MRawAudioBytes[x]) + 128))) * MRadius / 128;
					}
					MHeights[i] = -t;
				}
			}
		}

    }

}