#region Packages

using System;
using System.Drawing;
using System.Windows.Media.Media3D;
using Raytracer.Properties;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     Represents a object in 3D space
    /// </summary>
    internal class Primitive
    {
        private Color _colorTexture;
        public int Color, Glossiness, Bound;

        public Vector3D Direction;

        public double Distance,
            RadiusSquared,
            DiffuseCoefficient,
            SpecularCoefficient,
            ReflectionCoefficient,
            RefractionIndex;

        protected Bitmap Image;

        public Point3D Intersection;
        public Vector3D Normal;
        public Plane Plane;
        public Point3D SecondPoint;

        public Point3D StartPoint;
        protected string Texture;
        public Point3D ThirdPoint;
        public string Type;

        /// <summary>
        ///     Calculates what color is needed at a certain location
        /// </summary>
        /// <param name="x">The X-coordinate of the location</param>
        /// <param name="y">The Y-coordinate of the location</param>
        /// <param name="z">The Z-coordinate of the location</param>
        /// <returns>A color as an integer</returns>
        public int GetColor(double x, double y, double z)
        {
            //image.Save("IMG_0200.png");
            double factor = 1.5;
            switch (Texture)
            {
                case "chess":
                    if ((Math.Round(x * factor) + Math.Round(y * factor)) % 2 == 0)
                        return 0xffffff;
                    else
                        return 0x000000;

                case "horizontal":
                    if (Math.Round(y) % 2 == 0)
                        return 0xffffff;
                    else
                        return 0x000000;

                case "vertical":
                    if ((int)Math.Round(y) % 2 == 1)
                        return 0xffffff;
                    else
                        return 0x000000;

                case "snow":
                    _colorTexture = Image.GetPixel((int)(Math.Abs(x * Image.Width / 2) % Image.Width),
                        (int)(Math.Abs(y * Image.Height / 2) % Image.Height));
                    return (_colorTexture.R << 16) + (_colorTexture.G << 8) + _colorTexture.B;

                case "sphere":
                    double theta = Math.Atan2(-(x - StartPoint.X), z - StartPoint.Z);
                    double u = (theta + Math.PI) / 2 * Math.PI;
                    double phi = Math.Acos(-(y - StartPoint.Y) / Distance);
                    double v = phi / Math.PI;
                    _colorTexture = Image.GetPixel((int)Math.Abs(u * Image.Width / 10) % Image.Width,
                        (int)Math.Abs(v * Image.Height * 2) % Image.Height);
                    return (_colorTexture.R << 16) + (_colorTexture.G << 8) + _colorTexture.B;

                default:
                    return Color;
            }
        }

        protected void SetType(string texture)
        {
            switch (texture)
            {
                case "mirror":
                    Type = "mirror";
                    break;

                case "vacuum":
                    RefractionIndex = 1;
                    Type = "refraction";
                    break;

                case "air":
                    RefractionIndex = 1.000293;
                    Type = "refraction";
                    break;

                case "water":
                    RefractionIndex = 1.333;
                    Type = "refraction";
                    break;

                case "ice":
                    RefractionIndex = 1.31;
                    Type = "refraction";
                    break;

                case "glass":
                    RefractionIndex = 1.52;
                    Type = "refraction";
                    break;

                case "diamond":
                    RefractionIndex = 2.42;
                    Type = "refraction";
                    break;

                default:
                    Type = "solid";
                    break;
            }
        }
    }

    /// <summary>
    ///     Represents a Ray in 3D space based on a Primitive
    /// </summary>
    internal class Ray : Primitive
    {
        public Ray(Point3D origin, Vector3D direction)
        {
            StartPoint = origin;
            SecondPoint = origin + direction;
            Direction = direction;
            Direction.Normalize();
        }

        public Ray(Point3D origin, Point3D point)
        {
            StartPoint = origin;
            Direction = point - origin;
            Direction.Normalize();
            SecondPoint = origin + Direction;
        }
    }

    /// <summary>
    ///     Represents a Sphere in 3D space based on a Primitive
    /// </summary>
    internal class Sphere : Primitive
    {
        public Sphere(Point3D position, double radius, int color, string texture, double diffuseCoefficient,
            int glossiness, double specularCoefficient, double reflectionCoefficient, int bound)
        {
            StartPoint = position;
            Distance = radius;
            RadiusSquared = radius * radius;
            Color = color;
            Texture = texture;
            DiffuseCoefficient = diffuseCoefficient;
            Glossiness = glossiness;
            SpecularCoefficient = specularCoefficient;
            ReflectionCoefficient = reflectionCoefficient;
            Bound = bound;

            SetType(texture);
            if (texture == "lava")
            {
                Image = Resource.lava;
                Texture = "sphere";
            }
            else if (texture == "earth")
            {
                Image = Resource.earth;
                Texture = "sphere";
            }
            else if (texture == "rock")
            {
                Image = Resource.rock;
                Texture = "sphere";
            }
        }
    }

    /// <summary>
    ///     Represents a Plane in 3D space based on a Primitive
    /// </summary>
    internal class Plane : Primitive
    {
        public Plane(double distance, Vector3D normal, int color, string texture, double diffuseCoefficient,
            int glossiness, double specularCoefficient, double reflectionCoefficient, int bound)
        {
            Distance = distance;
            Normal = normal;
            Normal.Normalize();
            Color = color;
            StartPoint = new Point3D(Normal.X * distance, Normal.Y * distance, Normal.Z * distance);
            Texture = texture;
            DiffuseCoefficient = diffuseCoefficient;
            Glossiness = glossiness;
            SpecularCoefficient = specularCoefficient;
            ReflectionCoefficient = reflectionCoefficient;
            Bound = bound;
            Type = texture == "mirror" ? "mirror" : "solid";

            Image = Resource.snow;
        }
    }

    /// <summary>
    ///     Represents a Triangle in 3D space based on a Primitive
    /// </summary>
    internal class Triangle : Primitive
    {
        public Triangle(Point3D point1, Point3D point2, Point3D point3, int color, string texture,
            double diffuseCoefficient, int glossiness, double specularCoefficient, double reflectionCoefficient,
            int bound)
        {
            StartPoint = point1;
            SecondPoint = point2;
            ThirdPoint = point3;
            Color = color;
            Texture = texture;
            DiffuseCoefficient = diffuseCoefficient;
            Glossiness = glossiness;
            SpecularCoefficient = specularCoefficient;
            ReflectionCoefficient = reflectionCoefficient;
            Bound = bound;
            Normal = Vector3D.CrossProduct(point2 - point1, point3 - point1);
            Distance = Math.Abs(-Normal.X * point1.X - Normal.Y * point1.Y - Normal.Z * point1.Z) /
                       Math.Sqrt(Normal.X * Normal.X + Normal.Y * Normal.Y + Normal.Z * Normal.Z);
            Plane = new Plane(Distance, Vector3D.CrossProduct(point2 - point1, point3 - point1), color, texture,
                    diffuseCoefficient, glossiness, specularCoefficient, reflectionCoefficient, bound)
            { Type = Type };
        }
    }
}