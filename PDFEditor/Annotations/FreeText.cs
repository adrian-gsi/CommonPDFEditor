using kahua.kdk.property;

namespace PDFEditorNS
{
    public class FreeText: BaseAnnotation
    {
        internal static class Names
        {
            public const string FreeText = "FreeTextAnnotation";
            public const string Text = "Text";
        }
        #region Constructors
        public FreeText()
            : base(Names.FreeText)
        {

        }

        public FreeText(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors

        #region Text
        public string Text() { return Properties.String(Names.Text); }
        public BaseAnnotation Text(string text) { Properties.String(Names.Text, text); return this; }
        #endregion Text
    }
}
