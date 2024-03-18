using System.Windows;

namespace Diplomka
{
    public partial class HelpWindow : Window
    {
        private const string HELP_TEXT = @"
Ovládanie aplikácie je spravené, aby využívala čo najviac štandarných skratiek z prostredia Windows:
                
Otvorenie súboru - CTRL + N
Uloženie súboru - CTRL + S
Uložiť súbor ako - CTRL + SHIFT + S                
Ukonečnie aplikácie alebo zatvorenie okna - ALT + F4

Prepnutie na zoznam s ponukou príkazov - F1
Potvrdenie vybratej možnosti zo zoznamu ponuky príkazov - ENTER
Návrat bez výberu možnosti zo zoznamu ponuky príkazov - ESC

Spustenie napísaného programu - F5
Skákanie po kóde jednej úrovni - CTRL + H
";


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

            HelpTab.Text = HELP_TEXT;
            HelpTab.Focus();

        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HelpTab_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Close();
        }
    }
}
