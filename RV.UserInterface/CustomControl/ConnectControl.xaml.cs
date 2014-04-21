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
using System.Windows.Navigation;
using System.Windows.Shapes;

using RV.AddIn;
using RV.ClientServer.Client;
using RV.UserInterface.Events;

namespace RV.UserInterface.CustomControl
{
	/// <summary>
	/// Interaction logic for ConnectControl.xaml
	/// </summary>
	public partial class ConnectControl : UserControl
	{

		#region Private props
		private readonly CancellationTokenSource tokenCancelSource = new CancellationTokenSource();
		private CancellationToken cancelToken;
		#endregion

		#region Public event
		public event EventHandler Closed;
		public event EventHandler<ConnectEventArgs> Connect;
		#endregion

		public ConnectControl()
		{
			InitializeComponent();

			cancelToken = tokenCancelSource.Token;
		}

		#region Private methods
		private void BtnLoginClick(object sender, RoutedEventArgs e)
		{
			//todo need refacor this place.
			var endpoint = CmbServicesAddressList.SelectedItem as EndpointAddress;
			if (endpoint == null)
			{
				var uri = CmbServicesAddressList.Text.Trim();
				if (uri == string.Empty)
				{
					MessageBox.Show("Пожалуйтса выберете точку подключения");
					return;
				}
				endpoint = new EndpointAddress(uri);
			}

			//if (Closed != null)
			//	Closed(null, EventArgs.Empty);

			if (Connect != null)
				Connect(sender, new ConnectEventArgs(endpoint));
		}

		private void BtnCancelClick(object sender, RoutedEventArgs e)
		{
			if (Closed != null)
			{
				Interrupt();
				Closed(null, EventArgs.Empty);
			}
		}

		private void BtnFindClick(object sender, RoutedEventArgs e)
		{
			ServicesAddressList = new ObservableCollection<EndpointAddress>();

			try
			{
				Task.Factory.StartNew(
					() =>
						{
							ServicesAddressList = FindService.ServicesAddressList();
							if (cancelToken.IsCancellationRequested)
								cancelToken.ThrowIfCancellationRequested();

							Action action = delegate
								{
									CmbServicesAddressList.ItemsSource = ServicesAddressList;
									if (ServicesAddressList.Count > 0)
										CmbServicesAddressList.SelectedIndex = 0;
								};
							CmbServicesAddressList.Dispatcher.Invoke(action);
						}, cancelToken);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc.ToString());
			}
		}

		private void Interrupt()
		{
			if (ServicesAddressList.Count <= 0)
				tokenCancelSource.Cancel();
		}

		public ObservableCollection<EndpointAddress> ServicesAddressList { get; private set; }
		#endregion
	}
}
