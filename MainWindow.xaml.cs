using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Diplomka.Runtime;
using Microsoft.Win32;
using Diplomka.Analyzators;
using Console = System.Diagnostics.Debug;
using System.Threading.Tasks;

namespace Diplomka
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			//CodeTab.Text = "hraj E#2 h:100 d:250\r\n\r\nhraj c".ToLower();
			CodeTab.Text = @"vlakno hlavné
	nastroj organ
	hraj c d:1000 h:0 
koniec
vlakno druhe
	nastroj flauta
	hraj c d:1000 h:0
koniec".ToLower();
			//CodeTab.Text = "a = 5\r\nvypis a".ToLower();
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

		private async Task<Syntax> Compile()
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

		private async void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F5)
			{
                //MidiPlayer.PlayMultipleNotes();
                //return;
                // preparation
                VirtualMachine.Reset();
				CodeTab.IsReadOnly = true;
				Syntax tree = await Compile();
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
				await VirtualMachine.Start();
				Console.WriteLine("program playing");
				await VirtualMachine.Play();
				Console.WriteLine("playing finished");
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
		}

	}
}
