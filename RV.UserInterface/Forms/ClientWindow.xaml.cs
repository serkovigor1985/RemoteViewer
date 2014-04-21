using System;
using System.ServiceModel;
using System.Windows;
using RV.ClientServer.Client;
using RV.UserInterface.Services;

namespace RV.UserInterface.Forms
{
	public partial class ClientWindow : Window
	{
		#region Private props
		private Client client;
		#endregion

		public ClientWindow()
		{
			InitializeComponent();
			client = new Client();
			client.OnImageChange += ClientOnImageChange;
			client.ServerDisconnected += ServerDisconnected;
		}

		#region Public method
		public void Init(EndpointAddress endpoint)
		{
			client.Init(endpoint);
			client.Start();
			this.Title = string.Format("Вы подключены к {0}", endpoint.Uri.AbsoluteUri);
			this.Show();
		}
		#endregion

		#region Private method
		private void ClientOnImageChange(System.Drawing.Image display)
		{
			if (display == null) return;
			
			Action action = delegate { ImgRemoteDesktop.Source = Utils.ConvertImage(display); };
			ImgRemoteDesktop.Dispatcher.Invoke(action);
		}

		private void ServerDisconnected(object sender, EventArgs e)
		{
			this.Dispatcher.Invoke(new Action(this.Close));
		}

		private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.Stop();
		}
		#endregion
	}
}
