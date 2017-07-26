using System;
using CoreGraphics;
using CoreImage;
using CoreMedia;
using CoreVideo;
using UIKit;

namespace Dogfood.CSharp
{
	public static class ImageUtils
	{
		public static UIImage CreateImage(CMSampleBuffer sampleBuffer)
		{
			using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
			{
				// Lock the base address
				pixelBuffer.Lock(CVPixelBufferLock.None);
				// Get the number of bytes per row for the pixel buffer
				var baseAddress = pixelBuffer.BaseAddress;
				int bytesPerRow = (int)pixelBuffer.BytesPerRow;
				int width = (int)pixelBuffer.Width;
				int height = (int)pixelBuffer.Height;
				var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
				// Create a CGImage on the RGB colorspace from the configured parameter above
				using (var cs = CGColorSpace.CreateDeviceRGB())
				using (var context = new CGBitmapContext(baseAddress, width, height, 8, bytesPerRow, cs, (CGImageAlphaInfo)flags))
				using (var cgImage = context.ToImage())
				{
					pixelBuffer.Unlock(CVPixelBufferLock.None);
					return UIImage.FromImage(cgImage);
				}
			}
		}

		public static UIImage CreateImage(CIImage ciImage)
		{
			var context = CIContext.FromOptions(null);
			var cgimage = context.CreateCGImage(ciImage, ciImage.Extent);
			return UIImage.FromImage(cgimage);
		}

		public static UIImage Resize(this UIImage self, CGSize newSize)
		{
			UIGraphics.BeginImageContextWithOptions(new CGSize(width: newSize.Width, height: newSize.Height), true, 1.0f);
			self.Draw(new CGRect(x: 0, y: 0, width: newSize.Width, height: newSize.Height));
			var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			return resizedImage;
		}

		public static CVPixelBuffer ToPixelBuffer(this UIImage self)
		{
			var width = self.Size.Width;

			var height = self.Size.Height;

			var attrs = new CVPixelBufferAttributes();
			attrs.CGBitmapContextCompatibility = true;
			attrs.CGImageCompatibility = true;

			var resultPixelBuffer = new CVPixelBuffer((int)(width),
					(int)(height),
					CVPixelFormatType.CV32ARGB,
											 attrs);

			resultPixelBuffer.Lock(CVPixelBufferLock.None);
			var pixelData = resultPixelBuffer.GetBaseAddress(0);

			var rgbColorSpace = CGColorSpace.CreateDeviceRGB();

			var context = new CGBitmapContext(data: pixelData,
										  width: (int)(width),
										  height: (int)(height),
										  bitsPerComponent: 8,
									bytesPerRow: resultPixelBuffer.GetBytesPerRowOfPlane(0),
										  colorSpace: rgbColorSpace,
											   bitmapInfo: CGImageAlphaInfo.NoneSkipFirst);

			context.TranslateCTM(tx: 0, ty: height);
			context.ScaleCTM(sx: 1.0f, sy: -1.0f);

			UIGraphics.PushContext(context);

			self.Draw(new CGRect(x: 0, y: 0, width: width, height: height));

			UIGraphics.PopContext();

			resultPixelBuffer.Unlock(CVPixelBufferLock.None);

			return resultPixelBuffer;
		}
	}
}
