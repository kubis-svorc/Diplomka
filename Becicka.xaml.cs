using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text.RegularExpressions; 



namespace coshi2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string NewLineCharacter = "\r\n";
        public Canvas current_canvas;
        public Dictionary<int, UIElement> main_elements = new Dictionary<int, UIElement>();
        public int focus = 0;
        public bool is_running = false;
        public string soundPackage;
        private int caretInd;
        public Ellipse robot;
        double sirkaC;
        double vyskaC;


        public List<int[]> positions;

        private int lastCursorPosition = 0;
        private int startIndex = 0;


        private DispatcherTimer timer = new DispatcherTimer();
        public int index = 0;


        public MainWindow()
        {
            //priprav canvas
            Console.WriteLine();
            InitializeComponent();
            WindowState = WindowState.Maximized; 

            DrawGrid();
            UpdateLineNumbers();
            //nastavenia
            Settings.MAP = this.get_map();


            //nacitaj zvukove balicky
            string[] subdirectories = Directory.GetDirectories(SoundsHandler.mainDirectory);
            if (subdirectories != null && subdirectories.Length > 0)
            {
                string firstPackageName = System.IO.Path.GetFileName(subdirectories[0]);
                Settings.set_sound_package(firstPackageName);
            }
            changeSize(Settings.PACKAGE_SIZE);
            WritePackagesMenu();

            textBox.Focus();
            //spusti kreslenie
            timer.Interval = TimeSpan.FromSeconds(Settings.SPEED);
            timer.Tick += Draw_Robot;

            main_elements.Add(0, textBox);
            main_elements.Add(1, textBox);
            main_elements.Add(2, Terminal);
        }




        public void WritePackagesMenu()
        {
            string soundsDirectory = "../../../sounds";

            if (Directory.Exists(soundsDirectory))
            {
                string[] packageDirectories = Directory.GetDirectories(soundsDirectory);

                foreach (string packageDirectory in packageDirectories)
                {
                    string packageName = new DirectoryInfo(packageDirectory).Name;
                    MenuItem packageMenuItem = new MenuItem { Header = packageName };
                    packageMenuItem.Click += SoundPackageMenuItem_Click;
                    soundPackagesMenu.Items.Add(packageMenuItem);
                    if (soundPackagesMenu.Items.Count == 1)
                    {
                        packageMenuItem.IsChecked = true;
                    }
                }

            }
        }

        private void SoundPackageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                foreach (MenuItem item in soundPackagesMenu.Items)
                {
                    item.IsChecked = false;
                }
                string selectedSoundPackage = menuItem.Header as string;
                Settings.set_sound_package(selectedSoundPackage);
                changeSize(Settings.PACKAGE_SIZE);
                menuItem.IsChecked = true;
                DrawLabels();
            }
        }

        public void DrawGrid() {
            uniformGrid.Rows = Settings.MAP_SQRT_SIZE;
            uniformGrid.Columns = Settings.MAP_SQRT_SIZE;

            for (int i = 1; i <= Settings.MAP_SQRT_SIZE * Settings.MAP_SQRT_SIZE; i++)
            {


                Border border = new Border();
                border.BorderBrush = Settings.FG;
                if(Settings.THEME == Theme.Light)
                {
                    border.BorderBrush = Brushes.DarkGray;
                }
                border.BorderThickness = new Thickness(0.5);

                Canvas canvas = new Canvas();
                canvas.Background = Settings.BG;
                canvas.Name = "c" + i;
                canvas.Focusable = true;

             

                if (i == 1)
                {
                    this.current_canvas = canvas;

                }

                border.Child = canvas;

                uniformGrid.Children.Add(border);
            }
        }


        public void DrawLabels()
        {
            Commands.labelnames.Clear();

            for (int i = 0; i < Settings.MAP.GetLength(0); i++)
            {
                for (int j = 0; j < Settings.MAP.GetLength(1); j++)
                {
                    //odstranime stary label
                    Label foundLabel = null;

                    foreach (var child in Settings.MAP[i, j].Children)
                    {
                        if (child is Label oldlabel)
                        {
                            foundLabel = oldlabel;
                            break;
                        }
                    }
                    if (foundLabel != null)
                    {
                        Settings.MAP[i, j].Children.Remove(foundLabel);
                    }

                    if (i >= 0 && i < Math.Sqrt(SoundsHandler.sounds_map.Length) && j >= 0 && j < Math.Sqrt(SoundsHandler.sounds_map.Length))
                    {
                        if (SoundsHandler.sounds_map[i, j] is SoundItem) {
                            Label label = new Label
                            {
                                Content = SoundsHandler.sounds_map[i, j].Name, // Názov zo sounds_map
                                FontSize = 12,
                                Foreground = Settings.FG
                            };

                            if (!Commands.labelnames.Contains(SoundsHandler.sounds_map[i, j].Name)) {
                                Commands.labelnames.Add(SoundsHandler.sounds_map[i, j].Name);
                            }

                            Canvas.SetLeft(label, 0);
                            Canvas.SetTop(label, 0);
                            Canvas.SetZIndex(label, 2); 

                            Settings.MAP[i, j].Children.Add(label);
                        }
                    }
                }
            }
        }
        
        private void UpdateLineNumbers()
        {
            int lineCount = textBox.LineCount;
            if(lineCount <= 0) { lineCount = 1; }
            string lineNumbersText = ""; 
            for (int i = 1; i <= lineCount; i++) 
            {
                lineNumbersText += i.ToString().PadLeft(3) + "  \n";
            }
            lineNumberTextBox.Text = lineNumbersText;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Predict_Commands();
            UpdateLineNumbers();
        }

        private void Predict_Commands() 
        {
            int currentCursorPosition = textBox.CaretIndex - 1;
            startIndex = currentCursorPosition;

            if (currentCursorPosition != lastCursorPosition)
            {
                string zmeneneSlovo = "";

                while (startIndex > 0 && !char.IsWhiteSpace(textBox.Text[startIndex - 1]))
                {
                    startIndex -= 1;
                }

                if (startIndex < currentCursorPosition)
                {
                    zmeneneSlovo = textBox.Text.Substring(startIndex, currentCursorPosition - startIndex + 1);
                }

                if (zmeneneSlovo != null && zmeneneSlovo.Length >= 2)
                {
                    predictionBox.Items.Clear();
                    List<string> commands = Commands.find_command(zmeneneSlovo.ToLower());
                    foreach (string command in commands)
                    {
                        predictionBox.Items.Add(command);
                    }
                }
                else
                {
                    predictionBox.Items.Clear();
                }

                lastCursorPosition = currentCursorPosition;
            }
        }

        private void map_click(object sender, RoutedEventArgs e)
        {
            move_focus(1);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Neuložené zmeny budú stratené. Chcete ich uložiť a vytvoriť nový súbor?", "Upozornenie", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveToFile(); // Uložiť zmeny
                newFile();  // Novy subor
                e.Handled = true; // Zastaviť ďalšie spracovanie klávesnice
            }
            else if (result == MessageBoxResult.No)
            {
                newFile();
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void newFile() {
            Settings.CURRENTFILEPATH = null;
            textBox.Text = string.Empty;
            Title = "Coshi2";
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            openFile();
        }

        private void openFile()
        {
            string code = FilesHandler.open();
            Title = "Coshi2 - " + Settings.CURRENTFILEPATH;
            changeSize(Settings.MAP_SQRT_SIZE);


            DrawLabels();
            textBox.Text = code;
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Textové súbory (*.txt)|*.txt|Všetky súbory (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                Settings.CURRENTFILEPATH = saveFileDialog.FileName;
                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            FilesHandler.save(textBox.Text);
            Title = "Coshi2 - " + Settings.CURRENTFILEPATH;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }


        private void Robot_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            focus_toggle();
        }

        private void move_focus(int f)
        {
            switch (f)
            {
                case 0: //kod
                    textBox.Focusable = true;
                    textBox.IsReadOnly = false;
                    textBox.Focus();
                    break;
                case 1: //graf plocha
                    textBox.IsReadOnly = true;
                    textBox.Focusable = false;
                    pomocnyCanvas.Focus();
                    Keyboard.Focus(pomocnyCanvas);
                    break;
                case 2: //terminal
                    Terminal.Focus();
                    break;
            }
            focus = f;
        }

        private void focus_toggle()
        {
            switch (focus)
            {
                case 0:
                    move_focus(1);
                    break;
                case 1:
                    move_focus(2);
                    break;
                case 2:
                    move_focus(0);
                    break;
            }
        }



        public Canvas[,] get_map()
        {
            Canvas[,] canvases = new Canvas[Settings.MAP_SQRT_SIZE, Settings.MAP_SQRT_SIZE];

            int index = 0;

            for (int i = 0; i < Settings.MAP_SQRT_SIZE; i++)
            {
                for (int j = 0; j < Settings.MAP_SQRT_SIZE; j++)
                {
                    Border border = (Border)uniformGrid.Children[index];
                    canvases[i, j] = (Canvas)border.Child;
                    index++;
                }
            }
            return canvases;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void size3_Click(object sender, RoutedEventArgs e)
        {
            changeSize(3);
        }

        private void size5_Click(object sender, RoutedEventArgs e)
        {
            changeSize(5);
        }

        private void size7_Click(object sender, RoutedEventArgs e)
        {
            changeSize(7);
        }

        public void changeSize(int size) {
            Settings.set_size(size);
            uniformGrid.Children.Clear();
            DrawGrid();
            Settings.MAP = this.get_map();
            SoundsHandler.fill_sound_map();
            DrawLabels();
            Draw_User(0, 0);
        }



        private void CloseApp(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show("Neuložené zmeny budú stratené. Chcete ich uložiť a zatvoriť aplikáciu?", "Upozornenie", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveToFile(); // Uložiť zmeny
                Application.Current.Shutdown(); // Zatvoriť aplikáciu
                e.Handled = true; // Zastaviť ďalšie spracovanie klávesnice
            }
            else if (result == MessageBoxResult.No)
            {
                Application.Current.Shutdown(); 
                e.Handled = true; 
            }
            else
            {
                e.Handled = true; 
            }
        }


        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (textBox.IsFocused == false)
            {
                e.Handled = true;
            }
            if (e.Key == Key.Tab) {
                if (predictionBox.Items.Count > 0)
                {
                    caretInd = textBox.CaretIndex;
                    predictionBox.Focus();
                }
                else
                {
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Enter && focus == 0 && textBox.LineCount >= 200)
            {
                e.Handled = true;
            }
        }

        private void Stop()
        {
            timer.Stop();
            return;
        }


        private void Play()
        {
            try
            {
                Terminal.Text = "";
                this.is_running = true;
                this.index = 0;
                VirtualMachine.reset();
                VirtualMachine.SetTextBoxReference(Terminal);
                Robot.reset();
                Settings.reset_program_settings();
                Compiler cmp = new Compiler(textBox.Text);
                Block tree = cmp.parse();
                cmp.jumpOverVariables();
                tree.generate();
                VirtualMachine.execute_all();


                this.index += 1;
                this.timer.Start();
            }
            catch (Exception ex)
            {
                Terminal.Text = "Chyba: " + ex.Message;
            }
            this.is_running = false;
        }
      
        private void Move_Robot(KeyEventArgs e)
        {
            
            int name = int.Parse(this.current_canvas.Name.Replace("c", "")) - 1;
            int i = name / Settings.MAP_SQRT_SIZE;
            int j = name % Settings.MAP_SQRT_SIZE;

            int i0 = i;
            int j0 = j;

            if (e.Key == Key.Left && j != 0)
            {
                j -= 1;
            }
            else if (e.Key == Key.Up && i != 0)
            {
                i -= 1;
            }
            else if (e.Key == Key.Down && i + 1 < Settings.MAP_SQRT_SIZE)
            {
                i += 1;
            }
            else if (e.Key == Key.Right && j + 1 < Settings.MAP_SQRT_SIZE)
            {
                j += 1;
            }

            if (i != i0 || j != j0)
            {
                DrawRobotOnCanvas(i, j);
                SoundsHandler.play_sound(i, j);
            }
            
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            int indexC = textBox.CaretIndex;
            string text = Regex.Replace(textBox.Text, @"(?<! )\r\n", " \r\n");
            if(text != textBox.Text)
            {
                textBox.Text = text;
                textBox.CaretIndex = indexC;
            }            
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (focus == 1)
            {
                Move_Robot(e);
            }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                Show_Help(null, e);
                e.Handled = true;
            }

            if (e.Key == Key.F2)
            {
                if (focus == 1)
                {
                    textBox.IsReadOnly = false;
                    focus = 0;
                }
                subor_volba.Focus();
            }


            if (Keyboard.IsKeyDown(Key.LeftShift) && e.Key == Key.F5)
            {
                Stop();
            }

            if (!Keyboard.IsKeyDown(Key.LeftShift) && e.Key == Key.F5 && !this.is_running)
            {
                Play();  
            }


            if (e.Key == Key.F6)
            {
                Stop();
                focus_toggle();
            }

            if (e.Key == Key.F7)
            {
                Decrease_Speed(sender, null);
                e.Handled = true;
            }

            if (e.Key == Key.F8)
            {
                Increase_Speed(sender, null);
                e.Handled = true;
            }

            if (e.Key == Key.F9)
            {
                SwitchColorTheme(sender, null);
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && e.Key == Key.S)
            {
                SaveAs_Click(sender, null);
                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S)
            {
                SaveToFile();
                e.Handled = true;
            }


            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.O)
            {
                openFile();
                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.N)
            {

                New_Click(null, e);
                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.H)
            {
                FindNextKeyword();
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.OemPlus)
            {
                Increase_Font(sender, null);
                e.Handled = true;
                    
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.OemMinus)
            {
                Decrease_Font(sender, null);
                e.Handled = true;

            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.G)
            {
                ShowGoToLineDialog();
                e.Handled = true;

            }

        }

        private void FindNextKeyword()
        {
            var headerPattern = string.Join("|", Commands.get_block_starts());
            var regex = new System.Text.RegularExpressions.Regex(headerPattern);
            var matches = regex.Matches(textBox.Text);
            int caret = textBox.CaretIndex;

            // prejdi vsetky nalezi kluc. slov a najdi najblizsi dalsi
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Index > caret)
                {
                    textBox.CaretIndex = match.Index;
                    return;
                }
            }

            // ak ani jeden nie je za aktualny caretom, nastav prvy vyskyt - cyklicke hladanie
            if (matches.Count > 0)
            {
                textBox.CaretIndex = matches[0].Index;
                return;
            }

            //inak zostan kde si
        }



        private void ShowGoToLineDialog()
        {
            // Vytvorenie dialógového okna
            var dialog = new GoToLineDialog();
            if (dialog.ShowDialog() == true) // Zobrazenie dialógového okna a čakanie na užívateľský vstup
            {
                int lineNumber;
                if (int.TryParse(dialog.LineNumber, out lineNumber))
                {
                    int lineIndex = Math.Min(Math.Max(0, lineNumber - 1), textBox.LineCount - 1); // Prevedenie čísla riadku na index riadku

                    if (lineNumber <= 0 || lineNumber > textBox.LineCount)
                    {
                        MessageBox.Show("Zadajte platné číslo riadku.", "Varovanie", MessageBoxButton.OK, MessageBoxImage.Warning);

                    }
                    else
                    {

                        textBox.ScrollToLine(lineIndex); // Presun na zvolený riadok
                        textBox.Focus(); // Focus na textBox, aby sa zvýraznil kurzor
                        textBox.Select(textBox.GetCharacterIndexFromLineIndex(lineIndex), 0); // Označenie pozície kurzora
                    }
                }
                else
                {
                    MessageBox.Show("Zadajte platné číslo riadku.", "Varovanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }



        public void Draw_User(int riadok, int stlpec)
        {
            this.current_canvas.Children.Remove(robot);

            this.current_canvas = Settings.MAP[riadok, stlpec];
            this.robot = new Ellipse();
            this.robot.Width = 80;
            this.robot.Height = 80;

            if (Settings.MAP_SQRT_SIZE == 3)
            {
                Canvas.SetLeft(this.robot, this.robot.Width - 30);
                Canvas.SetTop(this.robot, this.robot.Width - 30);
            }


            if (Settings.MAP_SQRT_SIZE == 5)
            {
                Canvas.SetLeft(this.robot, this.robot.Width - 40);
                Canvas.SetTop(this.robot, this.robot.Width - 40);
                this.robot.Width = 50;
                this.robot.Height = 50;
            }
            else if (Settings.MAP_SQRT_SIZE == 7)
            {
                Canvas.SetLeft(this.robot, this.robot.Width - 50);
                Canvas.SetTop(this.robot, this.robot.Width - 50);
                this.robot.Width = 50;
                this.robot.Height = 50;
            }
            this.robot.Fill = Settings.FG;

            Canvas.SetZIndex(this.robot, 1);

            this.current_canvas.Children.Add(this.robot);
        }
        

        public void Draw_Robot(object sender, EventArgs e)
        {
            if(Robot.positions.Count == 1)
            {
                this.timer.Stop();
                DrawRobotOnCanvas(0, 0);
                Terminal.Text += " Program úspešne zbehol.";
                return;
            }

            if (this.index >= Robot.positions.Count)
            {
                this.timer.Stop();
                Terminal.Text += " Program úspešne zbehol.";
                return;
            }

            int[] position = Robot.positions[this.index];
            int row = position[0];
            int column = position[1];

            if (row == 100 && column == 100)
            {
                Settings.SILENCE = false;
            }
            else if (row == -100 && column == -100)
            {
                Settings.SILENCE = true;
            }
            else
            {
                if (row < 0 || row >= Settings.MAP_SQRT_SIZE || column < 0 || column >= Settings.MAP_SQRT_SIZE)
                {
                    Terminal.Focus();
                    this.timer.Stop();
                    return;
                }

                DrawRobotOnCanvas(row, column);
                SoundsHandler.play_sound(row, column);
            }

            this.index += 1;
            if (this.index >= Robot.positions.Count)
            {
                this.timer.Stop();
                Terminal.Text += " Program úspešne zbehol.";
            }
        }

        private void DrawRobotOnCanvas(int row, int column)
        {
            Robot.position = (row) * Settings.MAP_SQRT_SIZE + (column + 1);

            double canvasSize = CalculateCanvasSize();
            double robotSize = canvasSize / 2; 

            this.current_canvas.Children.Remove(robot);

            this.current_canvas = Settings.MAP[row, column];
            this.robot = new Ellipse();

            this.robot.Width = robotSize;
            this.robot.Height = robotSize;

            double leftOffset = (canvasSize - robotSize) / 2;
            Canvas.SetLeft(this.robot, leftOffset);
            Canvas.SetTop(this.robot, leftOffset + 7);

            this.robot.Fill = Settings.FG;

            Canvas.SetZIndex(this.robot, 1);

            this.current_canvas.Children.Add(this.robot);
        }

        private double CalculateCanvasSize()
        {
            if (this.current_canvas == null)
            {
                return 0;
            }

            double canvasWidth = this.current_canvas.ActualWidth; 
            double canvasHeight = this.current_canvas.ActualHeight; 

            return Math.Min(canvasWidth, canvasHeight);
        }



        private void lineNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }



        private void ListBox_Selection(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Space)
            {
                try
                {
                    if (predictionBox.SelectedItem != null)
                    {
                        string selected = predictionBox.SelectedItem.ToString();
                        string part1 = textBox.Text[0..startIndex];
                        string part2 = textBox.Text.Substring(textBox.CaretIndex);
                        textBox.Text = part1 + selected + part2;
                        caretInd += selected.Length - 2;
                    }

                }
                catch
                {

                }
                textBox.Focus();
                textBox.CaretIndex = caretInd;
            }
            if (e.Key == Key.Escape) {
                textBox.Focus();
                textBox.CaretIndex = caretInd;
            }
            
        }
        private void predictionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void predictionBox_GotFocus(object sender, RoutedEventArgs e)
        {
    
        }

        private void SwitchColorTheme(object sender, RoutedEventArgs e)
        {
            // ak je dark daj light inak daj dark
            Settings.THEME = (Theme.Dark == Settings.THEME) ? Theme.Light : Theme.Dark;
            Settings.FG = (Theme.Dark == Settings.THEME) ? Brushes.White : Brushes.Black;
            Settings.BG = (Theme.Dark == Settings.THEME) ? Brushes.Black : Brushes.White;

            if(Settings.THEME == Theme.Dark)
            {
                ThemeState.Header = "_Svetlý režim";
            }
            else
            {
                ThemeState.Header = "_Tmavý režim";
            }

            textBox.Background = Settings.BG;
            textBox.Foreground = Settings.FG;

            Terminal.Background = Settings.BG;
            Terminal.Foreground = Settings.FG;

            if(Settings.THEME == Theme.Light)
            {
                Terminal.Background = Brushes.LightGray;
            }
            
            lineNumberTextBox.Background = Settings.BG;
            lineNumberTextBox.Foreground = Settings.FG;

            predictionBox.Background = Settings.BG;
            predictionBox.Foreground = Settings.FG;

            uniformGrid.Children.Clear();
            DrawGrid();
            Settings.MAP = this.get_map();
            DrawLabels();

            int x = (Robot.position - 1) / Settings.MAP_SQRT_SIZE;
            int y = (Robot.position - 1) % Settings.MAP_SQRT_SIZE;
            Draw_User(x, y);
        }

        private void Increase_Font(object sender, RoutedEventArgs e)
        {
            if (textBox.FontSize >= 32)
            {
                return;
            }
            lineNumberTextBox.FontSize += 2.0;
            textBox.FontSize += 2.0;
            predictionBox.FontSize += 2.0;
        }

        private void Decrease_Font(object sender, RoutedEventArgs e)
        {
            if (textBox.FontSize <= 10)
            {
                return;
            }
            lineNumberTextBox.FontSize -= 2.0;
            textBox.FontSize -= 2.0;
            predictionBox.FontSize -= 2.0;
        }

        private void Increase_Speed(object sender, RoutedEventArgs e)
        {
            if (Settings.SPEED <= 0.6)
            {
                return;
            }
            Settings.SPEED -= 0.2;
            timer.Interval = TimeSpan.FromSeconds(Settings.SPEED);
        }

        private void Decrease_Speed(object sender, RoutedEventArgs e)
        {
            if (Settings.SPEED >= 1.4)
            {
                return;
            }
            Settings.SPEED += 0.2;
            timer.Interval = TimeSpan.FromSeconds(Settings.SPEED);
        }

        private void Show_Help(object sender, RoutedEventArgs e)
        {
            var window = new Help();
            window.Owner = this;
            window.ShowDialog();
        }

        private void Terminal_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            move_focus(2);
        }

        private void UniformGrid_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            move_focus(1);
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            move_focus(0);
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F6)
            {
                e.Handled = true;
            }
        }
    }
}

