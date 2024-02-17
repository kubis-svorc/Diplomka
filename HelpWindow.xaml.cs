using System;
using System.Windows;
using System.IO;
using System.Text;

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

            try
            {
                string content = File.ReadAllText("Help.txt", Encoding.UTF8);
                HelpTab.Text = content;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Súbor s návodom nebol nájdený", "Chyba súboru", MessageBoxButton.OK);
            }
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
