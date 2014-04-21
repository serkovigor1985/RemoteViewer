using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RV.AddIn;
using RV.ClientServer.Client;
using RV.UserInterface.Services;

namespace RV.UserInterface.Forms
{
	/// <summary>
	/// Interaction logic for TrayWindow.xaml
	/// </summary>
	public partial class TrayWindow : Window
	{
		private ObservableCollection<EndpointAddress> servicesAddressList;
		
		#region Public event 
		public event EventHandler ClosedEvent;
		public EventHandler<NeedUpdateEventArgs> Update;
		#endregion

		public TrayWindow()
		{
			InitializeComponent();
			MinimizeToTray.Enable(this);
		}

		private void BtnCancelClick(object sender, RoutedEventArgs e)
		{
			if ((servicesAddressList != null) && (servicesAddressList.Count <= 0))
			{
				WindowClosed(sender, EventArgs.Empty);
			}
		}

		public void FindAndConnect()
		{

			try
			{
				servicesAddressList = new ObservableCollection<EndpointAddress>();

				Task.Factory.StartNew(
					() =>
						{
							do
							{
								servicesAddressList = FindService.ServicesAddressList();
							
							} while (servicesAddressList.Count <= 0);

							Dispatcher.Invoke(new Action(OpenClient));
						});
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc.ToString());
			}
		}

		private void OpenClient()
		{

			var mainWindow = new MainWindow();
			mainWindow.Connect(servicesAddressList[0]);
			mainWindow.Show();
			mainWindow.WindowState = WindowState.Maximized;
			mainWindow.Update += (sender, e) =>
				{
					if (Update != null)
						Update(sender, e);
				};
			mainWindow.Activate();
			mainWindow.ClosedEvent += (sender, e) => Dispatcher.Invoke(new Action(FindAndConnect));
		}

		private void WindowClosed(object sender, EventArgs e)
		{
			if (ClosedEvent != null)
				ClosedEvent(sender, e);
		}
	}
}
