using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using pdftron.PDF;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using pdftron.PDF.Annots;
using static pdftron.PDF.Annot;
using PDFEditor.Controls;
using System.Linq;
using System.Net.Http;

namespace PDFEditorNS
{
    public partial class PDFEditor : UserControl
    {
        #region Class vars
        private PDFViewWPF _viewer;
        private AnnotationOptions _activeOption = AnnotationOptions.NONE;
        private AnnotationsMannager _userAnnots = new AnnotationsMannager();
        private System.Windows.Point _lastDoubleClick;
        //private string currentLoadedAnnotsFileName = Environment.CurrentDirectory + "\\annotations.xml";
        private Window popupsOwner;
        public Window PopupsOwner { get => popupsOwner; set => popupsOwner = value; }
        #endregion Class vars

        #region Constructors
        public PDFEditor()
        {
            InitializeComponent();

            _viewer = new PDFViewWPF();
            PDFViewerBorder.Child = _viewer;

            _viewer.MouseDown += Viewer_MouseDown;
            _viewer.MouseUp += Viewer_MouseUp;

            _viewer.MouseDoubleClick += _viewer_MouseDoubleClick;
            _viewer.MouseMove += _viewer_MouseMove;

            CurrentSaveFile = "Save to " + Environment.CurrentDirectory + "\\annotations.xml";
        }

        
        #endregion Constructors

        #region Dependecy Properties

        // We need this to be called from the OnFilepathToLoadPropertyChanged static method
        public PDFViewWPF Viewer { get => _viewer; }

        #region CurrentDoc
        public static readonly DependencyProperty currentDocProperty = DependencyProperty.Register("CurrentDoc", typeof(string), typeof(PDFEditor), new PropertyMetadata(OnCurrentDocPropertyChanged));

        // Dependency Property for the PDF document
        public string CurrentDoc
        {
            get
            {
                return (string)GetValue(currentDocProperty);
            }
            set
            {
                SetValue(currentDocProperty, value);
                // More logic in OnCurrentDocPropertyChanged
            }
        }

        // This code is necessary for in the CurrentDoc property Setter, SetValue() is called directly - any extra code won't be executed when called by Binding
        private static void OnCurrentDocPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty((string)e.NewValue))
                return;

            PDFEditor editor = ((PDFEditor)source);
            var viewer = editor.Viewer;

            string doc = (string)e.NewValue;
            if (doc.Substring(0,5).ToLower().Equals("http:"))
            {
                viewer.OpenURLAsync(doc);
            }
            else
            {
                PDFDoc docToLoad = new PDFDoc(doc);
                viewer.SetDoc(docToLoad);
            }
            viewer.SetPagePresentationMode(PDFViewWPF.PagePresentationMode.e_single_continuous);
            //viewer.SetPageViewMode(PDFViewWPF.PageViewMode.e_fit_width);
            editor._userAnnots.ClearAnnotations();
            editor.tbCurrentPage.Text = "1";
            editor.Viewer.SetZoom(1);
            editor.UpdateZoomValueInUI();
        }
        #endregion CurrentDoc
        
        #region CurrentSaveFile
        public static readonly DependencyProperty currentSaveFileProperty = DependencyProperty.Register("CurrentSaveFile", typeof(string), typeof(PDFEditor));

        public string CurrentSaveFile
        {
            get
            {
                return (string)GetValue(currentSaveFileProperty);
            }
            set
            {
                SetValue(currentSaveFileProperty, value);
            }
        }

        #endregion CurrentSaveFile

        #endregion Dependecy Properties

        #region Annotations Handling
        private void createAnnotation(double x1,double y1,double x2,double y2, bool fromViewer = false)
        {
            PDFDoc currentDoc = _viewer.GetDoc();
            int currentPage = _viewer.CurrentPageNumber;

            //Need to convert coordinates
            if (!fromViewer)
            {
                AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x1,ref y1);
                AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x2,ref y2);
            }

            var rect = new pdftron.PDF.Rect(x1, y1, x2, y2);
            TextPopup popup;

            // Option selected in the Toolbar
            //****************************************************************refactorize with Factory???????????????
            switch (_activeOption)
            {
                case AnnotationOptions.HIGHLIGHTAREA:
                    BaseAnnotation userHL = new XMLHighlight()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(rect));
                    _userAnnots.AddAnnotation(userHL);
                    setHighlight((XMLHighlight)userHL);
                    break;
                //case AnnotationOptions.SQUIGGLY:
                //    BaseAnnotation userSQ = new XMLSquiggly()
                //        .Page(currentPage)
                //        .RectArea(AnnotationsMannager.ConvertRect(rect));
                //    _userAnnots.AddAnnotation(userSQ);
                //    setSquiggly((XMLSquiggly)userSQ);
                //    break;
                case AnnotationOptions.COMMENT:
                    StickyNote userSN = (StickyNote)(new StickyNote()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(rect)));

                    pdftron.PDF.Annots.Text txt = pdftron.PDF.Annots.Text.Create(currentDoc, rect, "");
                    popup = new TextPopup();
                    popup.Closed += (object closedSender, EventArgs eClosed) => { userSN.Comment(popup.Text); };
                    popup.Owner = this.PopupsOwner;
                    popup.Show();

                    txt.SetColor(new ColorPt(1, 0, 0));
                    txt.RefreshAppearance();

                    currentDoc.GetPage(currentPage).AnnotPushBack(txt);
                    _userAnnots.AddAnnotation(userSN);
                    break;
                case AnnotationOptions.MARKAREA:
                    BaseAnnotation userMA = new MarkArea()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(rect));
                    _userAnnots.AddAnnotation(userMA);
                    setMarkArea((MarkArea)userMA);
                    break;
                case AnnotationOptions.CIRCLE:
                    BaseAnnotation userCircle = new Circle()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userCircle);
                    setCircle((Circle)userCircle);
                    break;
                case AnnotationOptions.SQUARE:
                    BaseAnnotation userSquare = new Square()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userSquare);
                    setSquare((Square)userSquare);
                    break;
                case AnnotationOptions.LINE:
                    BaseAnnotation userLine = new Line()
                        .XStart(x1)
                        .YStart(y1)
                        .XEnd(x2)
                        .YEnd(y2)
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userLine);
                    setLine((Line)userLine);
                    break;
                case AnnotationOptions.FREETEXT:
                    FreeText userFT = (FreeText)(new FreeText()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(rect)));

                    pdftron.PDF.Annots.FreeText freetext = pdftron.PDF.Annots.FreeText.Create(currentDoc, rect);
                    freetext.SetTextColor(new ColorPt(0.7, 0, 0.7), 3);
                    freetext.SetFontSize(20);

                    popup = new TextPopup();
                    popup.Closed += (object closedSender, EventArgs eClosed) => { userFT.Text(popup.Text); freetext.SetContents(popup.Text); _viewer.Update(freetext, currentPage); };
                    popup.Owner = this.PopupsOwner;
                    popup.Show();

                    currentDoc.GetPage(currentPage).AnnotPushBack(freetext);
                    _viewer.Update(freetext, currentPage);
                    _userAnnots.AddAnnotation(userFT);
                    break;
                case AnnotationOptions.STAMPER:
                    BaseAnnotation userStamper = new Stamper()
                        .IsImage(true)
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userStamper);
                    setStamper((Stamper)userStamper);
                    break;
                case AnnotationOptions.STAMPERTEXT:
                    BaseAnnotation userStamperText = new Stamper()
                        .Text("TEXT MODE")
                        .IsImage(false)
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userStamperText);
                    setStamper((Stamper)userStamperText);
                    break;
                case AnnotationOptions.RUBBERSTAMP:
                    BaseAnnotation userRubberStamp = new RubberStamp()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userRubberStamp);
                    setRubberStamp((RubberStamp)userRubberStamp);
                    break;
                case AnnotationOptions.HIGHLIGHTTEXT:
                case AnnotationOptions.SQUIGGLY:
                case AnnotationOptions.STRIKEOUT:
                case AnnotationOptions.UNDERLINE:
                    setTextAnnotation(new pdftron.PDF.Rect(x1, y1, x2, y2),currentPage);
                    break;
                case AnnotationOptions.NONE:
                    break;
                default:
                    break;
            }

            _viewer.SetCurrentPage(currentPage);
            _viewer.Update();
        }

        private void setTextAnnotation(pdftron.PDF.Rect r,int currentPage)
        {
            _viewer.SetTextSelectionMode(PDFViewWPF.TextSelectionMode.e_structural);
            bool textSelected = _viewer.Select(r.x1,r.y1,currentPage,r.x2,r.y2,currentPage);

            if (!textSelected)
                return;

            double[] selectedTextBox = _viewer.GetSelection(currentPage).GetQuads();
            //Quads have the 8 coords, these are the coords to define a Rect
            pdftron.PDF.Rect textBoxRect = new pdftron.PDF.Rect(selectedTextBox[0], selectedTextBox[1], selectedTextBox[4], selectedTextBox[5]);

            PDFDoc currentDoc = _viewer.GetDoc();
            Annot textAnnot;

            BaseAnnotation b;
            switch (_activeOption)
            {
                case AnnotationOptions.HIGHLIGHTTEXT:
                    b = new XMLHighlight()
                        .RectArea(AnnotationsMannager.ConvertRect(r))
                        .Page(currentPage);
                    textAnnot = Highlight.Create(currentDoc, textBoxRect);
                    textAnnot.SetColor(new ColorPt(0.7,1,0.7,1),3);
                    break;
                case AnnotationOptions.SQUIGGLY:
                    b = new XMLSquiggly()
                        .RectArea(AnnotationsMannager.ConvertRect(r))
                        .Page(currentPage);
                    textAnnot = Squiggly.Create(currentDoc, textBoxRect);
                    textAnnot.SetColor(new ColorPt(1,0.3,0.4,0.2),3);
                    break;
                case AnnotationOptions.STRIKEOUT:
                    b = new XMLStrikeout()
                        .RectArea(AnnotationsMannager.ConvertRect(r))
                        .Page(currentPage);
                    textAnnot = StrikeOut.Create(currentDoc, textBoxRect);
                    textAnnot.SetColor(new ColorPt(1, 0.3, 0.4, 0.2), 3);
                    break;
                case AnnotationOptions.UNDERLINE:
                    b = new XMLUnderline()
                        .RectArea(AnnotationsMannager.ConvertRect(r))
                        .Page(currentPage);
                    textAnnot = Underline.Create(currentDoc, textBoxRect);
                    textAnnot.SetColor(new ColorPt(1, 0.3, 0.4, 0.2), 3);
                    break;
                default:
                    //For the compiler. Should never reach this line.
                    b = new BaseAnnotation("");
                    textAnnot = Annot.Create(currentDoc, Annot.Type.e_3D, r);
                    break;
            }
            _userAnnots.AddAnnotation(b);
            currentDoc.GetPage(currentPage).AnnotPushBack(textAnnot);
            _viewer.Update(textAnnot, currentPage);
        }

        private void setMarkArea(MarkArea ma)
        {
            PDFDoc currentDoc = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(ma.RectArea());
            Ink ink = Ink.Create(currentDoc.GetSDFDoc(), r);

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

            ink.SetColor(new ColorPt(0, 0.7, 0.7), 3);
            currentDoc.GetPage(ma.Page()).AnnotPushBack(ink);
            _viewer.Update(ink, ma.Page());
        }

        private void setHighlight(XMLHighlight xhl)
        {
            PDFDoc currentDoc = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xhl.RectArea());
            Highlight hl = Highlight.Create(currentDoc, r);
            //hl.SetQuadPoint(0, new QuadPoint(new pdftron.PDF.Point(xDown, yUp), new pdftron.PDF.Point(xUp, yUp), new pdftron.PDF.Point(xUp, yDown), new pdftron.PDF.Point(xDown, yDown)));
            hl.SetColor(new ColorPt(0.7, 1, 0.7, 1), 3);
            currentDoc.GetPage(xhl.Page()).AnnotPushBack(hl);
            _viewer.Update(hl,xhl.Page());
        }

        private void setSquiggly(XMLSquiggly xsq)
        {
            PDFDoc currentDoc = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xsq.RectArea());
            Squiggly sq = Squiggly.Create(currentDoc, r);
            sq.SetColor(new ColorPt(1, 0.3, 0.4, 0.2), 3);
            currentDoc.GetPage(xsq.Page()).AnnotPushBack(sq);
            _viewer.Update(sq, xsq.Page());
        }

        private void setStickyNote(StickyNote sn)
        {
            PDFDoc currentDoc = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(sn.RectArea());
            pdftron.PDF.Annots.Text t = pdftron.PDF.Annots.Text.Create(currentDoc.GetSDFDoc(), r);
            t.SetContents(sn.Comment());
            t.SetColor(new ColorPt(1, 0, 0));
            currentDoc.GetPage(sn.Page()).AnnotPushBack(t);
            _viewer.Update(t, sn.Page());
        }

        private void setFreeText(FreeText ft)
        {
            PDFDoc currentDoc = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(ft.RectArea());
            pdftron.PDF.Annots.FreeText freetext = pdftron.PDF.Annots.FreeText.Create(currentDoc, r);
            freetext.SetTextColor(new ColorPt(0.7, 0, 0.7), 3);
            freetext.SetFontSize(20);
            freetext.SetContents(ft.Text());
            currentDoc.GetPage(ft.Page()).AnnotPushBack(freetext);
            _viewer.Update(freetext, ft.Page());
        }

        private void setCircle(Circle c)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(c.RectArea());
            pdftron.PDF.Annots.Circle hl = pdftron.PDF.Annots.Circle.Create(temp.GetSDFDoc(), r);
            hl.SetColor(new ColorPt(0.7, 0, 0.7, 0), 3);
            temp.GetPage(c.Page()).AnnotPushBack(hl);
            _viewer.Update(hl, c.Page());
        }
        private void setSquare(Square sq)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(sq.RectArea());
            pdftron.PDF.Annots.Square hl = pdftron.PDF.Annots.Square.Create(temp.GetSDFDoc(), r);
            hl.SetColor(new ColorPt(0.7, .8, 0, 0), 3);
            temp.GetPage(sq.Page()).AnnotPushBack(hl);
            _viewer.Update(hl, sq.Page());
        }
        private void setLine(Line xl)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xl.RectArea());
            pdftron.PDF.Annots.Line hl = pdftron.PDF.Annots.Line.Create(temp.GetSDFDoc(), r); hl.SetStartPoint(new pdftron.PDF.Point(xl.XStart(), xl.YStart()));
            hl.SetEndPoint(new pdftron.PDF.Point(xl.XEnd(), xl.YEnd()));
            hl.SetColor(new ColorPt(0, 0, .7, .8), 3);
            temp.GetPage(xl.Page()).AnnotPushBack(hl);
            _viewer.Update(hl, xl.Page());
        }
        private void setRubberStamp(RubberStamp rubberStamp)
        {
            //RubberStamp
            var doc = _viewer.GetDoc();
            pdftron.PDF.Annots.RubberStamp stamp = pdftron.PDF.Annots.RubberStamp.Create(doc, AnnotationsMannager.ConvertRect(rubberStamp.RectArea()));
            stamp.SetIcon(pdftron.PDF.Annots.RubberStamp.Icon.e_Approved);//Enum of possible values for icon (text)            
            stamp.SetOpacity(.3);
            doc.GetPage(Viewer.GetCurrentPage()).AnnotPushBack(stamp);
            _viewer.Update(stamp, rubberStamp.Page());
        }
        private void setStamper(Stamper stp)
        {
            var doc = _viewer.GetDoc();
            var page = doc.GetPage(stp.Page());
            using (pdftron.PDF.Stamper s = new pdftron.PDF.Stamper(pdftron.PDF.Stamper.SizeType.e_relative_scale, .5, .5))
            {
                s.SetAsAnnotation(true);
                var rect = AnnotationsMannager.ConvertRect(stp.RectArea());
                doc.InitSecurityHandler();
                pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc, String.IsNullOrEmpty(stp.ImagePath()) ? "SuccessStamp.jpg" : stp.ImagePath());
                //s.SetSize(Stamper.SizeType.e_relative_scale, 0.5, 0.5);
                //s.SetAsAnnotation(true);
                s.SetTextAlignment(pdftron.PDF.Stamper.TextAlignment.e_align_center);
                s.SetAlignment(pdftron.PDF.Stamper.HorizontalAlignment.e_horizontal_left, pdftron.PDF.Stamper.VerticalAlignment.e_vertical_bottom);
                s.SetFontColor(new ColorPt(1, .4, .3, 0));
                s.SetSize(pdftron.PDF.Stamper.SizeType.e_absolute_size, rect.x2 - rect.x1, rect.y2 - rect.y1);
                s.SetPosition(rect.x1, rect.y1);
                s.SetAsBackground(false);
                s.SetOpacity(.3);
                if (stp.IsImage())
                    s.StampImage(doc, img, new PageSet(stp.Page()));
                else
                    s.StampText(doc, String.IsNullOrEmpty(stp.Text()) ? "Text undefined" : stp.Text(), new PageSet(stp.Page()));
            }
            var annot = page.GetAnnot(page.GetNumAnnots() - 1);
            stp.RectArea(AnnotationsMannager.ConvertRect(annot.GetRect()));
            _viewer.Update(annot, stp.Page());
        }

        private void deleteAllAnnotations()
        {
            this._userAnnots.ClearAnnotations();

            for (int i = 1; i <= _viewer.GetDoc().GetPageCount(); i++)
                for (int j = 0; j < _viewer.GetDoc().GetPage(i).GetNumAnnots(); j++)
                {
                    pdftron.PDF.Annot a = _viewer.GetDoc().GetPage(i).GetAnnot(j);
                    _viewer.GetDoc().GetPage(i).AnnotRemove(j);
                    _viewer.Update(a, i);
                }
        }

        #endregion Annotations Handling

        #region MouseEvents
        private bool DoubleClickOngoing;
        double? xSelecting, ySelecting;
        private void _viewer_MouseMove(object sender, MouseEventArgs e)
        {
            //For Selecting
            if (_activeOption != AnnotationOptions.NONE && xSelecting.HasValue && ySelecting.HasValue)
            {
                var pos = e.GetPosition(this.gContainer);
                //The addition at the end of each value is needed for recapturing mouseup event out the rectangle
                //because rectangle is not located in the same brach of the visual tree.
                previewRect.Margin = new Thickness(
                    Math.Min(xSelecting.Value, pos.X) + 1,
                    Math.Min(pos.Y, ySelecting.Value) + 1,
                    gContainer.ActualWidth - Math.Max(xSelecting.Value, pos.X) + 1,
                    gContainer.ActualHeight - Math.Max(ySelecting.Value, pos.Y) + 1);

                previewRect.Width = Math.Abs(gContainer.ActualWidth - (previewRect.Margin.Left + previewRect.Margin.Right));
                previewRect.Height = Math.Abs(gContainer.ActualHeight - (previewRect.Margin.Bottom + previewRect.Margin.Top));
            }
        }

        private void HidePreviewRect()
        {
            previewRect.Width = 0;
            previewRect.Height = 0;
            previewRect.Margin = new Thickness(gContainer.ActualWidth / 2, gContainer.ActualHeight / 2, gContainer.ActualWidth / 2, gContainer.ActualHeight / 2);
        }

        private void Viewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickOngoing)
            {
                DoubleClickOngoing = false;
                return;
            }

            //For Selecting
            if (_activeOption != AnnotationOptions.NONE)
            {
                HidePreviewRect();
                xSelecting = null;
                ySelecting = null;
            }

            if (xDown.HasValue && yDown.HasValue)
            {
                // No invalid MouseClick
                if ((xDown <= 0) && (yDown <= 0))
                    return;

                double xUp = e.GetPosition(_viewer).X;
                double yUp = e.GetPosition(_viewer).Y;

                // If it was a Click and not a Drag
                if ((xDown == xUp) && (yDown == yUp))
                    return;

                //Check for Annotation and Create it!
                createAnnotation(xDown.Value, yDown.Value, xUp, yUp);

                xDown = null;
                yDown = null;
            }
        }

        // Mouse Click
        double? xDown, yDown;
        private void Viewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickOngoing)
            {
                DoubleClickOngoing = false;
                return;
            }

            xDown = e.GetPosition(_viewer).X;
            yDown = e.GetPosition(_viewer).Y;

            //For Selecting
            if (_activeOption != AnnotationOptions.NONE)
            {
                xSelecting = e.GetPosition(this.gContainer).X;
                ySelecting = e.GetPosition(this.gContainer).Y;
                previewRect.Margin = new Thickness(xSelecting.Value + 1, ySelecting.Value + 1, gContainer.ActualWidth - xSelecting.Value + 1, gContainer.ActualHeight - yDown.Value + 1);
            }
        }

        private void _viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClickOngoing = true;
            xDown = null;
            yDown = null;
            this._lastDoubleClick = e.GetPosition(_viewer);

            //Need to PopUp?
            var page = _viewer.GetDoc().GetPage(_viewer.GetCurrentPage());
            if (page.GetNumAnnots() > 0)
            {
                Annot annot;
                for (int i = 0; i < page.GetNumAnnots(); i++)
                {
                    annot = page.GetAnnot(i);
                    var rect = annot.GetRect();
                    double relX = _lastDoubleClick.X;
                    double relY = _lastDoubleClick.Y;
                    AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, page.GetIndex(), ref relX, ref relY);
                    if (relX >= rect.x1 && relX <= rect.x2 && relY >= rect.y1 && relY <= rect.y2)
                    {
                        switch (annot.GetType())
                        {
                            case Annot.Type.e_RichMedia:
                                break;
                            case Annot.Type.e_Projection:
                                break;
                            case Annot.Type.e_Redact:
                                break;
                            case Annot.Type.e_3D:
                                break;
                            case Annot.Type.e_Unknown:
                                break;
                            case Annot.Type.e_Watermark:
                                break;
                            case Annot.Type.e_PrinterMark:
                                break;
                            case Annot.Type.e_Movie:
                                break;
                            case Annot.Type.e_Sound:
                                break;
                            case Annot.Type.e_FileAttachment:
                                break;
                            case Annot.Type.e_Squiggly:
                                break;
                            case Annot.Type.e_Underline:
                                break;
                            case Annot.Type.e_TrapNet:
                                break;
                            case Annot.Type.e_Screen:
                                break;
                            case Annot.Type.e_Widget:
                                break;
                            case Annot.Type.e_Popup:
                                break;
                            case Annot.Type.e_Ink:
                                break;
                            case Annot.Type.e_Caret:
                                break;
                            case Annot.Type.e_Stamp:
                                break;
                            case Annot.Type.e_StrikeOut:
                                break;
                            case Annot.Type.e_Highlight:
                                
                                //var border = new BorderStyle(BorderStyle.Style.e_solid, 3);  
                                
                                //annot.SetBorderStyle(border);
                                //page.AnnotRemove(i);
                                //page.AnnotPushFront(annot);
                                //_viewer.Update();
                                break;
                            case Annot.Type.e_Polyline:
                                break;
                            case Annot.Type.e_Polygon:
                                break;
                            case Annot.Type.e_Circle:
                                break;
                            case Annot.Type.e_Square:
                                break;
                            case Annot.Type.e_Line:
                                break;
                            case Annot.Type.e_FreeText:
                                break;
                            case Annot.Type.e_Link:
                                break;
                            case Annot.Type.e_Text:

                                var a = (StickyNote)_userAnnots.AnnotationCollection.FirstOrDefault(t =>
                                                                                        (t.Page() ==page.GetIndex()) 
                                                                                        && (relX >= t.RectArea().X1() 
                                                                                        && relX <= t.RectArea().X2() 
                                                                                        && relY >= t.RectArea().Y1() 
                                                                                        && relY <= t.RectArea().Y2()));

                                if (a == null)
                                    return;

                                TextPopup popup = new TextPopup();
                                popup.Text = a.Comment();
                                popup.Closed += (object closedSender, EventArgs eClosed) => { a.Comment(popup.Text); };

                                popup.Owner = this.PopupsOwner;
                                popup.Show();
                                break;
                            default:
                                break;
                        }
                        //page.AnnotRemove(i);
                        //_viewer.Update();
                        //_userAnnots.RemoveAnnotation(rect);
                    }
                }
            }
        }

        #endregion MouseEvents

        #region ToolbarEvents
        #region ActiveOption
        private void rbUnderline_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.UNDERLINE;
        }
        private void rbStrikeout_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.STRIKEOUT;
        }
        private void rbHighlightText_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.HIGHLIGHTTEXT;
        }
        private void rbFreeText_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.FREETEXT;
        }
        private void rbHighlight_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.HIGHLIGHTAREA;
        }
        private void rbSquiggly_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.SQUIGGLY;
        }
        private void rbNote_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.COMMENT;
        }
        private void rbStamper_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.STAMPER;
        }
        private void rbStamperText_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.STAMPERTEXT;
        }
        private void rbRubberStamp_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.RUBBERSTAMP;
        }
        private void rb_Unchecked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.NONE;
        }
        // Unselect Action
        private void radioButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((RadioButton)sender).IsChecked.GetValueOrDefault())
            {
                ((RadioButton)sender).IsChecked = false;
                e.Handled = true;
                _activeOption = AnnotationOptions.NONE;
            }
        }
        private void rbMarkArea_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.MARKAREA;
        }
        private void rbCircle_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.CIRCLE;
        }
        private void rbSquare_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.SQUARE;
        }
        private void rbLine_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.LINE;
        }
        #endregion ActiveOption
        #region ButtonsClick
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string xml = _userAnnots.GetAnnotationsXml();

            File.WriteAllText(CurrentSaveFile, xml);
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            var page = _viewer.GetDoc().GetPage(_viewer.GetCurrentPage());
            if (page.GetNumAnnots() > 0)
            {
                Annot annot;
                for (int i = 0; i < page.GetNumAnnots(); i++)
                {
                    annot = page.GetAnnot(i);
                    var rect = annot.GetRect();
                    double relX = _lastDoubleClick.X;
                    double relY = _lastDoubleClick.Y;
                    AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, page.GetIndex(), ref relX, ref relY);
                    if (relX >= rect.x1 && relX <= rect.x2 && relY >= rect.y1 && relY <= rect.y2)
                    {
                        page.AnnotRemove(i);
                        _viewer.Update(annot, page.GetIndex());
                        _userAnnots.RemoveAnnotation(rect, page.GetIndex());
                    }
                }
            }
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.CheckPathExists = true;
            saveDialog.Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*";
            saveDialog.DefaultExt = ".xml";
            
           if(saveDialog.ShowDialog() == true)
            {
                string xml = _userAnnots.GetAnnotationsXml();
                File.WriteAllText(saveDialog.FileName, xml);
                CurrentSaveFile = saveDialog.FileName;
            }
        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            setLoadedAnnotsFile();

            if (File.Exists(CurrentSaveFile))
            {
                // SWITCH for different Annotations
                _userAnnots.LoadAnnotationsFromXml(File.ReadAllText(CurrentSaveFile));

                foreach (BaseAnnotation a in _userAnnots.AnnotationCollection)
                {
                    if (a is XMLHighlight)
                        this.setHighlight((XMLHighlight)a);
                    else
                    if (a is XMLSquiggly)
                        this.setSquiggly((XMLSquiggly)a);
                    else
                    if (a is StickyNote)
                        this.setStickyNote((StickyNote)a);
                    else
                    if (a is MarkArea)
                        this.setMarkArea((MarkArea)a);
                    else
                    if (a is FreeText)
                        this.setFreeText((FreeText)a);
                    else
                    if (a is Circle)
                        this.setCircle((Circle)a);
                    else
                    if (a is Square)
                        this.setSquare((Square)a);
                    else
                    if (a is Line)
                        this.setLine((Line)a);
                    else
                    if (a is Stamper)
                        this.setStamper((Stamper)a);
                    else
                    if (a is RubberStamp)
                        this.setRubberStamp((RubberStamp)a);
                }
            }
        }

        private void setLoadedAnnotsFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            
            fileDialog.Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*";
            fileDialog.DefaultExt = ".xml";
            
            if (fileDialog.ShowDialog() == true)
            {
                deleteAllAnnotations();

                CurrentSaveFile = fileDialog.FileName;
            }
        }
        
        private void btZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _viewer.SetZoom(_viewer.GetZoom() + .25);

            //Garantizar siempre un zoom menor o igual a 20
            if (_viewer.GetZoom() > 10)
            {
                _viewer.SetZoom(10);
            }
            UpdateZoomValueInUI();
        }

        private void UpdateZoomValueInUI()
        {
            tbZoomValue.Text = Math.Floor(Viewer.GetZoom() * 100) + "%";
        }

        private void btZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _viewer.SetZoom(_viewer.GetZoom() - .25);
            //Garantizar un zoom siempre mayor que .5
            if (_viewer.GetZoom() < .25)
            {
                _viewer.SetZoom(.25);
            }
            UpdateZoomValueInUI();
        }

        private void fromWebApi_Click(object sender, RoutedEventArgs e)
        {
            string uri = "";
            uri = @"http://localhost/PDFWebApi/somefile";
            //uri = @"http://localhost:8627/somefile";
            
            _viewer.OpenURLAsync(uri);
        }
        #endregion ButtonsClick
        #region Paging
        private void btPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (tbCurrentPage.Text != "1")
            {
                var newPage = int.Parse(tbCurrentPage.Text) - 1;
                _viewer.SetCurrentPage(newPage);
                tbCurrentPage.Text = newPage.ToString();
            }
        }
        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            if (tbCurrentPage.Text != _viewer.GetPageCount().ToString())
            {
                var newPage = int.Parse(tbCurrentPage.Text) + 1;
                _viewer.SetCurrentPage(newPage);
                tbCurrentPage.Text = newPage.ToString();
            }
        }
        private void tbCurrentPage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");

            return !regex.IsMatch(text);
        }
        private void tbCurrentPage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewer != null && !String.IsNullOrEmpty(tbCurrentPage.Text))
            {
                var newPage = int.Parse(tbCurrentPage.Text);
                if (newPage >= 0 && newPage <= _viewer.GetPageCount())
                {
                    _viewer.SetCurrentPage(newPage);
                    tbCurrentPage.Text = newPage.ToString();
                }
                else
                    MessageBox.Show("Page number out of range");
                e.Handled = true;
            }
        }
        #endregion Paging
        #endregion ToolbarEvents
    }
}
