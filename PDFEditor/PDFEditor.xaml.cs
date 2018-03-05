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

        // We need this to be called from the OnCurrentDocPropertyChanged static method
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
            PDFEditor editor = ((PDFEditor)source);

            if (!String.IsNullOrEmpty((string)e.NewValue))
            {
                var viewer = editor.Viewer;
                viewer.SetPagePresentationMode(PDFViewWPF.PagePresentationMode.e_single_continuous);
                viewer.SetPageViewMode(PDFViewWPF.PageViewMode.e_fit_width);

                PDFDoc docToLoad = new PDFDoc((string)e.NewValue);
                viewer.SetDoc(docToLoad);
                editor._userAnnots.ClearAnnotations();

                editor.tbCurrentPage.Text = "1";
            }
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
                // More logic in OnCurrentSaveFilePropertyChanged
            }
        }

        #endregion CurrentSaveFile

        #endregion Dependecy Properties

        #region Annotations Handling
        private void createAnnotation(double x1,double y1,double x2,double y2, bool fromViewer = false)
        {
            PDFDoc temp = _viewer.GetDoc();
            int currentPage = _viewer.CurrentPageNumber;

            //Need to convert coordinates
            if (!fromViewer)
            {
                AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x1,ref y1);
                AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x2,ref y2);
            }

            // Option selected in the Toolbar
            switch (_activeOption)
            {
                case AnnotationOptions.HIGHLIGHT:
                    BaseAnnotation userHL = new XMLHighlight()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userHL);
                    setHighlight((XMLHighlight)userHL);
                    break;
                case AnnotationOptions.COMMENT:
                    var rect = new pdftron.PDF.Rect(x1, y1, x2, y2);
                    rect = AnnotationsMannager.NormalizeRect(rect);
                    pdftron.PDF.Annots.Text txt = pdftron.PDF.Annots.Text.Create(temp, rect, "");

                    StickyNote sn = (StickyNote)(new StickyNote()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(rect)));

                    TextPopup popup = new TextPopup();
                    popup.Text = txt.GetContents();
                    popup.Closed += (object closedSender, EventArgs eClosed) => { txt.SetContents(popup.Text); _viewer.Update(); sn.Comment(popup.Text); };

                    popup.Owner = this.PopupsOwner;
                    popup.Show();

                    txt.SetColor(new ColorPt(1, 0, 0));
                    txt.RefreshAppearance();

                    temp.GetPage(currentPage).AnnotPushBack(txt);

                    _userAnnots.AddAnnotation(sn);
                    break;
                case AnnotationOptions.MARKAREA:
                    BaseAnnotation userMA = new MarkArea()
                        .Page(currentPage)
                        .RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));
                    _userAnnots.AddAnnotation(userMA);
                    setMarkArea((MarkArea)userMA);
                    break;
                case AnnotationOptions.NONE:
                    break;
                default:
                    break;
            }
            
            _viewer.SetCurrentPage(currentPage);
            _viewer.Update();
        }
        
        private void setMarkArea(MarkArea ma)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(ma.RectArea());
            Ink ink = Ink.Create(temp.GetSDFDoc(), r);
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
            temp.GetPage(ma.Page()).AnnotPushBack(ink);
            _viewer.Update(ink, ma.Page());
        }

        private void setHighlight(XMLHighlight xhl)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(xhl.RectArea());
            Highlight hl = Highlight.Create(temp.GetSDFDoc(), r);
            //hl.SetQuadPoint(0, new QuadPoint(new pdftron.PDF.Point(xDown, yUp), new pdftron.PDF.Point(xUp, yUp), new pdftron.PDF.Point(xUp, yDown), new pdftron.PDF.Point(xDown, yDown)));
            hl.SetColor(new ColorPt(0.7, 1, 0.7, 1), 3);
            temp.GetPage(xhl.Page()).AnnotPushBack(hl);
            _viewer.Update(hl,xhl.Page());
        }

        private void setStickyNote(StickyNote sn)
        {
            PDFDoc temp = _viewer.GetDoc();
            pdftron.PDF.Rect r = AnnotationsMannager.ConvertRect(sn.RectArea());
            pdftron.PDF.Annots.Text t = pdftron.PDF.Annots.Text.Create(temp.GetSDFDoc(), r);
            t.SetContents(sn.Comment());
            t.SetColor(new ColorPt(1, 0, 0));
            temp.GetPage(sn.Page()).AnnotPushBack(t);
            _viewer.Update(t, sn.Page());
        }

        #endregion Annotations Handling

        #region MouseEvents

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
                                popup.Text = annot.GetContents();
                                popup.Closed += (object closedSender, EventArgs eClosed) => { annot.SetContents(popup.Text); _viewer.Update(); a.Comment(popup.Text); };

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
        private void rbHighlight_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.HIGHLIGHT;
        }
        private void rbNote_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.COMMENT;
        }
        private void rbHighlight_Unchecked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.NONE;
        }
        private void rbNote_Unchecked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.NONE;
        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string xml = _userAnnots.GetAnnotationsXml();

            File.WriteAllText(CurrentSaveFile, xml);
        }
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
                    // Aqui hay que hacer un Factory ??
                    if (a is XMLHighlight)
                        this.setHighlight((XMLHighlight)a);

                    if (a is StickyNote)
                        this.setStickyNote((StickyNote)a);

                    if (a is MarkArea)
                        this.setMarkArea((MarkArea)a);
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

        private void rbMarkArea_Unchecked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.NONE;
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

        #endregion ToolbarEvents
    }
}
