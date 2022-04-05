 
using Android.Graphics;

namespace WoWonder.Library.AudioVisualizer.Utils
{
 
	public class BezierSpline
	{

		private readonly int NSize;
		private readonly PointF[] firstControlPoints, secondControlPoints;

		public BezierSpline(int size)
		{
			NSize = size - 1;
			firstControlPoints = new PointF[NSize];
			secondControlPoints = new PointF[NSize];
			for (int i = 0; i < NSize; i++)
			{
				firstControlPoints[i] = new PointF();
				secondControlPoints[i] = new PointF();
			}
		}

		/// <summary>
		/// Get open-ended bezier spline control points.
		/// </summary>
		/// <param name="knots"> bezier spline points </param>
		/// <exception cref="IllegalArgumentException"> if less than two knots are passed. </exception>
		public virtual void UpdateCurveControlPoints(PointF[] knots)
		{
			if (knots == null || knots.Length < 2)
			{
				throw new System.ArgumentException("At least two knot points are required");
			}
			 
			int n = knots.Length - 1;

			// Special case: bezier curve should be a straight line
			if (n == 1)
			{
				// 3P1 = 2P0 + P3
				float x = (2 * knots[0].X + knots[1].X) / 3;
				float y = (2 * knots[0].Y + knots[1].Y) / 3;

				firstControlPoints[0].X = x;
				firstControlPoints[0].Y = y;

				// P2 = 2P1 - P0
				x = 2 * firstControlPoints[0].X - knots[0].X;
				y = 2 * firstControlPoints[0].Y - knots[0].Y;

				secondControlPoints[0].X = x;
				secondControlPoints[0].Y = y;

			}
			else
			{

				// Calculate first bezier control points
				// Right hand side vector
				float[] rhs = new float[n];

				// Set right hand side X values
				for (int i = 1; i < n - 1; i++)
				{
					rhs[i] = 4 * knots[i].X + 2 * knots[i + 1].X;
				}
				rhs[0] = knots[0].X + 2 * knots[1].X;
				rhs[n - 1] = (8 * knots[n - 1].X + knots[n].X) / 2f;

				// Get first control points X-values
				float[] x = GetFirstControlPoints(rhs);

				// Set right hand side Y values
				for (int i = 1; i < n - 1; i++)
				{
					rhs[i] = 4 * knots[i].Y + 2 * knots[i + 1].Y;
				}
				rhs[0] = knots[0].Y + 2 * knots[1].Y;
				rhs[n - 1] = (8 * knots[n - 1].Y + knots[n].Y) / 2f;

				// Get first control points Y-values
				float[] y = GetFirstControlPoints(rhs);

				for (int i = 0; i < n; i++)
				{
					// First control point
					firstControlPoints[i].X = x[i];
					firstControlPoints[i].Y = y[i];

					// Second control point
					if (i < n - 1)
					{
						float xx = 2 * knots[i + 1].X - x[i + 1];
						float yy = 2 * knots[i + 1].Y - y[i + 1];
						secondControlPoints[i].X = xx;
						secondControlPoints[i].Y = yy;
					}
					else
					{
						float xx = (knots[n].X + x[n - 1]) / 2;
						float yy = (knots[n].Y + y[n - 1]) / 2;
						secondControlPoints[i].X = xx;
						secondControlPoints[i].Y = yy;
					}
				}
			}
		}

		/// <summary>
		/// Solves a tridiagonal system for one of coordinates (x or y) of first
		/// bezier control points.
		/// </summary>
		/// <param name="rhs"> right hand side vector. </param>
		/// <returns> Solution vector. </returns>
		private float[] GetFirstControlPoints(float[] rhs)
		{
			int n = rhs.Length;
			float[] x = new float[n]; // Solution vector
			float[] tmp = new float[n]; // Temp workspace

			float b = 2.0f;
			x[0] = rhs[0] / b;

			// Decomposition and forward substitution
			for (int i = 1; i < n; i++)
			{
				tmp[i] = 1 / b;
				b = (i < n - 1 ? 4.0f : 3.5f) - tmp[i];
				x[i] = (rhs[i] - x[i - 1]) / b;
			}

			// Backsubstitution
			for (int i = 1; i < n; i++)
			{
				x[n - i - 1] -= tmp[n - i] * x[n - i];
			}

			return x;
		}

		public virtual PointF[] FirstControlPoints
		{
			get
			{
				return firstControlPoints;
			}
		}

		public virtual PointF[] SecondControlPoints
		{
			get
			{
				return secondControlPoints;
			}
		}
	}
}