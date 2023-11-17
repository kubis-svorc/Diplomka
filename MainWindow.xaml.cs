﻿using System;
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

namespace Diplomka
{
	public partial class MainWindow : Window
	{
		private int _startSubstr, _endSubstr, _caretPos;

		private bool _isTextBoxFocused;

		private CancellationTokenSource _cancelTokenSource;

		public MainWindow()
		{
			InitializeComponent();			
			CodeTab.Text = @"vlakno hlavné
  opakuj 5 krat
    hraj c d:500 h:100
    pauza 500
  koniec
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

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		}

		private void OnExitClick(object sender, RoutedEventArgs e)
		{
			Close();
			Application.Current.Shutdown(0);
		}

		private void OnSave(object sender, ExecutedRoutedEventArgs e)
		{

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.InitialDirectory = "C:\\";
			dialog.Filter = "MIDI files (*.mid)|*.mid|All files (*.*)|*.*";

			if (dialog.ShowDialog().Value)
			{
				string path = dialog.FileName;
				VirtualMachine.sequence.Save(path);
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

		private async void CodeTab_PreviewKeyUp(object sender, KeyEventArgs e)
		{
            switch (e.Key)
            {
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
					if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift))
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
					break;
				
				case Key.H:
					if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                    {
						CodeTab.CaretIndex = FindNextHeading(CodeTab.Text, CodeTab.CaretIndex);
                    }
					break;

				default:
					break;
            }
		}

        private int FindNextHeading(string text, int caret)
        {
			MatchCollection matches = Wrappers.KeywordContainer.RegexObj.Matches(text);

            foreach (Match match in matches)
            {
				if (match.Index > caret)
                {
					return match.Index;
                }
            }

			if (matches.Count > 0)
            {
				return matches[0].Index;
            }

			return 0;
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
