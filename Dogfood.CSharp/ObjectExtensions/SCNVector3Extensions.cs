using System;
using Foundation;
using UIKit;
using CoreGraphics;
using SceneKit;
using ARKit;
using OpenTK;

namespace Dogfood.CSharp.ObjectExtensions
{
	public static class SCNVector3Extensions
	{
		public static SCNVector3 One()
		{
			return new SCNVector3(1.0f, 1.0f, 1.0f);
		}

		public static SCNVector3 Uniform(float value)
		{
			return new SCNVector3(value, value, value);
		}

		public new static string ToString(this SCNVector3 vector3)
		{
			return $"({vector3.X:#.00}, {vector3.Y:#.00}, {vector3.Z:#.00})";
		}

		public static float ComputeLength(this SCNVector3 vector3)
		{
			return (float)Math.Sqrt(vector3.X * vector3.X + vector3.Y * vector3.Y + vector3.Z * vector3.Z);
		}

		public static void SetLength(this SCNVector3 vector3, float length)
		{
			vector3.Normalize();
			vector3 *= length;
		}

		public static void SetMaximumLength(this SCNVector3 vector3, float maxLength)
		{
			if (vector3.ComputeLength() > maxLength)
			{
				vector3.SetLength(maxLength);
			}
		}

		public static void Normalize(this SCNVector3 vector3)
		{
			var normalizedVector = vector3.Normalized();
			vector3.X = normalizedVector.X;
			vector3.Y = normalizedVector.Y;
			vector3.Z = normalizedVector.Z;
		}

		public static SCNVector3 Normalized(this SCNVector3 vector3)
		{

			if (vector3.ComputeLength() == 0f)
			{
				return vector3;
			}
			else
			{
				return vector3 / vector3.ComputeLength();
			}
		}

		public static float Dot(this SCNVector3 vector3, SCNVector3 vec)
		{
			return (vector3.X * vec.X) + (vector3.Y * vec.Y) + (vector3.Z * vec.Z);
		}

		public static SCNVector3 Cross(this SCNVector3 vector3, SCNVector3 vec)
		{
			return new SCNVector3(vector3.Y * vec.Z - vector3.Z * vec.Y, vector3.Z * vec.X - vector3.X * vec.Z, vector3.X * vec.Y - vector3.Y * vec.X);
		}

		public static SCNVector3 PositionFromTransform(Matrix4 transform)
		{
			return new SCNVector3(transform.Column3.X, transform.Column3.Y, transform.Column3.Z);
		}

		public static SCNVector3 Add(this SCNVector3 vector3, SCNVector3 right)
		{
			return new SCNVector3(vector3.X + right.X, vector3.Y + right.Y, vector3.Z + right.Z);
		}

		public static SCNVector3 Subtract(this SCNVector3 vector3, SCNVector3 right)
		{
			return new SCNVector3(vector3.X - right.X, vector3.Y - right.Y, vector3.Z - right.Z);
		}

		public static SCNVector3 Multiply(this SCNVector3 vector3, SCNVector3 right)
		{
			return new SCNVector3(vector3.X * right.X, vector3.Y * right.Y, vector3.Z * right.Z);
		}

		public static SCNVector3 Divide(this SCNVector3 vector3, SCNVector3 right)
		{
			return new SCNVector3(vector3.X / right.X, vector3.Y / right.Y, vector3.Z / right.Z);
		}
	}
}
