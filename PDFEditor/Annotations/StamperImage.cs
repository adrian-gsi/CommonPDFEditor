using kahua.kdk.property;

namespace PDFEditorNS
{
    public class StamperImage : BaseAnnotation
    {
        internal static class Names
        {
            public const string Stamper = "StamperImageAnnotation";
            public const string Rotation = "Rotation";
            public const string Opacity = "Opacity";
            public const string ImagePath = "ImagePath";
        }

        #region Constructors
        public StamperImage()
            : base(Names.Stamper)
        {

        }

        public StamperImage(PropertyCollection properties)
             : base(properties)
        {

        }
        #endregion Constructors

        public double Rotation() { return Properties.Double(Names.Rotation); }
        public StamperImage Rotation(double rotation) { Properties.Double(Names.Rotation, rotation); return this; }

        public double Opacity() { return Properties.Double(Names.Opacity); }
        public StamperImage Opacity(double opacity) { Properties.Double(Names.Opacity, opacity); return this; }

        public string ImagePath() { return Properties.String(Names.ImagePath); }
        public StamperImage ImagePath(string imagePath) { Properties.String(Names.ImagePath, imagePath); return this; }
    }
}