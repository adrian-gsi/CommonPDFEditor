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
                case XMLHighlightText.Names.Highlight:
                    return new XMLHighlightText(properties);

                case XMLHighlightArea.Names.Highlight:
                    return new XMLHighlightArea(properties);

                case StickyNote.Names.StickyNote:
                    return new StickyNote(properties);

                case MarkArea.Names.MarkArea:
                    return new MarkArea(properties);

                case FreeText.Names.FreeText:
                    return new FreeText(properties);

                case Circle.Names.Circle:
                    return new Circle(properties);

                case Square.Names.Square:
                    return new Square(properties);

                case Line.Names.Line:
                    return new Line(properties);

                case StamperImage.Names.Stamper:
                    return new StamperImage(properties);

                case StamperText.Names.Stamper:
                    return new StamperText(properties);

                case RubberStamp.Names.RubberStamp:
                    return new RubberStamp(properties);

                case XMLSquiggly.Names.Squiggly:
                    return new XMLSquiggly(properties);

                case XMLStrikeout.Names.Strikeout:
                    return new XMLStrikeout(properties);

                case XMLUnderline.Names.Underline:
                    return new XMLUnderline(properties);

                default:
                    throw new Exception($"Unknown assignment {properties.PropertyName}");
            }
        }
    }
}
