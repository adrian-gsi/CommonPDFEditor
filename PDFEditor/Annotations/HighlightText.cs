using kahua.kdk.property;

namespace PDFEditorNS
{
    public class XMLHighlightText : BaseAnnotation
    {
        internal static class Names
        {
            public const string Highlight = "HighlightTextAnnotation";
        }

        #region Constructors
        public XMLHighlightText()
            : base(Names.Highlight)
        {

        }

        public XMLHighlightText(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
