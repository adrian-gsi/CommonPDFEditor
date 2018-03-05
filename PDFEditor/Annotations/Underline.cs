using kahua.kdk.property;

namespace PDFEditorNS
{
    public class XMLUnderline : BaseAnnotation
    {
        internal static class Names
        {
            public const string Underline = "UnderlineAnnotation";
        }

        #region Constructors
        public XMLUnderline()
            : base(Names.Underline)
        {

        }

        public XMLUnderline(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}