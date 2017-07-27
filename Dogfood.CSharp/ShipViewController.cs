using System;
using System.Threading.Tasks;
using ARKit;
using Foundation;
using SceneKit;
using UIKit;

namespace Dogfood.CSharp
{
    public partial class ShipViewController : UIViewController, IARSCNViewDelegate
    {
        public ShipViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			// Set the view's delegate
			SceneView.Delegate = this;

			// Show statistics such as fps and timing information
			SceneView.ShowsStatistics = true;

			var scene = SCNScene.FromFile("art.scnassets/ship.scn");

			// Set the scene to the view
			SceneView.Scene = scene;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			// Create a session configuration
			var configuration = new ARWorldTrackingSessionConfiguration();

			// Run the view's session
			SceneView.Session.Run(configuration);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			// Pause the view's session
			SceneView.Session.Pause();
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			Invoke(() => {
				// Test manual bindings for ARPointCloud
				var cloud = SceneView.Session.CurrentFrame.RawFeaturePoints;

				if (cloud != null)
				{
					var points = cloud.Points;
					for (int i = 0; i < points.Length; i++)
						Console.WriteLine($"Point [{i}]: Vector 3 {points[i].ToString()}");
				}
			}, 5);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		[Export("session:didFailWithError:")]
		public void DidFail(ARSession session, Foundation.NSError error)
		{
			// Present an error message to the user
		}

		[Export("sessionWasInterrupted:")]
		public void SessionWasInterrupted(ARSession session)
		{
			// Inform the user that the session has been interrupted, for example, by presenting an overlay
		}

		[Export("sessionInterruptionEnded:")]
		public void SessionInterruptionEnded(ARSession session)
		{
			// Reset tracking and/or remove existing anchors if consistent tracking is required
		}
    }
}