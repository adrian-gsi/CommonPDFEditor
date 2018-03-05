using kahua.kdk.property;

namespace PDFEditorNS
{
    public class XMLSquiggly : BaseAnnotation
    {
        internal static class Names
        {
            public const string Squiggly = "SquigglyAnnotation";
        }

        #region Constructors
        public XMLSquiggly()
            : base(Names.Squiggly)
        {

        }

        public XMLSquiggly(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
