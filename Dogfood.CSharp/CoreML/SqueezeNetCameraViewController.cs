using System;
using UIKit;
using Foundation;
using CoreGraphics;
using AVFoundation;
using CoreFoundation;
using CoreMedia;
using CoreVideo;
using CoreML;
using XibFree;

namespace Dogfood.CSharp.CoreML
{
	public class SqueezeNetCameraViewController : UIViewController, IAVCaptureVideoDataOutputSampleBufferDelegate
	{
		public SqueezeNetCameraViewController()
		{
			LoadModel();
		}
		static AVCaptureSession session;
		MLModel model;

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

		UILabel uiMessage;
		AVCaptureDevice frontCamera = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video, AVCaptureDevicePosition.Back);

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
		}

		public override void ViewDidAppear(bool animated)
		{

			base.ViewDidAppear(animated);

			if (!previewLayer.IsValueCreated)
				return;

			RotateVideo();

			var layout = new LinearLayout(Orientation.Vertical)
			{
				SubViews = new View[]
				{
					new LinearLayout(Orientation.Vertical) {
						LayoutParameters = new LayoutParameters()
									{
										Width = AutoSize.FillParent,
										Height = AutoSize.FillParent,
									},
						SubViews = new View[] {
							new LinearLayout(Orientation.Vertical) {
								Layer = previewLayer.Value,
								LayoutParameters = new LayoutParameters()
									{
										Width = AutoSize.FillParent,
										Height = AutoSize.FillParent,
									}
							},
							new NativeView() {
								View = uiMessage = new UILabel() {
									Text = "",
									Font = UIFont.SystemFontOfSize(25),
									BackgroundColor = UIColor.Black,
									TextColor = UIColor.White,
									Lines = 0
								},
								LayoutParameters = new LayoutParameters()
								{
									Width = AutoSize.FillParent,
									Height = AutoSize.WrapContent,
									Gravity = Gravity.CenterHorizontal
								}
							}
						}
					}
				},
			};
			this.View = new XibFree.UILayoutHost(layout);
		}

		public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
		{
			base.ViewWillTransitionToSize(toSize, coordinator);

			if (!previewLayer.IsValueCreated) return;

			RotateVideo();
		}

		void RotateVideo()
		{
			var videoPreviewLayerConnection = previewLayer.Value.Connection;
			if (videoPreviewLayerConnection != null)
			{
				var deviceOrientation = UIDevice.CurrentDevice.Orientation;

				AVCaptureVideoOrientation newVideoOrientation;
				if (!TryConvertToVideoOrientation(deviceOrientation, out newVideoOrientation))
					return;
				if (!deviceOrientation.IsPortrait() && !deviceOrientation.IsLandscape())
					return;

				videoPreviewLayerConnection.VideoOrientation = newVideoOrientation;
			}
		}

		static bool TryConvertToVideoOrientation(UIDeviceOrientation orientation, out AVCaptureVideoOrientation result)
		{
			switch (orientation)
			{
				case UIDeviceOrientation.Portrait:
					result = AVCaptureVideoOrientation.Portrait;
					return true;

				case UIDeviceOrientation.PortraitUpsideDown:
					result = AVCaptureVideoOrientation.PortraitUpsideDown;
					return true;

				case UIDeviceOrientation.LandscapeLeft:
					result = AVCaptureVideoOrientation.LandscapeRight;
					return true;

				case UIDeviceOrientation.LandscapeRight:
					result = AVCaptureVideoOrientation.LandscapeLeft;
					return true;

				default:
					result = 0;
					return false;
			}
		}

		void PrepareSession()
		{
			session = new AVCaptureSession();
			var captureDevice = frontCamera;

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
			using (var image = ImageUtils.CreateImage(sampleBuffer))
			{
				MakePrediction(image);
			}
			sampleBuffer.Dispose();
		}

		IMLFeatureProvider CreateInput(UIImage image)
		{
			var pixelBuffer = image.Resize(new CGSize(227, 227)).ToPixelBuffer();

			var imageValue = MLFeatureValue.FromPixelBuffer(pixelBuffer);

			var inputs = new NSDictionary<NSString, NSObject>(new NSString("image"), imageValue);

			return new MLDictionaryFeatureProvider(inputs, out var error);
		}

		void MakePrediction(UIImage image)
		{
			IMLFeatureProvider input = CreateInput(image);

			var output = model.GetPrediction(input, out var error);

			if (error != null)
			{
				Console.WriteLine($"Error predicting: {error}");
				return;
			}

			var classLabel = output.GetFeatureValue("classLabel").StringValue;

			var message = $"{classLabel.ToUpperInvariant()}: {output.GetFeatureValue("classLabelProbs").DictionaryValue[classLabel]}";

			Console.WriteLine(message);
			BeginInvokeOnMainThread(() =>
			{
				if (uiMessage != null)
					uiMessage.Text = message;
			});
		}

		void LoadModel()
		{
			var modelUrl = NSBundle.MainBundle.GetUrlForResource("SqueezeNet", "mlmodelc");

			model = MLModel.FromUrl(modelUrl, out var error);

			if (error != null)
			{
				Console.WriteLine($"Error writing model: {error}");
			}
			else
			{
				Console.WriteLine($"Loaded model: {model}");
			}
		}
	}
}

