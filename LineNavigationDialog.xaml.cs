using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Diplomka
{
    /// <summary>
    /// Interaction logic for LineNavigationDialog.xaml
    /// </summary>
    public partial class LineNavigationDialog : Window
    {
        public string Result => TextBox_LineNumber.Text;

        public LineNavigationDialog(bool darkTheme)
        {
            InitializeComponent();
            Brush text, bg;
            if (darkTheme)
            {
                text = Brushes.White;
                bg = Brushes.Black;
            }
            else
            {
                text = Brushes.Black;
                bg = Brushes.White;
            }
            GoToLineDialog.Foreground = text;
            GoToLineDialog.Background = bg;

            TextBox_LineNumber.Focus();
        }

        private void GoToLineDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Escape == e.Key) 
            {
                e.Handled = true;
                Close();
            }
            else if (Key.Enter == e.Key) 
            {
                e.Handled = true;
                DialogResult = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            e.Handled = true;
            Close();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            e.Handled = true;
        }
    }
}
