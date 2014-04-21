using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ScreenshotCaptureWithMouse.ScreenCapture;

namespace RV.ClientServer.Services
{
	internal class CaptureScreen
	{

		public struct SIZE
		{
			public int cx;
			public int cy;
		}

		internal Bitmap CaptureCursor(ref int cursorX, ref int cursorY)
		{
			//cursorX = Cursor.Position.X;
			//cursorY = Cursor.Position.Y;
			//var icon = Icon.FromHandle(Cursor.Current.Handle);
			//var cursorBmp = icon.ToBitmap();
			//return cursorBmp;

			Bitmap bmp;
			IntPtr hicon;
			Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
			Win32Stuff.ICONINFO icInfo;
			ci.cbSize = Marshal.SizeOf(ci);
			if (Win32Stuff.GetCursorInfo(out ci))
			{
				if (ci.flags == Win32Stuff.CURSOR_SHOWING)
				{
					hicon = Win32Stuff.CopyIcon(ci.hCursor);
					if (Win32Stuff.GetIconInfo(hicon, out icInfo))
					{
						cursorX = ci.ptScreenPos.x - ((int)icInfo.xHotspot);
						cursorY = ci.ptScreenPos.y - ((int)icInfo.yHotspot);

						Icon ic = Icon.FromHandle(hicon);
						bmp = ic.ToBitmap();
						return bmp;
					}
				}
			}

			return null;
		}

		public static Bitmap CaptureDesktop()
		{
			//var desktopBmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

			//var graphics = Graphics.FromImage(desktopBmp as Image);

			//graphics.CopyFromScreen(0, 0, 0, 0, desktopBmp.Size);
			//return desktopBmp;

			SIZE size;
			IntPtr hBitmap;
			IntPtr hDC = Win32Stuff.GetDC(Win32Stuff.GetDesktopWindow());
			IntPtr hMemDC = GDIStuff.CreateCompatibleDC(hDC);

			size.cx = Win32Stuff.GetSystemMetrics
					  (Win32Stuff.SM_CXSCREEN);

			size.cy = Win32Stuff.GetSystemMetrics
					  (Win32Stuff.SM_CYSCREEN);

			hBitmap = GDIStuff.CreateCompatibleBitmap(hDC, size.cx, size.cy);

			if (hBitmap != IntPtr.Zero)
			{
				IntPtr hOld = (IntPtr)GDIStuff.SelectObject
									   (hMemDC, hBitmap);

				GDIStuff.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC,
											   0, 0, GDIStuff.SRCCOPY);

				GDIStuff.SelectObject(hMemDC, hOld);
				GDIStuff.DeleteDC(hMemDC);
				Win32Stuff.ReleaseDC(Win32Stuff.GetDesktopWindow(), hDC);
				Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap);
				GDIStuff.DeleteObject(hBitmap);
				GC.Collect();
				return bmp;
			}
			return null;
		}
	}
}
