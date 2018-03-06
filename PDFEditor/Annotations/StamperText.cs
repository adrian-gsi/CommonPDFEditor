using kahua.kdk.property;

namespace PDFEditorNS
{
    public class StamperText : BaseAnnotation
    {
        internal static class Names
        {
            public const string Stamper = "StamperTextAnnotation";
            public const string Rotation = "Rotation";
            public const string Opacity = "Opacity";
            public const string Text = "Text";
        }

        #region Constructors
        public StamperText()
            : base(Names.Stamper)
        {

        }

        public StamperText(PropertyCollection properties)
             : base(properties)
        {

        }
        #endregion Constructors

        public double Rotation() { return Properties.Double(Names.Rotation); }
        public StamperText Rotation(double rotation) { Properties.Double(Names.Rotation, rotation); return this; }

        public double Opacity() { return Properties.Double(Names.Opacity); }
        public StamperText Opacity(double opacity) { Properties.Double(Names.Opacity, opacity); return this; }

        public string Text() { return Properties.String(Names.Text); }
        public StamperText Text(string text) { Properties.String(Names.Text, text); return this; }
    }
}