using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Diplomka.Runtime;
using Microsoft.Win32;
using Diplomka.Analyzators;
using System.Linq;
using Console = System.Diagnostics.Debug;
using System.Windows.Controls;

namespace Diplomka
{
	public partial class MainWindow : Window
	{
		private bool IsShiftPressed = false;

		public static System.Threading.CancellationTokenSource CancelToken;
		
		public MainWindow()
		{
			InitializeComponent();
			
			CodeTab.Text = @"vlakno hlavné
	nastroj organ
	hraj c d:1000 h:100 
	nastroj flauta 
	hraj e d:1000 h:100
koniec

vlakno druhe
	nastroj bicie
	hraj e d:1000 h:100 
	nastroj spev 
	hraj g d:1000 h:100
koniec".ToLower();
			//CodeTab.Text = "a = 5\r\nvypis a".ToLower();
			CancelToken = new System.Threading.CancellationTokenSource();
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

		public static void PrintInfo(string message)
		{
			_ = MessageBox.Show(message, "Info", MessageBoxButton.OK);
		}

        private async System.Threading.Tasks.Task StartExec(System.Threading.CancellationToken cancellationToken) 
		{
			VirtualMachine.Reset();
			CodeTab.IsReadOnly = true;
			Syntax tree = Compile();
			CodeTab.IsReadOnly = false;
			if (null == tree)
			{
				Console.WriteLine("tree is null");
				return;
			}
			// execution
			Console.WriteLine("Parse tree finished");
			VirtualMachine.SetJumpToProgramBody();
			tree.Generate();
			Console.WriteLine("program started");
			VirtualMachine.Start();
			Console.WriteLine("program playing");
			try
            {
				await VirtualMachine.Play(CancelToken.Token);
			}
			catch (System.Threading.Tasks.TaskCanceledException ex)
            {
				Console.WriteLine(ex.Message);
            }
			Console.WriteLine("playing finished");
		}

		private void CodeTab_KeyUp(object sender, KeyEventArgs e)
		{
			if (Key.F1 == e.Key)
			{
				_ = SuggestionList.Focus();
				return;
			}

			int from, until;
			
			for (from = CodeTab.CaretIndex - 1; 0 < from && from < CodeTab.Text.Length && !char.IsWhiteSpace(CodeTab.Text[from]); from--)
			{ ; }

			for (until = CodeTab.CaretIndex + 1; 0 < until && until < CodeTab.Text.Length && !char.IsWhiteSpace(CodeTab.Text[until]); until++)
			{ ; }
			
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
				.Where(kw => System.Text.RegularExpressions.Regex.IsMatch(kw, wordUnderCaret))
				.ToArray();
			SuggestionList.ItemsSource = suggestions;	
		}

		private void Suggestion_Selected(object sender, SelectionChangedEventArgs e) 
		{
            if (SuggestionList.SelectedItem != null)
            {
				//todo: ako vlozit text na miesto?! 				
				_ = CodeTab.Focus();
            }            
        }

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		}
		
        private void CodeTab_PreviewKeyDown(object sender, KeyEventArgs e)
        {
			if (Key.LeftShift == e.Key || Key.RightShift == e.Key)
            {
				IsShiftPressed = true;
            }
        }

        private async void CodeTab_PreviewKeyUp(object sender, KeyEventArgs e)
        {
			if (Key.F5 == e.Key)
			{
				if (IsShiftPressed)
				{
					if (!CancelToken.Token.IsCancellationRequested)
                    {
						CancelToken.Cancel();
                    }
				}
				else
				{
					if (!CancelToken.Token.IsCancellationRequested)
                    {
						CancelToken.Cancel();
                    }
					CancelToken = new System.Threading.CancellationTokenSource();
                    try
                    {
						await StartExec(CancelToken.Token);
                    }
                    catch (System.Threading.Tasks.TaskCanceledException ex)
                    {
						Console.WriteLine(ex.Message);
                    }   
				}
			}

			else if (Key.LeftShift == e.Key || Key.RightShift == e.Key)
			{
				IsShiftPressed = false;
			}
		}
    }
}
