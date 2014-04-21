using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Interop;
using RV.AddIn;
using RV.UserInterface.CustomControl;
using RV.UserInterface.Events;

namespace RV.UserInterface.Forms
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Private props
		private ServerControl serverControl = new ServerControl();
		private ConnectControl connectControl = new ConnectControl();
		private ClientControl clientControl = new ClientControl();
		#endregion

		#region Public event
		public event EventHandler ClosedEvent;
		public event EventHandler<NeedUpdateEventArgs> Update;
		#endregion

		public MainWindow()
		{
			InitializeComponent();

			Init();
		}

		#region Public methods
		public void Connect(EndpointAddress endpoint)
		{
			Connect(null, new ConnectEventArgs(endpoint));
		}
		#endregion

		#region Private methods
		private void Init()
		{
			//serverControl.Closed += CloseControl;
			//connectControl.Closed += CloseControl;
			//connectControl.Connect += Connect;
			//clientControl.Closed += CloseControl;
			//clientControl.Update += NeedUpdate;
		}

		private void NeedUpdate(object sender, NeedUpdateEventArgs e)
		{
			if (Update != null)
				Update(
					sender,
					new NeedUpdateEventArgs(e.Endpoint)
						{
							BackupPath = ClientServer.Properties.Settings.Default.BackupPath,
							FilesExclude = ClientServer.Services.Utils.ConvertToArray(ClientServer.Properties.Settings.Default.FilesExclude),
							UpdatePath = ClientServer.Properties.Settings.Default.UpdatePath,
							Mask = ClientServer.Properties.Settings.Default.Mask
						});
		}

		private void Connect(object sender, ConnectEventArgs e)
		{
			clientControl = new ClientControl();
			clientControl.Closed += CloseControl;
			clientControl.Update += NeedUpdate;
			clientControl.Init(e.Endpoint);

			this.Panel.Children.Clear();
			this.Panel.Children.Add(clientControl);
		}

		private void MiServerClick(object sender, RoutedEventArgs e)
		{
			serverControl= new ServerControl();
			serverControl.Closed += CloseControl;
			this.Panel.Children.Clear();
			this.Panel.Children.Add(serverControl);
		}

		private void MiClientClick(object sender, RoutedEventArgs e)
		{
			connectControl= new ConnectControl();
			connectControl.Closed += CloseControl;
			connectControl.Connect += Connect;

			this.Panel.Children.Clear();
			this.Panel.Children.Add(connectControl);
		}

		private void CloseControl(object sender, EventArgs e)
		{
			this.Panel.Children.Clear();
			this.Panel.Children.Add(TbContent);
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			if (ClosedEvent != null)
				ClosedEvent(null, EventArgs.Empty);
		}
		#endregion

		#region Protected methods
		protected override void OnSourceInitialized(EventArgs e)
		{
			var hwndSource = PresentationSource.FromVisual(this) as HwndSource;

			if (hwndSource != null)
				hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;

			base.OnSourceInitialized(e);
		}

		#endregion
	}
}
