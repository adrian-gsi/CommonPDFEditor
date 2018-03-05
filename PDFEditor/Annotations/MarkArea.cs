using kahua.kdk.property;

namespace PDFEditorNS
{
    public class MarkArea : BaseAnnotation
    {
        internal static class Names
        {
            public const string MarkArea = "MarkAreaAnnotation";
        }

        #region Constructors
        public MarkArea()
            : base(Names.MarkArea)
        {

        }

        public MarkArea(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
