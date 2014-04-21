using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;

using Microsoft.Win32;

using RV.ClientServer.Server;

namespace RV.ClientServer.Services
{
	public static class Utils
	{
		public static WindowData PackScreenCaptureData(Image image, Rectangle bounds)
		{
			// Pack the image data into a byte stream to
			//	be transferred over the wire.
			//
			var result= new WindowData {Top = bounds.Top, Bottom = bounds.Bottom, Left = bounds.Left, Right = bounds.Right};
			if (image != null)
			{
				using (var ms = new MemoryStream())
				{
					image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
					result.Image = ms.ToArray();
				}
			}
			return result;
		}

		public static void UnpackScreenCaptureData(ServiceReference.WindowData data, out Image image, out Rectangle bounds)
		{
			var ms = new MemoryStream(data.Image, 0, data.Image.Length);
			bounds = new Rectangle();
			image = null;
			if (data.Image != null)
			{
				ms.Write(data.Image, 0, data.Image.Length);
				image = Image.FromStream(ms, true);
				bounds = new Rectangle(data.Left, data.Top, data.Right, data.Bottom);
			}
		}

		public static CursorData PackCursorCaptureData(Image image, int cursorX, int cursorY)
		{
			var result = new CursorData {X = cursorX, Y = cursorY};
			if (image != null)
			{
				using (var ms = new MemoryStream())
				{
					image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
					result.Image = ms.ToArray();
				}
			}
			return result;
		}

		public static void UnpackCursorCaptureData(ServiceReference.CursorData data, out Image image, out int cursorX, out int cursorY)
		{
			// Unpack the data that is transferred over the wire.
			//

			// Create byte arrays to hold the unpacked parts.
			//
			cursorX = data.X;
			cursorY = data.Y;
			image = null;
			if (data.Image != null)
			{
				var ms = new MemoryStream(data.Image, 0, data.Image.Length);
				ms.Write(data.Image, 0, data.Image.Length);
				image = Image.FromStream(ms, true);
			}
		}

		public static void UpdateScreen(ref Image screen, Image newPartialScreen, Rectangle boundingBox)
		{
			// Create the first screen if one does not exist.
			//
			if (screen == null)
			{
				screen = new Bitmap(boundingBox.Width, boundingBox.Height);
			}

			// Draw the partial image into the current
			//	screen. This replaces the changed pixels.
			//
			Graphics g = null;
			try
			{
				lock (screen)
				{
					g = Graphics.FromImage(screen);
					g.DrawImageUnscaled(newPartialScreen, boundingBox);
					g.Flush();
				}
			}
			catch
			{
				// Do something with this info.
			}
			finally
			{
				if (g != null) g.Dispose();
			}
		}

		public static Image MergeScreenAndCursor(Image screen, Image cursor, int cursorX, int cursorY)
		{
			Image mergedImage = null;
			Graphics g = null;
			try
			{
				lock (screen)
				{
					mergedImage = (Image)screen.Clone();
				}
				Rectangle r;
				lock (cursor)
				{
					r = new Rectangle(cursorX, cursorY, cursor.Width, cursor.Height);
				}
				g = Graphics.FromImage(mergedImage);
				g.DrawImage(cursor, r);
				g.Flush();
			}
			catch (Exception)
			{
				// Do something with this info.
			}
			finally
			{
				if (g != null)
				{
					g.Dispose();
				}
			}

			return mergedImage;
		}

		public static NetTcpBinding GetTcpBinding()
		{
			var binding = new NetTcpBinding
				{
					MaxBufferPoolSize = 2147483647,
					MaxReceivedMessageSize = 2147483647,
					ReceiveTimeout = new TimeSpan(0, 0, 1, 0),
					SendTimeout = new TimeSpan(0, 0, 1, 0),
					OpenTimeout = new TimeSpan(0, 0, 1, 0),
					ReaderQuotas =
						{
							MaxArrayLength = 2147483647,
							MaxDepth = 2147483647,
							MaxStringContentLength = 2147483647,
							MaxBytesPerRead = 2147483647,
							MaxNameTableCharCount = 2147483647
						},
					ReliableSession = {Enabled = true, InactivityTimeout = new TimeSpan(0, 0, 1, 0)},
					Security = {Mode = SecurityMode.None}
				};

			return binding;
		}

		public static WSHttpBinding GetHttpBinding()
		{
			var binding = new WSHttpBinding
			{
				MaxBufferPoolSize = 2147483647,
				MaxReceivedMessageSize = 2147483647,
				MessageEncoding = WSMessageEncoding.Mtom,
				TextEncoding = System.Text.Encoding.UTF8,
				ReceiveTimeout = new TimeSpan(0, 0, 1, 0),
				SendTimeout = new TimeSpan(0, 0, 1, 0),
				OpenTimeout = new TimeSpan(0, 0, 1, 0),
				ReaderQuotas =
				{
					MaxArrayLength = 2147483647,
					MaxDepth = 2147483647,
					MaxStringContentLength = 2147483647,
					MaxBytesPerRead = 2147483647,
					MaxNameTableCharCount = 2147483647
				},
				ReliableSession = { Enabled = true, InactivityTimeout = new TimeSpan(0, 0, 1, 0) },
				Security = { Mode = SecurityMode.None , Transport = {ClientCredentialType = HttpClientCredentialType.None}}
			};

			return binding;
		}


		public static IPAddress LocalIPAddress()
		{
			if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
			{
				return null;
			}

			var host = Dns.GetHostAddresses(Dns.GetHostName());

			return host.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
		}

		public static void RegisterProgramInStartup()
		{
			try
			{
				var rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				var args = string.Format("{0} start", Application.ExecutablePath);
				rkApp.SetValue("RemoteViewer", args);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc.ToString());
			}
		}

		public static string ApplicationPath
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		public static string[] ConvertToArray(StringCollection filesExclude)
		{
			var files = new string[filesExclude.Count];
			var index = 0;

			foreach (var file in filesExclude)
			{
				files[index] = file;
				index++;
			}
			return files;
		}
	}
}
