using System;
using Foundation;
using UIKit;
using CoreGraphics;
using SceneKit;
using ARKit;
using OpenTK;

namespace Dogfood.CSharp.Utilities
{
	[Register("FeatureHitTestResult")]
	public class FeatureHitTestResult : NSObject
	{
		#region Computed Properties
		[Export("position")]
		public SCNVector3 Position { get; set; }

		[Export("distanceToRayOrigin")]
		public float DistanceToRayOrigin { get; set; }

		[Export("featureHit")]
		public SCNVector3 FeatureHit { get; set; }

		[Export("featureDistanceToHitResult")]
		public float FeatureDistanceToHitResult { get; set; }
		#endregion

		#region Constructors
		public FeatureHitTestResult()
		{
		}

		public FeatureHitTestResult(SCNVector3 position, float distanceToRayOrigin, SCNVector3 featureHit, float featureDistanceToHitResult)
		{
			// Initialize
			Position = position;
			DistanceToRayOrigin = distanceToRayOrigin;
			FeatureHit = featureHit;
			FeatureDistanceToHitResult = featureDistanceToHitResult;
		}
		#endregion
	}
}
