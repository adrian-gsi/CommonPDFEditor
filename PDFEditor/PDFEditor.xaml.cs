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
        private void createAnnotation(double x1,double y1,double x2,double y2)
        {
            if (_activeOption == AnnotationOptions.NONE) //This shouldn't happen
                return;

            PDFDoc currentDoc = _viewer.GetDoc();
            int currentPage = _viewer.CurrentPageNumber;

            //Need to convert coordinates
            AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x1,ref y1);
            AnnotationsMannager.ConvertScreenPositionsToPagePositions(_viewer, currentPage, ref x2,ref y2);

            var currentColor = AnnotationsMannager.ConvertColor(getUserSelectedColor());

            BaseAnnotation inputAnnotation = AnnotationsMannager.CreateAnnotation(_activeOption)
                .Page(currentPage)
                .ColorRed(currentColor[0])
                .ColorGreen(currentColor[1])
                .ColorBlue(currentColor[2]);

            // RectArea does not overrides, but adds as another element of Rect Collection - Need to check this.
            if (_activeOption != AnnotationOptions.HighlightTextAnnotation
                && _activeOption != AnnotationOptions.SquigglyAnnotation
                && _activeOption != AnnotationOptions.StrikeoutAnnotation
                && _activeOption != AnnotationOptions.UnderlineAnnotation)
                inputAnnotation.RectArea(AnnotationsMannager.ConvertRect(new pdftron.PDF.Rect(x1, y1, x2, y2)));

            //Line requires special coords
            if (_activeOption == AnnotationOptions.LineAnnotation)
            {
                ((Line)inputAnnotation)
                    .XStart(x1)
                    .YStart(y1)
                    .XEnd(x2)
                    .YEnd(y2);
            }

            // If it is a Text Annotation I need to select only the text. i.e. reshape the rectangle
            // Future refactor: TextAnnotation:BaseAnnotation
            if (   _activeOption == AnnotationOptions.HighlightTextAnnotation
                || _activeOption == AnnotationOptions.SquigglyAnnotation
                || _activeOption == AnnotationOptions.StrikeoutAnnotation
                || _activeOption == AnnotationOptions.UnderlineAnnotation)
            {
                _viewer.SetTextSelectionMode(PDFViewWPF.TextSelectionMode.e_structural);
                bool textSelected = _viewer.Select(x1, y1, inputAnnotation.Page(), x2, y2, inputAnnotation.Page());

                if (!textSelected)
                    return;

                double[] selectedTextBox = _viewer.GetSelection(inputAnnotation.Page()).GetQuads();
                //Quads have the 8 coords, these are the coords to define a Rect
                pdftron.PDF.Rect textBoxRect = new pdftron.PDF.Rect(selectedTextBox[0], selectedTextBox[1], selectedTextBox[4], selectedTextBox[5]);
                inputAnnotation.RectArea(AnnotationsMannager.ConvertRect(textBoxRect));
            }

            _userAnnots.AddAnnotation(inputAnnotation);
            setPDFAnnotation(inputAnnotation);

            _viewer.SetCurrentPage(currentPage);
        }

        private void setPDFAnnotation(BaseAnnotation inputAnnotation, bool fromViewer = true)
        {
            PDFDoc currentDoc = _viewer.GetDoc();

            Annot pdfAnnot = PDFAnnotationsFactory.CreatePDFAnnotation(inputAnnotation, currentDoc, fromViewer);
            
            //If the annotation requires PopUp I need to handle it
            // Future refactor: Comment(popup-requiring)Annotation:BaseAnnotation
            if (fromViewer && (_activeOption == AnnotationOptions.StickyNoteAnnotation
                            || _activeOption == AnnotationOptions.FreeTextAnnotation
                            /*|| _activeOption == AnnotationOptions.StamperTextAnnotation*/)) //Need to check this
            {
                TextPopup popup = new TextPopup();

                if (_activeOption == AnnotationOptions.StickyNoteAnnotation)
                    popup.Closed += (object closedSender, EventArgs eClosed) => { ((StickyNote)inputAnnotation).Comment(popup.Text); };

                if (_activeOption == AnnotationOptions.FreeTextAnnotation)
                    popup.Closed += (object closedSender, EventArgs eClosed) => { ((FreeText)inputAnnotation).Text(popup.Text); pdfAnnot.SetContents(popup.Text); _viewer.Update(pdfAnnot, inputAnnotation.Page()); };

                if (_activeOption == AnnotationOptions.StamperTextAnnotation)
                    popup.Closed += (object closedSender, EventArgs eClosed) => { ((StamperText)inputAnnotation).Text(popup.Text); };

                popup.Owner = this.PopupsOwner;
                popup.Show();
            }

            currentDoc.GetPage(inputAnnotation.Page()).AnnotPushBack(pdfAnnot);
            _viewer.Update(pdfAnnot, inputAnnotation.Page());
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
            _activeOption = AnnotationOptions.UnderlineAnnotation;
        }
        private void rbStrikeout_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.StrikeoutAnnotation;
        }
        private void rbHighlightText_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.HighlightTextAnnotation;
        }
        private void rbFreeText_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.FreeTextAnnotation;
        }
        private void rbHighlight_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.HighlightAreaAnnotation;
        }
        private void rbSquiggly_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.SquigglyAnnotation;
        }
        private void rbNote_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.StickyNoteAnnotation;
        }
        private void rbStamper_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.StamperImageAnnotation;
        }
        private void rbStamperText_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.StamperTextAnnotation;
        }
        private void rbRubberStamp_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.RubberStampAnnotation;
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
            _activeOption = AnnotationOptions.MarkAreaAnnotation;
        }
        private void rbCircle_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.CircleAnnotation;
        }
        private void rbSquare_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.SquareAnnotation;
        }
        private void rbLine_Checked(object sender, RoutedEventArgs e)
        {
            _activeOption = AnnotationOptions.LineAnnotation;
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
                    setPDFAnnotation(a, false);
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
        #region Color
        private ColorPt getUserSelectedColor()
        {
            var color = this.cpActiveColor.SelectedColor;
            //color.Value.
            return new ColorPt(color.Value.ScR, color.Value.ScG, color.Value.ScB, color.Value.ScA);
        }
        #endregion Color
        #endregion ToolbarEvents
    }
}
