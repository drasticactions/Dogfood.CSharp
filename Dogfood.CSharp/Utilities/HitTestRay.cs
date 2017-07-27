using System;
using Foundation;
using UIKit;
using CoreGraphics;
using SceneKit;
using ARKit;

namespace Dogfood.CSharp.Utilities
{
	[Register("HitTestRay")]
	public class HitTestRay : NSObject
	{
		#region Computed Properties
		[Export("origin")]
		public SCNVector3 Origin { get; set; }

		[Export("direction")]
		public SCNVector3 Direction { get; set; }
		#endregion

		#region Constructors
		public HitTestRay()
		{
		}

		public HitTestRay(SCNVector3 origin, SCNVector3 direction)
		{
			// Initialize
			Origin = origin;
			Direction = direction;
		}
		#endregion
	}
}
