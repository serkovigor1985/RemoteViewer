using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
using RV.UserInterface.Services;

namespace RV.UserInterface.CustomControl
{
	/// <summary>
	/// Interaction logic for ClientControl.xaml
	/// </summary>
	public partial class ClientControl : UserControl
	{
		#region Private props
		private Client client;
		#endregion

		#region Public event
		public event EventHandler Closed;
		public event EventHandler<NeedUpdateEventArgs> Update;
		#endregion

		public ClientControl()
		{
			InitializeComponent();

			client = new Client();
			client.OnImageChange += ClientOnImageChange;
			client.ServerDisconnected += ServerDisconnected;
			client.Restart += Restart;
		}

		#region Public method
		public void Init(EndpointAddress endpoint)
		{
			client.Init(endpoint);
			client.Start();
		}
		#endregion

		#region Private methods
		private void Restart(EndpointAddress endpoint)
		{
			if (Update != null)
				Update(null, new NeedUpdateEventArgs(endpoint.Uri.AbsoluteUri));
		}

		private void ClientOnImageChange(System.Drawing.Image display)
		{
			if (display == null) return;

			Action action = delegate { ImgRemoteDesktop.Source = Utils.ConvertImage(display); };
			ImgRemoteDesktop.Dispatcher.Invoke(action);
		}

		private void ServerDisconnected(object sender, EventArgs e)
		{
			ClosedConnectionClick(null, new RoutedEventArgs());
		}

		private void ClosedConnectionClick(object sender, RoutedEventArgs e)
		{
			client.Stop();

			if (Closed != null)
				Closed(null, EventArgs.Empty);
		}
		#endregion
	}
}
