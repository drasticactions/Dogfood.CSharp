using System;
using ARKit;
using Vision;
using UIKit;
using Foundation;
using CoreGraphics;
using AVFoundation;
using CoreAnimation;
using CoreFoundation;
using CoreMedia;
using CoreVideo;
using CoreImage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace Dogfood.CSharp.Vision
{
    public class TextCameraViewController : UIViewController, IAVCaptureVideoDataOutputSampleBufferDelegate 
    {
        public TextCameraViewController()
        {
        }

        VNDetectTextRectanglesRequest textDetection = new VNDetectTextRectanglesRequest(null);
        VNSequenceRequestHandler textDetectionRequest = new VNSequenceRequestHandler();

		static AVCaptureSession session;
		CAShapeLayer shapeLayer = new CAShapeLayer();

		Lazy<AVCaptureVideoPreviewLayer> previewLayer = new Lazy<AVCaptureVideoPreviewLayer>(() =>
		{
			if (session == null)
				return null;

			var previewLayer = new AVCaptureVideoPreviewLayer(session)
			{
				VideoGravity = AVLayerVideoGravity.ResizeAspectFill
			};
			return previewLayer;
		});

		AVCaptureDevice backCamera = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video, AVCaptureDevicePosition.Back);

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			PrepareSession();
			session.StartRunning();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			previewLayer.Value.Frame = View.Frame;
			shapeLayer.Frame = View.Frame;
		}

		public override void ViewDidAppear(bool animated)
		{

			base.ViewDidAppear(animated);

			if (!previewLayer.IsValueCreated)
				return;

			View.Layer.AddSublayer(previewLayer.Value);

			shapeLayer.StrokeColor = UIColor.Red.CGColor;
			shapeLayer.LineWidth = 2;

			//// Needs to filp coordinate system for Vision
			//shapeLayer.AffineTransform = CGAffineTransform.MakeScale(-1, -1);
			View.Layer.AddSublayer(shapeLayer);
		}

		void PrepareSession()
		{
            textDetection.ReportCharacterBoxes = true;
			session = new AVCaptureSession();
			var captureDevice = backCamera;

			if (session == null || captureDevice == null)
				return;

			try
			{
				var deviceInput = new AVCaptureDeviceInput(captureDevice, out var deviceInputError);
				if (deviceInputError != null)
					throw new NSErrorException(deviceInputError);

				session.BeginConfiguration();

				if (session.CanAddInput(deviceInput))
					session.AddInput(deviceInput);

				var output = new AVCaptureVideoDataOutput
				{
					WeakVideoSettings = new CVPixelBufferAttributes { PixelFormatType = CVPixelFormatType.CV32BGRA }.Dictionary,
					AlwaysDiscardsLateVideoFrames = true
				};

				if (session.CanAddOutput(output))
					session.AddOutput(output);

				session.CommitConfiguration();

				var queue = new DispatchQueue("output.queue");
				output.SetSampleBufferDelegateQueue(this, queue);

				Console.WriteLine($"PrepareSession: Done setting up delegate");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"PrepareSession Error: {ex.Message}");
			}
		}

		[Export("captureOutput:didOutputSampleBuffer:fromConnection:")]
		public void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			using (var pixelBuffer = sampleBuffer.GetImageBuffer())
			using (var attachments = sampleBuffer.GetAttachments<NSString, NSObject>(CMAttachmentMode.ShouldPropagate))
			using (var ciimage = new CIImage(pixelBuffer, attachments))
			using (var ciImageWithOrientation = ciimage.CreateWithOrientation(CIImageOrientation.RightTop))
			{
				DetectText(ciImageWithOrientation);
			}
			// make sure we do not run out of sampleBuffers
			sampleBuffer.Dispose();
		}

        void DetectText(CIImage image)
        {
            textDetectionRequest.Perform(new VNRequest[] { textDetection }, image, out var performError);
            var results = textDetection.GetResults<VNTextObservation>() ?? Array.Empty<VNTextObservation>();
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (shapeLayer.Sublayers != null) {
                    shapeLayer.Sublayers = new CALayer[0];
                }

                foreach (var result in results)
                {
                    if (result.CharacterBoxes == null || result.CharacterBoxes.Length <= 0) continue;
                    HighlightWord(result);

                    foreach (var box in result.CharacterBoxes)
                    {
                        HighlightLetters(box);
                    }
                }
            });
        }

        void HighlightWord(VNTextObservation box) {
            var maxX = 9999.0;
            var minX = 0.0;
            var maxY = 9999.0;
            var minY = 0.0;

            foreach(var characterBox in box.CharacterBoxes) {
                if (characterBox.BottomLeft.X < maxX) 
                {
                    maxX = characterBox.BottomLeft.X;
                }
				if (characterBox.BottomRight.X > minX)
				{
					minX = characterBox.BottomRight.X;
				}
				if (characterBox.BottomRight.Y < maxY)
				{
					maxY = characterBox.BottomRight.Y;
				}
				if (characterBox.TopRight.Y > minY)
				{
					minY = characterBox.TopRight.Y;
				}
            }

            var xCord = maxX * shapeLayer.Frame.Size.Width;
            var yCord = (1 - minY) * shapeLayer.Frame.Size.Height;
            var width = (minX - maxX) * shapeLayer.Frame.Size.Width;
            var height = (minY - maxY) * shapeLayer.Frame.Size.Height;

            Draw((float)xCord, (float)yCord, (float)width, (float)height, UIColor.Red);
        }

        void HighlightLetters(VNRectangleObservation box) {
            var xCord = box.TopLeft.X * shapeLayer.Frame.Size.Width;
            var yCord = (1 - box.TopRight.Y) * shapeLayer.Frame.Size.Height;
            var width = (box.TopRight.X - box.BottomLeft.X) * shapeLayer.Frame.Size.Width;
            var height = (box.TopLeft.Y - box.BottomLeft.Y) * shapeLayer.Frame.Size.Height;

            Draw((float)xCord, (float)yCord, (float)width, (float)height, UIColor.Blue); 
        }

        void Draw(float x, float y, float width, float height, UIColor color) {
			var outline = new CALayer();
			outline.Frame = new CGRect(x, y, width, height);
			outline.BorderColor = color.CGColor;
			outline.BorderWidth = 2.0f;
			shapeLayer.AddSublayer(outline);
        }
    }
}
