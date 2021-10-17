#region Packages

using System.Collections.Generic;
using System.Windows.Media.Media3D;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     Represents a 3D world with objects and light sources in it
    /// </summary>
    internal class Scene
    {
        public readonly int BgrColor;
        public readonly List<Light> LightSources;
        public readonly List<Primitive> Objects;

        /// <summary>
        ///     Fills the Scene with objects and light sources
        /// </summary>
        /// <param name="presetInd">Selects a preset scene</param>
        public Scene(int presetInd)
        {
            int preset = presetInd; // Use this preset value to see various pre programmed scenes (choose between 0 - 5)
            Objects = new List<Primitive>();
            LightSources = new List<Light>();
            BgrColor = 0x87CEFA;

            switch (preset)
            {
                case 0: // Three spheres and a plane
                    Objects.Add(new Sphere(new Point3D(-3, 8, 0), 1, 0xff0000, "none", 0.8, 300, 0.2, 0.4, 7));
                    Objects.Add(new Sphere(new Point3D(0, 4, 0), 1, 0x00ff00, "none", 0.7, 750, 0.1, 0.5, 7));
                    Objects.Add(new Sphere(new Point3D(3, 6.5, 0), 1, 0x0000ff, "none", 0.75, 500, 0.2, 0.4, 5));

                    Objects.Add(new Plane(1, new Vector3D(0, 0, -1), 0xcc00ff, "none", 1, 750, 0.8, 0.5, 9)
                    { Type = "none" });

                    LightSources.Add(new Light(new Point3D(0, -2, 3), 75, new Vector3D(1, 1, 1), 0.1));
                    LightSources.Add(new Light(new Point3D(-4, -2, 9), 45, new Vector3D(1, 1, 1), 0.1));
                    break;

                case 1: // Three mirror spheres and a checker pattern mirror plane
                    Objects.Add(new Sphere(new Point3D(-3, 8, 0), 1, 0xff0000, "mirror", 0.4, 600, 0.2, 0.4, 7));
                    Objects.Add(new Sphere(new Point3D(0, 4, 0), 1, 0x00ff00, "mirror", 0, 750, 0.1, 0.5, 7));
                    Objects.Add(new Sphere(new Point3D(3, 6.5, 0), 1, 0x0000ff, "mirror", 0.75, 750, 0.2, 0.4, 5));

                    Objects.Add(new Plane(1, new Vector3D(0, 0, -1), 0xffffff, "chess", 1, 750, 0.8, 0.5, 9)
                    { Type = "mirror" });

                    LightSources.Add(new Light(new Point3D(0, -2, 3), 75, new Vector3D(1, 1, 1), 0.1));
                    LightSources.Add(new Light(new Point3D(-4, -2, 9), 10, new Vector3D(1, 1, 1), 0.1));
                    break;

                case 2: // Four triangles forming a pyramid and a checker pattern mirror plane
                    Objects.Add(new Triangle(new Point3D(-1, 6, -1), new Point3D(0, 7, 0.5), new Point3D(1, 6, -1),
                            0xffff00, "none", 0.4, 600, 0.2, 0.4, 9)
                    { Type = "none" });
                    Objects.Add(new Triangle(new Point3D(1, 8, -1), new Point3D(0, 7, 0.5), new Point3D(1, 6, -1),
                            0xffff00, "none", 0.4, 600, 0.2, 0.4, 9)
                    { Type = "none" });
                    Objects.Add(new Triangle(new Point3D(-1, 6, -1), new Point3D(0, 7, 0.5), new Point3D(-1, 8, -1),
                            0xffff00, "none", 0.4, 600, 0.2, 0.4, 9)
                    { Type = "none" });
                    Objects.Add(new Triangle(new Point3D(-1, 8, -1), new Point3D(0, 7, 0.5), new Point3D(1, 8, -1),
                            0xffff00, "none", 0.4, 600, 0.2, 0.4, 9)
                    { Type = "none" });
                    Objects.Add(new Plane(1, new Vector3D(0, 0, -1), 0xffffff, "chess", 1, 750, 0.8, 0.5, 9)
                    { Type = "mirror" });
                    LightSources.Add(new Light(new Point3D(0, -4, 3), 75, new Vector3D(1, 1, 1), 0.1));
                    break;

                case 3: // Three spheres and a two spotlight
                    Objects.Add(new Sphere(new Point3D(-3, 12, 0), 1, 0xff0000, "none", 0.8, 300, 0.2, 0.4, 7));
                    Objects.Add(new Sphere(new Point3D(0, 8, 0), 1, 0x00ff00, "none", 0.7, 750, 0.1, 0.5, 7));
                    Objects.Add(new Sphere(new Point3D(3, 10.5, 0), 1, 0x0000ff, "none", 0.75, 500, 0.2, 0.4, 5));

                    Objects.Add(new Plane(1, new Vector3D(0, 0, -1), 0xcc00ff, "none", 1, 750, 0.8, 0.5, 9)
                    { Type = "none" });
                    LightSources.Add(new Light(new Point3D(-6, -2, 9), new Vector3D(1, 2, -2), 0.5, 80,
                        new Vector3D(1, 1, 1), 0.1));
                    LightSources.Add(new Light(new Point3D(6, -2, 9), new Vector3D(-1, 2, -2), 0.5, 80,
                        new Vector3D(1, 1, 1), 0.1));
                    break;

                case 4: // Three spheres one only has reflection the other two have both refraction and reflection (small remark: the sphere on the right has no color)
                    Objects.Add(new Sphere(new Point3D(-3, 10, 0), 1, 0xcccccc, "mirror", 0.4, 250, 0.2, 0.6, 7));
                    Objects.Add(new Sphere(new Point3D(0, 6.5, 0.125), 1.25, 0x000099, "ice", 0, 1250, 0.4, 0.45, 7));
                    Objects.Add(new Sphere(new Point3D(3, 8, 0), 1, 0, "diamond", 0, 0, 0, 0.3, 7));

                    Primitive infintePlane = new Plane(1, new Vector3D(0, 0, -1), 0x333333, "chess", 0.75, 750, 0.8,
                        0.5, 9);
                    infintePlane.Type = "mirror";
                    Objects.Add(infintePlane);
                    LightSources.Add(new Light(new Point3D(0, -2, 3), 75, new Vector3D(1, 1, 1), 0.1));
                    LightSources.Add(new Light(new Point3D(-4, -2, 9), 45, new Vector3D(1, 1, 1), 0.1));
                    break;

                case 5: // Three textured spheres with earth, lava and rock texture and a snow textured plane
                    Objects.Add(new Sphere(new Point3D(-3, 8, 0), 1, 0xff0000, "earth", 0.8, 300, 0.2, 0.4, 7));
                    Objects.Add(new Sphere(new Point3D(0, 4, 0), 1, 0x00ff00, "lava", 0.7, 750, 0.1, 0.5, 7));
                    Objects.Add(new Sphere(new Point3D(3, 6.5, 0), 1, 0x0000ff, "rock", 0.75, 500, 0.2, 0.4, 5));

                    Objects.Add(new Plane(1, new Vector3D(0, 0, -1), 0xcc00ff, "snow", 1, 750, 0.8, 0.5, 9)
                    { Type = "none" });

                    LightSources.Add(new Light(new Point3D(0, -2, 3), 40, new Vector3D(1, 1, 1), 0.1));
                    LightSources.Add(new Light(new Point3D(-4, -2, 9), 40, new Vector3D(1, 1, 1), 0.1));
                    break;
            }
        }
    }
}