using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RV.UserInterface.Services
{
	public static class Utils
	{
		public static Guid Id = Guid.NewGuid();

		public static System.Windows.Media.Imaging.BitmapImage ConvertImage(Image image)
		{
			//var image = (System.Drawing.Image)value;
			// Winforms Image we want to get the WPF Image from...
			var bitmap = new System.Windows.Media.Imaging.BitmapImage();
			bitmap.BeginInit();
			MemoryStream memoryStream = new MemoryStream();
			// Save to a memory stream...
			image.Save(memoryStream, ImageFormat.Bmp);
			// Rewind the stream...
			memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
			bitmap.StreamSource = memoryStream;
			bitmap.EndInit();
			return bitmap;

		}

	}
}
