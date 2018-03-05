using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDFEditor.Controls
{
    /// <summary>
    /// Interaction logic for TextPopup.xaml
    /// </summary>
    public partial class TextPopup : Window
    {
        public TextPopup()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string Text { get => tbPopupContent.Text; set => tbPopupContent.Text = value; }

        private void Window_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        
    }
}
