using pdftron.PDF;
using pdftron.PDF.Annots;
using System;

namespace PDFEditorNS
{
    public class PDFAnnotationsFactory
    {
        static PDFDoc _currentDoc;

        public static Annot CreatePDFAnnotation(BaseAnnotation annotation, PDFDoc pdfDocument, bool fromViewer)
        {
            _currentDoc = pdfDocument;
            Annot pdfAnnotation;

            switch (annotation.Properties.PropertyName)
            {
                case XMLHighlightText.Names.Highlight:
                case XMLHighlightArea.Names.Highlight:
                    pdfAnnotation = setHighlight(annotation);
                    break;
                case StickyNote.Names.StickyNote:
                    pdfAnnotation = setStickyNote((StickyNote)annotation, fromViewer);
                    break;
                case MarkArea.Names.MarkArea:
                    pdfAnnotation = setMarkArea((MarkArea)annotation);
                    break;
                case FreeText.Names.FreeText:
                    pdfAnnotation = setFreeText((FreeText)annotation, fromViewer);
                    break;
                case Circle.Names.Circle:
                    pdfAnnotation = setCircle((Circle)annotation);
                    break;
                case Square.Names.Square:
                    pdfAnnotation = setSquare((Square)annotation);
                    break;
                case Line.Names.Line:
                    pdfAnnotation = setLine((Line)annotation);
                    break;
                case StamperImage.Names.Stamper:
                    pdfAnnotation = setStamperImage((StamperImage)annotation);
                    break;
                case StamperText.Names.Stamper:
                    pdfAnnotation = setStamperText((StamperText)annotation);
                    break;
                case RubberStamp.Names.RubberStamp:
                    pdfAnnotation = setRubberStamp((RubberStamp)annotation);
                    break;
                case XMLSquiggly.Names.Squiggly:
                    pdfAnnotation = setSquiggly((XMLSquiggly)annotation);
                    break;
                case XMLStrikeout.Names.Strikeout:
                    pdfAnnotation = setStrikeout((XMLStrikeout)annotation);
                    break;
                case XMLUnderline.Names.Underline:
                    pdfAnnotation = setUnderline((XMLUnderline)annotation);
                    break;
                default:
                    pdfAnnotation = Annot.Create(null, Annot.Type.e_3D, null); // For Compiler. Should never execute this
                    break;
            }

            if (!(annotation is FreeText)) // Quick Fix, need to revamp it
                pdfAnnotation.SetColor(AnnotationsMannager.ConvertColor(new double[] { annotation.ColorRed(), annotation.ColorGreen(), annotation.ColorBlue() }), 3);
            
            _currentDoc.GetPage(annotation.Page()).AnnotPushBack(pdfAnnotation);
            return pdfAnnotation;
        }

        private static Underline setUnderline(XMLUnderline u)
        {
            return Underline.Create(_currentDoc, AnnotationsMannager.ConvertRect(u.RectArea()));
        }
        private static StrikeOut setStrikeout(XMLStrikeout so)
        {
            return StrikeOut.Create(_currentDoc, AnnotationsMannager.ConvertRect(so.RectArea()));
        }
        private static Ink setMarkArea(MarkArea ma)
        {
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(ma.RectArea());
            Ink ink = Ink.Create(_currentDoc, r);

            pdftron.PDF.Point pt3 = new pdftron.PDF.Point();
            #region Path Calculations
            //Bottom Path
            pt3.x = r.x1; pt3.y = r.y1;
            ink.SetPoint(0, 0, pt3);
            pt3.x = r.x1 + 10 * (r.x2 - r.x1) / 100; pt3.y = r.y1 + 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(0, 1, pt3);
            pt3.x = r.x1 + 20 * (r.x2 - r.x1) / 100; pt3.y = r.y1;
            ink.SetPoint(0, 2, pt3);
            pt3.x = r.x1 + 30 * (r.x2 - r.x1) / 100; pt3.y = r.y1 + 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(0, 3, pt3);
            pt3.x = r.x1 + 40 * (r.x2 - r.x1) / 100; pt3.y = r.y1;
            ink.SetPoint(0, 4, pt3);
            pt3.x = r.x1 + 50 * (r.x2 - r.x1) / 100; pt3.y = r.y1 + 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(0, 5, pt3);
            pt3.x = r.x1 + 60 * (r.x2 - r.x1) / 100; pt3.y = r.y1;
            ink.SetPoint(0, 6, pt3);
            pt3.x = r.x1 + 70 * (r.x2 - r.x1) / 100; pt3.y = r.y1 + 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(0, 7, pt3);
            pt3.x = r.x1 + 80 * (r.x2 - r.x1) / 100; pt3.y = r.y1;
            ink.SetPoint(0, 8, pt3);
            pt3.x = r.x1 + 90 * (r.x2 - r.x1) / 100; pt3.y = r.y1 + 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(0, 9, pt3);
            pt3.x = r.x2; pt3.y = r.y1;
            ink.SetPoint(0, 10, pt3);

            //Top Path
            pt3.x = r.x1; pt3.y = r.y2;
            ink.SetPoint(1, 0, pt3);
            pt3.x = r.x1 + 10 * (r.x2 - r.x1) / 100; pt3.y = r.y2 - 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(1, 1, pt3);
            pt3.x = r.x1 + 20 * (r.x2 - r.x1) / 100; pt3.y = r.y2;
            ink.SetPoint(1, 2, pt3);
            pt3.x = r.x1 + 30 * (r.x2 - r.x1) / 100; pt3.y = r.y2 - 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(1, 3, pt3);
            pt3.x = r.x1 + 40 * (r.x2 - r.x1) / 100; pt3.y = r.y2;
            ink.SetPoint(1, 4, pt3);
            pt3.x = r.x1 + 50 * (r.x2 - r.x1) / 100; pt3.y = r.y2 - 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(1, 5, pt3);
            pt3.x = r.x1 + 60 * (r.x2 - r.x1) / 100; pt3.y = r.y2;
            ink.SetPoint(1, 6, pt3);
            pt3.x = r.x1 + 70 * (r.x2 - r.x1) / 100; pt3.y = r.y2 - 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(1, 7, pt3);
            pt3.x = r.x1 + 80 * (r.x2 - r.x1) / 100; pt3.y = r.y2;
            ink.SetPoint(1, 8, pt3);
            pt3.x = r.x1 + 90 * (r.x2 - r.x1) / 100; pt3.y = r.y2 - 7 * (r.y2 - r.y1) / 100;
            ink.SetPoint(1, 9, pt3);
            pt3.x = r.x2; pt3.y = r.y2;
            ink.SetPoint(1, 10, pt3);

            //Left Path
            pt3.x = r.x1; pt3.y = r.y1;
            ink.SetPoint(2, 0, pt3);
            pt3.y = r.y1 + 10 * (r.y2 - r.y1) / 100; pt3.x = r.x1 + 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(2, 1, pt3);
            pt3.y = r.y1 + 20 * (r.y2 - r.y1) / 100; pt3.x = r.x1;
            ink.SetPoint(2, 2, pt3);
            pt3.y = r.y1 + 30 * (r.y2 - r.y1) / 100; pt3.x = r.x1 + 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(2, 3, pt3);
            pt3.y = r.y1 + 40 * (r.y2 - r.y1) / 100; pt3.x = r.x1;
            ink.SetPoint(2, 4, pt3);
            pt3.y = r.y1 + 50 * (r.y2 - r.y1) / 100; pt3.x = r.x1 + 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(2, 5, pt3);
            pt3.y = r.y1 + 60 * (r.y2 - r.y1) / 100; pt3.x = r.x1;
            ink.SetPoint(2, 6, pt3);
            pt3.y = r.y1 + 70 * (r.y2 - r.y1) / 100; pt3.x = r.x1 + 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(2, 7, pt3);
            pt3.y = r.y1 + 80 * (r.y2 - r.y1) / 100; pt3.x = r.x1;
            ink.SetPoint(2, 8, pt3);
            pt3.y = r.y1 + 90 * (r.y2 - r.y1) / 100; pt3.x = r.x1 + 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(2, 9, pt3);
            pt3.x = r.x1; pt3.y = r.y2;
            ink.SetPoint(2, 10, pt3);

            //Right Path
            pt3.x = r.x2; pt3.y = r.y1;
            ink.SetPoint(3, 0, pt3);
            pt3.y = r.y1 + 10 * (r.y2 - r.y1) / 100; pt3.x = r.x2 - 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(3, 1, pt3);
            pt3.y = r.y1 + 20 * (r.y2 - r.y1) / 100; pt3.x = r.x2;
            ink.SetPoint(3, 2, pt3);
            pt3.y = r.y1 + 30 * (r.y2 - r.y1) / 100; pt3.x = r.x2 - 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(3, 3, pt3);
            pt3.y = r.y1 + 40 * (r.y2 - r.y1) / 100; pt3.x = r.x2;
            ink.SetPoint(3, 4, pt3);
            pt3.y = r.y1 + 50 * (r.y2 - r.y1) / 100; pt3.x = r.x2 - 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(3, 5, pt3);
            pt3.y = r.y1 + 60 * (r.y2 - r.y1) / 100; pt3.x = r.x2;
            ink.SetPoint(3, 6, pt3);
            pt3.y = r.y1 + 70 * (r.y2 - r.y1) / 100; pt3.x = r.x2 - 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(3, 7, pt3);
            pt3.y = r.y1 + 80 * (r.y2 - r.y1) / 100; pt3.x = r.x2;
            ink.SetPoint(3, 8, pt3);
            pt3.y = r.y1 + 90 * (r.y2 - r.y1) / 100; pt3.x = r.x2 - 7 * (r.x2 - r.x1) / 100;
            ink.SetPoint(3, 9, pt3);
            pt3.x = r.x2; pt3.y = r.y2;
            ink.SetPoint(3, 10, pt3);
            #endregion Path Calculations
            
            return ink;
        }
        private static Highlight setHighlight(BaseAnnotation xhl) //I need it to work for both Area and Text Highlights
        {
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xhl.RectArea());
            Highlight hl = Highlight.Create(_currentDoc, r);
            return hl;
        }
        private static Squiggly setSquiggly(XMLSquiggly xsq)
        {
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xsq.RectArea());
            Squiggly sq = Squiggly.Create(_currentDoc, r);

            return sq;
        }
        private static Text setStickyNote(StickyNote sn, bool fromViewer)
        {
            Rect r = AnnotationsMannager.ConvertRect(sn.RectArea());
            Text t = Text.Create(_currentDoc, r);

            if (!fromViewer)
                t.SetContents(sn.Comment());

            return t;
        }
        private static pdftron.PDF.Annots.FreeText setFreeText(FreeText ft, bool fromViewer)
        {
            Rect r = AnnotationsMannager.ConvertRect(ft.RectArea());
            pdftron.PDF.Annots.FreeText freetext = pdftron.PDF.Annots.FreeText.Create(_currentDoc, r);
            freetext.SetFontSize(20);
            freetext.SetTextColor(AnnotationsMannager.ConvertColor(new double[] { ft.ColorRed(), ft.ColorGreen(), ft.ColorBlue() }), 3);

            if (!fromViewer)
                freetext.SetContents(ft.Text());

            return freetext;
        }
        private static pdftron.PDF.Annots.Circle setCircle(Circle c)
        {
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(c.RectArea());
            pdftron.PDF.Annots.Circle circle = pdftron.PDF.Annots.Circle.Create(_currentDoc, r);

            return circle;
        }
        private static pdftron.PDF.Annots.Square setSquare(Square sq)
        {
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(sq.RectArea());
            pdftron.PDF.Annots.Square s = pdftron.PDF.Annots.Square.Create(_currentDoc, r);

            return s;
        }
        private static pdftron.PDF.Annots.Line setLine(Line xl)
        {
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xl.RectArea());
            pdftron.PDF.Annots.Line l = pdftron.PDF.Annots.Line.Create(_currentDoc, r);
            l.SetStartPoint(new pdftron.PDF.Point(xl.XStart(), xl.YStart()));
            l.SetEndPoint(new pdftron.PDF.Point(xl.XEnd(), xl.YEnd()));

            return l;
        }
        private static pdftron.PDF.Annots.RubberStamp setRubberStamp(RubberStamp rubberStamp)
        {
            pdftron.PDF.Annots.RubberStamp stamp = pdftron.PDF.Annots.RubberStamp.Create(_currentDoc, AnnotationsMannager.ConvertRect(rubberStamp.RectArea()));
            stamp.SetIcon(pdftron.PDF.Annots.RubberStamp.Icon.e_Approved);//Enum of possible values for icon (text) 
            stamp.SetInteriorColor(AnnotationsMannager.ConvertColor(new double[] {rubberStamp.ColorRed(), rubberStamp.ColorGreen(), rubberStamp.ColorBlue() }), 3);
            stamp.SetOpacity(.4);

            return stamp;
        }
        private static Annot setStamperImage(StamperImage stp)
        {
            var page = _currentDoc.GetPage(stp.Page());
            using (pdftron.PDF.Stamper s = new pdftron.PDF.Stamper(pdftron.PDF.Stamper.SizeType.e_relative_scale, .5, .5))
            {
                s.SetAsAnnotation(true);
                var rect = AnnotationsMannager.ConvertRect(stp.RectArea());
                _currentDoc.InitSecurityHandler();
                //pdftron.PDF.Image img = pdftron.PDF.Image.Create(_currentDoc, String.IsNullOrEmpty(stp.ImagePath()) ? "SuccessStamp.jpg" : stp.ImagePath());
                pdftron.PDF.Image img = pdftron.PDF.Image.Create(_currentDoc, System.Convert.FromBase64String(stp.Image()));
                s.SetTextAlignment(pdftron.PDF.Stamper.TextAlignment.e_align_center);
                s.SetAlignment(pdftron.PDF.Stamper.HorizontalAlignment.e_horizontal_left, pdftron.PDF.Stamper.VerticalAlignment.e_vertical_bottom);
                s.SetSize(pdftron.PDF.Stamper.SizeType.e_absolute_size, rect.x2 - rect.x1, rect.y2 - rect.y1);
                s.SetPosition(rect.x1, rect.y1);
                s.SetAsBackground(false);
                s.SetOpacity(.3);
                s.StampImage(_currentDoc, img, new PageSet(stp.Page()));
            }
            var annot = page.GetAnnot(page.GetNumAnnots() - 1);
            stp.RectArea(AnnotationsMannager.ConvertRect(annot.GetRect()));

            return annot;
        }
        private static Annot setStamperText(StamperText stp)
        {
            var page = _currentDoc.GetPage(stp.Page());
            using (pdftron.PDF.Stamper s = new pdftron.PDF.Stamper(pdftron.PDF.Stamper.SizeType.e_relative_scale, .5, .5))
            {
                s.SetAsAnnotation(true);
                var rect = AnnotationsMannager.ConvertRect(stp.RectArea());
                _currentDoc.InitSecurityHandler();
                s.SetTextAlignment(pdftron.PDF.Stamper.TextAlignment.e_align_center);
                s.SetAlignment(pdftron.PDF.Stamper.HorizontalAlignment.e_horizontal_left, pdftron.PDF.Stamper.VerticalAlignment.e_vertical_bottom);
                s.SetFontColor(new ColorPt(stp.ColorRed(),stp.ColorGreen(),stp.ColorBlue()));
                s.SetSize(pdftron.PDF.Stamper.SizeType.e_absolute_size, rect.x2 - rect.x1, rect.y2 - rect.y1);
                s.SetPosition(rect.x1, rect.y1);
                s.SetAsBackground(false);
                s.SetOpacity(.3);
                s.StampText(_currentDoc, String.IsNullOrEmpty(stp.Text()) ? "Sample Text" : stp.Text(), new PageSet(stp.Page()));
            }
            var annot = page.GetAnnot(page.GetNumAnnots() - 1);
            stp.RectArea(AnnotationsMannager.ConvertRect(annot.GetRect()));

            return annot;
        }
    }
}
