using kahua.kdk.property;

namespace PDFEditorNS
{
    public class XMLStrikeout : BaseAnnotation
    {
        internal static class Names
        {
            public const string Strikeout = "StrikeoutAnnotation";
        }

        #region Constructors
        public XMLStrikeout()
            : base(Names.Strikeout)
        {

        }

        public XMLStrikeout(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors
    }
}
