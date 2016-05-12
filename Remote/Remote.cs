using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Remote
{
	public class Screen
	{
		//public static BitmapImage bmi;
		public Screen()
		{
		}
		public static void Capture(int width, int height, ref Bitmap bmpScreenCapture, System.Drawing.Imaging.PixelFormat format)
		{
			/*using (Bitmap */
			bmpScreenCapture = new Bitmap(width, height, format);/*)*/
			//{
				using (Graphics g = Graphics.FromImage(bmpScreenCapture))
				{
					//g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.
					g.CopyFromScreen(0,
										0,
										0, 0,
										bmpScreenCapture.Size,
										CopyPixelOperation.SourceCopy);
				}
				//using (FileStream ms = new FileStream(@"H:\" + DateTime.Now.Ticks + ".png", FileMode.Create))
				/*using (MemoryStream ms = new MemoryStream())
				{
					bmpScreenCapture.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
					/*BitmapImage bmi = new BitmapImage();
					bmi.BeginInit();
					bmi.CacheOption = BitmapCacheOption.OnLoad;
					ms.Position = 0;
					bmi.StreamSource = ms;
					bmi.EndInit();					
					Image_Test.Source = bmi;*/
				//}
				//ptr = bmpScreenCapture.GetHbitmap();
				//Image_Test.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				//ptr,
				//IntPtr.Zero,
				//Int32Rect.Empty,
				//BitmapSizeOptions.FromEmptyOptions());
				//bmpScreenCapture.Save(@"H:\" + DateTime.Now.Ticks + ".png", ImageFormat.Png);
				//DeleteObject(ptr);
				//return bmpScreenCapture;
			//}
		}
		public static void Capture2(int width, int height, ref Bitmap bmpScreenCapture)
		{
			//Rectangle r = Screen.PrimaryScreen.Bounds;

			//using (Bitmap bitmap = new Bitmap(width, height))
			//{
			bmpScreenCapture = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
				const int SRCCOPY = 13369376;

				using (Graphics g = Graphics.FromImage(bmpScreenCapture))
				{
					// Get a device context to the windows desktop and our destination  bitmaps
					int hdcSrc = GDI.GetDC(0);
					IntPtr hdcDest = g.GetHdc();

					// Copy what is on the desktop to the bitmap
					GDI.BitBlt(hdcDest.ToInt32(), 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);

					// Release device contexts
					g.ReleaseHdc(hdcDest);
					GDI.ReleaseDC(0, hdcSrc);

					//string formatExtension = screenshotImageFormat.ToString().ToLower();
					//string expectedExtension = string.Format(".{0}", formatExtension);

					/*if (Path.GetExtension(fileName) != expectedExtension)
					{
						fileName += expectedExtension;
					}*/

					/*switch (formatExtension)
					{
						case "jpeg":
							BitmapToJPEG(bitmap, fileName, 80);
							break;
						default:
							bitmap.Save(fileName, screenshotImageFormat);
							break;
					}*/

					// Save the complete path/filename of the screenshot for possible later use
					//ScreenshotFullPath = fileName;


				}
			//}
		}
		/*public static async Task<Bitmap> CaptureAsync(int width, int height, Bitmap image)
		{
			return Capture(width, height, image);
		}*/
		//public static async /*Task<BitmapImage>*/void CompressImage(Bitmap image)
		//{
		//	bmi = await Task.Run(() => CompressImageWorker(image));
			//image.Dispose();
			//return bmi;
		//}
		public static async Task<BitmapImage> CompressImageAsync(Bitmap image)
		{
			BitmapImage bmi = new BitmapImage();
			//bmi = await Task.Run(() => CompressImageWorker(image));
			//image.Dispose();
			/*image.Save(@"H:\" + DateTime.Now.Ticks + ".png",System.Drawing.Imaging.ImageFormat.Jpeg);
			using (MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
				bmi.BeginInit();
				bmi.CacheOption = BitmapCacheOption.OnLoad;
				ms.Position = 0;
				bmi.StreamSource = ms;
				bmi.EndInit();
			}*/
			return bmi;
		}
		/*private static BitmapImage CompressImageWorker(Bitmap image)
		{
			//image.Save(@"H:\" + DateTime.Now.Ticks + ".png", System.Drawing.Imaging.ImageFormat.Jpeg);
			BitmapImage bmi = new BitmapImage();
			using (MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
				//bmi.BeginInit();
				//bmi.CacheOption = BitmapCacheOption.OnLoad;
				//ms.Position = 0;
				//bmi.StreamSource = ms;
				//bmi.EndInit();
			}
			return bmi;
		}*/
		public static byte[] CompressImageLZ4(Bitmap image)
		{
			byte[] arr = /*await Task.Run(() => */CompressImageWorkerToArray(image)/*)*/;
			//return LZ4.LZ4Codec.Encode(arr, 0, arr.Length);
			return arr;
		}
		private static byte[] CompressImageWorkerToArray(Bitmap image)
		{
			//image.Save("G:/test_" + Environment.TickCount + ".bmp", ImageFormat.Bmp);
			using (MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
				return ms.ToArray();
			}
		}
		private static int[] arr11;
		private static int[] arr22;
		private static int[] arr;
		private static byte[] arr0;
		public static byte[] ReturnDifference(byte[] arr1, byte[] arr2, ref int count)
		{
			Buffer.BlockCopy(arr1, 0, arr11, 0, arr.Length * 4);
			Buffer.BlockCopy(arr2, 0, arr22, 0, arr.Length * 4);
			int diff = arr1.Length - arr.Length * 4;
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = arr11[i] ^ arr22[i];
				/*if (arr11[i] == arr22[i])
				{
					if (arr[i] != 0) count++;
				}*/
				if (arr[i] != 0)
				{
					count++;
				}
			}
			Buffer.BlockCopy(arr, 0, arr0, 0, arr.Length);
			return LZ4.LZ4Codec.Encode(arr0, 0, arr.Length);
		}
		public static void InitReturnDifference(int length)
		{
			arr11 = new int[length / 4];
			arr22 = new int[length / 4];
			arr = new int[arr11.Length];
			arr0 = new byte[arr.Length * 4];
		}
		private static byte[] CompressImageWorkerLZ45(Bitmap image)
		{
			Rectangle rec = new Rectangle(0, 0, image.Width, image.Height);
			BitmapData bitmapData = image.LockBits(rec, ImageLockMode.ReadWrite, image.PixelFormat);

			int bytesPerPixel = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
			int byteCount = bitmapData.Stride * image.Height;
			byte[] pixels = new byte[byteCount];
			IntPtr ptrFirstPixel = bitmapData.Scan0;
			Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
			image.UnlockBits(bitmapData);

			return pixels;
		}
		//public static class Composition
		//{
		const uint DWM_EC_DISABLECOMPOSITION = 0;
		const uint DWM_EC_ENABLECOMPOSITION = 1;
 
		[DllImport("dwmapi.dll", EntryPoint = "DwmEnableComposition")]
		extern static uint DwmEnableComposition(uint compositionAction);
 
		/// <summary>
		/// Enable/Disable DWM composition (aka Aero)
		/// </summary>
		/// <param name="enable">True to enable composition, false to disable composition.</param>
		/// <returns>True if the operation was successful.</returns>
		public static bool EnableComposition(bool enable)
		{
			try
			{
				if (enable)
				{
					DwmEnableComposition(DWM_EC_ENABLECOMPOSITION);
				}
				else
				{
					DwmEnableComposition(DWM_EC_DISABLECOMPOSITION);
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
		//}
	}
	public class GDI
	{
		[DllImport("gdi32", EntryPoint = "BitBlt", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int BitBlt(int hDestDC, int x, int y, int nWidth, int nHeight, int hSrcDC, int xSrc, int ySrc, int dwRop);
		[DllImport("user32", EntryPoint = "GetDC", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int GetDC(int hwnd);
		[DllImport("user32", EntryPoint = "ReleaseDC", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int ReleaseDC(int hwnd, int hdc);
	}

	public abstract class Window
	{
		// Baesd on: http://stackoverflow.com/questions/4372055/detect-active-window-changed-using-c-sharp-without-polling
		private WinEventDelegate dele = null;

		public void Init()
		{
			dele = new WinEventDelegate(WinEventProc);
			IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
		}

		public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll")]
		static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		private const uint WINEVENT_OUTOFCONTEXT = 0;
		private const uint EVENT_SYSTEM_FOREGROUND = 3;

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		public string GetActiveWindowTitle()
		{
			const int nChars = 256;
			IntPtr handle = IntPtr.Zero;
			StringBuilder Buff = new StringBuilder(nChars);
			handle = GetForegroundWindow();

			if (GetWindowText(handle, Buff, nChars) > 0)
			{
				return Buff.ToString();
			}
			return null;
		}

		public abstract void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
	}
	public class RemoteDesktopHost : Window
	{
		private Settings.RemoteDesktopSettings settings;
		public SOCKET.Client clientStream;
		public SOCKET.Client clientInput;

		public RemoteDesktopHost(Settings.RemoteDesktopSettings s)
		{
			settings = s;
		}

		public bool OnReceiveAsync(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
			return false;
		}

		public bool OnReceiveAsyncInput(System.Net.Sockets.SocketAsyncEventArgs arg)
		{
			return false;
		}

		public override void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			
		}

	}
	public static class Remote
	{

	}
}
