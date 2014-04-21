using System;
using System.Drawing;
using System.Threading;
using System.Timers;
using RV.ClientServer.Services;
using Timer = System.Timers.Timer;

namespace RV.ClientServer.Server
{
	internal class Screen
	{
		private object _lockObject = new object();

		internal WindowData WindowData { get; set; }
		
		internal object LockObject
		{
			get { return _lockObject; }
			set { _lockObject = value; }
		}
	}

	internal class Cursor
	{
		private object _lockObject = new object();

		internal CursorData CursorData { get; set; }
		
		internal object LockObject
		{
			get { return _lockObject; }
			set { _lockObject = value; }
		}
	}

	internal class ScreenSigleton
	{
		#region Private props
		private readonly Timer _timer;
		private readonly ScreenCapture _screenCapture;
		private volatile Cursor _cursorImage = new Cursor();
		private volatile Screen _fullImage = new Screen();
		private volatile Screen _diffImage = new Screen();
		private Guid _currLabel;
		private Guid _prevLabel;
		private const double Count = 0.15;
		private const int Second = 1000;
		#endregion

		protected ScreenSigleton()
		{
			_screenCapture = new ScreenCapture();
			_prevLabel = Guid.Empty;
			_currLabel = Guid.Empty;

			_timer = new Timer(Second*Count);
			_timer.Elapsed += TakeNewImage;
			_timer.Enabled = true;
		}
		#region Internal methods
		internal WindowData GetNewImage(ref Guid label)
		{
			Screen screen;
			if ((label == _currLabel) || (label == _prevLabel))
			{
				screen = _diffImage;
			}
			else
			{
				screen = _fullImage;
			}
			label = _currLabel;
			return GetImage(screen);
		}
		#endregion

		#region Internal props
		internal CursorData CursorImage
		{
			get
			{
				CursorData cursorData = null;
				if (Monitor.TryEnter(_cursorImage.LockObject))
				{
					try
					{
						cursorData = _cursorImage.CursorData;
					}
					finally
					{
						Monitor.Exit(_cursorImage.LockObject);
					}
				}
				return cursorData;
			}
		}
		#endregion

		#region Private methods
		private static WindowData GetImage(Screen screen)
		{
			WindowData windowData = null;
			if (Monitor.TryEnter(screen.LockObject))
			{
				try
				{
					windowData = screen.WindowData;
				}
				finally
				{
					Monitor.Exit(screen.LockObject);
				}
			}
			return windowData;
		}

		private void TakeNewImage(object sender, ElapsedEventArgs e)
		{
			if (!Monitor.TryEnter(_screenCapture)) 
				return;

			try
			{
				_timer.Enabled = false;
				//todo need refactor this place.
				SetDiffImage();
				SetFullImage();
				SetCursorImage();

				_prevLabel = _currLabel;
				_currLabel = Guid.NewGuid();
			}
			finally
			{
				Monitor.Exit(_screenCapture);
				_timer.Enabled = true;
			}
		}

		private void SetCursorImage()
		{
			if (!Monitor.TryEnter(_cursorImage.LockObject)) 
				return;

			try
			{
				int x = 0;
				int y = 0;
				var img = _screenCapture.Cursor(ref x, ref y);

				_cursorImage.CursorData = Utils.PackCursorCaptureData(img, x, y);
			}
			finally
			{
				Monitor.Exit(_cursorImage.LockObject);
			}
		}

		private void SetFullImage()
		{
			if (!Monitor.TryEnter(_fullImage.LockObject)) 
				return;

			try
			{
				var bound = default(Rectangle);
				var img = _screenCapture.GetFullBitmap(ref bound);

				_fullImage.WindowData = Utils.PackScreenCaptureData(img, bound);
			}
			finally
			{
				Monitor.Exit(_fullImage.LockObject);
			}
		}

		private void SetDiffImage()
		{
			if (!Monitor.TryEnter(_diffImage.LockObject)) 
				return;

			try
			{
				var bound = default(Rectangle);
				var img = _screenCapture.Screen(ref bound);

				_diffImage.WindowData = Utils.PackScreenCaptureData(img, bound);
			}
			finally
			{
				Monitor.Exit(_diffImage.LockObject);
			}
		}
		#endregion

		#region Private class
		private class SingletonCreator
		{
			static SingletonCreator()
			{
			}

			// Private object instantiated with private constructor
			internal static readonly
				ScreenSigleton uniqueInstance = new ScreenSigleton();
		}
		#endregion

		#region Public props
		// Public static property to get the object
		public static ScreenSigleton UniqueInstance
		{
			get { return SingletonCreator.uniqueInstance; }
		}
		#endregion
	}
}
