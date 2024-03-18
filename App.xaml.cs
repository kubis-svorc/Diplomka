using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Diplomka
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (args, e) => 
            {
                MessageBox.Show(e.Exception.Message);
                e.Handled = true;
            };
        }

    }
}
