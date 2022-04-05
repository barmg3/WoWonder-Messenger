﻿namespace WoWonder.Library.Anjo.Share.Abstractions
{
	/// <summary>
	/// Platform specific Share Options
	/// </summary>
	public class ShareOptions
	{
		/// <summary>
		/// Android: Gets or sets the title of the app chooser popup.
		/// If null (default) the system default title is used.
		/// </summary>
		public string ChooserTitle { get; set; } = null!;


		/// <summary>
		/// Tizen: Excluded App Types
		/// </summary>
		public ShareAppControlType ExcludedAppControlTypes { get; set; } = 0;

		/// <summary>
		/// iOS: Gets or sets the UIActivityTypes that should not be displayed.
		/// If null (default) the value of <see cref="Plugin.Share.ShareImplementation.ExcludedUIActivityTypes"/> is used.
		/// </summary>
		public ShareUIActivityType[] ExcludedUIActivityTypes { get; set; } = null!;

		/// <summary>
		/// iOS only: Gets or sets the popover anchor rectangle.
		/// If null (default) the option is not used.
		/// </summary>
		public ShareRectangle PopoverAnchorRectangle { get; set; } = null!;
	}

}
