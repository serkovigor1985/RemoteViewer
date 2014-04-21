using System;
using System.Runtime.Remoting.Lifetime;
using System.ServiceModel;
using System.Windows;
using RV.AddIn;
using RV.UserInterface.Forms;

namespace RV.UserInterface.Contoller
{
	public class GuiController : MarshalByRefObject, IGuiController
	{

		public override object InitializeLifetimeService()
		{
			var lease = (ILease)base.InitializeLifetimeService();
			if (lease.CurrentState == LeaseState.Initial)
			{
				lease.InitialLeaseTime = TimeSpan.FromMinutes(1);
				lease.SponsorshipTimeout = TimeSpan.FromMinutes(2);
				lease.RenewOnCallTime = TimeSpan.FromSeconds(2);
			}
			return lease;
		}

		#region Public methods
		public void Main()
		{
			this.InitializeLifetimeService();

			var mainWindow = new MainWindow();
			mainWindow.ClosedEvent += OnClosed;
			mainWindow.Update += OnUpdate;

			mainWindow.Show();
		}

		public void Tray()
		{
			var trayWindow = new TrayWindow();
			trayWindow.Update += OnUpdate;
			trayWindow.ClosedEvent += OnClosed;
			trayWindow.Show();
			trayWindow.WindowState = WindowState.Minimized;
			trayWindow.FindAndConnect();
		}

		public void Register()
		{
			ClientServer.Services.Utils.RegisterProgramInStartup();
		}

		public void Restart(string endpoint)
		{
			var mainWindow = new MainWindow();
			mainWindow.Connect(new EndpointAddress(endpoint));
			mainWindow.ClosedEvent += OnClosed;
			mainWindow.Update += OnUpdate;
			mainWindow.Show();
		}
		#endregion

		#region Private methods
		private void OnClosed(object sender, EventArgs e)
		{
			Environment.Exit(0);
			//if(Closed!=null)
			//	Closed(sender,new EventArgs());
			//if (Update != null)
			//	Update(sender, new NeedUpdateEventArgs(string.Empty));
			//if (Closed != null)
			//{
			//	//GC.Collect();
			//	Closed(sender, e);
			//}
				
		}

		private void OnUpdate(object sender, NeedUpdateEventArgs e)
		{
			if (Update != null)
			{
				//GC.Collect();
				Update(sender, e);
			}
		}
		#endregion

		#region Public events
		public event EventHandler Closed;
		public event EventHandler<NeedUpdateEventArgs> Update;
		#endregion
	}
}
