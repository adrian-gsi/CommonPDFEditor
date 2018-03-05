using pdftron.PDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace PDFEditorNS
{
    public class AnnotationsMannager
    {
        #region Class vars
        private bool hasUnsavedAnnotations = false;

        //public List<BaseAnnotation> userAnnotations = new List<BaseAnnotation>();
        private AnnotationsContainer _annotations = new AnnotationsContainer("Annotations");
        #endregion Class vars

        #region Annotations Handling
        public AnnotationsContainer AnnotationCollection { get { return _annotations; } }

        public bool HasUnsavedAnnotations { get => hasUnsavedAnnotations; set => hasUnsavedAnnotations = value; }

        public void AddAnnotation(BaseAnnotation item)
        {
            _annotations.Add(item);
            HasUnsavedAnnotations = true;
        }

        public bool RemoveAnnotation(Rect rect, int pageIndex)
        {
            foreach (BaseAnnotation b in _annotations)
                if ((b.Page()==pageIndex) &&(b.RectArea().X1() == rect.x1) && (b.RectArea().Y1() == rect.y1) && (b.RectArea().X2() == rect.x2) && (b.RectArea().Y2() == rect.y2))
                {
                    _annotations.Remove(b);
                    HasUnsavedAnnotations = true;
                    return true;
                }

            return false;
        }

        public void ClearAnnotations()
        {
            _annotations.Clear();
            HasUnsavedAnnotations = true;
        }

        #endregion Annotations Handling

        #region Propertied Serialization
        //Propertied Deserialize
        public void LoadAnnotationsFromXml(string loadedXml)
        {
            _annotations = new AnnotationsContainer("Annotations");
            _annotations.Definition = loadedXml;
            HasUnsavedAnnotations = false;
        }

        //Propertied Serialize
        public string GetAnnotationsXml()
        {
            HasUnsavedAnnotations = false;
            return _annotations.Definition;
        }
        #endregion Propertied Serialization

        #region Tools
        public static string getFileName(string filePath)
        {
            string[] filenamePathParts = filePath.Split('\\');
            return filenamePathParts[filenamePathParts.Count() - 1];
        }

        public static void ConvertScreenPositionsToPagePositions(PDFViewWPF viewer, int currentPageIndex, ref double x, ref double y)
        {
            viewer.ConvScreenPtToPagePt(ref x, ref y, currentPageIndex);
        }

        public static Rect NormalizeRect(Rect rect)
        {
            double x1 = Math.Min(rect.x1, rect.x2);
            double y1 = Math.Min(rect.y1, rect.y2);
            double x2 = Math.Max(rect.x1, rect.x2);
            double y2 = Math.Max(rect.y1, rect.y2);

            return new Rect(x1, y1, x2, y2);
        }

        public static AnnotationRect ConvertRect(Rect rect)
        {
            rect = NormalizeRect(rect);

            return new AnnotationRect()
                .X1(rect.x1)
                .X2(rect.x2)
                .Y1(rect.y1)
                .Y2(rect.y2);
        }

        public static Rect ConvertRect(AnnotationRect rect)
        {
            return new Rect(rect.X1(), rect.Y1(), rect.X2(), rect.Y2());
        }

        #endregion Tools

        #region Old Serialization
        /*
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType, new List<Type> { typeof(XMLHighlight), typeof(StickyNote) });
                return deserializer.ReadObject(stream);
            }
        }
        */
        #endregion Old Serialization
    }
}
