using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media.Audiofx;
using Android.Runtime;
using Android.Util;
using Android.Views;
using WoWonder.Library.AudioVisualizer.Model;
using WoWonder.Library.AudioVisualizer.Utils;
using Object = Java.Lang.Object;

namespace WoWonder.Library.AudioVisualizer.Base
{ 
	public abstract class BaseVisualizer : View
	{

		internal byte[] MRawAudioBytes;
		internal Paint MPaint;
		internal Visualizer MVisualizer;
		internal Color MColor = AvConstants.DefaultColor;

		internal PaintStyle MPaintStyle = PaintStyle.Fill;
		internal PositionGravity MPositionGravity = PositionGravity.Bottom;

		internal float MStrokeWidth = AvConstants.DefaultStrokeWidth;
		internal float MDensity = AvConstants.DefaultDensity;

		internal AnimSpeed MAnimSpeed = AnimSpeed.Medium;
		internal bool IsVisualizationEnabled = true;


        protected BaseVisualizer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected BaseVisualizer(Context context) : base(context)
        {
            Init(context, null);
            Init();
        }

        protected BaseVisualizer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
            Init();
        }

        protected BaseVisualizer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
            Init();
        }

        protected BaseVisualizer(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs);
            Init();
        }

		private void Init(Context context, IAttributeSet attrs)
		{

			//get the attributes specified in attrs.xml using the name we included
			TypedArray typedArray = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.BaseVisualizer, 0, 0);
			if (typedArray != null && typedArray.Length() > 0)
			{
				try
				{
					//get the text and colors specified using the names in attrs.xml
					MDensity = typedArray.GetFloat(Resource.Styleable.BaseVisualizer_avDensity, AvConstants.DefaultDensity);
					MColor = typedArray.GetColor(Resource.Styleable.BaseVisualizer_avColor, AvConstants.DefaultColor);
					MStrokeWidth = typedArray.GetDimension(Resource.Styleable.BaseVisualizer_avWidth, AvConstants.DefaultStrokeWidth);

					string paintType = typedArray.GetString(Resource.Styleable.BaseVisualizer_avType);
					if (!ReferenceEquals(paintType, null) && !paintType.Equals(""))
					{
						MPaintStyle = paintType.ToLower().Equals("outline") ? PaintStyle.Outline : PaintStyle.Fill;
					}

					string gravityType = typedArray.GetString(Resource.Styleable.BaseVisualizer_avGravity);
					if (!ReferenceEquals(gravityType, null) && !gravityType.Equals(""))
					{
						MPositionGravity = gravityType.ToLower().Equals("top") ? PositionGravity.Top : PositionGravity.Bottom;
					}

					string speedType = typedArray.GetString(Resource.Styleable.BaseVisualizer_avSpeed);
					if (!ReferenceEquals(speedType, null) && !speedType.Equals(""))
					{
						MAnimSpeed = AnimSpeed.Medium;
						if (speedType.ToLower().Equals("slow"))
						{
							MAnimSpeed = AnimSpeed.Slow;
						}
						else if (speedType.ToLower().Equals("fast"))
						{
							MAnimSpeed = AnimSpeed.Fast;
						}
					}

				}
				finally
				{
					typedArray.Recycle();
				}
			}

			MPaint = new Paint();
			MPaint.Color = MColor;
			MPaint.StrokeWidth = MStrokeWidth;
			if (MPaintStyle == PaintStyle.Fill)
			{
				MPaint.SetStyle(Paint.Style.Fill);  
			}
			else
			{
				MPaint.SetStyle(Paint.Style.Stroke);  
			}
		}

		/// <summary>
		/// Set color to visualizer with color resource id.
		/// </summary>
		/// <param name="color"> color resource id. </param>
		public Color Color
		{
			set
			{
				MColor = value;
				MPaint.Color = MColor;
			}
		}

		/// <summary>
		/// Set the density of the visualizer
		/// </summary>
		/// <param name="density"> density for visualization </param>
		public float Density
		{
			set
			{
				//TODO: Check dynamic value change, may cause crash
				lock (this)
				{
					MDensity = value;
					Init();
				}
			}
		}

		/// <summary>
		/// Sets the paint style of the visualizer
		/// </summary>
		/// <param name="paintStyle"> style of the visualizer. </param>
		public virtual PaintStyle PaintStyle
		{
			set
			{
				MPaintStyle = value;
				MPaint.SetStyle(value == PaintStyle.Fill ? Paint.Style.Fill : Paint.Style.Stroke);  
			}
		}

		/// <summary>
		/// Sets the position of the Visualization<seealso cref="PositionGravity"/>
		/// </summary>
		/// <param name="positionGravity"> position of the Visualization </param>
		public PositionGravity PositionGravity
		{
			set
			{
				MPositionGravity = value;
			}
		}

		/// <summary>
		/// Sets the Animation speed of the visualization<seealso cref="AnimSpeed"/>
		/// </summary>
		/// <param name="animSpeed"> speed of the animation </param>
		public virtual AnimSpeed AnimationSpeed
		{
			set
			{
				MAnimSpeed = value;
			}
		}

		/// <summary>
		/// Sets the width of the outline <seealso cref="PaintStyle"/>
		/// </summary>
		/// <param name="width"> style of the visualizer. </param>
		public float StrokeWidth
		{
			set
			{
				MStrokeWidth = value;
				MPaint.StrokeWidth = value;
			}
		}

		/// <summary>
		/// Sets the audio bytes to be visualized form <seealso cref="Visualizer"/> or other sources
		/// </summary>
		/// <param name="bytes"> of the raw bytes of music </param>
		public byte[] RawAudioBytes
		{
			set
			{
				MRawAudioBytes = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Sets the audio session id for the currently playing audio
		/// </summary>
		/// <param name="audioSessionId"> of the media to be visualised </param>
		public int AudioSessionId
		{
			set
			{
				if (MVisualizer != null)
				{
					Release();
				}
    
				MVisualizer = new Visualizer(value);
                MVisualizer.SetCaptureSize(Visualizer.GetCaptureSizeRange()[1]);
    
				MVisualizer.SetDataCaptureListener(new OnDataCaptureListenerAnonymousInnerClass(this), Visualizer.MaxCaptureRate / 2, true, false);

                MVisualizer.SetEnabled(true);
            }
		}

		private class OnDataCaptureListenerAnonymousInnerClass : Object, Visualizer.IOnDataCaptureListener
		{
			private readonly BaseVisualizer OuterInstance;

			public OnDataCaptureListenerAnonymousInnerClass(BaseVisualizer outerInstance)
			{
				OuterInstance = outerInstance;
			}
			public void OnWaveFormDataCapture(Visualizer visualizer, byte[] waveform, int samplingRate)
            {
                OuterInstance.MRawAudioBytes = waveform;
                OuterInstance.Invalidate();
            }

			public void OnFftDataCapture(Visualizer  visualizer, byte[] fft, int samplingRate)
            {
                
            } 
        }

		/// <summary>
		/// Releases the visualizer
		/// </summary>
		public void Release()
		{
			if (MVisualizer != null)
			{
				MVisualizer.Release();
			}
		}

		/// <summary>
		/// Enable Visualization
		/// </summary>
		public void Show()
		{
			IsVisualizationEnabled = true;
		}

		/// <summary>
		/// Disable Visualization
		/// </summary>
		public void Hide()
		{
			IsVisualizationEnabled = false;
		}

		protected abstract void Init();

    }
}