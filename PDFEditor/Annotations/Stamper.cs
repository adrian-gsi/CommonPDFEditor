using kahua.kdk.property;

namespace PDFEditorNS
{
    public class Stamper : BaseAnnotation
    {
        internal static class Names
        {
            public const string Stamper = "StamperAnnotation";
            public const string Rotation = "Rotation";
            public const string Opacity = "Opacity";
            public const string IsImage = "IsImage";
            public const string ImagePath = "ImagePath";
            public const string Text = "Text";
        }

        #region Constructors
        public Stamper()
            : base(Names.Stamper)
        {

        }

        public Stamper(PropertyCollection properties)
             : base(properties)
        {

        }
        #endregion Constructors

        public double Rotation() { return Properties.Double(Names.Rotation); }
        public Stamper Rotation(double rotation) { Properties.Double(Names.Rotation, rotation); return this; }

        public double Opacity() { return Properties.Double(Names.Opacity); }
        public Stamper Opacity(double opacity) { Properties.Double(Names.Opacity, opacity); return this; }

        public bool IsImage() { return Properties.Bool(Names.IsImage); }
        public Stamper IsImage(bool isImage) { Properties.Bool(Names.IsImage, isImage); return this; }

        public string ImagePath() { return Properties.String(Names.ImagePath); }
        public Stamper ImagePath(string imagePath) { Properties.String(Names.ImagePath, imagePath); return this; }

        public string Text() { return Properties.String(Names.Text); }
        public Stamper Text(string text) { Properties.String(Names.Text, text); return this; }
    }
}