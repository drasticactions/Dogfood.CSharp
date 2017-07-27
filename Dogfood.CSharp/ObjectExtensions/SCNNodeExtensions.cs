﻿using System;
using Foundation;
using UIKit;
using CoreGraphics;
using SceneKit;
using ARKit;
using Dogfood.CSharp.Interfaces;

namespace Dogfood.CSharp.ObjectExtensions
{
	public static class SCNNodeExtensions
	{
		public static SCNNode ObjectThatReactsToScale(this SCNNode node)
		{

			// Attempt to cast object
			var supportsScale = node as IObjectThatReactsToScale;

			// Not found, try parent
			if (supportsScale == null && node.ParentNode != null)
			{
				return node.ParentNode.ObjectThatReactsToScale();
			}

			// Return results
			return (SCNNode)supportsScale;
		}

		public static void SetUniformScale(this SCNNode node, float scale) {
			node.Scale = new SCNVector3(scale, scale, scale);
		}

		public static void RenderOnTop(this SCNNode node) {
			node.RenderingOrder = 2;
			var geom = node.Geometry;

			if (geom != null) {
				// process all materials
				foreach(SCNMaterial material in geom.Materials) {
					material.ReadsFromDepthBuffer = false;
				}
			}

			foreach(SCNNode child in node.ChildNodes) {
				child.RenderOnTop();
			}
		}
	}
}
