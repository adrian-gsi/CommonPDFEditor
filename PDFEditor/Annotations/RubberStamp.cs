using kahua.kdk.property;

namespace PDFEditorNS
{
    public class RubberStamp : BaseAnnotation
    {
        internal static class Names
        {
            public const string RubberStamp = "RubberStampAnnotation";
            public const string Rotation = "Rotation";
            public const string Opacity = "Opacity";
        }

        #region Constructors
        public RubberStamp()
            : base(Names.RubberStamp)
        {

        }

        public RubberStamp(PropertyCollection properties)
             : base(properties)
        {

        }
        #endregion Constructors

        public double Rotation() { return Properties.Double(Names.Rotation); }
        public RubberStamp Rotation(double rotation) { Properties.Double(Names.Rotation, rotation); return this; }

        public double Opacity() { return Properties.Double(Names.Opacity); }
        public RubberStamp Opacity(double opacity) { Properties.Double(Names.Opacity, opacity); return this; }
    }
}
