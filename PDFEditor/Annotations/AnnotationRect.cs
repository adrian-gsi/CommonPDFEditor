using kahua.kdk.property;

namespace PDFEditorNS
{
    // al usar Propertied , la clase Rect the PDFTron no se puede serializar con propertied ya que Rect no hereda de Propertied
    public class AnnotationRect : Propertied
    {
        internal static class Names
        {
            public const string AnnotationRect = "AnnotationRect";
            public const string X1 = "X1";
            public const string Y1 = "Y1";
            public const string X2 = "X2";
            public const string Y2 = "Y2";
        }

        #region Constructors
        public AnnotationRect()
           : base(Names.AnnotationRect)
        {

        }

        public AnnotationRect(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors

        #region Rect Coords
        public double X1() { return Properties.Double(Names.X1); }
        public AnnotationRect X1(double x1) { Properties.Double(Names.X1, x1); return this; }

        public double Y1() { return Properties.Double(Names.Y1); }
        public AnnotationRect Y1(double y1) { Properties.Double(Names.Y1, y1); return this; }

        public double X2() { return Properties.Double(Names.X2); }
        public AnnotationRect X2(double x2) { Properties.Double(Names.X2, x2); return this; }

        public double Y2() { return Properties.Double(Names.Y2); }
        public AnnotationRect Y2(double y2) { Properties.Double(Names.Y2, y2); return this; }
        #endregion Rect Coords
    }
}
