using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;
using WoWonder.NiceArt.Utils;
using Exception = System.Exception;

namespace WoWonder.NiceArt
{
    public class NiceArtEditor : Java.Lang.Object, INiceArt.IBrushViewChangeListener, INiceArt.IOnGestureControl, INiceArt.IOnSaveBitmap, INiceArt.IOnMultiTouchListener
    {
        private static readonly string Tag = "NiceArtEditor";
        private readonly LayoutInflater MLayoutInflater;
        private Context Context;
        private NiceArtEditorView ParentView;
        private View ImageRootView;
        private View TextRootView;
        private View EmojiRootView;
        private ImageView ImageView;
        private FrameLayout FrmBorder;
        private TextView ImgClose, ImgEdit;
        private TextView TextInputTv;
        private TextView InputTextView;
        private TextView EmojiTextView;
        private View DeleteView;
        private BrushDrawingView BrushDrawingView;
        private List<View> AddedViews;
        private List<View> RedoViews;
        private INiceArt.IOnNiceArtEditorListener MOnNiceArtEditorListener;
        private bool IsTextPinchZoomable;
        private Typeface MDefaultTextTypeface;
        private Typeface MDefaultEmojiTypeface;
        private View FinalRootView;
        private string ImagePath;
        private INiceArt.IOnSaveListener OnSaveListener;
        private SaveSettings SaveSettings;
        private static ContentResolver Resolver;
       
        public NiceArtEditor(Builder builder, ContentResolver resolver)
        {
            try
            {
                Context = builder.context;
                ParentView = builder.parentView;
                ImageView = builder.imageView;
                DeleteView = builder.deleteView;
                BrushDrawingView = builder.brushDrawingView;
                IsTextPinchZoomable = builder.isTextPinchZoomable;
                MDefaultTextTypeface = builder.textTypeface;
                MDefaultEmojiTypeface = builder.emojiTypeface;
                MLayoutInflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
                BrushDrawingView.SetBrushViewChangeListener(this);
                AddedViews = new List<View>();
                RedoViews = new List<View>();
                Resolver = resolver;
                //wael
                //ParentView.DrawingCacheEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Get root view by its type i.e image,text and emojis
        /// </summary>
        /// <param name="viewType">image,text or emojis</param>
        /// <returns>rootView</returns>
        public View GetLayout(ViewType viewType)
        {
            try
            {
                View rootView = null!;

                if (viewType == ViewType.Text)
                {
                    rootView = MLayoutInflater.Inflate(Resource.Layout.Style_NiceArt_TextView, null);
                    TextView txtText = rootView.FindViewById<TextView>(Resource.Id.tvNiceArtText);
                    if (txtText != null && MDefaultTextTypeface != null)
                    {
                        //txtText.Gravity = GravityFlags.Center;
                        if (MDefaultEmojiTypeface != null)
                        {
                            txtText.SetTypeface(MDefaultTextTypeface, TypefaceStyle.Normal);
                        }
                    }
                }
                else if (viewType == ViewType.Image)
                {
                    rootView = MLayoutInflater.Inflate(Resource.Layout.Style_NiceArt_ImageView, null);
                }
                else if (viewType == ViewType.Emojis)
                {
                    rootView = MLayoutInflater.Inflate(Resource.Layout.Style_NiceArt_TextView, null);
                    TextView txtTextEmojis = rootView.FindViewById<TextView>(Resource.Id.tvNiceArtText);
                    if (txtTextEmojis != null)
                    {
                        if (MDefaultEmojiTypeface != null)
                        {
                            //txtTextEmojis.SetTypeface(mDefaultEmojiTypeface, TypefaceStyle.Normal);
                        }
                        //txtTextEmojis.Gravity = GravityFlags.Center;

                        txtTextEmojis.SetLayerType(LayerType.Software, null);
                    }
                }

                if (rootView != null)
                {
                    //We are setting tag as ViewType to identify what type of the view it is
                    //when we remove the view from stack i.e onRemoveViewListener(ViewType viewType, int numberOfAddedViews);

                    FinalRootView = rootView;
                    if (viewType == ViewType.Text)
                    {
                        ImgEdit = rootView.FindViewById<TextView>(Resource.Id.txtNiceArtEdit);
                        ImgEdit.Visibility = ViewStates.Visible;
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, ImgEdit, "\uf044");
                        ImgEdit.Click += ImgEditOnClick;
                    }
                }
                return rootView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;

            }
        }

        public void AddImage(Bitmap desiredImage)
        {
            try
            {
                ImageRootView = GetLayout(ViewType.Image);

                ImageView = ImageRootView.FindViewById<ImageView>(Resource.Id.imgNiceArtImage);
                FrmBorder = ImageRootView.FindViewById<FrameLayout>(Resource.Id.frmBorder);
                TextView imgCloseImage = ImageRootView.FindViewById<TextView>(Resource.Id.txtNiceArtClose);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, imgCloseImage, "\uf00d");
                imgCloseImage.Click += CloseAddImageOnClick;
                ImgClose = imgCloseImage;

                ImageView.SetImageBitmap(desiredImage);

                MultiTouchListener multiTouchListener = GetMultiTouchListener();
                multiTouchListener.SetOnGestureControl(this, ViewType.Image);

                ImageRootView.SetOnTouchListener(multiTouchListener);
                AddViewToParent(ImageRootView, ViewType.Image);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnClick()
        {
            try
            {
                bool isBackgroundVisible = FrmBorder.Tag != null && (bool)FrmBorder.Tag;
                FrmBorder.SetBackgroundResource(isBackgroundVisible ? 0 : Resource.Xml.rounded_border_tv);

                if (isBackgroundVisible)
                {
                    ImgClose.Visibility = ViewStates.Gone;

                    if (ImgEdit != null)
                    {
                        ImgEdit.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    ImgClose.Visibility = ViewStates.Visible;

                    if (ImgEdit != null)
                    {
                        ImgEdit.Visibility = ViewStates.Visible;
                    }
                }

                FrmBorder.Tag = !isBackgroundVisible;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnLongClick(ViewType type)
        {
            try
            {
                Console.WriteLine(type);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        /// <summary>
        /// This add the text on the {@link NiceArtEditorView} with provided parameters
        /// by default {@link TextView#setText(int)} will be 18sp
        /// </summary>
        /// <param name="text">text to display</param>
        /// <param name="colorCodeTextView">colorCodeTextView text color to be displayed</param>
        public void AddText(string text, string colorCodeTextView)
        {
            try
            {
                AddText(null, text, colorCodeTextView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// This add the text on the {@link NiceArtEditorView} with provided parameters
        /// by default {@link TextView#setText(int)} will be 18sp
        /// </summary>
        /// <param name="textTypeface">typeface for custom font in the text</param>
        /// <param name="text">text to display</param>
        /// <param name="colorCodeTextView">colorCodeTextView text color to be displayed</param>
        public void AddText(Typeface textTypeface, string text, string colorCodeTextView)
        {
            try
            {
                BrushDrawingView.SetBrushDrawingMode(false);
                TextRootView = GetLayout(ViewType.Text);
                TextInputTv = TextRootView.FindViewById<TextView>(Resource.Id.tvNiceArtText);
                FrmBorder = TextRootView.FindViewById<FrameLayout>(Resource.Id.frmBorder);
                TextView imgCloseText = TextRootView.FindViewById<TextView>(Resource.Id.txtNiceArtClose);
                imgCloseText.Click += AddTextCloseOnClick;
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, imgCloseText, "\uf00d");
                ImgClose = imgCloseText;

                TextInputTv.Text = text;

                TextInputTv.SetTextColor(Color.ParseColor(colorCodeTextView));
                if (textTypeface != null)
                {
                    TextInputTv.SetTypeface(textTypeface, TypefaceStyle.Normal);
                }
                MultiTouchListener multiTouchListener = GetMultiTouchListener();
                multiTouchListener.SetOnGestureControl(this, ViewType.Text);

                TextRootView.SetOnTouchListener(multiTouchListener);
                AddViewToParent(TextRootView, ViewType.Text);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void AddTextCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                ViewUndo(FinalRootView, ViewType.Text);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);

            }
        }
        public void CloseAddImageOnClick(object sender, EventArgs e)
        {
            try
            {
                ViewUndo(FinalRootView, ViewType.Image);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);

            }
        }

        public void AddEmojiCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                ViewUndo(FinalRootView, ViewType.Emojis);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);

            }
        }



        /// <summary>
        /// This will update text and color on provided view
        /// </summary>
        /// <param name="view">view on which you want update</param>
        /// <param name="inputText">text to update {@link TextView}</param>
        /// <param name="colorCode">color to update on {@link TextView}</param>
        public void EditText(View view, string inputText, string colorCode)
        {
            try
            {
                EditText(view, Typeface.Default, inputText, colorCode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void EditText(View view, Typeface textTypeface, string inputText, string colorCode)
        {
            try
            {
                InputTextView = view.FindViewById<TextView>(Resource.Id.tvNiceArtText);
                if (InputTextView != null && AddedViews.Contains(view) && !TextUtils.IsEmpty(inputText))
                {
                    InputTextView.Text = inputText;

                    if (textTypeface != null)
                        InputTextView.SetTypeface(textTypeface, TypefaceStyle.Normal);

                    InputTextView.SetTextColor(Color.ParseColor(colorCode));
                    ParentView.UpdateViewLayout(view, view.LayoutParameters);
                    int i = AddedViews.IndexOf(view);

                    if (i > -1)
                        AddedViews.Add(view);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void ImgEditOnClick(object sender, EventArgs e)
        {
            try
            {
                string textInput = TextInputTv.Text;
                int currentTextColor = TextInputTv.CurrentTextColor;
                MOnNiceArtEditorListener?.OnEditTextChangeListener(TextRootView, textInput, currentTextColor);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        /// <summary>
        ///  Adds emoji to the {@link NiceArtEditorView} which you drag,rotate and scale using pinch
        /// if {@link NiceArtEditor.Builder#setPinchTextScalable(boolean)} enabled
        /// </summary>
        /// <param name="emojiName">unicode in form of string to display emojis</param>
        public void AddEmojis(string emojiName)
        {
            try
            {
                AddEmojis(null, emojiName);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Adds emoji to the {@link NiceArtEditorView} which you drag,rotate and scale using pinch
        /// if {@link NiceArtEditor.Builder#setPinchTextScalable(boolean)} enabled
        /// </summary>
        /// <param name="emojisTypeface">typeface for custom font to show emojis unicode in specific font</param>
        /// <param name="emojisName">unicode in form of string to display emojis</param>
        public void AddEmojis(Typeface emojisTypeface, string emojisName)
        {
            try
            {
                BrushDrawingView.SetBrushDrawingMode(false);
                EmojiRootView = GetLayout(ViewType.Emojis);
                EmojiTextView = EmojiRootView.FindViewById<TextView>(Resource.Id.tvNiceArtText);
                FrmBorder = EmojiRootView.FindViewById<FrameLayout>(Resource.Id.frmBorder);
                TextView imgCloseEmojis = EmojiRootView.FindViewById<TextView>(Resource.Id.txtNiceArtClose);
                imgCloseEmojis.Click += AddEmojiCloseOnClick;
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, imgCloseEmojis, "\uf00d");
                ImgClose = imgCloseEmojis;

                if (emojisTypeface != null)
                {
                    EmojiTextView.SetTypeface(emojisTypeface, TypefaceStyle.Normal);
                }

                EmojiTextView.SetTextSize(ComplexUnitType.Sp, 56);
                EmojiTextView.Text = emojisName;
                MultiTouchListener multiTouchListener = GetMultiTouchListener();
                multiTouchListener.SetOnGestureControl(this, ViewType.Emojis);

                EmojiRootView.SetOnTouchListener(multiTouchListener);
                AddViewToParent(EmojiRootView, ViewType.Emojis);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Add to root view from image,emoji and text to our parent view
        /// </summary>
        /// <param name="rootView">rootView rootview of image,text and emoji</param>
        /// <param name="viewType"></param>
        public void AddViewToParent(View rootView, ViewType viewType)
        {
            try
            {
                RelativeLayout.LayoutParams Params = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                Params.AddRule(LayoutRules.CenterInParent);
                ParentView.AddView(rootView, Params);
                AddedViews.Add(rootView);

                MOnNiceArtEditorListener?.OnAddViewListener(viewType, AddedViews.Count);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Create a new instance and scalable TouchView
        /// </summary>
        /// <returns>scalable multitouch listener</returns>
        public MultiTouchListener GetMultiTouchListener()
        {
            try
            {
                MultiTouchListener multiTouchListener = new MultiTouchListener(
                    DeleteView,
                    ParentView,
                    ImageView,
                    IsTextPinchZoomable,
                    MOnNiceArtEditorListener);

                multiTouchListener.SetOnMultiTouchListener(this);

                return multiTouchListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;

            }
        }

        /// <summary>
        /// Enable/Disable drawing mode to draw on {@link NiceArtEditorView}
        /// </summary>
        /// <param name="brushDrawingMode">true if mode is enabled</param>
        public void SetBrushDrawingMode(bool brushDrawingMode)
        {
            try
            {
                BrushDrawingView?.SetBrushDrawingMode(brushDrawingMode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>true is brush mode is enabled</returns>
        public bool GetBrushDrawableMode()
        {
            try
            {
                return BrushDrawingView != null && BrushDrawingView.GetBrushDrawingMode();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        /// <summary>
        /// set the size of bursh user want to paint on canvas i.e {@link BrushDrawingView}
        /// </summary>
        /// <param name="size">size size of brush</param>
        public void SetBrushSize(float size)
        {
            try
            {
                BrushDrawingView?.SetBrushSize(size);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        /// <summary>
        /// set opacity/transparency of brush while painting on {@link BrushDrawingView}
        /// </summary>
        /// <param name="opacity">opacity is in form of percentage</param>
        public void SetOpacity(int opacity)
        {
            try
            {
                if (BrushDrawingView != null)
                {
                    opacity = (int)(opacity / 100.0 * 255.0);
                    BrushDrawingView.SetOpacity(opacity);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// set brush color which user want to paint
        /// </summary>
        /// <param name="color">color value for paint</param>
        public void SetBrushColor(string color)
        {
            try
            {
                BrushDrawingView?.SetBrushColor(Color.ParseColor(color));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// set the eraser size
        /// <b>Note :</b> Eraser size is different from the normal brush size
        /// </summary>
        /// <param name="brushEraserSize">size of eraser</param>
        public void SetBrushEraserSize(float brushEraserSize)
        {
            try
            {
                BrushDrawingView?.SetBrushEraserSize(brushEraserSize);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void SetBrushEraserColor(string color)
        {
            try
            {
                BrushDrawingView?.SetBrushEraserColor(Color.ParseColor(color));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// see NiceArtEditor#setBrushEraserSize(float)
        /// </summary>
        /// <returns>provide the size of eraser</returns>
        public float GetEraserSize()
        {
            try
            {
                return BrushDrawingView?.GetEraserSize() ?? 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }

        }

        /// <summary>
        /// NiceArtEditor#setBrushSize(float)
        /// </summary>
        /// <returns>provide the size of eraser</returns>
        public float GetBrushSize()
        {
            try
            {
                if (BrushDrawingView != null)
                    return BrushDrawingView.GetBrushSize();
                return 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }

        }


        /// <summary>
        /// provide the size of eraser
        /// </summary>
        /// <returns>NiceArtEditor#setBrushColor(int)</returns>
        public int GetBrushColor()
        {
            try
            {
                if (BrushDrawingView != null)
                    return BrushDrawingView.GetBrushColor();
                return 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }

        }

        /// <summary>
        /// Its enables eraser mode after that whenever user drags on screen this will erase the existing paint
        /// <b>Note</b> : This eraser will work on paint views only
        /// </summary>
        public void BrushEraser()
        {
            try
            {
                BrushDrawingView?.BrushEraser();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }

        }

        public void ViewUndo(View removedView, ViewType viewType)
        {
            try
            {
                if (AddedViews.Count > 0)
                {
                    if (AddedViews.Contains(removedView))
                    {
                        ParentView.RemoveView(removedView);
                        AddedViews.Remove(removedView);
                        RedoViews.Add(removedView);
                        if (MOnNiceArtEditorListener != null)
                        {
                            MOnNiceArtEditorListener.OnRemoveViewListener(AddedViews.Count);
                            MOnNiceArtEditorListener.OnRemoveViewListener(viewType, AddedViews.Count);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }

        }

        /// <summary>
        /// Undo the last operation perform on the {@link NiceArtEditor}
        /// </summary>
        /// <returns>true if there nothing more to undo</returns>
        public bool Undo(ViewType viewType)
        {
            try
            {
                if (AddedViews.Count > 0)
                {
                    var view = AddedViews.LastOrDefault();
                    if (view != null)
                    {

                        if (view.GetType() == typeof(BrushDrawingView))
                        {
                            return BrushDrawingView != null && BrushDrawingView.Undo();
                        }
                        else
                        {
                            AddedViews.Remove(view);
                            ParentView.RemoveView(view);
                            RedoViews.Add(view);
                        }
                    }

                    if (MOnNiceArtEditorListener != null)
                    {
                        MOnNiceArtEditorListener?.OnRemoveViewListener(AddedViews.Count);
                        MOnNiceArtEditorListener.OnRemoveViewListener(viewType, AddedViews.Count);
                    }
                }
                return AddedViews.Count != 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        /// <summary>
        /// Redo the last operation perform on the {@link NiceArtEditor}
        /// </summary>
        /// <returns>true if there nothing more to redo</returns>
        public bool Redo(ViewType viewType)
        {
            try
            {
                if (RedoViews.Count > 0)
                {
                    var redoView = RedoViews.LastOrDefault();
                    if (redoView != null)
                    {
                        if (redoView.GetType() == typeof(BrushDrawingView))
                        {
                            return BrushDrawingView != null && BrushDrawingView.Redo();
                        }
                        else
                        {
                            var redoViewLast = RedoViews.LastOrDefault();
                            if (redoViewLast != null)
                            {
                                RedoViews.Remove(redoViewLast);
                            }
                            ParentView.AddView(redoView);
                            AddedViews.Add(redoView);
                        }

                        MOnNiceArtEditorListener?.OnAddViewListener(viewType, AddedViews.Count);
                    }
                }
                return RedoViews.Count != 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public void ClearBrushAllViews()
        {
            try
            {
                BrushDrawingView?.ClearAll();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Removes all the edited operations performed {@link NiceArtEditorView}
        /// This will also clear the undo and redo stack
        /// </summary>
        public void ClearAllViews()
        {
            try
            {
                foreach (var view in AddedViews)
                {
                    ParentView.RemoveView(view);
                }
                if (AddedViews.Contains(BrushDrawingView))
                {
                    ParentView.AddView(BrushDrawingView);
                }
                AddedViews.Clear();
                RedoViews.Clear();
                ClearBrushAllViews();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }

        }

        /// <summary>
        /// Remove all helper boxes from views
        /// </summary>
        public void ClearHelperBox()
        {
            try
            {
                for (int i = 0; i < ParentView.ChildCount; i++)
                {
                    View childAt = ParentView.GetChildAt(i);
                    FrmBorder = childAt.FindViewById<FrameLayout>(Resource.Id.frmBorder);
                    FrmBorder?.SetBackgroundResource(0);
                    ImgClose = childAt.FindViewById<TextView>(Resource.Id.txtNiceArtClose);
                    if (ImgClose != null)
                    {
                        ImgClose.Visibility = ViewStates.Gone;
                    }
                    ImgEdit = childAt.FindViewById<TextView>(Resource.Id.txtNiceArtEdit);
                    if (ImgEdit != null)
                    {
                        ImgEdit.Visibility = ViewStates.Gone;
                    }

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Setup of custom effect using effect type and set parameters values
        /// </summary>
        /// <param name="customEffect">{@link CustomEffect.Builder#setParameter(String, Object)}</param>
        public void SetFilterEffect(CustomEffect customEffect)
        {
            try
            {
                ParentView.SetFilterEffect(customEffect);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Set pre-define filter available
        /// </summary>
        /// <param name="filterType">type of filter want to apply { @link NiceArtEditor }</param>
        public void SetFilterEffect(PhotoFilter filterType)
        {
            try
            {
                ParentView.SetFilterEffect(filterType);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Save the edited image on given path
        /// </summary>
        /// <param name="imagePath">path on which image to be saved</param>
        /// <param name="onSaveListener">callback for saving image</param>
        public void SaveImage(string imagePath, INiceArt.IOnSaveListener onSaveListener)
        {
            try
            {
                SaveAsFile(imagePath, onSaveListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void SaveAsFile(string imagePath, INiceArt.IOnSaveListener onSaveListener)
        {
            try
            {
                SaveSettings saveSettings = new SaveSettings.Builder()
                    .SetClearViewsEnabled(true)
                    .SetTransparencyEnabled(true)
                    .Build();

                SaveAsFile(imagePath, saveSettings, onSaveListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Save the edited image on given path
        /// </summary>
        /// <param name="imagePath">builder for multiple save options {@link SaveSettings}</param>
        /// <param name="saveSettings">builder for multiple save options {@link SaveSettings</param>
        /// <param name="onSaveListener">callback for saving image</param>
        public void SaveAsFile(string imagePath, SaveSettings saveSettings, INiceArt.IOnSaveListener onSaveListener)
        {
            try
            {
                Console.WriteLine(Tag, "Image Path: " + imagePath);

                ImagePath = imagePath;
                OnSaveListener = onSaveListener;
                SaveSettings = saveSettings;

                ParentView.SaveFilter(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public class MyAsyncTask : AsyncTask<string, string, string>
        {
            private readonly string ImagePath;
            private readonly NiceArtEditorView ParentView;
            private readonly NiceArtEditor Editor;
            private readonly INiceArt.IOnSaveListener OnSaveListener;
            private SaveType Type;
            private readonly SaveSettings SaveSettings;
            private Bitmap SaveBitmap;
            private Bitmap SavedResultBitmap;
            public MyAsyncTask(NiceArtEditor editor, string imagePath, NiceArtEditorView parentView, INiceArt.IOnSaveListener onSaveListener, SaveSettings saveSettings, Bitmap saveBitmap, SaveType type) : base()
            {
                try
                {
                    // do stuff
                    Editor = editor;
                    ImagePath = imagePath;
                    ParentView = parentView;
                    OnSaveListener = onSaveListener;
                    Type = type;
                    SaveSettings = saveSettings;
                    SaveBitmap = saveBitmap;

                    Editor?.ClearHelperBox();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);

                }
            }
               
            protected override string RunInBackground(string[] str)
            {
                try
                {
                    // Create a media file name
                    Java.IO.File file = new Java.IO.File(ImagePath);
                    try
                    {
                        var stream = new FileStream(file.Path, FileMode.Create);
                        if (ParentView != null)
                        { 
                            if (SaveSettings.IsTransparencyEnabled())
                            {
                                Bitmap drawingCache = BitmapUtil.LoadBitmapFromView(ParentView);
                                drawingCache.Compress(Bitmap.CompressFormat.Png, 100, stream);
                                SavedResultBitmap = drawingCache;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        stream.Flush();
                        stream.Close();
                        Console.WriteLine(Tag, "Filed Saved Successfully");
                        return "";
                    }
                    catch (FileNotFoundException ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                        //ex.PrintStackTrace();
                        Console.WriteLine(Tag, "Failed to save File");
                        return ex.Message;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return e.Message;
                }
            }

            protected override void OnPostExecute(string result)
            {
                try
                {
                    base.OnPostExecute(result);
                    if (result == "")
                    {
                        //Clear all views if its enabled in save settings
                        if (SaveSettings.IsClearViewsEnabled()) Editor?.ClearAllViews();
                         
                        OnSaveListener.OnSuccess(ImagePath, SavedResultBitmap); 
                    }
                    else
                    {
                        OnSaveListener.OnFailure("Failed to load the bitmap");
                    }

                    Editor?.ClearHelperBox();
                    Dispose(true); 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    OnSaveListener.OnFailure(result); 
                }
            }
        }

        public void OnBitmapReady(Bitmap saveBitmap, SaveType type)
        {
            try
            {
                MyAsyncTask task = new MyAsyncTask(this, ImagePath, ParentView, OnSaveListener, SaveSettings, saveBitmap, type);
                task.Execute();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }
         
        public void OnFailure(string e)
        {
            try
            {
                OnSaveListener.OnFailure(e);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);

            }
        }

        /// <summary>
        ///  Save the edited image as bitmap
        /// </summary>
        /// <param name="onSaveBitmap">callback for saving image as bitmap</param>
        public void SaveAsBitmap(INiceArt.IOnSaveBitmap onSaveBitmap)
        {
            try
            {
                SaveSettings saveSettings = new SaveSettings.Builder()
                    .SetClearViewsEnabled(true)
                    .SetTransparencyEnabled(true)
                    .Build();

                SaveAsBitmap(saveSettings, onSaveBitmap);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Save the edited image as bitmap
        /// </summary>
        /// <param name="saveSettings">builder for multiple save options {@link SaveSettings}</param>
        /// <param name="onSaveBitmap">callback for saving image as bitmap</param>
        public void SaveAsBitmap(SaveSettings saveSettings, INiceArt.IOnSaveBitmap onSaveBitmap)
        {
            try
            {
                SaveSettings = saveSettings;

                ParentView.SaveFilter(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static string ConvertEmoji(string emoji)
        {
            try
            {
                string returnedEmoji;
                try
                {
                    int convertEmojiToInt = Convert.ToInt32(emoji.Substring(2), 16);
                    returnedEmoji = new string(Character.ToChars(convertEmojiToInt));
                }
                catch (NumberFormatException e)
                {
                    Methods.DisplayReportResultTrack(e);
                    returnedEmoji = "";
                }
                return returnedEmoji;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";

            }
        }


        /// <summary>
        /// Callback on editing operation perform on {@link NiceArtEditorView}
        /// </summary>
        /// <param name="onNiceArtEditorListener">{@link OnNiceArtEditorListener}</param>
        public void SetOnNiceArtEditorListener(INiceArt.IOnNiceArtEditorListener onNiceArtEditorListener)
        {
            try
            {
                MOnNiceArtEditorListener = onNiceArtEditorListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Check if any changes made need to save
        /// </summary>
        /// <returns>true if nothing is there to change</returns>
        public bool IsCacheEmpty()
        {
            try
            {
                return AddedViews.Count == 0 && RedoViews.Count == 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public void OnViewAdd(BrushDrawingView brushDrawingView)
        {
            try
            {
                if (RedoViews.Count > 0)
                {
                    var item = RedoViews.LastOrDefault();
                    if (item != null)
                    {
                        RedoViews.Remove(item);
                    }
                }
                AddedViews.Add(brushDrawingView);
                MOnNiceArtEditorListener?.OnAddViewListener(ViewType.BrushDrawing, AddedViews.Count);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnViewRemoved(BrushDrawingView brushDrawingView)
        {
            try
            {
                if (AddedViews.Count > 0)
                {
                    var item = AddedViews.LastOrDefault();
                    if (item != null)
                    {
                        AddedViews.Remove(item);
                        if (!(item.GetType() == typeof(BrushDrawingView)))
                        {
                            ParentView.RemoveView(item);
                        }
                        RedoViews.Add(item);
                    }
                }

                if (MOnNiceArtEditorListener != null)
                {
                    MOnNiceArtEditorListener?.OnRemoveViewListener(AddedViews.Count);
                    MOnNiceArtEditorListener?.OnRemoveViewListener(ViewType.BrushDrawing, AddedViews.Count);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnStartDrawing()
        {
            try
            {
                MOnNiceArtEditorListener?.OnStartViewChangeListener(ViewType.BrushDrawing);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnStopDrawing()
        {
            try
            {
                MOnNiceArtEditorListener?.OnStopViewChangeListener(ViewType.BrushDrawing);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        /// <summary>
        /// Builder pattern to define {@link NiceArtEditor} Instance
        /// </summary>
        public class Builder
        {
            public Context context;
            public NiceArtEditorView parentView;
            public ImageView imageView;
            public View deleteView;
            public BrushDrawingView brushDrawingView;
            public Typeface textTypeface;
            public Typeface emojiTypeface;
            //By Default pinch zoom on text is enabled
            public bool isTextPinchZoomable = true;
            public ContentResolver resolverContent;
            /// <summary>
            /// Building a NiceArtEditor which requires a Context and NiceArtEditorView
            /// which we have setup in our xml layout
            /// </summary>
            /// <param name="context">context</param>
            /// <param name="NiceArtEditorView">NiceArtEditorView</param>
            public Builder(Context context, NiceArtEditorView NiceArtEditorView, ContentResolver resolver)
            {
                try
                {
                    this.context = context;
                    parentView = NiceArtEditorView;
                    imageView = NiceArtEditorView.GetSource();
                    brushDrawingView = NiceArtEditorView.GetBrushDrawingView();
                    resolverContent = resolver;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e); 
                }
            }

            public Builder SetDeleteView(View deleteView)
            {
                try
                {
                    this.deleteView = deleteView;
                    return this;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;

                }
            }

            /// <summary>
            /// set default text font to be added on image
            /// </summary>
            /// <param name="textTypeface">typeface for custom font</param>
            /// <returns>{@link Builder} instant to build {@link NiceArtEditor}</returns>
            public Builder SetDefaultTextTypeface(Typeface textTypeface)
            {
                try
                {
                    this.textTypeface = textTypeface;
                    return this;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;

                }

            }

            /// <summary>
            /// set default font specific to add emojis
            /// </summary>
            /// <param name="emojiTypeface">typeface for custom font</param>
            /// <returns>instant to build {@link NiceArtEditor}</returns>
            public Builder SetDefaultEmojiTypeface(Typeface emojiTypeface)
            {
                try
                {
                    this.emojiTypeface = emojiTypeface;
                    return this;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;

                }
            }

            /// <summary>
            /// set false to disable pinch to zoom on text insertion.By deafult its true
            /// </summary>
            /// <param name="isTextPinchZoomable">flag to make pinch to zoom</param>
            /// <returns>instant to build {@link NiceArtEditor}</returns>
            public Builder SetPinchTextScalable(bool isTextPinchZoomable)
            {
                try
                {
                    this.isTextPinchZoomable = isTextPinchZoomable;
                    return this;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;

                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>build NiceArtEditor instance</returns>
            public NiceArtEditor Build()
            {
                try
                {
                    return new NiceArtEditor(this, resolverContent);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;

                }
            }
        }

         
        /// <summary>
        /// Provide the list of emojis in form of unicode string
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>list of emojis unicode</returns>
        public static List<string> GetEmojis(Context context)
        {
            try
            {
                string[] emojiList = context.Resources?.GetStringArray(Resource.Array.NiceArtEmojis);

                return emojiList.Select(emojiUnicode => ConvertEmoji(emojiUnicode)).ToList(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;

            }
        }

        public void OnEditTextClickListener(string text, int colorCode)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnRemoveViewListener(View removedView)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}