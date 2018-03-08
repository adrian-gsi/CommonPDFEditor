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

        private Dictionary<string,AnnotationsContainer> userAnnotations = new Dictionary<string, AnnotationsContainer>();
        private AnnotationsContainer _defaultAnnotations = new AnnotationsContainer("default");
        #endregion Class vars

        public AnnotationsMannager()
        {
            //For now....
            userAnnotations.Add("default",_defaultAnnotations);
        }

        #region Annotations Handling
        public AnnotationsContainer AnnotationCollection(string annotationCollectionName = "default")
        { 
            return userAnnotations[annotationCollectionName];
        }

        public bool HasUnsavedAnnotations { get => hasUnsavedAnnotations; set => hasUnsavedAnnotations = value; }

        public void AddAnnotation(BaseAnnotation item, string annotationCollectionName = "default")
        {
            userAnnotations[annotationCollectionName].Add(item);
            HasUnsavedAnnotations = true;
        }

        public bool RemoveAnnotation(Rect rect, int pageIndex, string annotationCollectionName = "default")
        {
            AnnotationsContainer current = userAnnotations[annotationCollectionName];

            foreach (BaseAnnotation b in current)
                if ((b.Page()==pageIndex) &&(b.RectArea().X1() == rect.x1) && (b.RectArea().Y1() == rect.y1) && (b.RectArea().X2() == rect.x2) && (b.RectArea().Y2() == rect.y2))
                {
                    current.Remove(b);
                    HasUnsavedAnnotations = true;
                    return true;
                }

            return false;
        }

        public void ClearAnnotations(string annotationCollectionName = "default")
        {
            userAnnotations[annotationCollectionName].Clear();
            HasUnsavedAnnotations = true;
        }

        #endregion Annotations Handling

        #region Propertied Serialization
        //Propertied Deserialize
        public void LoadAnnotationsFromXml(string loadedXml, string annotationCollectionName = "default")
        {
            AnnotationsContainer current = new AnnotationsContainer(annotationCollectionName);
            current.Definition = loadedXml;

            if (userAnnotations.ContainsKey(annotationCollectionName))
            {
                userAnnotations[annotationCollectionName] = current;
            }
            else
            {
                userAnnotations.Add(annotationCollectionName, current);
            }
            
            HasUnsavedAnnotations = false;
        }

        //Propertied Serialize
        public string GetAnnotationsXml(string annotationCollectionName = "default")
        {
            if (!userAnnotations.ContainsKey(annotationCollectionName))
                return "";

            HasUnsavedAnnotations = false;
            return userAnnotations[annotationCollectionName].Definition;
        }
        #endregion Propertied Serialization

        #region Tools
        public static ColorPt ConvertColor(double[] RGB)
        {
            return new ColorPt(RGB[0], RGB[1], RGB[2]);
        }
        public static double[] ConvertColor(ColorPt color)
        {
            return new double[] {color.Get(0), color.Get(1), color.Get(2)};
        }
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

        #region Factory
        public static BaseAnnotation CreateAnnotation(AnnotationOptions Name)
        {
            switch (Name)
            {
                case AnnotationOptions.HighlightAreaAnnotation:
                    return new XMLHighlightArea();
                case AnnotationOptions.HighlightTextAnnotation:
                    return new XMLHighlightText();
                case AnnotationOptions.StickyNoteAnnotation:
                    return new StickyNote();
                case AnnotationOptions.MarkAreaAnnotation:
                    return new MarkArea();
                case AnnotationOptions.FreeTextAnnotation:
                    return new FreeText();
                case AnnotationOptions.CircleAnnotation:
                    return new Circle();
                case AnnotationOptions.SquareAnnotation:
                    return new Square();
                case AnnotationOptions.LineAnnotation:
                    return new Line();
                case AnnotationOptions.StamperImageAnnotation:
                    return new StamperImage();
                case AnnotationOptions.StamperTextAnnotation:
                    return new StamperText();
                case AnnotationOptions.RubberStampAnnotation:
                    return new RubberStamp();
                case AnnotationOptions.SquigglyAnnotation:
                    return new XMLSquiggly();
                case AnnotationOptions.StrikeoutAnnotation:
                    return new XMLStrikeout();
                case AnnotationOptions.UnderlineAnnotation:
                    return new XMLUnderline();
                case AnnotationOptions.NONE:
                default:
                    return null;
            }
        }
        #endregion Factory
    }
}
