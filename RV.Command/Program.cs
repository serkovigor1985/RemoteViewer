using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using RV.AddIn;

namespace RV.Command
{
	internal class Program : MarshalByRefObject
	{
		#region Private props
		private AppDomain _appController;
		private IGuiController _guiController;
		#endregion

		#region Public event

		public event EventHandler ShutDown;
		#endregion

		#region Public methods
		public void Init()
		{
			Load();
		}

		public void RegisterProgram()
		{
			_guiController.Register();
		}

		public void StartMainWindow()
		{
			_guiController.Main();
		}

		public void StartTray()
		{
			_guiController.Tray();
		}

		public void Connect(string uri)
		{
			_guiController.Restart(uri);
		}
		#endregion

		#region Private methods
		public void OnUpdate(object sender,NeedUpdateEventArgs e)
		{
			OnShutDown(sender, e);
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var command = string.Format("{0}", Path.Combine(path, "backup.exe"));
			var process = new ProcessStartInfo {Arguments = e.Endpoint, FileName = command, CreateNoWindow = false};
			
			Process.Start(process);

			//Unload();
			//var update = new Update(
			//	e.Mask,
			//	e.UpdatePath,
			//	e.BackupPath,
			//	e.FilesExclude);
			//update.Start();
			//Load();

			//guiController.Restart(new EndpointAddress(e.Endpoint));
		}

		private void Unload()
		{
			_guiController = null;
			///GC.Collect();


			try
			{
				AppDomain.Unload(_appController);
			}
			catch (Exception)
			{
				
			}
		}

		private void Load()
		{
			_appController = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, null);

			_appController.InitializeLifetimeService();


			_guiController =
				(IGuiController)
				_appController.CreateInstanceAndUnwrap(
					Properties.Settings.Default.AssemblyName,
					Properties.Settings.Default.TypeName);
			
			_guiController.Update += OnUpdate;
			_guiController.Closed += OnShutDown;
		}

		private void OnShutDown(object sender, EventArgs e)
		{
			//Unload();

			if (ShutDown != null)
				ShutDown(sender, e);

			//GC.Collect();
		}
		#endregion
	}
}
