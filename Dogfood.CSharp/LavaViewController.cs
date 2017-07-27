using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ARKit;
using CoreFoundation;
using Dogfood.CSharp.ObjectExtensions;
using Foundation;
using SceneKit;
using UIKit;

namespace Dogfood.CSharp
{
    public partial class LavaViewController : UIViewController, IARSCNViewDelegate
    {
        public UIImage Lava { get; set; } = new UIImage("lava-0.png");

        public LavaViewController(IntPtr handle) : base(handle)
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

            configuration.PlaneDetection = ARPlaneDetection.Horizontal;

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
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        [Export("session:didFailWithError:")]
        public void DidFail(ARSession session, Foundation.NSError error)
        {
            Console.WriteLine($"Failed: {error}");
        }

        [Export("sessionWasInterrupted:")]
        public void SessionWasInterrupted(ARSession session)
        {
            Console.WriteLine($"Interrupted");
            // Inform the user that the session has been interrupted, for example, by presenting an overlay
        }

        [Export("sessionInterruptionEnded:")]
        public void SessionInterruptionEnded(ARSession session)
        {
			Console.WriteLine($"Interrupted Ended");
            // Reset tracking and/or remove existing anchors if consistent tracking is required
        }

        #region ARSCNViewDelegate
        [Export("renderer:didAddNode:forAnchor:")]
        public void DidAddNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            Console.WriteLine("Add Node");
			DispatchQueue.MainQueue.DispatchAsync(() => {
				var planeNode = CreatePlaneNode((ARPlaneAnchor)anchor);
				SceneView.Scene.RootNode.AddChildNode(planeNode);
            });
        }

        [Export("renderer:didUpdateNode:forAnchor:")]
        public void DidUpdateNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            Console.WriteLine("Update Node");
			DispatchQueue.MainQueue.DispatchAsync(() => {
                //Array.Clear(node.ChildNodes, 0, node.ChildNodes.Length);
				var planeNode = CreatePlaneNode((ARPlaneAnchor)anchor);
				SceneView.Scene.RootNode.AddChildNode(planeNode);
			});
        }

        [Export("renderer:didRemoveNode:forAnchor:")]
        public void DidRemoveNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
			Console.WriteLine("Remove Node");
			DispatchQueue.MainQueue.DispatchAsync(() => {
				//Array.Clear(node.ChildNodes, 0, node.ChildNodes.Length);
			});
        }
        #endregion

        #region Plane

        public SCNNode CreatePlaneNode(ARPlaneAnchor anchor) {
            Console.WriteLine("Create Plane Node");
            var plane = new SCNPlane()
            {
                Width = anchor.Extent.X,
                Height = anchor.Extent.Z
            };

            var lavaDefuseImage = new SCNMaterial()
            {
                DoubleSided = true
            };
            lavaDefuseImage.Diffuse.Contents = Lava;

            plane.Materials = new SCNMaterial[] { lavaDefuseImage };

            var planeNode = new SCNNode()
            {
                Geometry = plane
            };

            planeNode.Position = new SCNVector3(anchor.Center.X, 0, anchor.Center.Z);

			// Original used SCNMatrix4MakeRotation
			planeNode.Transform = SCNMatrix4.CreateFromAxisAngle(new SCNVector3(1, 0,0 ), (float)(Math.PI / 2) * -1);

            return planeNode;
        }

        #endregion
    }
}