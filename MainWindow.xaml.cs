namespace Diplomka
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using Diplomka.Runtime;
    using Microsoft.Win32;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.IO;
    using System.ComponentModel;
    using System.Windows.Automation;
    using System.Text;
    using Diplomka.Analyzators.SyntaxNodes;

    public partial class MainWindow : Window
	{
		private int _caretPos;
		private bool _isTextBoxFocused;
		private CancellationTokenSource _cancelTokenSource;
		private bool DarkTheme;
		private static string CurrentFilePath = null;

		public MainWindow()
		{
			InitializeComponent();
			CodeTab.Text = "vlakno hlavné\r\n\r\nkoniec";
			_cancelTokenSource = new CancellationTokenSource();
			_isTextBoxFocused = true;
			CodeTab.Focus();
			VirtualMachine.Print = PrintInfo; 
		}

        public void PrintInfo(string message)
		{
			Application.Current.Dispatcher.Invoke(() => ErrorTab.Items.Add(message));	
        }

		public override void EndInit()
		{
			base.EndInit();
			CodeTab.Focus();
		}

        protected override void OnClosing(CancelEventArgs e)
        {
			Application.Current.Shutdown(0);
			Environment.Exit(0);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show(
				"Ste si istý, že chcete ukončiť aplikáciu?\r\n" +
				"Neuložené zmeny môžu byť stratené.\r\n" +
				"Stlačte Áno pre uloženie alebo Nie pre návrat do programu.",
				"Ukončiť aplikáciu",
				MessageBoxButton.YesNo);

			if (MessageBoxResult.Yes == result)
            {
				OnSave(sender, e);
            }

			Application.Current.MainWindow.Close();
			Application.Current.Shutdown(0);
			Environment.Exit(0);
		}

		private void OnNovyClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Neuložené zmeny môžu byť stratené.\r\n" +
                "Stlačte Áno pre uloženie alebo Nie pre návrat do programu.",
                "Nový program",
                MessageBoxButton.YesNo);

            if (MessageBoxResult.Yes == result)
            {
                OnSave(sender, e);
            }

            CodeTab.Text = "vlákno hlavné\r\n\r\nkoniec";
			ErrorTab.Items.Clear();
			SuggestionList.ItemsSource = null; //Enumerable.Empty<ItemsControl>();
			CurrentFilePath = string.Empty;
        }

		private void OnOpenFile(object sender, RoutedEventArgs e)
        {
			var dialog = new OpenFileDialog 
			{
				InitialDirectory = "C:\\",
				Filter = "Text Files|*.txt|All Files|*.*",
				Title = "Otvoriť program"
			};

			if (dialog.ShowDialog().Value)
            {
				string textPath = dialog.FileName;
				CurrentFilePath = Path.Combine(
                    Path.GetDirectoryName(textPath),
                    Path.GetFileNameWithoutExtension(textPath));
				try 
				{
					CodeTab.Text = File.ReadAllText(textPath);
				}
                catch (IOException ex)
                {
					ErrorTab.Items.Add(new ListViewItem() { Content = $"Nebolo možné otvoriť súbor: {ex.Message}" });
                }
			}
        }

		private void OnSaveAs(object sender, RoutedEventArgs e)
        {
			var dialog = new SaveFileDialog
			{
				InitialDirectory = "C:\\",
				Title = "Uložiť program ako"
			};
			if (dialog.ShowDialog().Value)
			{
				string textPath = string.Concat(dialog.FileName, ".txt");
				string midiPath = string.Concat(dialog.FileName, ".midi");
				CurrentFilePath = Path.GetDirectoryName(dialog.FileName);
				try
				{
					VirtualMachine.SaveMIDI(midiPath);
                    File.WriteAllText(textPath, CodeTab.Text, Encoding.UTF8);
				}
				catch (Exception ex)
				{
					//ErrorTab.Text = ex.Message;
					ErrorTab.Items.Add(new ListViewItem() { Content = ex.Message });
					ErrorTab.Focus();
				}
			}
		}

		private void OnSave(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(CurrentFilePath))
            {
				OnSaveAs(sender, e);
				return;
            }

			string textPath = string.Concat(CurrentFilePath, ".txt");
			string midiPath = string.Concat(CurrentFilePath, ".midi");

			try
			{
				VirtualMachine.SaveMIDI(midiPath);
				File.WriteAllText(textPath, CodeTab.Text, System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				ErrorTab.Items.Add(new ListViewItem() { Content = ex.Message });	
				ErrorTab.Focus();
			}			
		}
				
		private Syntax Compile()
		{
			var compiler = new Compiler(CodeTab.Text.ToLower());
			try
			{
				Syntax syntaxTree = compiler.Parse();
                return syntaxTree;
			}
			catch (Exception ex)
			{
				//ErrorTab.Text += "Kompilácia neprebehla úspešne..." + Environment.NewLine;
				ErrorTab.Items.Add("Chyba pri spúšťaní programu...");
				ErrorTab.Items.Add(new ListViewItem() { Content = ex.Message });
				_ = ErrorTab.Focus();
				return null;
			}
		}

		private async System.Threading.Tasks.Task StartExecAsync(CancellationToken cancellationToken)
		{
			VirtualMachine.Reset();
			CodeTab.IsReadOnly = true;
			Syntax tree = Compile();			
			if (null == tree)
			{
				CodeTab.IsReadOnly = false;
				return;
			}
			// execution
			VirtualMachine.SetJumpToProgramBody();
			tree.Generate();
            CodeTab.IsReadOnly = false;
			try
			{
                await VirtualMachine.StartAsync(cancellationToken);
            }
			catch (ApplicationException ex)
			{

				PrintInfo("Počas behu aplikácie došlo ku chybe: " + ex.Message);
			}
		}

		private void FillSpacesForNVDA() 
		{
            int indexCaret = CodeTab.CaretIndex;
            string replacedText = Regex.Replace(CodeTab.Text, @"(?<! )\r\n", " \r\n");
            if (replacedText != CodeTab.Text)
            {
                CodeTab.Text = replacedText;
                CodeTab.CaretIndex = indexCaret;
            }
        }

        private void UpdateSuggestionList() 
		{
            var builder = new StringBuilder();
			int i = CodeTab.CaretIndex - 1;
            //while (i >= 0 && !char.IsWhiteSpace(CodeTab.Text[i])) 
            //{
            //	builder.Insert(0, CodeTab.Text[i]);
            //	i--;
            //}
            while (i >= 0 && char.IsLetterOrDigit(CodeTab.Text[i]))
            {
                builder.Insert(0, CodeTab.Text[i]);
                i--;
            }

            string wordUnderCaret = builder.ToString();
			if (wordUnderCaret.Length < 2) 
			{
				wordUnderCaret = string.Empty;
			}
            string[] suggestions = Wrappers
                .KeywordContainer
                .Keywords
                .Where(kw => Regex.IsMatch(kw, wordUnderCaret))
                .ToArray();
            SuggestionList.ItemsSource = suggestions;
        }
		
		private void CodeTab_KeyUp(object sender, KeyEventArgs e)
		{
			if (!_isTextBoxFocused) 
			{
				return;
			}
			FillSpacesForNVDA();
			UpdateSuggestionList();            
		}

		private void CodeTab_PreviewKeyDown(object sender, KeyEventArgs e)
        {
			bool isCtrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            switch (e.Key)
            {
				case Key.Space:
					if (!isCtrlPressed || !CodeTab.IsFocused || !SuggestionList.HasItems)
					{
                        break;
                    }
					_isTextBoxFocused = false;
					_caretPos = CodeTab.CaretIndex;
					SuggestionList.SelectedIndex = 0;
					ListViewItem item = SuggestionList.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
					item.Focus();
					break;
				default:
					break;
            }
        }
		
		private void CodeTab_PreviewKeyUp(object sender, KeyEventArgs e)
		{
            bool isCtrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            switch (e.Key)
            {
				case Key.F1:
					OnHelpClick(this, null);
					break;

				case Key.F5:
					PlayMusic();
					break;

				case Key.F6:
					ErrorTab.Items.Refresh();
					ErrorTab.Focus();					
					break;

				case Key.H:
					if (isCtrlPressed)
                    {
						int wordStartIndex = CodeTab.CaretIndex;
						for (int i = CodeTab.CaretIndex; i > 0; i--)
                        {
							if (!char.IsWhiteSpace(CodeTab.Text[i]))
                            {
								wordStartIndex = i;
                            }
							else
                            {
								break;
                            }
                        }
						AutomationProperties.SetName(CodeTab, string.Empty);
						CodeTab.CaretIndex = FindNextHeading(CodeTab.Text, wordStartIndex);
						int row = CodeTab.GetLineIndexFromCharacterIndex(CodeTab.CaretIndex);
						string name = CodeTab.GetLineText(row);
						Focus();
						CodeTab.Focus();
						Keyboard.Focus(CodeTab);
						AutomationProperties.SetName(CodeTab, name);
                    }
					break;

				case Key.Subtract:
					if (isCtrlPressed)
                    {
						DecreaseFontSize();
					}						
					break;

				case Key.Add:
					if (isCtrlPressed)
                    {
						IncreaseFontSize();
					}					
					break;

				default:
					break;
            }
		}
		
		private void IncreaseFontSize()
        {
			if (CodeTab.FontSize >= 20)
            {
				return;
            }
			CodeTab.FontSize++;
			ErrorTab.FontSize++;
			SuggestionList.FontSize++;
			LineNumerator.FontSize++;
		}

        private void DecreaseFontSize()
        {
			if (CodeTab.FontSize <= 8)
            {
				return;
            }
			CodeTab.FontSize--;
			ErrorTab.FontSize--;
			SuggestionList.FontSize--;
			LineNumerator.FontSize--;
		}

		private void SwitchTheme(object sender, RoutedEventArgs e)
        {
			DarkTheme = !DarkTheme;
			Brush text, bg;
			if (DarkTheme)
            {
				text = Brushes.White;
				bg = Brushes.Black;
				ColorSwitcher.Header = "Svetlý _režim";
			}
			else 
			{
				text = Brushes.Black;
				bg = Brushes.White;
				ColorSwitcher.Header = "Tmavý _režim";
			}

			CodeTab.Foreground = text;
			CodeTab.Background = bg;

			ErrorTab.Foreground = text;
			ErrorTab.Background = bg;

			SuggestionList.Foreground = text;
			SuggestionList.Background = bg;

			LineNumerator.Foreground = text;
			LineNumerator.Background = bg;
		}
	
		private void CodeTab_TextChanged(object sender, TextChangedEventArgs e)
        {
			UpdateLineNumerator();
        }

		private void UpdateLineNumerator()
        {
			var regex = new Regex(Environment.NewLine);
			int lineCount = regex.Matches(CodeTab.Text).Count + 1;
			LineNumerator.Text = string.Join(Environment.NewLine, Enumerable.Range(1, lineCount).Select(i => i.ToString().PadLeft(3) + "  "));
		}

        private async void PlayMusic()
        {
            bool isShiftPressed = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
            if (isShiftPressed)
			{
				_cancelTokenSource.Cancel();
				return;
			}
			try
			{
                ErrorTab.Items.Clear();
                _cancelTokenSource = new CancellationTokenSource();
				await StartExecAsync(_cancelTokenSource.Token);
				await VirtualMachine.Play(_cancelTokenSource.Token);
			}
			catch (Exception)
			{
				CodeTab.IsReadOnly = false;
			}
		}

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
			var window = new HelpWindow(DarkTheme)
			{
				Owner = this
			};
			window.ShowDialog();
        }

        private void CodeTab_LostFocus(object sender, RoutedEventArgs e)
        {
			AutomationProperties.SetName(CodeTab, "Kód");
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
		{
			PlayMusic();
		}

		private int FindNextHeading(string text, int caret)
        {
			MatchCollection matches = Wrappers.KeywordContainer.RegexObj.Matches(text);
			var depth = 0;
			var currentDept = 0;
			var nearestIndex = 0;

            foreach (Match match in matches)
            {
				if (match.Index == caret)
                {
					break;
                }
				else if ("opakuj" == match.Value
					|| "ak" == match.Value
					|| "inak" == match.Value
					|| "urob" == match.Value
					|| "vlakno" == match.Value)
				{
					currentDept++;
				}
				else if ("koniec" == match.Value)
                {
					currentDept--;
                }
            }

            foreach (Match match in matches)
            {
				if ("opakuj" == match.Value 
					|| "ak" == match.Value 
					|| "inak" == match.Value 
					|| "urob" == match.Value 
					|| "vlakno" == match.Value)
                {
					depth++;
                }
				else if ("koniec" == match.Value)
                {
					depth--;
                }

				if (match.Index > caret && currentDept == depth)
				{
					nearestIndex = match.Index;
					break;
				}
			}
			return nearestIndex;
        }

		private void SuggestionList_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Key.Enter == e.Key || Key.Space == e.Key)
			{
                if (SuggestionList.SelectedItem != null)
                {

                    while (0 < _caretPos && !char.IsWhiteSpace(CodeTab.Text[_caretPos - 1])) 
					{
						_caretPos--;
					}
					var sb = new StringBuilder(CodeTab.Text);
					string selectedValue = SuggestionList.SelectedItem.ToString();
					sb.Remove(_caretPos, CodeTab.CaretIndex - _caretPos);
					sb.Insert(_caretPos, selectedValue);
					sb.Insert(_caretPos + selectedValue.Length, " ");
					CodeTab.Text = sb.ToString();
					_caretPos += selectedValue.Length + 1;
                    goto FocusCodeTab;
                }
			}
			else if (Key.Escape == e.Key) 
			{
				goto FocusCodeTab;
			}
			return;
			
		FocusCodeTab:
			_isTextBoxFocused = true;
			CodeTab.CaretIndex = _caretPos;
			CodeTab.Focus();			
			return;	
        }

        private void ErrorTab_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
				case Key.Escape:
				case Key.F6:					
					CodeTab.Focus();
					break;

				case Key.Enter:
				case Key.Space:
					if (ErrorTab.Items.IsEmpty)
					{
						CodeTab.Focus();
						break;
					}
					int index = GetCaretIndexForError(ErrorTab.SelectedItem.ToString());
					if (-1 != index)
					{
						_caretPos = index;
						CodeTab.CaretIndex = index;
						CodeTab.Focus();
					}
					break;
                default:
                    break;
            }
        }

		private int GetCaretIndexForError(string row)
		{
			Regex regex = new Regex(@"\d+");
			Match match = regex.Match(row);
			if (!match.Success)
			{
				return -1;
			}
			int rowIndex = Convert.ToInt32(match.Value) - 1;
			regex = new Regex(Environment.NewLine);
			var matches = regex.Matches(CodeTab.Text);
			if (!matches.Any())
			{
				return -1;
			}
			else if (rowIndex >= matches.Count) 
			{
				return matches[matches.Count - 1].Index;
			}
			return matches[rowIndex].Index;
		}
    }
}
