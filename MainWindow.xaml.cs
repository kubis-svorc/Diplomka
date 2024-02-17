using System;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Diplomka.Runtime;
using Microsoft.Win32;
using Diplomka.Analyzators;
using System.Linq;
using System.Text.RegularExpressions;
using Console = System.Diagnostics.Debug;
using System.Windows.Controls;
using System.Windows.Media;

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
	opakuj 5 krat
		hraj c
		c = 5
		ak  c > 12
			c = c - 1
		koniec
	koniec
	
	a = 12
	ak a = 3
		opakuj a krat
			hraj g
		koniec
	koniec
	inak
		hraj e
	koniec

	urob prvypodprogram
		opakuj 3 krat
		koniec
		b = 5
		ak b < 5
			b = b -1
		koniec
		inak 
			b = b + 1
		koniec
	koniec
	prvypodprogram
koniec".ToLower();
			 _cancelTokenSource = new CancellationTokenSource();
			_isTextBoxFocused = true;
			CodeTab.Focus();
		}

		public static void PrintInfo(string message)
		{
			MessageBox.Show(message, "Info", MessageBoxButton.OK);
		}

		public override void EndInit()
		{
			base.EndInit();
			CodeTab.Focus();
		}

		private void OnExitClick(object sender, RoutedEventArgs e)
		{
			Application.Current.MainWindow.Close();
			Application.Current.Shutdown(0);
			Environment.Exit(0);
		}

		private void OnNovyClick(object sender, RoutedEventArgs e)
        {
			CodeTab.Clear();
			ErrorTab.Clear();
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
				CurrentFilePath = System.IO.Path.GetDirectoryName(textPath);
				try 
				{
					CodeTab.Text = System.IO.File.ReadAllText(textPath);
				}
                catch (System.IO.IOException ex)
                {
					ErrorTab.AppendText($"Nebolo možné otvoriť súbor: {ex.Message}");
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
				CurrentFilePath = System.IO.Path.GetDirectoryName(dialog.FileName);
				try
				{
					VirtualMachine.SaveMIDI(midiPath);
					System.IO.File.WriteAllText(textPath, CodeTab.Text, System.Text.Encoding.UTF8);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					ErrorTab.Text = ex.Message;
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
				System.IO.File.WriteAllText(textPath, CodeTab.Text, System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				ErrorTab.Text = ex.Message;
				ErrorTab.Focus();
			}

			//var dialog = new SaveFileDialog 
			//{
			//	InitialDirectory = CurrentFilePath,
			//	Title = "Uložiť program"
			//};

			//if (dialog.ShowDialog().Value)
			//{
				
			//}
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
				Console.WriteLine(ex.Message);
				ErrorTab.Text = ex.Message;
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
				Console.WriteLine("tree is null");
				CodeTab.IsReadOnly = false;
				return;
			}
			// execution
			Console.WriteLine("Parse tree finished");
			VirtualMachine.SetJumpToProgramBody();
			tree.Generate();
			Console.WriteLine("program started");
			VirtualMachine.Start();
			CodeTab.IsReadOnly = false;
		}
		
		private void CodeTab_KeyUp(object sender, KeyEventArgs e)
		{
			if (!_isTextBoxFocused) 
			{
				return;
			}

			int from, until;

			for (from = CodeTab.CaretIndex - 1; 0 < from && from < CodeTab.Text.Length && char.IsLetterOrDigit(CodeTab.Text[from]); from--) ;            

			for (until = CodeTab.CaretIndex + 1; 0 < until && until < CodeTab.Text.Length && char.IsLetterOrDigit(CodeTab.Text[until]); until++) ;			

			if (from < 0)
			{
				from = 0;
			}

			if (until > CodeTab.Text.Length)
			{
				until = CodeTab.Text.Length;
			}

			string wordUnderCaret = CodeTab.Text.Substring(from, until - from).Trim();

			string[] suggestions = Wrappers
				.KeywordContainer
				.Keywords
				.Where(kw => Regex.IsMatch(kw, wordUnderCaret))
				.ToArray();
			SuggestionList.ItemsSource = suggestions;
			
			_startSubstr = from;
			_endSubstr = until;
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
					if (!CodeTab.IsFocused)
					{
						return;
					}
					_isTextBoxFocused = false;
					if (!SuggestionList.HasItems)
					{
						SuggestionList.Focus();
						return;
					}
					SuggestionList.SelectedIndex = 0;
					ListViewItem item = SuggestionList.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
					item.Focus();
					break;

				case Key.F5:					
					PlayMusic();
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
		}

		private void SwitchTheme(object sender, RoutedEventArgs e)
        {
			DarkTheme = !DarkTheme;
			Brush text, bg;
			if (DarkTheme)
            {
				text = Brushes.White;
				bg = Brushes.Black;
            }
			else 
			{
				text = Brushes.Black;
				bg = Brushes.White;
			}

			CodeTab.Foreground = text;
			CodeTab.Background = bg;

			ErrorTab.Foreground = text;
			ErrorTab.Background = bg;

			SuggestionList.Foreground = text;
			SuggestionList.Background = bg;
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
				_cancelTokenSource = new CancellationTokenSource();
				await StartExec(_cancelTokenSource.Token);
				await VirtualMachine.Play(_cancelTokenSource.Token);
			}
			catch (Exception ex)
			{
				CodeTab.IsReadOnly = false;
				Console.WriteLine(ex.Message);
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
				Console.WriteLine(selectedValue);
				string prefix = CodeTab.Text.Substring(0, _startSubstr);
				string postFix = CodeTab.Text.Substring(_endSubstr);
				CodeTab.Text = string.Concat(prefix, selectedValue, postFix);
				_caretPos = prefix.Length + selectedValue.Length;
				goto FocusCodeTab;
			}
			else if (Key.F1 == e.Key) 
			{
				goto FocusCodeTab;
			}
			return;
			
			FocusCodeTab:
			_isTextBoxFocused = true;
			CodeTab.Focus();
			CodeTab.CaretIndex = _caretPos;
			return;	
        }
    
		
    }
}
