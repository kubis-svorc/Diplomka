using System.Windows;

namespace Diplomka
{
    public partial class HelpWindow : Window
    {
        public HelpWindow(bool darkTheme)
        {
            InitializeComponent();            
            System.Windows.Media.Brush text, bg;
            if (darkTheme)
            {
                text = System.Windows.Media.Brushes.White;
                bg = System.Windows.Media.Brushes.Black;
            }
            else
            {
                text = System.Windows.Media.Brushes.Black;
                bg = System.Windows.Media.Brushes.White;
            }

            HelpTab.Foreground = text;
            HelpTab.Background = bg;

            //HelpTab.Text = HELP_TEXT;
            HelpTab.Focus();

        }

        private void HelpTab_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Key.Escape == e.Key || 
                System.Windows.Input.Key.Enter == e.Key ||
                System.Windows.Input.Key.Space == e.Key)
            {
                Close();
            }

        }
    }
}
