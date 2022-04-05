using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using BaseVisualizer = WoWonder.Library.AudioVisualizer.Base.BaseVisualizer;

namespace WoWonder.Library.AudioVisualizer.mVisualizer
{
    public class CircleLineVisualizer : BaseVisualizer
	{
		private const int BarMaxPoints = 240;
		private const int BarMinPoints = 30;
		private Rect MClipBounds;
		private int MPoints;
		private int MPointRadius;
		private float[] MSrcY;
		private int MRadius;
		private Paint MGPaint;
		private bool drawLine;



        public CircleLineVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CircleLineVisualizer(Context context) : base(context)
        {
        }

        public CircleLineVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public CircleLineVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public CircleLineVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

		public virtual bool DrawLine
		{
			get
			{
				return drawLine;
			}
			set
			{
				drawLine = value;
			}
		}


		protected override void Init()
		{
			MPoints = (int)(BarMaxPoints * MDensity);
			if (MPoints < BarMinPoints)
			{
				MPoints = BarMinPoints;
			}
			MSrcY = new float[MPoints];
			MClipBounds = new Rect();
			AnimationSpeed = MAnimSpeed;
			MPaint.AntiAlias = true;
			MGPaint = new Paint();
			MGPaint.AntiAlias = true;
		}

		protected   override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);
			MRadius = Math.Min(w, h) / 4;
			MPointRadius = Math.Abs((int)(2 * MRadius * Math.Sin(Math.PI / MPoints / 3)));
			LinearGradient lg = new LinearGradient(Width / 2 + MRadius, Height / 2, Width / 2 + MRadius + MPointRadius * 5, Height / 2, Color.ParseColor("#77FF5722"), Color.ParseColor("#10FF5722"), Shader.TileMode.Clamp);
            MGPaint.SetShader(lg);
        }

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			canvas.GetClipBounds(MClipBounds);
			UpdateData();
			// draw circle's points
			for (int i = 0; i < 360; i = i + 360 / MPoints)
			{
				float cx = (float)(Width / 2 + Math.Cos(i * Math.PI / 180) * MRadius);
				float cy = (float)(Height / 2 - Math.Sin(i * Math.PI / 180) * MRadius);
				canvas.DrawCircle(cx, cy, MPointRadius, MPaint);
			}
			// draw lines
			if (drawLine)
			{
				DrawLines(canvas);
			}
			// draw bar
			for (int i = 0; i < 360; i = i + 360 / MPoints)
			{
				if (MSrcY[i * MPoints / 360] == 0)
				{
					continue;
				}
				canvas.Save();
				canvas.Rotate(-i, Width / 2, Height / 2);
				float cx = (float)(Width / 2 + MRadius);
				float cy = (float)(Height / 2);
				canvas.DrawRect(cx, cy - MPointRadius, cx + MSrcY[i * MPoints / 360], cy + MPointRadius, MPaint);
				canvas.DrawCircle(cx + MSrcY[i * MPoints / 360], cy, MPointRadius, MPaint);
				canvas.Restore();
			}
		}

		/// <summary>
		/// Draw a translucent ray
		/// </summary>
		/// <param name="canvas"> target canvas </param>
		private void DrawLines(Canvas canvas)
		{
			int lineLen = 14 * MPointRadius; //default len,
			for (int i = 0; i < 360; i = i + 360 / MPoints)
			{
				canvas.Save();
				canvas.Rotate(-i, Width / 2, Height / 2);
				float cx = (float)(Width / 2 + MRadius) + MSrcY[i * MPoints / 360];
				float cy = (float)(Height / 2);
				Path path = new Path();
				path.MoveTo(cx, cy + MPointRadius);
				path.LineTo(cx, cy - MPointRadius);
				path.LineTo(cx + lineLen, cy);
				canvas.DrawPath(path, MGPaint);
				canvas.Restore();
			}
		}

		private void UpdateData()
		{
			if (IsVisualizationEnabled && MRawAudioBytes != null)
			{
				if (MRawAudioBytes.Length == 0)
				{
					return;
				}
				for (int i = 0; i < MSrcY.Length; i++)
				{
					int x = (int) Math.Ceiling((decimal) ((i + 1) * (MRawAudioBytes.Length / MPoints)));
					int t = 0;
					if (x < 1024)
					{
						t = (unchecked((sbyte)(Math.Abs(MRawAudioBytes[x]) + 128))) * MRadius / 128;
					}
					MSrcY[i] = -t;
				}
			}
		} 
    } 
}