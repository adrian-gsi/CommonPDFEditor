using System;
using kahua.kdk.property;
using kahua.kdk.collection;
using kahua.kdk.resourcebuilder;
using System.Linq;

namespace PDFEditorNS
{
    //IBuild is Propertied requirement for handling property collection (RectArea)
    public class BaseAnnotation : Propertied, IBuild
    {
        static class Names
        {
            public const string Id = "Id";
            public const string RectArea = "RectArea";
            public const string Page = "Page";
        }

        #region Constructors
        public BaseAnnotation(string name)
            : base(name)
        {
            Guid(System.Guid.NewGuid());
        }

        public BaseAnnotation(PropertyCollection properties)
            : base(properties)
        {

        }
        #endregion Constructors

        #region RectArea
        private AnnotationRectCollection _rectArea;

        public BaseAnnotation RectArea(AnnotationRect map)
        {
            createRectArea();

            _rectArea.Add(map);

            return this;
        }

        // The Rect Collection should only contain one.
        public AnnotationRect RectArea() { return _rectArea?.FirstOrDefault(); }

        private void createRectArea()
        {
            if (_rectArea == null)
            {
                _rectArea = new AnnotationRectCollection(Properties.New(Names.RectArea));
            }
        }

        public BaseAnnotation RemoveRectArea(AnnotationRect bindingSource)
        {
            _rectArea.Remove(bindingSource);
            return this;
        }

        public IOrdered<AnnotationRect> RectAreas() { return _rectArea ?? OrderedHelper<AnnotationRect>.Empty; }
        #endregion RectArea

        #region Page
        public int Page() { return Properties.Int(Names.Page); }
        public BaseAnnotation Page(int page) { Properties.Int(Names.Page, page); return this; }
        #endregion Page

        #region Id
        public Guid Id() { return Properties.Guid(Names.Id); }
        private BaseAnnotation Guid(Guid id) { Properties.Guid(Names.Id, id); return this; }
        #endregion Id

        #region IBuild
        protected override void onPropertiesSet()
        {
            base.onPropertiesSet();

            var rect = Properties.Properties(Names.RectArea);
            if (rect != null)
            {
                _rectArea = new AnnotationRectCollection(rect);
            }
        }

        void IBuild.Prepare()
        {
            createRectArea();
        }
        #endregion IBuild
    }
}
