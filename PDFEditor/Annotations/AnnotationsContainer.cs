using kahua.kdk.property;
using System;

namespace PDFEditorNS
{
    public class AnnotationsContainer : PropertiedCollection<BaseAnnotation>
    {
        #region Constructors
        public AnnotationsContainer(string name)
            : base(name, createAnnotation)
        {

        }

        public AnnotationsContainer(PropertyCollection properties)
            : base(properties, createAnnotation)
        {

        }
        #endregion Constructors

        //Propertied Collection requirement
        static BaseAnnotation createAnnotation(PropertyCollection properties)
        {
            switch (properties.PropertyName)
            {
                case XMLHighlight.Names.Highlight:
                    return new XMLHighlight(properties);

                case StickyNote.Names.StickyNote:
                    return new StickyNote(properties);

                case MarkArea.Names.MarkArea:
                    return new MarkArea(properties);

                default:
                    throw new Exception($"Unknown assignment {properties.PropertyName}");
            }
        }
    }
}
