using System;
using Foundation;
using UIKit;
using CoreGraphics;
using SceneKit;
using ARKit;

namespace Dogfood.CSharp.Interfaces
{
	public interface IObjectThatReactsToScale
	{
		SCNNode ObjectThatReactsToScale();
	}
}
