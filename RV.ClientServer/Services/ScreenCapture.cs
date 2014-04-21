using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RV.ClientServer.Services
{
	public class ScreenCapture
	{
		private Bitmap _prevBitmap;
		private Bitmap _newBitmap = new Bitmap(1, 1);

		/// <summary>
		/// Returns current screen bitmap, and its size.
		/// </summary>
		public Bitmap GetFullBitmap(ref Rectangle bounds)
		{
			Bitmap fullImage;
			lock (_newBitmap)
			{
				bounds = new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height);
				fullImage = _newBitmap.Clone(bounds, _newBitmap.PixelFormat);
			}
			return fullImage;
		}

		/// <summary>
		/// Capture the changes to the screen since the last
		/// capture.
		/// </summary>
		/// <param name="bounds">The bounding box that encompasses
		/// all changed pixels.</param>
		/// <returns>Full or partial bitmap, null for no changes</returns>
		public Bitmap Screen(ref Rectangle bounds)
		{
			Bitmap diff = null;

			// Capture a new screenshot.
			//
			lock (_newBitmap)
			{
				_newBitmap = CaptureScreen.CaptureDesktop();

				// If we have a previous screenshot, only send back
				//	a subset that is the minimum rectangular area
				//	that encompasses all the changed pixels.
				//
				if (_prevBitmap != null)
				{
					// Get the bounding box.
					//
					bounds = GetBoundingBoxForChanges(ref _prevBitmap, ref _newBitmap);
					if (bounds != Rectangle.Empty)
					{
						// Get the minimum rectangular area
						//
						//diff = new Bitmap(bounds.Width, bounds.Height);
						diff = _newBitmap.Clone(bounds, _newBitmap.PixelFormat);

						// Set the current bitmap as the previous to prepare
						//	for the next screen capture.
						//
						_prevBitmap = _newBitmap;
					}
				}
					// We don't have a previous screen capture. Therefore
					//	we need to send back the whole screen this time.
					//
				else
				{
					// Create a bounding rectangle.
					//
					bounds = new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height);

					// Set the previous bitmap to the current to prepare
					//	for the next screen capture.
					//
					_prevBitmap = _newBitmap;
					diff = _newBitmap.Clone(bounds, _newBitmap.PixelFormat);
				}
			}
			return diff;
		}

		/// <summary>
		/// Capture the cursor bitmap.
		/// </summary>
		/// <param name="cursorX">The cursor X.</param>
		/// <param name="cursorY">The cursor Y.</param>
		/// <returns>The bitmap or null.</returns>
		public Bitmap Cursor(ref int cursorX, ref int cursorY)
		{
			int screenWidth = 1;
			int screenHeight = 1;
			lock (_newBitmap)
			{
				try
				{
					screenWidth = _newBitmap.Width;
					screenHeight = _newBitmap.Height;
				}
				catch (Exception)
				{
					// Need to debug the exception!
				}
			}
			if (screenWidth == 1 && screenHeight == 1)
			{
				return null;
			}
			Bitmap img = new CaptureScreen().CaptureCursor(ref cursorX, ref cursorY);
			if (img != null && cursorX < screenWidth && cursorY < screenHeight)
			{
				// The cursor is mostly transparent. This makes it difficult
				//	to see when the cursor is the text editing icon. Easy
				//	fix is to make the cursor slighly less transparent.
				// 
				int width = img.Width;
				int height = img.Height;

				// Get the bitmap data.
				//
				BitmapData imgData = img.LockBits(
					new Rectangle(0, 0, width, height),
					ImageLockMode.ReadOnly, img.PixelFormat);

				// The images are ARGB (4 bytes)
				//
				const int numBytesPerPixel = 4;

				// Get the number of integers (4 bytes) in each row
				//	of the image.
				//
				int stride = imgData.Stride;
				IntPtr scan0 = imgData.Scan0;
				unsafe
				{
					// Cast the safe pointers into unsafe pointers.
					//
					byte* pByte = (byte*) (void*) scan0;
					for (int h = 0; h < height; h++)
					{
						for (int w = 0; w < width; w++)
						{
							int offset = h*stride + w*numBytesPerPixel + 3;
							if (*(pByte + offset) == 0)
							{
								*(pByte + offset) = 60;
							}
						}
					}
				}
				img.UnlockBits(imgData);

				return img;
			}
			return null;
		}

		/// <summary>
		/// Gets the bounding box for changes.
		/// </summary>
		/// <returns></returns>
		public static Rectangle GetBoundingBoxForChanges(ref Bitmap _prevBitmap, ref Bitmap _newBitmap)
		{
			// The search algorithm starts by looking
			//	for the top and left bounds. The search
			//	starts in the upper-left corner and scans
			//	left to right and then top to bottom. It uses
			//	an adaptive approach on the pixels it
			//	searches. Another pass is looks for the
			//	lower and right bounds. The search starts
			//	in the lower-right corner and scans right
			//	to left and then bottom to top. Again, an
			//	adaptive approach on the search area is used.
			//

			// Notice: The GetPixel member of the Bitmap class
			//	is too slow for this purpose. This is a good
			//	case of using unsafe code to access pointers
			//	to increase the speed.
			//

			// Validate the images are the same shape and type.
			//
			if (_prevBitmap.Width != _newBitmap.Width ||
			    _prevBitmap.Height != _newBitmap.Height ||
			    _prevBitmap.PixelFormat != _newBitmap.PixelFormat)
			{
				// Not the same shape...can't do the search.
				//
				return Rectangle.Empty;
			}

			// Init the search parameters.
			//
			int width = _newBitmap.Width;
			int height = _newBitmap.Height;
			int left = width;
			int right = 0;
			int top = height;
			int bottom = 0;

			BitmapData bmNewData = null;
			BitmapData bmPrevData = null;
			try
			{
				// Lock the bits into memory.
				//
				bmNewData = _newBitmap.LockBits(
					new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height),
					ImageLockMode.ReadOnly, _newBitmap.PixelFormat);
				bmPrevData = _prevBitmap.LockBits(
					new Rectangle(0, 0, _prevBitmap.Width, _prevBitmap.Height),
					ImageLockMode.ReadOnly, _prevBitmap.PixelFormat);

				// The images are ARGB (4 bytes)
				//
				const int numBytesPerPixel = 4;

				// Get the number of integers (4 bytes) in each row
				//	of the image.
				//
				int strideNew = bmNewData.Stride/numBytesPerPixel;
				int stridePrev = bmPrevData.Stride/numBytesPerPixel;

				// Get a pointer to the first pixel.
				//
				// Notice: Another speed up implemented is that I don't
				//	need the ARGB elements. I am only trying to detect
				//	change. So this algorithm reads the 4 bytes as an
				//	integer and compares the two numbers.
				//
				IntPtr scanNew0 = bmNewData.Scan0;
				IntPtr scanPrev0 = bmPrevData.Scan0;

				// Enter the unsafe code.
				//
				unsafe
				{
					// Cast the safe pointers into unsafe pointers.
					//

					var pNew = (int*) scanNew0.ToPointer();
					var pPrev = (int*) scanPrev0.ToPointer();
					for (int y = 0; y < _newBitmap.Height; ++y)
					{
						// For pixels up to the current bound (left to right)
						//
						for (int x = 0; x < left; ++x)
						{
							// Use pointer arithmetic to index the
							//	next pixel in this row.
							//
							var test1 = (pNew + x)[0];
							var test2 = (pPrev + x)[0];
							var b1 = test1 & 0xff;
							var g1 = (test1 & 0xff00) >> 8;
							var r1 = (test1 & 0xff0000) >> 16;
							var a1 = (test1 & 0xff000000) >> 24;

							var b2 = test2 & 0xff;
							var g2 = (test2 & 0xff00) >> 8;
							var r2 = (test2 & 0xff0000) >> 16;
							var a2 = (test2 & 0xff000000) >> 24;
							if (b1 != b2 || g1 != g2 || r1 != r2 || a1 != a2)
							{
								if (left > x)
									left = x;
								if (top > y)
									top = y;
							}
						}

						// Move the pointers to the next row.
						//
						pNew += strideNew;
						pPrev += stridePrev;
					}

					pNew = (int*) scanNew0.ToPointer();
					pPrev = (int*) scanPrev0.ToPointer();
					pNew += (_newBitmap.Height - 1)*strideNew;
					pPrev += (_prevBitmap.Height - 1)*stridePrev;

					for (int y = _newBitmap.Height - 1; y > top; y--)
					{
						for (int x = _newBitmap.Width - 1; x > left; x--)
						{

							var test1 = (pNew + x)[0];
							var test2 = (pPrev + x)[0];
							var b1 = test1 & 0xff;
							var g1 = (test1 & 0xff00) >> 8;
							var r1 = (test1 & 0xff0000) >> 16;
							var a1 = (test1 & 0xff000000) >> 24;

							var b2 = test2 & 0xff;
							var g2 = (test2 & 0xff00) >> 8;
							var r2 = (test2 & 0xff0000) >> 16;
							var a2 = (test2 & 0xff000000) >> 24;
							if (b1 != b2 || g1 != g2 || r1 != r2 || a1 != a2)
							{
								if (x > right)
								{
									right = x;
								}
								if (y > bottom)
								{
									bottom = y;
								}
							}
						}

						pNew -= strideNew;
						pPrev -= stridePrev;
					}

				}
			}
			catch (Exception)
			{
				// Do something with this info.
			}
			finally
			{
				// Unlock the bits of the image.
				//
				if (bmNewData != null)
				{
					_newBitmap.UnlockBits(bmNewData);
				}
				if (bmPrevData != null)
				{
					_prevBitmap.UnlockBits(bmPrevData);
				}
			}

			// Validate we found a bounding box. If not
			//	return an empty rectangle.
			//
			int diffImgWidth = right - left + 1;
			int diffImgHeight = bottom - top + 1;
			if (diffImgHeight < 0 || diffImgWidth < 0)
			{
				// Nothing changed
				return Rectangle.Empty;
			}

			// Return the bounding box.
			//
			return new Rectangle(left, top, diffImgWidth, diffImgHeight);
		}
	}
}