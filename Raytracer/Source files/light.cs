#region Packages

using System;
using System.Windows.Media.Media3D;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     Represents a light in world environment.
    /// </summary>
    internal class Light
    {
        private readonly double _ambientCoefficient; // The strengt of the ambientlight from a light source
        private readonly double _angle; // The total angle of a spotlight
        private readonly int _intensity; // Intensity of light source, number between 0 - 100
        private Vector3D _direction; // The direction of a spotlight
        private Vector3D _elight; // The light color represented by a floating point vector
        public Point3D Location; // Location of light source

        /// <summary>
        ///     A normal light uses the location (distance) the intensity and the ambientCoefficient to determine the strength of
        ///     the light.
        ///     And the light color could determine the strength of each individual rgb value.
        /// </summary>
        /// <param name="location"> Location of light source </param>
        /// <param name="intensity"> The overall intensity of a light represented by a number between 0-100</param>
        /// <param name="lightColor"> The light color represented by a floating point vector </param>
        /// <param name="ambientCoefficient"> The strength of the ambient light from a light source </param>
        public Light(Point3D location, int intensity, Vector3D lightColor, double ambientCoefficient)
        {
            Location = location;
            _intensity = intensity;
            _ambientCoefficient = ambientCoefficient;
            _elight = lightColor;
        }

        /// <summary>
        ///     Spotlight does exactly the same as a normal light except has a really small spread determined by the angle and the
        ///     direction.
        ///     If a shadowRay is outside this "spread" it will only return the ambient color;
        /// </summary>
        /// <param name="location"> The place of a light </param>
        /// <param name="direction"> The direction a spotlight is directed towards </param>
        /// <param name="angle"> Determines the widthness of the spread </param>
        /// <param name="intensity"> The overall intensity of a light represented by a number between 0-100</param>
        /// <param name="lightColor"> The light color represented by a floating point vector</param>
        /// <param name="ambientCoefficient"></param>
        public Light(Point3D location, Vector3D direction, double angle, int intensity, Vector3D lightColor,
            double ambientCoefficient)
        {
            Location = location;
            _intensity = intensity;
            _ambientCoefficient = ambientCoefficient;
            _elight = lightColor;
            _angle = angle;
            _direction = direction;
        }

        /// <summary>
        ///     Method to determine whether a point on a surface should be a shadow or not.
        ///     If not it determines its color, by applying: Ambient Light, Diffuse Shading, Specular Shading.
        ///     Else it will just return the Ambient Light color.
        /// </summary>
        /// <param name="obj"> Primitive which stores the point on a surface an all information about the surface</param>
        /// <param name="origin"> Point to determine the viewray </param>
        /// <param name="raytracer"> Used to gain access to the objects list</param>
        /// <returns> A color vector </returns>
        public Vector3D Shadow(Primitive obj, Point3D origin, Raytracer raytracer)
        {
            Ray shadowRay = new Ray(obj.Intersection, Location);
            Ray reverseShadowRay = new Ray(Location, obj.Intersection);
            reverseShadowRay.Direction.Normalize();
            _direction.Normalize();
            double angle = Math.Acos(Vector3D.DotProduct(reverseShadowRay.Direction, _direction));
            int color = obj.GetColor(obj.Intersection.X, obj.Intersection.Y, obj.Intersection.Z);
            Vector3D colorVector = new Vector3D((double)((color >> 16) & 0xFF) / 255,
                (double)((color >> 8) & 0xFF) / 255, (double)(color & 0xFF) / 255);
            Vector3D viewRay = obj.Intersection - origin;
            viewRay.Normalize();

            double distance = raytracer.GetDistance(obj.Intersection, Location);
            Intersection intersection = new Intersection();

            // Ambient Light;
            Vector3D ambient = _ambientCoefficient * new Vector3D(colorVector.X * _elight.X, colorVector.Y * _elight.Y,
                colorVector.Z * _elight.Z);
            if (angle > _angle / 2) return ambient;

            // Shadow
            foreach (Primitive primitive in raytracer.Scene.Objects)
            {
                primitive.Intersection = obj.Intersection;
                if (primitive != obj)
                {
                    Point3D[] intersectPoints = intersection.PointIntersect(shadowRay, primitive);
                    if (intersectPoints.Length != 0)
                    {
                        double distanceIntersect = raytracer.GetDistance(obj.Intersection, intersectPoints[0]);

                        if (intersectPoints.Length == 2)
                        {
                            double distanceIntersect2 = raytracer.GetDistance(obj.Intersection, intersectPoints[1]);
                            distanceIntersect = Math.Max(distanceIntersect, distanceIntersect2);
                        }

                        if (distance >= distanceIntersect) return ambient;
                    }
                }
            }

            // Diffuse Shading
            Vector3D normal = raytracer.GetNormal(obj);
            Vector3D direction = shadowRay.Direction;
            direction.Normalize();

            Vector3D energy = 1 / (distance * distance) * _intensity *
                              new Vector3D(colorVector.X * _elight.X, colorVector.Y * _elight.Y,
                                  colorVector.Z * _elight.Z) * obj.DiffuseCoefficient *
                              Math.Max(0, Vector3D.DotProduct(normal, direction));

            // Specular Shading
            if (obj.Glossiness > 1)
            {
                Vector3D specular = raytracer.GetSecondaryRay(direction, normal);
                energy += 1 / (distance * distance) * _intensity * _elight * (obj.SpecularCoefficient *
                                                                              Math.Pow(
                                                                                  Math.Max(0,
                                                                                      Vector3D.DotProduct(viewRay,
                                                                                          specular)), obj.Glossiness));
            }

            return energy + ambient;
        }
    }
}