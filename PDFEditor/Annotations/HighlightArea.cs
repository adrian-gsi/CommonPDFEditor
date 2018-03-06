using kahua.kdk.property;

namespace PDFEditorNS
{
    public class XMLHighlightArea : BaseAnnotation
    {
        internal static class Names
        {
            public const string Highlight = "HighlightAreaAnnotation";
        }

        #region Constructors
        public XMLHighlightArea()
            : base(Names.Highlight)
        {

        }

        public XMLHighlightArea(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
