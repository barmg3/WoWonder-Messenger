using Android.Graphics;
using Android.Views;

namespace WoWonder.NiceArt.Models
{
    public static class INiceArt
    {
        public interface IBrushViewChangeListener
        {
            void OnViewAdd(BrushDrawingView brushDrawingView);

            void OnViewRemoved(BrushDrawingView brushDrawingView);

            void OnStartDrawing();

            void OnStopDrawing();
        }

        public interface IOnGestureControl
        {
            void OnClick();

            //void OnLongClick(ViewType type);
        }

        public interface IOnImageChangedListener
        {
            void OnBitmapLoaded(Bitmap sourceBitmap);
        }

        public interface IOnMultiTouchListener
        {
            void OnEditTextClickListener(string text, int colorCode);

            void OnRemoveViewListener(View removedView);
        }


        public interface IOnNiceArtEditorListener
        {
            /// <summary>
            /// When user long press the existing text this event will trigger implying that user want to
            /// edit the current {@link android.widget.TextView}
            /// </summary>
            /// <param name="rootView">view on which the long press occurs</param>
            /// <param name="text">current text set on the view</param>
            /// <param name="colorCode">current color value set on view</param>
            void OnEditTextChangeListener(View rootView, string text, int colorCode);

            /// <summary>
            /// This is a callback when user adds any view on the {@link NiceArtEditorView} it can be
            /// brush,text or sticker i.e bitmap on parent view
            /// </summary>
            /// <param name="viewType">enum which define type of view is added</param>
            /// <param name="numberOfAddedViews">number of views currently added</param>
            void OnAddViewListener(ViewType viewType, int numberOfAddedViews);

            /// <summary>
            /// This is a callback when user remove any view on the {@link NiceArtEditorView} it happens when usually
            /// undo and redo happens or text is removed
            /// </summary>
            /// <param name="numberOfAddedViews">number of views currently added</param>
            void OnRemoveViewListener(int numberOfAddedViews);

            /// <summary>
            /// This is a callback when user remove any view on the {@link NiceArtEditorView} it happens when usually
            /// </summary>undo and redo happens or text is removed
            /// 
            /// <param name="viewType">enum which define type of view is added</param>
            /// <param name="numberOfAddedViews">number of views currently added</param>
            void OnRemoveViewListener(ViewType viewType, int numberOfAddedViews);

            /// <summary>
            /// A callback when user start dragging a view which can be
            /// any of {@link ViewType}
            /// </summary>
            /// <param name="viewType">enum which define type of view is added</param>
            void OnStartViewChangeListener(ViewType viewType);


            /// <summary>
            /// A callback when user stop/up touching a view which can be
            /// any of {@link ViewType}
            /// </summary>
            /// <param name="viewType">enum which define type of view is added</param>
            void OnStopViewChangeListener(ViewType viewType);
        }

        public interface IOnSaveBitmap
        {
            void OnBitmapReady(Bitmap saveBitmap, SaveType type);

            void OnFailure(string e);
        }


        public interface IOnSaveListener
        {
            /// <summary>
            /// Call when edited image is saved successfully on given path
            /// </summary>
            /// <param name="imagePath">path on which image is saved</param>
            /// <param name="savedResultBitmap"></param>
            void OnSuccess(string imagePath, Bitmap savedResultBitmap);

            /// <summary>
            /// Call when failed to saved image on given path
            /// </summary>
            /// <param name="exception">exception thrown while saving image</param>
            void OnFailure(string exception);
        }

        public interface IOnScaleGestureListener
        {

            /// <summary>
            /// Responds to scaling events for a gesture in progress. Reported by pointer motion.
            /// </summary>
            /// <param name="view"></param>
            /// <param name="detector">The detector reporting the event - use this to retrieve extended info about event state.</param>
            /// <returns>Whether or not the detector should consider this event as handled. If an event was not handled, the detector
            /// will continue to accumulate movement until an event is handled. This can be useful if an application, for example,
            /// only wants to update scaling factors if the change is greater than 0.01.
            /// </returns>
            bool OnScale(View view, ScaleGestureDetector detector);

            /// <summary>
            /// Responds to the beginning of a scaling gesture. Reported by new pointers going down.
            /// </summary>
            /// <param name="view"></param>
            /// <param name="detector">The detector reporting the event - use this to retrieve extended info about event state.</param>
            /// <returns>Whether or not the detector should continue recognizing this gesture. For example, if a gesture is beginning
            /// with a focal point outside of a region where it makes sense, onScaleBegin() may return false to ignore the
            /// rest of the gesture. 
            /// </returns>
            bool OnScaleBegin(View view, ScaleGestureDetector detector);

            /// <summary>
            /// Responds to the end of a scale gesture. Reported by existing pointers going up.
            /// 
            /// Once a scale has ended, {@link ScaleGestureDetector#getFocusX()}
            /// and {@link ScaleGestureDetector#getFocusY()} will return the location
            /// of the pointer remaining on the screen.
            /// </summary>
            /// <param name="view"></param>
            /// <param name="detector">The detector reporting the event - use this to retrieve extended info about event state.</param>
            void OnScaleEnd(View view, ScaleGestureDetector detector);
        }
    }
}