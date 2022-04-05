using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.Widget;

namespace WoWonder.Library.Anjo.AutoEditText
{

	/// <summary>
	/// <para>A <seealso cref="AppCompatEditText"/> that resizes its text to be no larger than the width of the display.
	/// 
	/// @attr ref R.styleable.AutoFitTextView_sizeToFit
	/// @attr ref R.styleable.AutoFitTextView_minTextSize
	/// @attr ref R.styleable.AutoFitTextView_precision
	/// </para>
	/// </summary>
	public class AutoFitEditText : AppCompatEditText, AutoFitHelper.IOnTextSizeChangeListener
	{
		private AutoFitHelper MHelper;


        protected AutoFitEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public AutoFitEditText(Context context) : base(context)
        {
            Init(context, null, 0);
        }

        public AutoFitEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, null, 0);
        }

        public AutoFitEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs, defStyleAttr);
        }
		private void Init(Context context, IAttributeSet attrs, int defStyleAttr)
		{
			MHelper = AutoFitHelper.Create(this, attrs, defStyleAttr).addOnTextSizeChangeListener(this);
		}

		// Getters and Setters 
        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void SetTextSize(ComplexUnitType unit, float size)
        {
            base.SetTextSize(unit, size);
			if (MHelper != null)
            {
                MHelper.SetTextSize(unit, size);
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void SetLines(int lines)
        {
            base.SetLines(lines);
            if (MHelper != null)
            {
                MHelper.SetMaxLines(lines);
            } 
		}

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void SetMaxLines(int maxLines)
        {
            base.SetMaxLines(maxLines);
			if (MHelper != null)
            {
                MHelper.SetMaxLines(maxLines);
            }
        }
		
		/// <summary>
		/// Returns the <seealso cref="AutoFitHelper"/> for this View.
		/// </summary>
		public virtual AutoFitHelper AutoFitHelper
		{
			get
			{
				return MHelper;
			}
		}

		/// <summary>
		/// Returns whether or not the text will be automatically re-sized to fit its constraints.
		/// </summary>
		public virtual bool SizeToFit
		{
			get
			{
				return MHelper.Enabled;
			}
			set
			{
				MHelper.SetEnabled(value);
			}
		}

		/// <summary>
		/// Sets the property of this field (sizeToFit), to automatically resize the text to fit its
		/// constraints.
		/// </summary>
		public virtual void SetSizeToFit()
		{
			SizeToFit = true;
		}


		/// <summary>
		/// Returns the maximum size (in pixels) of the text in this View.
		/// </summary>
		public virtual float MaxTextSize
		{
			get
			{
				return MHelper.MaxTextSize;
			}
			set
			{
				MHelper.SetMaxTextSize(value);
			}
		}


		/// <summary>
		/// Set the maximum text size to a given unit and value. See TypedValue for the possible
		/// dimension units.
		/// </summary>
		/// <param name="unit"> The desired dimension unit. </param>
		/// <param name="size"> The desired size in the given units.
		/// @attr ref android.R.styleable#TextView_textSize </param>
		public virtual void SetMaxTextSize(ComplexUnitType unit, float size)
		{
			MHelper.SetMaxTextSize(unit, size);
		}

		/// <summary>
		/// Returns the minimum size (in pixels) of the text in this View.
		/// </summary>
		public virtual float GetMinTextSize()
		{
			return MHelper.MinTextSize;
		}

		/// <summary>
		/// Set the minimum text size to the given value, interpreted as "scaled pixel" units. This size
		/// is adjusted based on the current density and user font size preference.
		/// </summary>
		/// <param name="minSize"> The scaled pixel size.
		/// @attr ref me.grantland.R.styleable#AutoFitTextView_minTextSize </param>
		public virtual void SetMinTextSize(int minSize)
		{
			MHelper.SetMinTextSize(ComplexUnitType.Sp, minSize);
		}

		/// <summary>
		/// Set the minimum text size to a given unit and value. See TypedValue for the possible
		/// dimension units.
		/// </summary>
		/// <param name="unit">    The desired dimension unit. </param>
		/// <param name="minSize"> The desired size in the given units.
		/// @attr ref me.grantland.R.styleable#AutoFitTextView_minTextSize </param>
		public virtual void SetMinTextSize(ComplexUnitType unit, float minSize)
		{
			MHelper.SetMinTextSize(unit, minSize);
		}

		/// <summary>
		/// Returns the amount of precision used to calculate the correct text size to fit within its
		/// bounds.
		/// </summary>
		public virtual float Precision
		{
			get
			{
				return MHelper.Precision;
			}
			set
			{
				MHelper.SetPrecision(value);
			}
		}


		public virtual void OnTextSizeChange(float textSize, float oldTextSize)
		{
			// do nothing
		}

    }

}