#region Packages

using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     Represents a camera in 3D space
    /// </summary>
    internal class Camera
    {
        public readonly uint Fov; // Field of view preferably between 0 - 180
        public Vector3D Direction; // Direction of the camera relative to location
        public Vector3D HeightRay; // Vector from the top left to the bottom left of the screen
        public Point3D LeftTop; // Top left point of the screen
        public Point3D Location; // Location of the camera
        public Point3D RightTop; // Top right point of the screen

        public Vector3D WidthRay; // Vector from the top left to the top right of the screen

        /// <summary>
        ///     Creates a camera and determines the screen which is used to project on
        /// </summary>
        /// <param name="location">Location of the camera</param>
        /// <param name="direction">Viewing direction of the camera</param>
        /// <param name="fov">The field of view in degrees of the camera</param>
        /// <param name="screen">The surface on which will be drawn, used for dimensions</param>
        public Camera(Point3D location, Vector3D direction, uint fov, Surface screen)
        {
            Location = location;
            Direction = direction;
            Fov = fov;

            var intersection = new Intersection();
            direction.Normalize();
            Point3D planePoint =
                new Point3D(location.X + direction.X, location.Y + direction.Y, location.Z + direction.Z);
            double rho = Math.Acos(direction.Z) / Math.PI * 180;
            double phi1 = Math.Acos(direction.X / Math.Sin(rho * Math.PI / 180)) / Math.PI * 180;
            double phi2 = Math.Asin(direction.Y / Math.Sin(rho * Math.PI / 180)) / Math.PI * 180;
            double phi;

            if (direction.Y > 0)
            {
                phi = phi1;
            }
            else
            {
                if (direction.X > 0)
                    phi = phi2;
                else
                    phi = Math.Abs(phi1) + 2 * Math.Abs(phi2);
            }

            double ratio = screen.height / (double)screen.width;
            Plane vScreen = new Plane(
                (direction.X * planePoint.X + direction.Y * planePoint.Y + direction.Z * planePoint.Z) /
                Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z), direction,
                0x00, "none", 0, 0, 0, 0, 0);

            Ray leftTopRay = new Ray(location,
                new Vector3D(
                    Math.Sin((rho - fov * ratio / 2) * Math.PI / 180) * Math.Cos((phi + fov / 2.0) * Math.PI / 180),
                    Math.Sin((rho - fov * ratio / 2) * Math.PI / 180) * Math.Sin((phi + fov / 2.0) * Math.PI / 180),
                    Math.Cos((rho - fov * ratio / 2) * Math.PI / 180)));
            Ray rightTopRay = new Ray(location,
                new Vector3D(
                    Math.Sin((rho - fov * ratio / 2) * Math.PI / 180) * Math.Cos((phi - fov / 2.0) * Math.PI / 180),
                    Math.Sin((rho - fov * ratio / 2) * Math.PI / 180) * Math.Sin((phi - fov / 2.0) * Math.PI / 180),
                    Math.Cos((rho - fov * ratio / 2) * Math.PI / 180)));
            Ray leftBottomRay = new Ray(location,
                new Vector3D(
                    Math.Sin((rho + fov * ratio / 2) * Math.PI / 180) * Math.Cos((phi + fov / 2.0) * Math.PI / 180),
                    Math.Sin((rho + fov * ratio / 2) * Math.PI / 180) * Math.Sin((phi + fov / 2.0) * Math.PI / 180),
                    Math.Cos((rho + fov * ratio / 2) * Math.PI / 180)));

            LeftTop = intersection.PointIntersect(leftTopRay, vScreen)[0];
            RightTop = intersection.PointIntersect(rightTopRay, vScreen)[0];
            Point3D leftBottom = intersection.PointIntersect(leftBottomRay, vScreen)[0];

            WidthRay = RightTop - LeftTop;
            HeightRay = leftBottom - LeftTop;
        }

        /// <summary>
        ///     Calculates the rays which are send along the middle of the screen
        /// </summary>
        /// <param name="nrOfRays">The number of rays which are projected</param>
        /// <returns>A list of rays</returns>
        public List<Ray> GetRays(int nrOfRays)
        {
            List<Ray> rays = new List<Ray>();
            double sectionDegree = Fov / (double)nrOfRays;
            for (double i = 0; i < nrOfRays; i++)
                rays.Add(GetRayByDegree(Direction, Location, -Fov / 2.0 + i * sectionDegree));
            return rays;
        }

        /// <summary>
        ///     Calculates a new direction of a ray after a rotation
        /// </summary>
        /// <param name="centerLine">The starting direction of a ray before rotation</param>
        /// <param name="origin">The starting point of the ray</param>
        /// <param name="degree">The angle in degrees of rotation</param>
        /// <returns></returns>
        private Ray GetRayByDegree(Vector3D centerLine, Point3D origin, double degree)
        {
            Vector3D vector =
                new Vector3D(
                    centerLine.X * Math.Cos(-degree * Math.PI / 180) - centerLine.Y * Math.Sin(-degree * Math.PI / 180),
                    centerLine.X * Math.Sin(-degree * Math.PI / 180) + centerLine.Y * Math.Cos(-degree * Math.PI / 180),
                    centerLine.Z);
            Ray returnRay = new Ray(origin, vector);
            return returnRay;
        }
    }
}