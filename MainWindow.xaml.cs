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
    using System.Globalization;

    public partial class MainWindow : Window
	{
		private int _caretPos;
		private bool _isTextBoxFocused;
		private CancellationTokenSource _cancelTokenSource;
		private bool DarkTheme;
		private static string CurrentFolderPath = null;
		private bool _isFileChanged = false;

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
			Application.Current.Dispatcher.Invoke(() => ErrorTab.Items.Add(new ListViewItem() { Content = message }));
		}

		public override void EndInit()
		{
			base.EndInit();
			CodeTab.Focus();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_isFileChanged)
			{
				MessageBoxResult result = MessageBox.Show(
					"Ste si istý, že chcete ukončiť aplikáciu?\r\n" +
					"Neuložené zmeny môžu byť stratené.\r\n" +
					"Stlačte Áno pre uloženie alebo Nie pre ukončenie bez uloženia.",
					"Ukončiť aplikáciu",
					MessageBoxButton.YesNoCancel);

				if (MessageBoxResult.Yes == result)
				{
					OnSave(this, null);
				}
				else if (MessageBoxResult.Cancel == result)
				{
					e.Cancel = true;
					return;
				}
			}
            Application.Current.Shutdown(0);
            Environment.Exit(0);
        }

		private void OnExitClick(object sender, RoutedEventArgs e)
		{
			if (_isFileChanged) 
			{
				MessageBoxResult result = MessageBox.Show(
					"Ste si istý, že chcete ukončiť aplikáciu?\r\n" +
					"Neuložené zmeny môžu byť stratené.\r\n" +
					"Stlačte Áno pre uloženie alebo Nie pre ukončenie bez uloženia.",
					"Ukončiť aplikáciu",
					MessageBoxButton.YesNoCancel);

				if (MessageBoxResult.Yes == result)
				{
					OnSave(sender, e);
				}
				else if (MessageBoxResult.Cancel == result) 
				{
					e.Handled = true;
					return;
				}
            }
            e.Handled = true;
            Application.Current.MainWindow.Close();
			Application.Current.Shutdown(0);
			Environment.Exit(0);
		}

		private void OnNovyClick(object sender, RoutedEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show(
				"Neuložené zmeny môžu byť stratené.\r\n" +
				"Stlačte Áno pre uloženie alebo Nie pre pokračovanie bez uloženia.",
				"Nový program",
				MessageBoxButton.YesNoCancel);

			if (MessageBoxResult.Yes == result)
			{
				OnSave(sender, e);
			}
			else if (MessageBoxResult.Cancel == result) 
			{
                e.Handled = true;
                return;
			}
            e.Handled = true;
            CodeTab.Text = "vlákno hlavné\r\n\r\nkoniec";
			ErrorTab.Items.Clear();
			SuggestionList.ItemsSource = null; //Enumerable.Empty<ItemsControl>();
			CurrentFolderPath = string.Empty;
		}

		private void OnOpenFile(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				Filter = "Text Files|*.txt",
				Title = "Otvoriť program"
			};

			if (dialog.ShowDialog().Value)
			{
				string textPath = dialog.FileName;
				CurrentFolderPath = Path.Combine(
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
				Title = "Uložiť program ako",
				InitialDirectory = !string.IsNullOrEmpty(CurrentFolderPath) ? CurrentFolderPath : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				AddExtension = false,
				Filter="Text Files|*.txt",
				DefaultExt="Text Files|*.txt"
				
            };

			if (dialog.ShowDialog().Value)
			{
                CurrentFolderPath = Path.GetDirectoryName(dialog.FileName);
                string textPath = dialog.FileName; 
				string midiPath = string.Concat(Path.Join(CurrentFolderPath, "MIDI", Path.GetFileNameWithoutExtension(dialog.FileName)), ".MIDI");

				try
				{
					if (!Directory.Exists(Path.Join(CurrentFolderPath, "MIDI"))) 
					{
						Directory.CreateDirectory(Path.Join(CurrentFolderPath, "MIDI"));
					}
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
			if (string.IsNullOrWhiteSpace(CurrentFolderPath))
			{
				OnSaveAs(sender, e);
				return;
			}

			string textPath = string.Concat(CurrentFolderPath, ".txt");
			string midiPath = string.Concat(CurrentFolderPath, ".midi");

			try
			{
				VirtualMachine.SaveMIDI(midiPath);
				File.WriteAllText(textPath, CodeTab.Text, Encoding.UTF8);
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
				ErrorTab.Items.Add(new ListViewItem() { Content = "Chyba pri spúšťaní programu..." });
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
			while (i >= 0 && char.IsLetterOrDigit(CodeTab.Text[i]))
			{
				builder.Insert(0, CodeTab.Text[i]);
				i--;
			}

			string wordUnderCaret = builder.ToString();
			CompareInfo ci = CultureInfo.InvariantCulture.CompareInfo;
            string[] suggestions = Wrappers
				.KeywordContainer
				.Keywords
				.Where(kw => ci.IsPrefix(kw, wordUnderCaret, CompareOptions.IgnoreNonSpace))
				.ToArray();
			SuggestionList.ItemsSource = suggestions;
		}

		private void CodeTab_KeyUp(object sender, KeyEventArgs e)
		{
			if (!_isTextBoxFocused)
			{
				return;
			}
			_isFileChanged = true;
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
					if (item != null)
					{
						item.Focus();
					}
					e.Handled = true;
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
				case Key.H:
					if (isCtrlPressed)
					{
						e.Handled = true;
						AutomationProperties.SetName(CodeTab, string.Empty);
						if (!FindNextHeading(CodeTab.Text, CodeTab.CaretIndex)) 
						{
							CodeTab.CaretIndex = 0;
							_caretPos = 0;
							FindNextHeading(CodeTab.Text, -1);
                        }
						Keyboard.Focus(CodeTab);
                        CodeTab.Focus();
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
			AutomationProperties.SetName(CodeTab, "Editor kódu");
		}

		private void OnPlayClick(object sender, RoutedEventArgs e)
		{
			PlayMusic();
		}

		private bool FindNextHeading(string text, int caret)
		{
			MatchCollection matches = Wrappers.KeywordContainer.RegexObj.Matches(text);
			var currentDept = 1;
			bool found = false;
			foreach (Match match in matches)
			{
				if (match.Index > caret && match.Value != "koniec")
				{
					string name = $"Riadok: {CodeTab.GetLineIndexFromCharacterIndex(match.Index) + 1}, slovo: {match.Value}, úroveň:  {currentDept}";
					CodeTab.CaretIndex = match.Index;
					PrintInfo(name);
					ErrorTab.Focus();
					AutomationProperties.SetName(CodeTab, name);
					CodeTab.Focus();
					found = true;
					break;
				}
				else if ("opakuj" == match.Value
					|| "ak" == match.Value
					|| "inak" == match.Value
					|| "urob" == match.Value
					|| "vlakno" == match.Value
					|| "vlákno" == match.Value)
				{
					currentDept++;
				}
				else if ("koniec" == match.Value)
				{
					currentDept--;
				}
			}
			return found;
		}

		private void SuggestionList_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Key.Enter == e.Key || Key.Space == e.Key)
			{
				//e.Handled = true;
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

		private void ErrorTab_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Escape:
				case Key.F6:
					e.Handled = true;
					CodeTab.Focus();
					break;

				case Key.Enter:
				case Key.Space:
                    e.Handled = true;
                    if (ErrorTab.Items.IsEmpty)
					{
						CodeTab.Focus();
						break;
					}
					if (null == ErrorTab.SelectedItem)
					{
						ErrorTab.SelectedIndex = 0;
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

		private void MyWindow_KeyDown(object sender, KeyEventArgs e)
		{
			bool isCtrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
			switch (e.Key)
			{
				case Key.F1:
					e.Handled = true;
					OnHelpClick(this, null);
					break;

                case Key.F5:
                    PlayMusic();
                    break;

                case Key.F6:
                    ErrorTab.Items.Refresh();
                    if (ErrorTab.Items.IsEmpty)
                    {
						CodeTab.Focus();
                        e.Handled = true;
                        break;
                    }
                    ErrorTab.SelectedIndex = 0;
                    var item = ErrorTab.Items[0] as ListViewItem;
                    if (item != null)
                    {
                        if (!item.Focus()) 
						{
							ErrorTab.Focus();
						}
                    }
                    e.Handled = true;
                    break;

                case Key.G:
                    if (isCtrlPressed) 
					{
                        e.Handled = true;
                        OnSHhowLineDialog();
                    }
					break;

                case Key.Subtract:
                    if (isCtrlPressed)
                    {
                        e.Handled = true;
                        DecreaseFontSize();
                    }
                    break;

                case Key.Add:
                    if (isCtrlPressed)
                    {
						e.Handled = true;
                        IncreaseFontSize();
                    }
                    break;

                default:
					break;
			}
		}

		private void OnSHhowLineDialog() 
		{
			var dialogWindow = new LineNavigationDialog(DarkTheme)
			{
				Owner = this
			};
			bool? hasResult = dialogWindow.ShowDialog();
			if (true == hasResult) 
			{
				int line;
				if (int.TryParse(dialogWindow.Result, out line)) 
				{
					if (line < 0 || line > CodeTab.LineCount)
					{
						MessageBox.Show("Zadané číslo riadka je mimo počtu riadkov alebo záporné", "Chybné číslo riadku", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
					int row = Math.Min(Math.Max(0, line - 1), CodeTab.LineCount - 1);
                    CodeTab.ScrollToLine(row); // Presun na zvolený riadok
                    CodeTab.Select(CodeTab.GetCharacterIndexFromLineIndex(row), 0);
					return;
                }
				MessageBox.Show("Zadané číslo nebolo platné", "Chybné číslo riadku", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
    }
}
