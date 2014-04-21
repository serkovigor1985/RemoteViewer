using System;
using System.Diagnostics;
using System.Windows;

namespace RV.Command
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public bool DoHandle { get; set; }

		#region Private props
		private Program program;
		#endregion

		#region Private methods
		private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			if (this.DoHandle)
			{
				if ((e.Exception != null) && (e.Exception.Message != string.Empty))
				{
					MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
					e.Handled = true;
				}
			}
			else
			{
				MessageBox.Show("Application is going to close! ", "Uncaught Exception");
				e.Handled = false;
			}
		}

		private void ApplicationOnStartup(object sender, StartupEventArgs e)
		{
			Trace.AutoFlush = true;
			TraceListener tl = new TextWriterTraceListener("log.txt");
			Trace.Listeners.Add(tl);
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
			program = new Program();

			program.ShutDown += (sender1, e1) => this.Dispatcher.Invoke(new Action(this.Shutdown));
			program.Init();
			program.RegisterProgram();


			if ((e.Args.Length > 0))
			{
				switch (e.Args[0])
				{
					case "start":
						program.StartTray();
						break;
					case "connect":
						if (e.Args.Length == 2)
						{
							program.Connect(e.Args[1]);
						}
						else
						{
							program.StartMainWindow();
						}
						break;
				}
			}
			else
			{
				program.StartMainWindow();
			}
		}

		private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exc = e.ExceptionObject as Exception;
			if (exc == null)
				return;

			MessageBox.Show(exc.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		#endregion
	}
}
