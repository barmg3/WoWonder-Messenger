namespace WoWonder.NiceArt
{
    public class SaveSettings
    {
        public bool MisTransparencyEnabled;
        public bool MisClearViewsEnabled;

        public bool IsTransparencyEnabled()
        {
            return MisTransparencyEnabled;
        }

        public bool IsClearViewsEnabled()
        {
            return MisClearViewsEnabled;
        }

        public SaveSettings(Builder builder)
        {
            MisClearViewsEnabled = builder.MisClearViewsEnabled;
            MisTransparencyEnabled = builder.MisTransparencyEnabled;
        }

        public class Builder
        {
            public bool MisTransparencyEnabled = true;
            public bool MisClearViewsEnabled = true;

            /// <summary>
            /// Define a flag to enable transparency while saving image
            /// </summary>
            /// <param name="transparencyEnabled"> true if enabled</param>
            /// <returns>Builder</returns>
            public Builder SetTransparencyEnabled(bool transparencyEnabled)
            {
                MisTransparencyEnabled = transparencyEnabled;
                return this;
            }

            /// <summary>
            /// Define a flag to clear the view after saving the image
            /// </summary>
            /// <param name="clearViewsEnabled">true if you want to clear all the views on {@link NiceArtEditorView}</param>
            /// <returns>Builder</returns>
            public Builder SetClearViewsEnabled(bool clearViewsEnabled)
            {
                MisClearViewsEnabled = clearViewsEnabled;
                return this;
            }

            public SaveSettings Build()
            {
                return new SaveSettings(this);
            }
        }
    }
}