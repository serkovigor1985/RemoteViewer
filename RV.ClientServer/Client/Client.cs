using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using RV.ClientServer.ServiceReference;
using RV.ClientServer.Services;
using RV.ClientServer.Update;
using ServiceException = RV.ClientServer.Server.ServiceException;

namespace RV.ClientServer.Client
{
	public class Client
	{
		#region Private props
		private EndpointAddress _serverAddress;
		private volatile bool _needStop;
		private string _loginName;
		private readonly ViewerSession _viewerSession = new ViewerSession();
		private RemoteServiceClient _client;
		private bool _update;
		private Task _task;
		#endregion

		#region Public event
		public delegate void ImageChangeHandler(Image display);
		public delegate void NeedRestart(EndpointAddress endpoint);
		public event ImageChangeHandler OnImageChange;
		public event NeedRestart Restart;
		public event EventHandler ServerDisconnected;
		#endregion

		public Client()
		{
			_serverAddress = null;
		}

		#region Public methods
		public void Start()
		{
#warning need add exception message.
			//todo need add exception message
			if (_serverAddress == null)
				throw new ArgumentNullException("");

			try
			{
				_task = new Task(StartClient);

				_task.ContinueWith((item) =>
					{
						if(item.IsFaulted ||item.IsCanceled)
							return;

						if (_update && Restart != null)
							Restart(_serverAddress);
					});

				_task.Start();
			}
			catch
			{
				
			}
		}

		public void Init(EndpointAddress serverAddress)
		{
			_loginName = Utils.LocalIPAddress().ToString();
			_serverAddress = serverAddress;
		}

		public void Stop()
		{
			try
			{
				_needStop = true;
				_client.Logout();
			}
			catch
			{
				
			}
		}
		#endregion

		#region Private method

		private void StartClient()
		{
			try
			{
				var binding = Utils.GetTcpBinding();
				_client = new RemoteServiceClient(binding, _serverAddress);
				_client.Login(_loginName);

				CheckUpdate();

				while (!_needStop)
				{
					try
					{
						ScreenUpdate(_client.UpdateScreenImage());
						CursorUpdate(_client.UpdateCursorImage());
					}
					catch (FaultException<ServiceException> exc)
					{
						Log(exc.Detail);
					}
				}

				if (_client.State == CommunicationState.Opened)
					_client.Logout();
			}
			catch (EndpointNotFoundException exc)
			{

				Trace.WriteLine(exc.ToString());
				OnServerDisconnected();
			}
			catch (FaultException<ServiceException> exc)
			{
				Log(exc.Detail);
				OnServerDisconnected();
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc.ToString());
				OnServerDisconnected();

			}
		}

		private void CheckUpdate()
		{
			var directory = new DirectoryHash(
				Properties.Settings.Default.Mask,
				Utils.ConvertToArray(Properties.Settings.Default.FilesExclude));

			var needUpdate = _client.GetDllsForUpdate();
			var files = directory.NeedUpdate(needUpdate, Utils.ApplicationPath);

			var directoryPath = Properties.Settings.Default.UpdatePath;

			Directory.CreateDirectory(directoryPath);
			foreach (var fileName in files)
			{
				var filePath = Path.Combine(directoryPath, fileName);
				File.WriteAllBytes(filePath, _client.DllData(fileName));
			}

			if (files.Length == 0) 
				return;

			_update = files.Length > 0;

			Stop();
		}

		private static void Log(ServiceException exc)
		{
			Trace.WriteLine("Client problem");
			Trace.WriteLine(exc.InnerException.Message);
		}

		private void ScreenUpdate(WindowData data)
		{
			if ((data != null) && (data.Image != null))
			{
				// Unpack the data.
				//
				Image partial;
				Rectangle bounds;
				Utils.UnpackScreenCaptureData(data, out partial, out bounds);

				Utils.UpdateScreen(ref _viewerSession.Screen, partial, bounds);

				UpdateScreenImage();
			}
		}

		private void CursorUpdate(CursorData data)
		{
			if ((data != null) && (data.Image != null))
			{
				// Unpack the data.
				//
				Image cursor;
				int cursorX, cursorY;

				Utils.UnpackCursorCaptureData(data, out cursor, out cursorX, out cursorY);

				_viewerSession.Cursor = cursor;
				_viewerSession.CursorX = cursorX;
				_viewerSession.CursorY = cursorY;
				UpdateScreenImage();
			}
		}

		private void UpdateScreenImage()
		{
			if (_viewerSession == null)
				return;

			if (_viewerSession.Screen == null) 
				return;

			if (_viewerSession.Cursor != null)
			{
				_viewerSession.Display = Utils.MergeScreenAndCursor(_viewerSession.Screen,
				                                                 _viewerSession.Cursor, _viewerSession.CursorX, _viewerSession.CursorY);
			}
			else
			{
				_viewerSession.Display = _viewerSession.Screen;
			}

			if ((_viewerSession.Display != null) &&
			    (_viewerSession.PrevScreen != null))
			{
				var display = new Bitmap(_viewerSession.Display);
				var prevDisplay = new Bitmap(_viewerSession.PrevScreen);
				try
				{
					if (ScreenCapture.GetBoundingBoxForChanges(ref display, ref prevDisplay) == Rectangle.Empty)
					{
						return;
					}
				}
				finally
				{
					display.Dispose();
					prevDisplay.Dispose();
					GC.Collect();
				}
				
			}

			if (OnImageChange != null)
				OnImageChange(_viewerSession.Display);

			_viewerSession.PrevScreen =
				new Bitmap(_viewerSession.Display).Clone(
					new Rectangle(0, 0, _viewerSession.Display.Width, _viewerSession.Display.Height),
					_viewerSession.Display.PixelFormat);
		}

		private void OnServerDisconnected()
		{
			if (ServerDisconnected != null) 
				ServerDisconnected(this, EventArgs.Empty);
		}
		#endregion
	}
}
