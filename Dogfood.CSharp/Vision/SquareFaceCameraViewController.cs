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
    public class SquareFaceCameraViewController : UIViewController, IAVCaptureVideoDataOutputSampleBufferDelegate
    {
        public SquareFaceCameraViewController()
        {
        }

        VNDetectFaceRectanglesRequest faceDetection = new VNDetectFaceRectanglesRequest(null);
        VNDetectFaceLandmarksRequest faceLandmarks = new VNDetectFaceLandmarksRequest(null);
        VNSequenceRequestHandler faceLandmarksDetectionRequest = new VNSequenceRequestHandler();
        VNSequenceRequestHandler faceDetectionRequest = new VNSequenceRequestHandler();

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

        AVCaptureDevice frontCamera = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaType.Video, AVCaptureDevicePosition.Front);

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

            // Needs to filp coordinate system for Vision
            shapeLayer.AffineTransform = CGAffineTransform.MakeScale(-1, -1);
            View.Layer.AddSublayer(shapeLayer);
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
            using (var pixelBuffer = sampleBuffer.GetImageBuffer())
            using (var attachments = sampleBuffer.GetAttachments<NSString, NSObject>(CMAttachmentMode.ShouldPropagate))
            using (var ciimage = new CIImage(pixelBuffer, attachments))
            using (var ciImageWithOrientation = ciimage.CreateWithOrientation(CIImageOrientation.RightTop))
            {
                DetectFace(ciImageWithOrientation);
            }
            // make sure we do not run out of sampleBuffers
            sampleBuffer.Dispose();
        }

        void DetectFace(CIImage image)
        {
            faceDetectionRequest.Perform(new VNRequest[] { faceDetection }, image, out var performError);
            var results = faceDetection.GetResults<VNFaceObservation>() ?? Array.Empty<VNFaceObservation>();
            if (results.Length > 0)
            {
                faceLandmarks.InputFaceObservations = results;
                DetectLandmarks(image);
            }
        }

        void DetectLandmarks(CIImage image)
        {
            faceLandmarksDetectionRequest.Perform(new VNRequest[] { faceLandmarks }, image, out var performError);
            var landmarksResults = faceLandmarks?.GetResults<VNFaceObservation>() ?? Array.Empty<VNFaceObservation>();
            foreach (var observation in landmarksResults)
            {
                if (shapeLayer.Sublayers != null)
                {
                    shapeLayer.Sublayers = new CALayer[0];
                }

                DispatchQueue.MainQueue.DispatchAsync(() => {
                    var box = faceLandmarks.InputFaceObservations.FirstOrDefault().BoundingBox;
                    DrawFace(box);
                });
            }
        }

        void DrawFace(CGRect box) {
            var maxX = box.GetMaxX();
            var minX = box.GetMinX();
            var maxY = box.GetMaxY();
            var minY = box.GetMinY();

            var xCord = maxX * shapeLayer.Frame.Size.Width;
            var yCord = (1 - minY) * shapeLayer.Frame.Size.Height;
            var width = (minX - maxX) * shapeLayer.Frame.Size.Width;
            var height = (minY - maxY) * shapeLayer.Frame.Size.Height;

            Draw((float)xCord, (float)yCord, (float)width, (float)height, UIColor.Red);
        }

        void Draw(float x, float y, float width, float height, UIColor color)
        {
            var outline = new CALayer();
            outline.Frame = new CGRect(x, y, width, height);
            outline.BorderColor = color.CGColor;
            outline.BorderWidth = 2.0f;
            shapeLayer.AddSublayer(outline);
        }
    }
}
