using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace WoWonder.Library.Anjo.AutoEditText
{ 
	/// <summary>
	/// <para>A helper class to enable automatically resizing <seealso cref="AppCompatEditText"/>`s {@code textSize} to fit
	/// within its bounds.
	/// 
	/// @author Victor Minerva<br>
	/// @attr ref R.styleable.AutoFitEditText_sizeToFit
	/// @attr ref R.styleable.AutoFitEditText_minTextSize
	/// @attr ref R.styleable.AutoFitEditText_precision
	/// @since Dec 3, 2017
	/// </para>
	/// </summary>
	public class AutoFitHelper
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			MTextWatcher = new AutoFitTextWatcher(this);
			MOnLayoutChangeListener = new AutoFitOnLayoutChangeListener(this);
		}


		private const string Tag = "AutoFitTextHelper";
		private const bool Spew = false;

		// Minimum size of the text in pixels
		private const int DefaultMinTextSize = 8; //sp
		// How precise we want to be when reaching the target textWidth size
		private const float DefaultPrecision = 0.5f;

		/// <summary>
		/// Creates a new instance of {@code AutoFitHelper} that wraps a <seealso cref="TextView"/> and enables
		/// automatically sizing the text to fit.
		/// </summary>
		public static AutoFitHelper Create(TextView view)
		{
			return Create(view, null, 0);
		}

		/// <summary>
		/// Creates a new instance of {@code AutoFitHelper} that wraps a <seealso cref="TextView"/> and enables
		/// automatically sizing the text to fit.
		/// </summary>
		public static AutoFitHelper Create(TextView view, IAttributeSet attrs)
		{
			return Create(view, attrs, 0);
		}

		/// <summary>
		/// Creates a new instance of {@code AutoFitHelper} that wraps a <seealso cref="TextView"/> and enables
		/// automatically sizing the text to fit.
		/// </summary>
		public static AutoFitHelper Create(TextView view, IAttributeSet attrs, int defStyle)
		{
			AutoFitHelper helper = new AutoFitHelper(view);
			bool sizeToFit = true;
			if (attrs != null)
			{
				Context context = view.Context;
				int minTextSize = (int) helper.MinTextSize;
				float precision = helper.Precision;

				TypedArray ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.AutoFitEditText, defStyle, 0);
				sizeToFit = ta.GetBoolean(Resource.Styleable.AutoFitEditText_sizeToFit, sizeToFit);
				minTextSize = ta.GetDimensionPixelSize(Resource.Styleable.AutoFitEditText_minTextSize, minTextSize);
				precision = ta.GetFloat(Resource.Styleable.AutoFitEditText_precision, precision);
				ta.Recycle();

				helper.SetMinTextSize(ComplexUnitType.Px, minTextSize).SetPrecision(precision);
			}
			helper.SetEnabled(sizeToFit);

			return helper;
		}

		/// <summary>
		/// Re-sizes the textSize of the TextView so that the text fits within the bounds of the View.
		/// </summary>
		private static void AutoFit(TextView view, TextPaint paint, float minTextSize, float maxTextSize, int maxLines, float precision)
		{
			if (maxLines <= 0 || maxLines == int.MaxValue)
			{
				// Don't auto-size since there's no limit on lines.
				return;
			}

			int targetWidth = view.Width - view.PaddingLeft - view.PaddingRight;
			if (targetWidth <= 0)
			{
				return;
			}

			string text = view.Text;
			var method = view.TransformationMethod;
			if (method != null)
			{
				text = method.GetTransformation(text, view);
			}

			Context context = view.Context;
			Resources r = Resources.System;
			DisplayMetrics displayMetrics;

			float size = maxTextSize;
			float high = size;
			float low = 0;

			if (context != null)
			{
				r = context.Resources;
			}
			displayMetrics = r.DisplayMetrics;

			paint.Set(view.Paint);
			paint.TextSize = size;

			if ((maxLines == 1 && paint.MeasureText(text, 0, text.Length) > targetWidth) || GetLineCount(text, paint, size, targetWidth, displayMetrics) > maxLines)
			{
				size = GetAutoFitTextSize(text, paint, targetWidth, maxLines, low, high, precision, displayMetrics);
			}

			if (size < minTextSize)
			{
				size = minTextSize;
			}

			view.SetTextSize(ComplexUnitType.Px, size);
		}

		/// <summary>
		/// Recursive binary search to find the best size for the text.
		/// </summary>
		private static float GetAutoFitTextSize(string text, TextPaint paint, float targetWidth, int maxLines, float low, float high, float precision, DisplayMetrics displayMetrics)
		{
			float mid = (low + high) / 2.0f;
			int lineCount = 1;
			StaticLayout layout = null;

			paint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Px, mid, displayMetrics);

			if (maxLines != 1)
			{
				layout = new StaticLayout(text, paint, (int) targetWidth, Layout.Alignment.AlignNormal, 1.0f, 0.0f, true);
				lineCount = layout.LineCount;
			}

			if (Spew)
			{
				Log.Debug(Tag, "low=" + low + " high=" + high + " mid=" + mid + " target=" + targetWidth + " maxLines=" + maxLines + " lineCount=" + lineCount);
			}

			if (lineCount > maxLines)
			{
				// For the case that `text` has more newline characters than `maxLines`.
				if ((high - low) < precision)
				{
					return low;
				}
				return GetAutoFitTextSize(text, paint, targetWidth, maxLines, low, mid, precision, displayMetrics);
			}
			else if (lineCount < maxLines)
			{
				return GetAutoFitTextSize(text, paint, targetWidth, maxLines, mid, high, precision, displayMetrics);
			}
			else
			{
				float maxLineWidth = 0;
				if (maxLines == 1)
				{
					maxLineWidth = paint.MeasureText(text, 0, text.Length);
				}
				else
				{
					for (int i = 0; i < lineCount; i++)
					{
						if (layout.GetLineWidth(i) > maxLineWidth)
						{
							maxLineWidth = layout.GetLineWidth(i);
						}
					}
				}

				if ((high - low) < precision)
				{
					return low;
				}
				else if (maxLineWidth > targetWidth)
				{
					return GetAutoFitTextSize(text, paint, targetWidth, maxLines, low, mid, precision, displayMetrics);
				}
				else if (maxLineWidth < targetWidth)
				{
					return GetAutoFitTextSize(text, paint, targetWidth, maxLines, mid, high, precision, displayMetrics);
				}
				else
				{
					return mid;
				}
			}
		}

		private static int GetLineCount(string text, TextPaint paint, float size, float width, DisplayMetrics displayMetrics)
		{
			paint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Px, size, displayMetrics);
			StaticLayout layout = new StaticLayout(text, paint, (int) width, Layout.Alignment.AlignNormal, 1.0f, 0.0f, true);
			return layout.LineCount;
		}

		private static int GetMaxLines(TextView view)
		{
			int maxLines = -1; // No limit (Integer.MAX_VALUE also means no limit)

			var method = view.TransformationMethod;
			if (method != null && method is SingleLineTransformationMethod)
			{
				maxLines = 1;
			}
			else if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
			{
				// setMaxLines() and getMaxLines() are only available on android-16+
				maxLines = view.MaxLines;
			}

			return maxLines;
		}

		// Attributes
		private TextView MTextView;
		private TextPaint MPaint;
		/// <summary>
		/// Original textSize of the TextView.
		/// </summary>
		private float MTextSize;

		private int MMaxLines;
		private float MMinTextSize;
		private float MMaxTextSize;
		private float MPrecision;

		private bool MEnabled;
		private bool MIsAutoFitting;

		private List<IOnTextSizeChangeListener> MListeners;

		private ITextWatcher MTextWatcher;

		private View.IOnLayoutChangeListener MOnLayoutChangeListener;

		private AutoFitHelper(TextView view)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}


			Context context = view.Context;
			float scaledDensity = context.Resources.DisplayMetrics.ScaledDensity;

			MTextView = view;
			MPaint = new TextPaint();
			RawTextSize = view.TextSize;

			MMaxLines = GetMaxLines(view);
			MMinTextSize = scaledDensity * DefaultMinTextSize;
			MMaxTextSize = MTextSize;
			MPrecision = DefaultPrecision;
		}

		/// <summary>
		/// Adds an <seealso cref="OnTextSizeChangeListener"/> to the list of those whose methods are called
		/// whenever the <seealso cref="TextView"/>'s {@code textSize} changes.
		/// </summary>
		public virtual AutoFitHelper addOnTextSizeChangeListener(IOnTextSizeChangeListener listener)
		{
			if (MListeners == null)
			{
				MListeners = new List<IOnTextSizeChangeListener>();
			}
			MListeners.Add(listener);
			return this;
		}

		/// <summary>
		/// Removes the specified <seealso cref="OnTextSizeChangeListener"/> from the list of those whose methods
		/// are called whenever the <seealso cref="TextView"/>'s {@code textSize} changes.
		/// </summary>
		public virtual AutoFitHelper removeOnTextSizeChangeListener(IOnTextSizeChangeListener listener)
		{
			if (MListeners != null)
			{
				MListeners.Remove(listener);
			}
			return this;
		}

		/// <summary>
		/// Returns the amount of precision used to calculate the correct text size to fit within its
		/// bounds.
		/// </summary>
		public virtual float Precision
		{
			get
			{
				return MPrecision;
			}
		}

		/// <summary>
		/// Set the amount of precision used to calculate the correct text size to fit within its
		/// bounds. Lower precision is more precise and takes more time.
		/// </summary>
		/// <param name="precision"> The amount of precision. </param>
		public virtual AutoFitHelper SetPrecision(float precision)
		{
			if (MPrecision != precision)
			{
				MPrecision = precision;

				AutoFit();
			}
			return this;
		}

		/// <summary>
		/// Returns the minimum size (in pixels) of the text.
		/// </summary>
		public virtual float MinTextSize
		{
			get
			{
				return MMinTextSize;
			}
		}

		/// <summary>
		/// Set the minimum text size to the given value, interpreted as "scaled pixel" units. This size
		/// is adjusted based on the current density and user font size preference.
		/// </summary>
		/// <param name="size"> The scaled pixel size.
		/// @attr ref me.grantland.R.styleable#AutoFitEditText_minTextSize </param>
		public virtual AutoFitHelper SetMinTextSize(float size)
		{
			return SetMinTextSize(ComplexUnitType.Sp, size);
		}

		/// <summary>
		/// Set the minimum text size to a given unit and value. See TypedValue for the possible
		/// dimension units.
		/// </summary>
		/// <param name="unit"> The desired dimension unit. </param>
		/// <param name="size"> The desired size in the given units.
		/// @attr ref me.grantland.R.styleable#AutoFitEditText_minTextSize </param>
		public virtual AutoFitHelper SetMinTextSize(ComplexUnitType unit, float size)
		{
			Context context = MTextView.Context;
			Resources r = Resources.System;

			if (context != null)
			{
				r = context.Resources;
			}

			RawMinTextSize = TypedValue.ApplyDimension(unit, size, r.DisplayMetrics);
			return this;
		}

		private float RawMinTextSize
		{
			set
			{
				if (value != MMinTextSize)
				{
					MMinTextSize = value;
    
					AutoFit();
				}
			}
		}

		/// <summary>
		/// Returns the maximum size (in pixels) of the text.
		/// </summary>
		public virtual float MaxTextSize
		{
			get
			{
				return MMaxTextSize;
			}
		}

		/// <summary>
		/// Set the maximum text size to the given value, interpreted as "scaled pixel" units. This size
		/// is adjusted based on the current density and user font size preference.
		/// </summary>
		/// <param name="size"> The scaled pixel size.
		/// @attr ref android.R.styleable#TextView_textSize </param>
		public virtual AutoFitHelper SetMaxTextSize(float size)
		{
			return SetMaxTextSize(ComplexUnitType.Sp, size);
		}

		/// <summary>
		/// Set the maximum text size to a given unit and value. See TypedValue for the possible
		/// dimension units.
		/// </summary>
		/// <param name="unit"> The desired dimension unit. </param>
		/// <param name="size"> The desired size in the given units.
		/// @attr ref android.R.styleable#TextView_textSize </param>
		public virtual AutoFitHelper SetMaxTextSize(ComplexUnitType unit, float size)
		{
			Context context = MTextView.Context;
			Resources r = Resources.System;

			if (context != null)
			{
				r = context.Resources;
			}

			RawMaxTextSize = TypedValue.ApplyDimension(unit, size, r.DisplayMetrics);
			return this;
		}

		private float RawMaxTextSize
		{
			set
			{
				if (value != MMaxTextSize)
				{
					MMaxTextSize = value;
    
					AutoFit();
				}
			}
		}

		/// <seealso cref= TextView#getMaxLines() </seealso>
		public virtual int MaxLines
		{
			get
			{
				return MMaxLines;
			}
		}

		/// <seealso cref= TextView#setMaxLines(int) </seealso>
		public virtual AutoFitHelper SetMaxLines(int lines)
		{
			if (MMaxLines != lines)
			{
				MMaxLines = lines;

				AutoFit();
			}
			return this;
		}

		/// <summary>
		/// Returns whether or not automatically resizing text is enabled.
		/// </summary>
		public virtual bool Enabled
		{
			get
			{
				return MEnabled;
			}
		}

		/// <summary>
		/// Set the enabled state of automatically resizing text.
		/// </summary>
		public virtual AutoFitHelper SetEnabled(bool enabled)
		{
			if (MEnabled != enabled)
			{
				MEnabled = enabled;

				if (enabled)
				{
					MTextView.AddTextChangedListener(MTextWatcher);
					MTextView.AddOnLayoutChangeListener(MOnLayoutChangeListener);

					AutoFit();
				}
				else
				{
					MTextView.RemoveTextChangedListener(MTextWatcher);
					MTextView.RemoveOnLayoutChangeListener(MOnLayoutChangeListener);

					MTextView.SetTextSize(ComplexUnitType.Px, MTextSize);
				}
			}
			return this;
		}

		/// <summary>
		/// Returns the original text size of the View.
		/// </summary>
		/// <seealso cref= TextView#getTextSize() </seealso>
		public virtual float TextSize
		{
			get
			{
				return MTextSize;
			}
			set
			{
				SetTextSize(ComplexUnitType.Sp, value);
			}
		}


		/// <summary>
		/// Set the original text size of the View.
		/// </summary>
		/// <seealso cref= TextView#setTextSize(int, float) </seealso>
		public virtual void SetTextSize(ComplexUnitType unit, float size)
		{
			if (MIsAutoFitting)
			{
				// We don't want to update the TextView's actual textSize while we're autofitting
				// since it'd get set to the autofitTextSize
				return;
			}
			Context context = MTextView.Context;
			Resources r = Resources.System;

			if (context != null)
			{
				r = context.Resources;
			}

			RawTextSize = TypedValue.ApplyDimension(unit, size, r.DisplayMetrics);
		}

		private float RawTextSize
		{
			set
			{
				if (MTextSize != value)
				{
					MTextSize = value;
				}
			}
		}

		private void AutoFit()
		{
			float oldTextSize = MTextView.TextSize;
			float textSize;

			MIsAutoFitting = true;
			AutoFit(MTextView, MPaint, MMinTextSize, MMaxTextSize, MMaxLines, MPrecision);
			MIsAutoFitting = false;

			textSize = MTextView.TextSize;
			if (textSize != oldTextSize)
			{
				SendTextSizeChange(textSize, oldTextSize);
			}
		}

		private void SendTextSizeChange(float textSize, float oldTextSize)
		{
			if (MListeners == null)
			{
				return;
			}

			foreach (var listener in MListeners)
			{
				listener.OnTextSizeChange(textSize, oldTextSize);
			}
		}

		private class AutoFitTextWatcher : Object, ITextWatcher
		{
			private readonly AutoFitHelper OuterInstance;

			public AutoFitTextWatcher(AutoFitHelper outerInstance)
			{
				OuterInstance = outerInstance;
			}

            public void AfterTextChanged(IEditable s)
            {
                 
            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {
               
            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {
				OuterInstance.AutoFit();
			}
        }

		private class AutoFitOnLayoutChangeListener : Object, View.IOnLayoutChangeListener
		{
			private readonly AutoFitHelper OuterInstance;

			public AutoFitOnLayoutChangeListener(AutoFitHelper outerInstance)
			{
				OuterInstance = outerInstance;
			}


            public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
            {
				OuterInstance.AutoFit();
			}
        }

		/// <summary>
		/// When an object of a type is attached to an {@code AutoFitHelper}, its methods will be called
		/// when the {@code textSize} is changed.
		/// </summary>
		public interface IOnTextSizeChangeListener
		{
			/// <summary>
			/// This method is called to notify you that the size of the text has changed to
			/// {@code textSize} from {@code oldTextSize}.
			/// </summary>
            public void OnTextSizeChange(float textSize, float oldTextSize);
		}
	}

}