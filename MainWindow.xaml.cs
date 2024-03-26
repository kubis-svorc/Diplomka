using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Diplomka.Runtime;
using Microsoft.Win32;
using Diplomka.Analyzators;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.ComponentModel;

namespace Diplomka
{
    public partial class MainWindow : Window
	{
		private int _startSubstr, _endSubstr, _caretPos;
		private bool _isTextBoxFocused;
		private CancellationTokenSource _cancelTokenSource;
		private bool IsShiftPressed, IsCtrlPressed, IsAltPressed, DarkTheme;
		private static string CurrentFilePath = null;

		public MainWindow()
		{
			InitializeComponent();
			CodeTab.Text = @"vlakno hlavné

koniec".ToLower();
			_cancelTokenSource = new CancellationTokenSource();
			_isTextBoxFocused = true;
			CodeTab.Focus();
			VirtualMachine.Print = PrintInfo; 
		}

		public void PrintInfo(string message)
		{
			//ErrorTab.Text += message + Environment.NewLine;
			ErrorTab.Items.Add(new ListViewItem() { Content = message });
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

			if (MessageBoxResult.No == result)
            {
				return;
            }

			if (!string.IsNullOrWhiteSpace(CurrentFilePath)) 
			{

			}

			Application.Current.MainWindow.Close();
			Application.Current.Shutdown(0);
			Environment.Exit(0);
		}

		private void OnNovyClick(object sender, RoutedEventArgs e)
        {
			CodeTab.Clear();
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
                    File.WriteAllText(textPath, CodeTab.Text, System.Text.Encoding.UTF8);
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

		private async System.Threading.Tasks.Task StartExec(CancellationToken cancellationToken)
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
			VirtualMachine.Start();
			CodeTab.IsReadOnly = false;
		}
		
		private void CodeTab_KeyUp(object sender, KeyEventArgs e)
		{
			if (!_isTextBoxFocused) 
			{
				return;
			}

			int len = CodeTab.Text.Length;
			var builder = new System.Text.StringBuilder();
			
			for (int pos = CodeTab.CaretIndex; pos > 0 && pos <= len - 1 && !char.IsWhiteSpace(CodeTab.Text[pos]); --pos)
			{
				builder.Insert(0, CodeTab.Text[pos]);
			}
			
			string wordUnderCaret = builder.ToString();

			string[] suggestions = Wrappers
				.KeywordContainer
				.Keywords
				.Where(kw => Regex.IsMatch(kw, wordUnderCaret))
				.ToArray();
			SuggestionList.ItemsSource = suggestions;
		}

		private void CodeTab_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
				case Key.LeftShift:
				case Key.RightShift:
					IsShiftPressed = true;
					break;
				case Key.LeftCtrl:
				case Key.RightCtrl:
					IsCtrlPressed = true;
					break;
				case Key.LeftAlt:
				case Key.RightAlt:
					IsAltPressed = true;
					break;
				case Key.Space:
					if (!IsCtrlPressed)
						break;
					if (!CodeTab.IsFocused)
					{
						return;
					}
					_isTextBoxFocused = false;
					_caretPos = CodeTab.CaretIndex;
					if (!SuggestionList.HasItems)
					{
						SuggestionList.Focus();
						return;
					}
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
            switch (e.Key)
            {
				case Key.LeftShift:
				case Key.RightShift:
					IsShiftPressed = false;
					break;

				case Key.LeftCtrl:
				case Key.RightCtrl:
					IsCtrlPressed = false;
					break;

				case Key.LeftAlt:
				case Key.RightAlt:
					IsAltPressed = false;
					break;

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
					if (IsCtrlPressed)
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
						CodeTab.CaretIndex = FindNextHeading(CodeTab.Text, wordStartIndex);
                    }
					break;

				case Key.Subtract:
					if (IsCtrlPressed)
                    {
						DecreaseFontSize();
					}						
					break;

				case Key.Add:
					if (IsCtrlPressed)
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
			if (IsShiftPressed)
			{
				_cancelTokenSource.Cancel();
				return;
			}
			try
			{
                ErrorTab.Items.Clear();
                _cancelTokenSource = new CancellationTokenSource();
				await StartExec(_cancelTokenSource.Token);
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
			if (Key.Enter == e.Key)
			{
				string selectedValue = SuggestionList.SelectedItem.ToString();
				string prefix = CodeTab.Text.Substring(0, _startSubstr);
				string postFix = CodeTab.Text.Substring(selectedValue.Length);
				CodeTab.Text = string.Concat(prefix, selectedValue, postFix);
				_caretPos = prefix.Length + selectedValue.Length;
				goto FocusCodeTab;
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
