using kahua.kdk.property;

namespace PDFEditorNS
{
    // In Propertied, complex types need to be included in Collections
    public class AnnotationRectCollection : PropertiedCollection<AnnotationRect>
    {
        #region Constructors
        public AnnotationRectCollection(string name)
        : base(name, createField)
        {


        }

        public AnnotationRectCollection(PropertyCollection properties)
            : base(properties, createField)
        {


        }
        #endregion Constructors

        //Propertied Collection requirement
        static AnnotationRect createField(PropertyCollection properties)
        {
            return new AnnotationRect(properties);
        }
    }
}
