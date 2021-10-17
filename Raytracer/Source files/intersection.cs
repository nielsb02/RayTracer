#region Packages

using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     Determines intersections between primitives
    /// </summary>
    internal class Intersection
    {
        /// <summary>
        ///     Calculates the intersection point between a Ray and a Primitive in 3D space
        /// </summary>
        /// <param name="ray">The ray used to intersect</param>
        /// <param name="second">The Primitive being intersected with</param>
        /// <returns>An array of 3D Points where the Primitives intersected</returns>
        public Point3D[] PointIntersect(Ray ray, Primitive second)
        {
            Point3D[] intersections;
            if (second is Sphere)
            {
                double b = 2 * Vector3D.DotProduct(ray.Direction, ray.StartPoint - second.StartPoint);
                Vector3D vector = new Vector3D(ray.StartPoint.X - second.StartPoint.X,
                    ray.StartPoint.Y - second.StartPoint.Y, ray.StartPoint.Z - second.StartPoint.Z);
                double c = vector.LengthSquared - second.RadiusSquared;
                double d = b * b - 4 * c;

                if (d < 0)
                {
                    intersections = new Point3D[0];
                    return intersections;
                }

                if (d == 0)
                {
                    double lPlus = -b / 2;
                    if (lPlus > 0)
                    {
                        intersections = new Point3D[1];
                        intersections[0] = Point3D.Add(ray.StartPoint, ray.Direction * lPlus);
                        return intersections;
                    }

                    intersections = new Point3D[0];
                    return intersections;
                }
                else
                {
                    double lPlus = (-b + Math.Sqrt(d)) / 2;
                    double lMinus = (-b - Math.Sqrt(d)) / 2;
                    if (lPlus > 0 && lMinus > 0)
                    {
                        intersections = new Point3D[2];
                        intersections[0] = Point3D.Add(ray.StartPoint, ray.Direction * lPlus);
                        intersections[1] = Point3D.Add(ray.StartPoint, ray.Direction * lMinus);
                        return intersections;
                    }

                    intersections = new Point3D[0];
                    return intersections;
                }
            }

            if (second is Plane)
            {
                intersections = new Point3D[1];
                double l = Vector3D.DotProduct(Point3D.Subtract(second.StartPoint, ray.StartPoint), second.Normal) /
                           Vector3D.DotProduct(ray.Direction, second.Normal);
                if (l > 0)
                {
                    intersections[0] = Point3D.Add(ray.StartPoint, l * ray.Direction);
                    return intersections;
                }

                intersections[0] = new Point3D(1000, 1000, 1000);
                return intersections;
            }

            if (second is Triangle)
            {
                // Comparing the sum of the angles from the intersection to the three corner points to 2 PI
                Primitive org = second;
                second = second.Plane;
                intersections = new Point3D[1];
                double l = Vector3D.DotProduct(Point3D.Subtract(second.StartPoint, ray.StartPoint), second.Normal) /
                           Vector3D.DotProduct(ray.Direction, second.Normal);
                if (l > 0)
                {
                    Point3D intersection = Point3D.Add(ray.StartPoint, l * ray.Direction);

                    Vector3D a = org.StartPoint - intersection;
                    Vector3D b = org.SecondPoint - intersection;
                    Vector3D c = org.ThirdPoint - intersection;

                    double sum = 0;

                    sum += Math.Acos(Vector3D.DotProduct(a, b) / (a.Length * b.Length));
                    sum += Math.Acos(Vector3D.DotProduct(b, c) / (b.Length * c.Length));
                    sum += Math.Acos(Vector3D.DotProduct(c, a) / (c.Length * a.Length));
                    double offset = 0.001;

                    if (sum <= Math.PI * 2 + offset && sum >= Math.PI * 2 - offset)
                    {
                        intersections[0] = intersection;
                        return intersections;
                    }
                }

                intersections[0] = new Point3D(1000, 1000, 1000);
                return intersections;
            }

            intersections = new Point3D[0];
            return intersections;
        }

        /// <summary>
        ///     Determines the closest Primitive to a ray's start point
        /// </summary>
        /// <param name="ray">The ray used to intersect</param>
        /// <param name="objects">The objects in 3D space used to intersect with</param>
        /// ///
        /// <param name="obj">The object in 3D a secondary ray is shot from to prevent the object from intersecting with itself</param>
        /// <returns>The closest Primitive</returns>
        public Primitive ClosestIntersect(Ray ray, List<Primitive> objects, Primitive obj)
        {
            int primitivePointer = 0;
            List<Tuple<Primitive, Point3D>> intersects = new List<Tuple<Primitive, Point3D>>();
            while (primitivePointer < objects.Count)
            {
                if (objects[primitivePointer] != obj)
                {
                    if (objects[primitivePointer] is Plane || objects[primitivePointer] is Triangle)
                    {
                        Point3D point = PointIntersect(ray, objects[primitivePointer])[0];
                        intersects.Add(new Tuple<Primitive, Point3D>(objects[primitivePointer], point));
                    }
                    else if (objects[primitivePointer] is Sphere)
                    {
                        foreach (Point3D point in PointIntersect(ray, objects[primitivePointer]))
                            intersects.Add(new Tuple<Primitive, Point3D>(objects[primitivePointer], point));
                    }
                }

                primitivePointer++;
            }

            int intersectPointer = 0;
            double distance = 1000;
            Primitive designated = new Primitive();
            while (intersectPointer < intersects.Count)
            {
                double distanceTemp = GetDistance(ray.StartPoint, intersects[intersectPointer].Item2);
                if (distanceTemp < distance)
                {
                    distance = GetDistance(ray.StartPoint, intersects[intersectPointer].Item2);
                    designated = intersects[intersectPointer].Item1;
                    designated.Intersection = intersects[intersectPointer].Item2;
                }

                intersectPointer++;
            }

            return designated;
        }

        /// <summary>
        ///     Calculates the distance between two point in 3D space
        /// </summary>
        /// <param name="point1">The first point in 3D space</param>
        /// <param name="point2">The second point in 3D space</param>
        /// <returns>A absolute distance as a double</returns>
        private double GetDistance(Point3D point1, Point3D point2)
        {
            return Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) +
                             (point1.Y - point2.Y) * (point1.Y - point2.Y) +
                             (point1.Z - point2.Z) * (point1.Z - point2.Z));
        }
    }
}