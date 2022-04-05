using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Activities.Editor.Adapters;
using WoWonder.Helpers.Model.Editor;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace WoWonder.Activities.Editor.Tools
{
    public class TextEditorFragment : DialogFragment, ITextEditor
    {
        private readonly EditColorActivity EditColorActivity;

        //private readonly EditImageActivity ImageActivity;
        private readonly string ExtraColorCode = "extra_color_code";
        private readonly string ExtraInputText = "extra_input_text";
        private RecyclerView AddTextColorPickerRecyclerView;
        private ColorPickerAdapter ColorPickerAdapter;
        private RecyclerView FontTypeFaceRecyclerView;
        private LinearLayoutManager LayoutManager, LayoutManagerTypeFace;
        private TextView MAddTextDoneTextView;
        private EditText MAddTextEditText;
        private string MColorCode;
        private InputMethodManager MInputMethodManager;
        private ViewTextType TextType;

        private FontTypeFaceAdapter TypeFaceAdapter;

        private Typeface FontTxtResult;
        private ITextEditor MTextEditor;

        public TextEditorFragment(/*EditImageActivity imageActivity,*/ EditColorActivity editColorActivity)
        {
            try
            {
                //ImageActivity = imageActivity;
                EditColorActivity = editColorActivity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnDone(string inputText, string colorCode, ViewTextType changeText, Typeface textTypeface)
        {
            try
            {
                //if (ImageActivity != null)
                //{
                //    if (TextType == ViewTextType.Add)
                //        ImageActivity.MNiceArtEditor.AddText(textTypeface, inputText, colorCode);
                //    else
                //        ImageActivity.MNiceArtEditor.EditText(ImageActivity.MView, textTypeface, inputText,
                //            colorCode);
                //}
                //else
                if (EditColorActivity != null)
                {
                    if (TextType == ViewTextType.Add)
                        EditColorActivity.NewColorEditor.AddText(textTypeface, inputText, colorCode);
                    else
                        EditColorActivity.NewColorEditor.EditText(EditColorActivity.RootView, textTypeface,
                            inputText, colorCode);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.DialogAddText, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                MAddTextEditText = view.FindViewById<EditText>(Resource.Id.add_text_edit_text);
                MAddTextDoneTextView = view.FindViewById<TextView>(Resource.Id.add_text_done_tv);

                //Setup the color picker for text color
                AddTextColorPickerRecyclerView =
                    view.FindViewById<RecyclerView>(Resource.Id.add_text_color_picker_recycler_view);
                FontTypeFaceRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.fontTypeFace_recycler_view);

                LayoutManager = new LinearLayoutManager(view.Context, LinearLayoutManager.Horizontal, false);
                AddTextColorPickerRecyclerView.SetLayoutManager(LayoutManager);
                AddTextColorPickerRecyclerView.HasFixedSize = true;

                LayoutManagerTypeFace = new LinearLayoutManager(view.Context, LinearLayoutManager.Horizontal, false);
                FontTypeFaceRecyclerView.SetLayoutManager(LayoutManagerTypeFace);
                FontTypeFaceRecyclerView.HasFixedSize = true;

                ColorPickerAdapter = new ColorPickerAdapter(Activity, ColorType.ColorNormal);
                ColorPickerAdapter.ItemClick += ColorPickerAdapterOnItemClick;
                AddTextColorPickerRecyclerView.SetAdapter(ColorPickerAdapter);

                TypeFaceAdapter = new FontTypeFaceAdapter(Activity);
                TypeFaceAdapter.ItemClick += TypeFaceAdapterOnItemClick;
                FontTypeFaceRecyclerView.SetAdapter(TypeFaceAdapter);

                MAddTextEditText.Text = Arguments.GetString(ExtraInputText);

                var dd = Arguments.GetInt(ExtraColorCode);

                MColorCode = Arguments.GetString(ExtraColorCode) ?? "#ffffff";
                MAddTextEditText.SetTextColor(Color.ParseColor(MColorCode));

                MInputMethodManager = (InputMethodManager)view.Context.GetSystemService(Android.Content.Context.InputMethodService);
                MInputMethodManager.ShowSoftInput(MAddTextEditText, ShowFlags.Forced);

                MAddTextDoneTextView.Click += MAddTextDoneTextViewOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MAddTextDoneTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                MInputMethodManager.HideSoftInputFromWindow(MAddTextDoneTextView.WindowToken, 0);
                Dismiss();

                var inputText = MAddTextEditText.Text;
                if (!TextUtils.IsEmpty(inputText))
                    MTextEditor?.OnDone(inputText, MColorCode, TextType, FontTxtResult);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void TypeFaceAdapterOnItemClick(object sender, FontTypeFaceAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = TypeFaceAdapter.GetItem(position);
                    if (item != null)
                    {
                        FontTxtResult = item;
                        MAddTextEditText.SetTypeface(item, TypefaceStyle.Normal);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window.SetLayout(width, height);
                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ColorPickerAdapterOnItemClick(object sender, ColorPickerAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = ColorPickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        MColorCode = item.ColorFirst;
                        MAddTextEditText.SetTextColor(Color.ParseColor(item.ColorFirst));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show dialog with provide text and text color
        public TextEditorFragment Show(Activity contextActivity/*, EditImageActivity imageActivity*/, EditColorActivity editColorActivity, string inputText, int colorCode, ViewTextType type)
        {
            try
            {
                TextType = type;

                var args = new Bundle();
                args.PutString(ExtraInputText, inputText);
                args.PutInt(ExtraColorCode, colorCode);
                var fragment = new TextEditorFragment(/*imageActivity,*/ editColorActivity) { Arguments = args };

                //if (imageActivity != null)
                //    fragment.Show(imageActivity.SupportFragmentManager, "TextEditorFragment");
                //else 
                if (editColorActivity != null)
                    fragment.Show(editColorActivity.SupportFragmentManager, "TextEditorFragment");

                fragment.SetOnTextEditorListener(this);
                return fragment;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        //Show dialog with default text input as empty and text color white
        public TextEditorFragment Show(Activity contextActivity, ViewTextType type)
        {
            try
            {
                return Show(contextActivity, /*ImageActivity,*/ EditColorActivity, "", Color.White, type);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        //Callback to listener if user is done with text editing
        public void SetOnTextEditorListener(ITextEditor textEditor)
        {
            try
            {
                MTextEditor = textEditor;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnColorPickerClickListener(string colorCode)
        {
            try
            {
                //mColorCode = colorCode;
                //mAddTextEditText.SetTextColor(Color.ParseColor(colorCode));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}