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
            public const string Image = "Image";
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

        public string Image() { return Properties.String(Names.Image); }
        public StamperImage Image(string image) { Properties.String(Names.Image, image); return this; }
    }
}