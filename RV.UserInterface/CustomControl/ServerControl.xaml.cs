using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using RV.ClientServer.Server;

namespace RV.UserInterface.CustomControl
{
	/// <summary>
	/// Interaction logic for ServerControl.xaml
	/// </summary>
	public partial class ServerControl : UserControl
	{
		#region Private props
		private readonly Server server;
		private const string Caption = "Внимание";
		private const string ConnectExc = "Пожалуйста заполните порт для подключения";
		private const string ServerCreateExc = "Не удалось создать сервер смотри лог";
		#endregion

		#region Public event
		public event EventHandler Closed;
		#endregion

		public ServerControl()
		{
			InitializeComponent();

			server = new Server();
			server.UserCollection.CollectionChanged += UserCollectionCollectionChanged;
			server.ServerStart += ServerStart;
			server.ServerProblem += ServerProblem;
		}

		#region Private method
		private void BtnServerStartClick(object sender, RoutedEventArgs e)
		{
			if (TxbPort.Text.Trim() == string.Empty)
			{
				var owner = GetOwner();
				if (owner != null)
					MessageBox.Show(
						owner,
						ConnectExc,
						Caption,
						MessageBoxButton.OK,
						MessageBoxImage.Warning);
				return;
			}
			server.SetPort(TxbPort.Text.Trim());
			server.Start();
		}

		private void ServerStart(object sender, EventArgs e)
		{
			Action action = delegate { this.TbServerCreate.Text = "Сервер создан"; };
			this.Dispatcher.Invoke(action);
		}

		private void BtnServerFinish(object sender, RoutedEventArgs e)
		{
			server.Stop();

			if (Closed != null)
				Closed(sender, e);
		}

		private void ServerProblem(object sender, EventArgs e)
		{
			Action action = delegate
				{
					var owner = GetOwner();
					if (owner != null)
						MessageBox.Show(
							owner,
							ServerCreateExc,
							Caption,
							MessageBoxButton.OK,
							MessageBoxImage.Warning);
				};
			Dispatcher.Invoke(action);
		}

		private void UserCollectionCollectionChanged(object sender,
													  System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var userCollection = new ObservableCollection<string>(server.UserCollection);

			Action action = delegate { UserCollection.ItemsSource = userCollection; };
			UserCollection.Dispatcher.Invoke(action);
		}

		private Window GetOwner()
		{
			return Window.GetWindow(this);
		}
		#endregion
	}
}
