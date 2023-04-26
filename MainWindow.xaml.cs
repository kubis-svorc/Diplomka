using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Diplomka.Runtime;
using Microsoft.Win32;

namespace Diplomka
{
    public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			//CodeTab.Text = "hraj E#2 h:100 d:250\r\n\r\nhraj c".ToLower();
			CodeTab.Text = "opakuj 5 krat\r\n\thraj c d:1000 h:127\r\nkoniec".ToLower();
			//CodeTab.Text = "a = 5\r\nvypis a".ToLower();

		}

		private void OnExitClick(object sender, RoutedEventArgs e)
		{
			Close();
			VirtualMachine.sequence.Dispose();
			Application.Current.Shutdown(0);
		}

		private void OnSave(object sender, ExecutedRoutedEventArgs e)
		{

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.InitialDirectory = "C:\\";
			dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

			if (dialog.ShowDialog().Value)
            {
				string path = dialog.FileName;
				sequence.Save(path);
			}
        }

		private void OnF5Press(object sender, RoutedEventArgs e)
		{
			ErrorTab.Clear();
			using (var machine = new VirtualMachine())
            {
				try
				{
					VirtualMachine.Reset();
					var compiler = new Runtime.Compiler(CodeTab.Text.ToLower());
                    Analyzators.Block tree = compiler.Parse();					
					compiler.JumpToProgramBody();
					tree.Generate();					
					VirtualMachine.Start();
				}
				catch (Exception ex)
				{
					ErrorTab.Text = ex.Message;
					_ = ErrorTab.Focus();
				}
			}
		}

		public static void PrintInfo(string message)
        {
			_ = MessageBox.Show(message, "Info", MessageBoxButton.OK);
        }

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F5)
            {
                OnF5Press(e, null);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

    }
}
