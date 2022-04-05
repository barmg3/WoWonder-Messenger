using System;
using System.IO;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using MaterialDialogsCore;
using WoWonder.Activities.Editor.Adapters;
using WoWonder.Activities.Editor.Tools;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model.Editor;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.AutoEditText;
using WoWonder.NiceArt;
using WoWonder.NiceArt.Models;
using WoWonder.NiceArt.Utils;
using File = Java.IO.File;
using FileNotFoundException = Java.IO.FileNotFoundException;

namespace WoWonder.Activities.Editor
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditColorActivity : AppCompatActivity, INiceArt.IOnSaveListener, ITextEditor, INiceArt.IOnNiceArtEditorListener
    {
        private RecyclerView AddTextColorPickerRecyclerView;
        private ImageView CloseImageView;
        private ColorPickerAdapter ColorPickerAdapter;
        public string MColorCode = "#ffffff";
        private RecyclerView FontTypeFaceRecyclerView;
        private int Height;
        private LinearLayoutManager LayoutManager;
        private LinearLayoutManager LayoutManagerTypeFace;
        public AutoFitEditText MAutoResizeEditText;
        private NiceArtEditorView NewColorView;
        private TextView SaveTextView, ColorTextView;
        private FontTypeFaceAdapter TypeFaceAdapter;
        private int Width;
        private Typeface FontTxtResult;
        public NiceArtEditor NewColorEditor;
        public View RootView;
        private ColorFragment MColorFragment;

        public void OnAddViewListener(ViewType viewType, int numberOfAddedViews)
        {
        }

        public void OnRemoveViewListener(int numberOfAddedViews)
        {
        }

        public void OnRemoveViewListener(ViewType viewType, int numberOfAddedViews)
        {
        }

        public void OnStartViewChangeListener(ViewType viewType)
        {
        }

        public void OnStopViewChangeListener(ViewType viewType)
        {
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this, true);

                // Create your application here
                SetContentView(Resource.Layout.EditColorLayout);

                MAutoResizeEditText = (AutoFitEditText)FindViewById(Resource.Id.rEdittext);

                var mEmojiTypeFace = Typeface.CreateFromAsset(Assets, "emojione-android.ttf");

                NewColorView = FindViewById<NiceArtEditorView>(Resource.Id.imgColorView);

                NewColorEditor = new NiceArtEditor.Builder(this, NewColorView, ContentResolver)
                    .SetPinchTextScalable(true) // set false to disable pinch to zoom on text insertion.By default its true
                    .SetDefaultEmojiTypeface(mEmojiTypeFace) // set default font TypeFace to add emojis
                    .Build(); // build NiceArt Editor sdk

                NewColorEditor.SetOnNiceArtEditorListener(this);

                //Setup the color picker for text color
                AddTextColorPickerRecyclerView = FindViewById<RecyclerView>(Resource.Id.add_text_color_picker_recycler_view);
                FontTypeFaceRecyclerView = FindViewById<RecyclerView>(Resource.Id.fontTypeFace_recycler_view);

                //Color 
                MColorFragment = new ColorFragment(NewColorEditor, this);

                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                AddTextColorPickerRecyclerView.SetLayoutManager(LayoutManager);
                AddTextColorPickerRecyclerView.HasFixedSize = true;

                LayoutManagerTypeFace = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                FontTypeFaceRecyclerView.SetLayoutManager(LayoutManagerTypeFace);
                FontTypeFaceRecyclerView.HasFixedSize = true;

                ColorPickerAdapter = new ColorPickerAdapter(this, ColorType.ColorGradient, false);
                ColorPickerAdapter.ItemClick += ColorPickerAdapterOnItemClick;
                AddTextColorPickerRecyclerView.SetAdapter(ColorPickerAdapter);

                TypeFaceAdapter = new FontTypeFaceAdapter(this);
                TypeFaceAdapter.ItemClick += TypeFaceAdapterOnItemClick;
                FontTypeFaceRecyclerView.SetAdapter(TypeFaceAdapter);

                CloseImageView = FindViewById<ImageView>(Resource.Id.imgClose);
                SaveTextView = FindViewById<TextView>(Resource.Id.imgSave);
                ColorTextView = FindViewById<TextView>(Resource.Id.txtColor);

                SaveTextView.Tag = "Add";
                SaveTextView.Text = GetText(Resource.String.Lbl_Add);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ColorTextView, "\uf37b");

                CloseImageView.Click += CloseImageViewOnClick;
                SaveTextView.Click += SaveTextViewOnClick;
                ColorTextView.Click += ColorTextViewOnClick;

                var point = Methods.App.OverrideGetSize(this);
                if (point != null)
                {
                    Width = point.X;
                    Height = point.Y;
                }

                int[] color = { Color.ParseColor("#6ec052"), Color.ParseColor("#28c4f3") };
                var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, Width, Height);
                if (bitmap != null)
                {
                    NewColorEditor.ClearAllViews();
                    NewColorView.GetSource().ClearColorFilter();

                    NewColorView.GetSource()?.SetImageBitmap(bitmap);
                }

                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (!Directory.Exists(Methods.Path.FolderDiskNiceArt))
                        Directory.CreateDirectory(Methods.Path.FolderDiskNiceArt);
                }
                else
                {
                    RequestPermissions(new[]
                    {
                        Manifest.Permission.ReadExternalStorage,
                        Manifest.Permission.WriteExternalStorage,
                        Manifest.Permission.ManageExternalStorage,
                        Manifest.Permission.AccessMediaLocation,
                    }, 10);
                }

                AdsGoogle.Ad_RewardedVideo(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Color Image
        public void ColorPickerAdapterOnItemClick(object sender, ColorPickerAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = ColorPickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (item.ColorFirst == "#ecf0f1")
                        {
                            MAutoResizeEditText.SetTextColor(Color.Black);
                            MAutoResizeEditText.SetHintTextColor(Color.Black);
                        }
                        else
                        {
                            MAutoResizeEditText.SetTextColor(Color.White);
                            MAutoResizeEditText.SetHintTextColor(Color.White);
                        }

                        if (!string.IsNullOrEmpty(item.ColorSecond))
                        {
                            int[] color = { Color.ParseColor(item.ColorFirst), Color.ParseColor(item.ColorSecond) };
                            var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, Width, Height);

                            NewColorView.GetSource()?.SetImageBitmap(bitmap);
                        }
                        else
                        {
                            int[] color = { Color.ParseColor(item.ColorFirst), Color.ParseColor(item.ColorFirst) };
                            var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, Width, Height);
                            NewColorView.GetSource()?.SetImageBitmap(bitmap);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Color Text
        private void ColorTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                MColorFragment.Show(SupportFragmentManager, MColorFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        SaveImage();
                    else
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                }
                else if (requestCode == 10)
                {
                    if (!Directory.Exists(Methods.Path.FolderDiskNiceArt))
                        Directory.CreateDirectory(Methods.Path.FolderDiskNiceArt);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Save

        public void SaveTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(MColorCode))
                    MColorCode = "#ffffff";

                if (SaveTextView.Tag?.ToString() == "Add")
                {
                    MAutoResizeEditText.Visibility = ViewStates.Invisible;
                    ColorTextView.Visibility = ViewStates.Invisible;

                    OnDone(MAutoResizeEditText.Text, MColorCode, ViewTextType.Add, FontTxtResult);

                    SaveTextView.Tag = "Save";
                    SaveTextView.Text = GetText(Resource.String.Lbl_Save);
                }
                else if (SaveTextView.Tag?.ToString() == "Save")
                {
                    SaveImage();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SaveImage()
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "... ");

                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            if (!Directory.Exists(Methods.Path.FolderDiskNiceArt))
                                Directory.CreateDirectory(Methods.Path.FolderDiskNiceArt);

                            var file = new File(Methods.Path.FolderDiskNiceArt + File.Separator + "" + BitmapUtil.GetTimestamp(DateTime.Now) +
                                                ".png");
                            try
                            {
                                file.CreateNewFile();

                                var saveSettings = new SaveSettings.Builder()
                                    .SetClearViewsEnabled(true)
                                    .SetTransparencyEnabled(true)
                                    .Build();

                                NewColorEditor.SaveAsFile(file.Path, saveSettings, this);
                            }
                            catch (FileNotFoundException e)
                            {
                                e.PrintStackTrace();
                            }
                            catch (IOException e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }
                        else
                        {
                            if (PermissionsController.CheckPermissionStorage())
                            {
                                if (!Directory.Exists(Methods.Path.FolderDiskNiceArt))
                                    Directory.CreateDirectory(Methods.Path.FolderDiskNiceArt);

                                var file = new File(Methods.Path.FolderDiskNiceArt + File.Separator + "" + BitmapUtil.GetTimestamp(DateTime.Now) + ".png");
                                try
                                {
                                    file.CreateNewFile();

                                    var saveSettings = new SaveSettings.Builder()
                                        .SetClearViewsEnabled(true)
                                        .SetTransparencyEnabled(true)
                                        .Build();

                                    NewColorEditor.SaveAsFile(file.Path, saveSettings, this);
                                }
                                catch (FileNotFoundException e)
                                {
                                    e.PrintStackTrace();
                                }
                                catch (IOException e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(100);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ShowSaveDialog()
        {
            try
            {

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(GetText(Resource.String.Lbl_Warning));
                dialog.Content(GetText(Resource.String.Lbl_Are_you_want_to_exit_without_saving_image));
                dialog.PositiveText(GetText(Resource.String.Lbl_Save)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        SaveImage();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialog.NeutralText(GetText(Resource.String.Lbl_Discard)).OnNeutral((materialDialog, action) =>
                {
                    try
                    {
                        var resultIntent = new Intent(); 
                        SetResult(Result.Canceled, resultIntent);
                        Finish();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSuccess(string imagePath, Bitmap savedResultBitmap)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        AndHUD.Shared.Dismiss(this);

                        //File pathOfFile = new File(imagePath);
                        //NewColorView.GetSource().SetImageURI(Uri.FromFile(new File(imagePath)));

                        ////Show image in Gallery
                        //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                        //mediaScanIntent.SetData(Uri.FromFile(pathOfFile));
                        //SendBroadcast(mediaScanIntent);

                        // Tell the media scanner about the new file so that it is
                        // immediately available to the user.
                        MediaScannerConnection.ScanFile(Application.Context, new string[] { imagePath }, null, null);

                        // put the String to pass back into an Intent and close this activity
                        var resultIntent = new Intent();
                        resultIntent.PutExtra("ImagePath", imagePath);
                        SetResult(Result.Ok, resultIntent);
                        Finish();
                        NewColorEditor.ClearAllViews();
                        NewColorView.GetSource().ClearColorFilter();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFailure(string exception)
        {
            try
            {
                //Show a Error image with a message
                AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_Failed_to_save_Image), MaskType.Clear, TimeSpan.FromSeconds(1));

                // put the String to pass back into an Intent and close this activity
                var resultIntent = new Intent();
                SetResult(Result.Canceled, resultIntent);
                Finish();
                AndHUD.Shared.Dismiss(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Close

        public void CloseImageViewOnClick(object sender, EventArgs e)
        {
            try
            {
                OnBackPressed();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnBackPressed()
        {
            try
            {
                if (!NewColorEditor.IsCacheEmpty() && !string.IsNullOrEmpty(MAutoResizeEditText.Text))
                    ShowSaveDialog();
                else
                {
                    base.OnBackPressed();
                    var resultIntent = new Intent();
                    SetResult(Result.Canceled, resultIntent);
                    Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Text

        public void OnDone(string inputText, string colorCode, ViewTextType changeText, Typeface textTypeface)
        {
            try
            {
                if (changeText == ViewTextType.Add)
                    NewColorEditor.AddText(textTypeface, inputText, colorCode);
                else
                    NewColorEditor.EditText(RootView, textTypeface, inputText, colorCode);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Change Text
        public void OnEditTextChangeListener(View rootView, string text, int colorCode)
        {
            try
            {
                RootView = rootView;
                var textEditorDialogFragment = new TextEditorFragment(/*null,*/ this);
                textEditorDialogFragment.Show(this, /*null,*/ this, text, colorCode, ViewTextType.Change);
                textEditorDialogFragment.SetOnTextEditorListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Type Face Text
        public void TypeFaceAdapterOnItemClick(object sender, FontTypeFaceAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = TypeFaceAdapter.GetItem(position);
                    if (item != null)
                    {
                        FontTxtResult = item;
                        MAutoResizeEditText.SetTypeface(item, TypefaceStyle.Normal);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}