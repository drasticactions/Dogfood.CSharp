using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Dogfood.CSharp.ObjectExtensions
{
	public static class NSSetExtenions
	{
		public static List<UITouch> ToTouchList(this NSSet touches) {

			var touchArray = touches.ToArray<UITouch>();
			var touchList = new List<UITouch>(touchArray);
			return touchList;
		}
	}
}
