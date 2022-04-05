namespace WoWonder.Helpers.Model.Editor
{
    public interface IProperties
    {
        void OnColorChanged(string colorCode);

        void OnOpacityChanged(int opacity);

        void OnBrushSizeChanged(int brushSize);
    }
}