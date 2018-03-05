using kahua.kdk.property;

namespace PDFEditorNS
{
    public class XMLHighlight : BaseAnnotation
    {
        internal static class Names
        {
            public const string Highlight = "HighlightAnnotation";
        }

        #region Constructors
        public XMLHighlight()
            : base(Names.Highlight)
        {

        }

        public XMLHighlight(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors

        //[DataMember]
        //public double[] color { get; set; }
    }
}
