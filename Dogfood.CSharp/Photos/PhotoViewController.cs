using System;
using UIKit;
using Photos;
using CoreImage;
using Vision;
using System.Linq;
using CoreGraphics;
using System.Collections.Generic;
using CoreAnimation;

namespace Dogfood.CSharp.Photos
{
	public class PhotoViewController : UIViewController
	{
		UIImageView imageView;
		UIBarButtonItem filterButton;
		CIContext ciContext;
		List<CGRect> rects = new List<CGRect>();

		VNDetectFaceRectanglesRequest faceDetection = new VNDetectFaceRectanglesRequest(null);
		VNDetectFaceLandmarksRequest faceLandmarks = new VNDetectFaceLandmarksRequest(null);
		VNSequenceRequestHandler faceLandmarksDetectionRequest = new VNSequenceRequestHandler();
		VNSequenceRequestHandler faceDetectionRequest = new VNSequenceRequestHandler();

		public PHAsset Asset { get; set; }

		public PhotoViewController()
		{
			imageView = new UIImageView();

			filterButton = new UIBarButtonItem("Finger", UIBarButtonItemStyle.Plain, ApplyFingerFilter);

			NavigationItem.RightBarButtonItem = filterButton;

			ciContext = CIContext.FromOptions(null);
		}

		void ApplyFingerFilter(object sender, EventArgs e)
		{
			Asset.RequestContentEditingInput(new PHContentEditingInputRequestOptions(), (input, options) => {
                
				var image = CIImage.FromUrl(input.FullSizeImageUrl);
				var cspamImage = UIImage.FromFile("cspam.png");

				image = image.CreateWithOrientation((CIImageOrientation)input.FullSizeImageOrientation);
				var cgImage = ciContext.CreateCGImage(image, image.Extent);

				CALayer parentLayer = new CALayer()
				{
					Frame = new CGRect(0, 0, cgImage.Width, cgImage.Height)
				};

				parentLayer.AddSublayer(new CALayer()
				{
					Contents = cgImage,
					Frame = new CGRect(0, 0, cgImage.Width, cgImage.Height),
					Opacity = 1.0f
				});

				DetectFaces(image);

				if (rects.Any())
				{
					foreach (var rect in rects)
					{

						parentLayer.AddSublayer(new CALayer()
						{
							Contents = cspamImage.CGImage,
							Frame = new CGRect(
								rect.X * parentLayer.Frame.Width,
								parentLayer.Frame.Height * (1 - rect.Y) - (rect.Height * parentLayer.Frame.Height),
								rect.Width * parentLayer.Frame.Width,
								rect.Height * parentLayer.Frame.Height
							),
							Opacity = 1.0f
						});
					}
				}

				UIGraphics.BeginImageContextWithOptions(parentLayer.Bounds.Size, false, 0);
				var context = UIGraphics.GetCurrentContext();
				parentLayer.RenderInContext(context);
				var image2 = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();

				imageView.Image = image2;
			});
		}

		public override void ViewDidLoad()
		{
			View.BackgroundColor = UIColor.White;

			imageView = new UIImageView(View.Frame);

			PHImageManager.DefaultManager.RequestImageForAsset(Asset, View.Frame.Size,
				PHImageContentMode.AspectFit, new PHImageRequestOptions(), (img, info) => {
					imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
					imageView.Image = img;
				});

			View.AddSubview(imageView);

			imageView.StartAnimating();
		}

		void DetectFaces(CIImage image)
		{
			faceDetectionRequest.Perform(new VNRequest[] { faceDetection }, image, out var performError);
			var results = faceDetection.GetResults<VNFaceObservation>() ?? Array.Empty<VNFaceObservation>();
			if (results.Length > 0)
			{
				Console.WriteLine($"Faces Found: {results.Length}");
				faceLandmarks.InputFaceObservations = results;
				DetectLandmarks(image);
			}
		}

		void DetectLandmarks(CIImage image)
		{
			faceLandmarksDetectionRequest.Perform(new VNRequest[] { faceLandmarks }, image, out var performError);
			var landmarksResults = faceLandmarks?.GetResults<VNFaceObservation>() ?? Array.Empty<VNFaceObservation>();
			for (var i = 0; i < landmarksResults.Length; i++)
			{
				var observation = landmarksResults[i];
				var boundingBox = faceLandmarks.InputFaceObservations.FirstOrDefault()?.BoundingBox;
				if (!boundingBox.HasValue) continue;
				Console.WriteLine($"Face {i}: {boundingBox.Value}");
				rects.Add(boundingBox.Value);
			}
		}
	}
}
