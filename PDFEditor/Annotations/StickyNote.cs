using kahua.kdk.property;

namespace PDFEditorNS
{
    public class StickyNote : BaseAnnotation
    {
        internal static class Names
        {
            public const string StickyNote = "StickyNoteAnnotation";  // Notice when serializing it will use this name
            public const string Comment = "Comment";
        }

        #region Constructors
        public StickyNote()
            : base(Names.StickyNote)
        {

        }

        public StickyNote(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors

        #region Comment
        public string Comment() { return Properties.String(Names.Comment); }
        public BaseAnnotation Comment(string comment) { Properties.String(Names.Comment, comment); return this; }
        #endregion Comment
    }
}
