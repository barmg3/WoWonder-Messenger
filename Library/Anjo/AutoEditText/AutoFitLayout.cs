using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;

namespace WoWonder.Library.Anjo.AutoEditText
{
    /// <summary>
	/// <para>A <seealso cref="ViewGroup"/> that re-sizes the text of it's children to be no larger than the width of the
	/// view.
	/// 
	/// @attr ref R.styleable.AutoFitTextView_sizeToFit
	/// @attr ref R.styleable.AutoFitTextView_minTextSize
	/// @attr ref R.styleable.AutoFitTextView_precision
	/// </para>
	/// </summary>
	public class AutoFitLayout : FrameLayout
	{

		private bool MEnabled;
		private float MMinTextSize;
		private float MPrecision;
		private Dictionary<View, AutoFitHelper> MHelpers = new Dictionary<View, AutoFitHelper>();


        protected AutoFitLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public AutoFitLayout([NotNull] Context context) : base(context)
        {
            Init(context, null, 0);
        }

        public AutoFitLayout([NotNull] Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs, 0);
        }

        public AutoFitLayout([NotNull] Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs, defStyleAttr);
        }

        public AutoFitLayout([NotNull] Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs, defStyleAttr);
        }
		private void Init(Context context, IAttributeSet attrs, int defStyleAttr)
		{
			bool sizeToFit = true;
			int minTextSize = -1;
			float precision = -1;

			if (attrs != null)
			{
				TypedArray ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.AutoFitEditText, defStyleAttr, 0);
				sizeToFit = ta.GetBoolean(Resource.Styleable.AutoFitEditText_sizeToFit, sizeToFit);
				minTextSize = ta.GetDimensionPixelSize(Resource.Styleable.AutoFitEditText_minTextSize, minTextSize);
				precision = ta.GetFloat(Resource.Styleable.AutoFitEditText_precision, precision);
				ta.Recycle();
			}

			MEnabled = sizeToFit;
			MMinTextSize = minTextSize;
			MPrecision = precision;
		}

		public override void AddView(View child, int index, ViewGroup.LayoutParams @params)
		{
			base.AddView(child, index, @params);
			TextView textView = (TextView) child;
			AutoFitHelper helper = AutoFitHelper.Create(textView).SetEnabled(MEnabled);
			if (MPrecision > 0)
			{
				helper.SetPrecision(MPrecision);
			}
			if (MMinTextSize > 0)
			{
				helper.SetMinTextSize(ComplexUnitType.Px, MMinTextSize);
			}
			MHelpers.Add(textView, helper);
		}

		/// <summary>
		/// Returns the <seealso cref="AutoFitHelper"/> for this child View.
		/// </summary>
		public virtual AutoFitHelper GetAutoFitHelper(TextView textView)
        {
			var dd = MHelpers.FirstOrDefault(a => a.Key == textView).Value;
            return dd;
        }

		/// <summary>
		/// Returns the <seealso cref="AutoFitHelper"/> for this child View.
		/// </summary>
		public virtual AutoFitHelper GetAutoFitHelper(int index)
		{
            var dd = MHelpers.FirstOrDefault(a => a.Key == GetChildAt(index)).Value;
            return dd;
        }

    }

}