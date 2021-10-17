#region Packages

using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

#endregion Packages

namespace Raytracer
{
    /// <summary>
    ///     Creates a scene and sends rays into this 3D world
    ///     Output is a pixel array which represents the 3D world on a 2D screen
    /// </summary>
    internal class Raytracer
    {
        private readonly bool _SSAAon;
        public bool AAon;
        private Camera _camera;
        private readonly Intersection _intersection;
        private int[] _pixels;
        public Scene Scene;

        /// <summary>
        ///     Gives the raytracer it's initial values such as a camera
        /// </summary>
        /// <param name="screen">The surface on which will be drawn</param>
        public Raytracer(Surface screen)
        {
            _camera = new Camera(new Point3D(0, 0, 0), new Vector3D(0, 1, 0), 60, screen);
            _intersection = new Intersection();
            Scene = new Scene(2);
            AAon = false;
            _SSAAon = false;
        }

        /// <summary>
        ///     Calculates the pixel array of the screen in the 3D world
        /// </summary>
        /// <param name="screen">The surface on which will be drawn</param>
        public void RenderSim(Surface screen)
        {
            _pixels = new int[screen.pixels.Length];
            screen.Clear(0);

            for (int z = 0; z < screen.height; z++)
                for (int x = 0; x < screen.width; x++)
                {
                    Ray ray = new Ray(_camera.Location,
                        _camera.LeftTop + x * _camera.WidthRay / screen.width + z * _camera.HeightRay / screen.height);
                    Vector3D elight = Trace(ray, 0, null, _camera.Location);
                    int red = Math.Min((int)(elight.X * 256), 255);
                    int green = Math.Min((int)(elight.Y * 256), 255);
                    int blue = Math.Min((int)(elight.Z * 256), 255);
                    if (_SSAAon) // Super Sampling Anti Aliasing (very slow)
                    {
                        ray = new Ray(_camera.Location,
                            _camera.LeftTop + (x + 0.5) * _camera.WidthRay / screen.width +
                            z * _camera.HeightRay / screen.height);
                        elight = Trace(ray, 0, null, _camera.Location);
                        red += Math.Min((int)(elight.X * 256), 255);
                        green += Math.Min((int)(elight.Y * 256), 255);
                        blue += Math.Min((int)(elight.Z * 256), 255);

                        ray = new Ray(_camera.Location,
                            _camera.LeftTop + (x + 0.5) * _camera.WidthRay / screen.width +
                            (z + 0.5) * _camera.HeightRay / screen.height);
                        elight = Trace(ray, 0, null, _camera.Location);
                        red += Math.Min((int)(elight.X * 256), 255);
                        green += Math.Min((int)(elight.Y * 256), 255);
                        blue += Math.Min((int)(elight.Z * 256), 255);

                        ray = new Ray(_camera.Location,
                            _camera.LeftTop + x * _camera.WidthRay / screen.width +
                            (z + 0.5) * _camera.HeightRay / screen.height);
                        elight = Trace(ray, 0, null, _camera.Location);
                        red += Math.Min((int)(elight.X * 256), 255);
                        green += Math.Min((int)(elight.Y * 256), 255);
                        blue += Math.Min((int)(elight.Z * 256), 255);

                        red /= 4;
                        green /= 4;
                        blue /= 4;
                    }

                    int color = (red << 16) + (green << 8) + blue;
                    _pixels[z * screen.width + x] = color;
                }

            AntiAliasing(screen);
        }

        /// <summary>
        ///     Calculates the pixel array of the screen in the 2D world for debug purposes
        /// </summary>
        /// <param name="screen">The surface on which will be drawn</param>
        public void RenderDebug(Surface screen)
        {
            screen.Clear(0);

            List<Ray> rays = _camera.GetRays(40);

            foreach (Ray ray in rays) PlotRay(screen, Scene.Objects, Scene.LightSources, ray);

            foreach (Primitive primitive in Scene.Objects)
                if (primitive is Sphere)
                    PlotSphere(screen, primitive);

            PlotSphere(screen, new Sphere(_camera.Location, 0.05, 0xffffff, "none", 0, 0, 0, 0, 0));

            Point3D newScreenLeft = TranslatePoint(_camera.LeftTop, screen);
            Point3D newScreenRight = TranslatePoint(_camera.RightTop, screen);
            screen.Line((int)newScreenLeft.X, (int)newScreenLeft.Y, (int)newScreenRight.X, (int)newScreenRight.Y,
                0x00ff00);
        }

        /// <summary>
        ///     Method to determine the color a ray should return.
        ///     By intersecting a ray with one of our objects, if it doesn't intersect with any it will return the background color.
        ///     For reflective and refractive materials it uses the objects bound to determine the amount of bounces.
        /// </summary>
        /// <param name="ray">Direction ray which is being shot into an object</param>
        /// <param name="counter">Counts the bounces of a reflected/refracted ray </param>
        /// <param name="originated">Object to prevent a reflected ray from intersecting with itself</param>
        /// <param name="origin">Point which determines the incident/view ray</param>
        /// <returns>The colorVector of a pixel</returns>
        private Vector3D Trace(Ray ray, int counter, Primitive originated, Point3D origin)
        {
            Primitive intersectedObject = _intersection.ClosestIntersect(ray, Scene.Objects, originated);

            Vector3D elight = new Vector3D(0, 0, 0);
            if (intersectedObject.Type != null)
            {
                foreach (Light light in Scene.LightSources) elight += light.Shadow(intersectedObject, origin, this);

                if (intersectedObject.Type == "mirror" && counter <= intersectedObject.Bound)
                {
                    elight += intersectedObject.ReflectionCoefficient * Reflection(intersectedObject, origin, counter);
                }
                else if (intersectedObject.Type == "refraction" && counter <= intersectedObject.Bound)
                {
                    //Refraction index of air.
                    double n1 = 1.000293;
                    double n2 = intersectedObject.RefractionIndex;
                    Vector3D reflectionColor = new Vector3D(0, 0, 0);

                    Vector3D normal = GetNormal(intersectedObject);
                    Vector3D incidentRay = intersectedObject.Intersection - origin;
                    incidentRay.Normalize();
                    Vector3D nTemp = normal;
                    double dot = Vector3D.DotProduct(nTemp, incidentRay);

                    //outside the surface so dot product has to be flipped.
                    if (dot < 0)
                    {
                        dot *= -1;
                    }
                    // Inside the object so we have to flip the refractionIndexes and the normal.
                    else
                    {
                        nTemp *= -1;
                        n1 = n2;
                        n2 = 1.000293;
                    }

                    double n = n1 / n2;
                    double sqrt = 1 - n * n * (1 - dot * dot);
                    //Total internal refraction
                    if (sqrt < 0) elight += new Vector3D(0, 0, 0);
                    Vector3D refractionDirection = n * (incidentRay - nTemp * dot) - nTemp * Math.Sqrt(sqrt);
                    refractionDirection.Normalize();

                    // A small offset to prevent shadow acne
                    Point3D p = intersectedObject.Intersection + 1 * refractionDirection * 0.000001;
                    Ray refractionRay = new Ray(p, refractionDirection);
                    Vector3D refractionColor = Trace(refractionRay, counter + 1, null, intersectedObject.Intersection);

                    if (intersectedObject.ReflectionCoefficient > 0)
                        reflectionColor = intersectedObject.ReflectionCoefficient *
                                          Reflection(intersectedObject, origin, counter);

                    //Fresnel's coefficient to determine how reflective and refractive an object is.
                    double angle = dot;
                    double R0 = (n2 - n1) / (n2 + n1);
                    R0 *= R0;
                    double x = 1 - angle;
                    double fresnel = R0 + (1 - R0) * (x * x * x * x * x);

                    if (refractionColor != new Vector3D(0, 0, 0))
                        elight += fresnel * reflectionColor + (1 - fresnel) * refractionColor;
                    else
                        elight += fresnel * reflectionColor;
                }
            }
            else
            {
                int intersectColor = Scene.BgrColor;
                elight = new Vector3D((double)((intersectColor >> 16) & 0xFF) / 255,
                    (double)((intersectColor >> 8) & 0xFF) / 255, (double)(intersectColor & 0xFF) / 255);
            }

            return elight;
        }

        /// <summary>
        ///     Determines the normal of an object.
        /// </summary>
        /// <param name="obj">The Primitive from which the normal should be determined</param>
        /// <returns>A normalized normal from a point on a surface</returns>
        public Vector3D GetNormal(Primitive obj)
        {
            if (obj is Triangle) obj = obj.Plane;

            Vector3D normal = obj.Normal;
            if (obj is Sphere)
                normal = new Vector3D(obj.Intersection.X - obj.StartPoint.X, obj.Intersection.Y - obj.StartPoint.Y,
                    obj.Intersection.Z - obj.StartPoint.Z);
            if (obj is Plane) normal *= -1;

            normal.Normalize();
            return normal;
        }

        /// <summary>
        ///     Method to determine the color of the reflection on a surface,
        ///     by shooting a second ray into the Trace method.
        /// </summary>
        /// <param name="intersectedObject"> The object from which a ray is reflected</param>
        /// <param name="counter">Counts the bounces of a reflected/refracted ray </param>
        /// <param name="origin">Point which determines the incident/view ray</param>
        /// <returns>Returns the color of a reflective surface</returns>
        private Vector3D Reflection(Primitive intersectedObject, Point3D origin, int counter)
        {
            Vector3D normal = GetNormal(intersectedObject);
            Vector3D viewRay = intersectedObject.Intersection - origin;
            viewRay.Normalize();

            Vector3D reflectDir = GetSecondaryRay(viewRay, normal);
            Ray reflection = new Ray(intersectedObject.Intersection, reflectDir);
            Primitive intersect = _intersection.ClosestIntersect(reflection, Scene.Objects, intersectedObject);
            Vector3D eReflect = new Vector3D(0, 0, 0);
            if (intersect.Type != null && intersect != intersectedObject)
            {
                eReflect += Trace(reflection, counter + 1, intersectedObject, intersectedObject.Intersection);
            }
            else
            {
                int intersectColor = Scene.BgrColor;
                eReflect = new Vector3D((double)((intersectColor >> 16) & 0xFF) / 255,
                    (double)((intersectColor >> 8) & 0xFF) / 255, (double)(intersectColor & 0xFF) / 255);
            }

            return eReflect;
        }

        /// <summary>
        ///     Calculates the distance between two point in 3D space
        /// </summary>
        /// <param name="point1">The first point in 3D space</param>
        /// <param name="point2">The second point in 3D space</param>
        /// <returns>A absolute distance as a double</returns>
        public double GetDistance(Point3D point1, Point3D point2)
        {
            return Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) +
                             (point1.Y - point2.Y) * (point1.Y - point2.Y) +
                             (point1.Z - point2.Z) * (point1.Z - point2.Z));
        }

        /// <summary>
        ///     Calculates the reflected ray (secondary ray) from a point on a reflective surface.
        /// </summary>
        /// <param name="direction">Direction of the view ray</param>
        /// <param name="normal"> normal of a point on a surface </param>
        /// <returns> the secondary ray direction</returns>
        public Vector3D GetSecondaryRay(Vector3D direction, Vector3D normal)
        {
            return direction - 2 * Vector3D.DotProduct(direction, normal) * normal;
        }

        /// <summary>
        ///     Translates a point from 3D space onto a 2D plane for the debug mode
        /// </summary>
        /// <param name="before">The point before translation</param>
        /// <param name="screen">The surface on which will be drawn, used for dimensions</param>
        /// <returns></returns>
        private Point3D TranslatePoint(Point3D before, Surface screen)
        {
            double virtualSize = 15;
            double measure = screen.width <= screen.height ? screen.width : screen.height;
            double change = measure / 2D / (virtualSize / 2D);
            Point3D after = new Point3D((before.X + virtualSize / 2D) * change,
                (-before.Y + virtualSize / 2D) * change + measure / 2 - 20, before.Z);
            return after;
        }

        /// <summary>
        ///     Translates a 3D ray into a 2D line segment for the debug mode and draws it on the screen
        ///     Also plots this ray, shadow ray and reflection ray if present
        /// </summary>
        /// <param name="screen">The surface on which will be drawn, used for dimensions</param>
        /// <param name="objects">The objects in the scene</param>
        /// <param name="lights">The lights in the scene</param>
        /// <param name="ray">The ray before translation</param>
        private void PlotRay(Surface screen, List<Primitive> objects, List<Light> lights, Ray ray)
        {
            Point3D nulll = new Point3D(0, 0, 0);
            Primitive temp = _intersection.ClosestIntersect(ray, objects, null);
            Point3D endPoint = temp.Intersection;
            if (endPoint != nulll)
            {
                Point3D newStartPoint = TranslatePoint(ray.StartPoint, screen);
                Point3D newEndPoint = TranslatePoint(endPoint, screen);
                screen.Line((int)newStartPoint.X, (int)newStartPoint.Y, (int)newEndPoint.X, (int)newEndPoint.Y,
                    0xffff00);

                foreach (Light light in lights)
                {
                    Ray lightRay =
                        new Ray(
                            endPoint - new Vector3D(0.001 * light.Location.X, 0.001 * light.Location.Y,
                                0.001 * light.Location.Z), light.Location);
                    if (_intersection.ClosestIntersect(lightRay, objects, null) != temp || temp is Plane)
                    {
                        newStartPoint = TranslatePoint(lightRay.StartPoint, screen);
                        newEndPoint = TranslatePoint(light.Location, screen);
                        screen.Line((int)newStartPoint.X, (int)newStartPoint.Y, (int)newEndPoint.X,
                            (int)newEndPoint.Y, 0xff0000);
                    }
                }

                Vector3D normal = GetNormal(temp);
                Vector3D viewRay = endPoint - _camera.Location;
                viewRay.Normalize();

                Vector3D reflectDir = GetSecondaryRay(viewRay, normal);
                Ray raytje = new Ray(endPoint, reflectDir);

                Point3D point = _intersection.ClosestIntersect(raytje, objects, null).Intersection;
                if (point != nulll)
                {
                    newStartPoint = TranslatePoint(endPoint, screen);
                    newEndPoint = TranslatePoint(point, screen);
                    screen.Line((int)newStartPoint.X, (int)newStartPoint.Y, (int)newEndPoint.X, (int)newEndPoint.Y,
                        0x0000ff);
                }
            }
        }

        /// <summary>
        ///     Translates a 3D sphere onto a 2D plane for the debug mode by making a circle and draws it on the screen
        /// </summary>
        /// <param name="screen">The surface on which will be drawn, used for dimensions</param>
        /// <param name="sphere">The sphere used for the circle</param>
        private void PlotSphere(Surface screen, Primitive sphere)
        {
            for (int degree = 0; degree < 360; degree++)
            {
                Point3D point = new Point3D(sphere.StartPoint.X + sphere.Distance * Math.Cos(degree * Math.PI / 180),
                    sphere.StartPoint.Y + sphere.Distance * Math.Sin(degree * Math.PI / 180), sphere.StartPoint.Z);
                Point3D pointNew = TranslatePoint(point, screen);
                screen.Plot((int)pointNew.X, (int)pointNew.Y, sphere.Color);
            }
        }

        /// <summary>
        ///     Moves the camera in 5 degrees of freedom (not included: roll)
        /// </summary>
        /// <param name="command">The instruction for the camera movement</param>
        /// <param name="screen">The surface on which will be drawn</param>
        public void MoveCamera(string command, Surface screen)
        {
            double moveStepSize = 0.1;
            uint fovStepSize = 1;
            double rotateDegree = 5;
            switch (command)
            {
                case "moveForward":
                    _camera = new Camera(
                        new Point3D(_camera.Location.X, _camera.Location.Y + moveStepSize, _camera.Location.Z),
                        _camera.Direction, _camera.Fov, screen);
                    return;

                case "moveBackward":
                    _camera = new Camera(
                        new Point3D(_camera.Location.X, _camera.Location.Y - moveStepSize, _camera.Location.Z),
                        _camera.Direction, _camera.Fov, screen);
                    return;

                case "moveLeft":
                    _camera = new Camera(
                        new Point3D(_camera.Location.X - moveStepSize, _camera.Location.Y, _camera.Location.Z),
                        _camera.Direction, _camera.Fov, screen);
                    return;

                case "moveRight":
                    _camera = new Camera(
                        new Point3D(_camera.Location.X + moveStepSize, _camera.Location.Y, _camera.Location.Z),
                        _camera.Direction, _camera.Fov, screen);
                    return;

                case "moveUp":
                    _camera = new Camera(
                        new Point3D(_camera.Location.X, _camera.Location.Y, _camera.Location.Z + moveStepSize),
                        _camera.Direction, _camera.Fov, screen);
                    return;

                case "moveDown":
                    _camera = new Camera(
                        new Point3D(_camera.Location.X, _camera.Location.Y, _camera.Location.Z - moveStepSize),
                        _camera.Direction, _camera.Fov, screen);
                    return;

                case "decrease":
                    _camera = new Camera(_camera.Location, _camera.Direction, _camera.Fov - fovStepSize, screen);
                    return;

                case "increase":
                    _camera = new Camera(_camera.Location, _camera.Direction, _camera.Fov + fovStepSize, screen);
                    return;

                case "turnLeft":
                    _camera = new Camera(_camera.Location, RotateVectorHorizontal(_camera.Direction, -rotateDegree),
                        _camera.Fov, screen);
                    return;

                case "turnRight":
                    _camera = new Camera(_camera.Location, RotateVectorHorizontal(_camera.Direction, rotateDegree),
                        _camera.Fov, screen);
                    return;

                case "turnUp":
                    _camera = new Camera(_camera.Location, RotateVectorVertical(_camera.Direction, -rotateDegree),
                        _camera.Fov, screen);
                    return;

                case "turnDown":
                    _camera = new Camera(_camera.Location, RotateVectorVertical(_camera.Direction, rotateDegree),
                        _camera.Fov, screen);
                    return;
            }
        }

        /// <summary>
        ///     Rotates a vector by yaw, used by the camera to look around
        /// </summary>
        /// <param name="before">The vector before rotation</param>
        /// <param name="degree">The angle of rotation</param>
        /// <returns>The rotated vector</returns>
        private Vector3D RotateVectorHorizontal(Vector3D before, double degree)
        {
            double theta = -degree * Math.PI / 180;
            Vector3D after = new Vector3D(before.X * Math.Cos(theta) - before.Y * Math.Sin(theta),
                before.X * Math.Sin(theta) + before.Y * Math.Cos(theta), before.Z);
            return after;
        }

        /// <summary>
        ///     Rotates a vector by pitch, used by the camera to look up and down
        /// </summary>
        /// <param name="before">The vector before rotation</param>
        /// <param name="degree">The angle of rotation</param>
        /// <returns>The rotated vector</returns>
        private Vector3D RotateVectorVertical(Vector3D before, double degree)
        {
            double theta = -degree * Math.PI / 180;
            Vector3D after = new Vector3D(before.X, before.Y * Math.Cos(theta) - before.Z * Math.Sin(theta),
                before.Y * Math.Sin(theta) + before.Z * Math.Cos(theta));
            return after;
        }

        /// <summary>
        ///     Applies fast approximate anti-aliasing to the generated pixel array
        ///     by looking at the 8 pixels around a certain pixel
        /// </summary>
        /// <param name="screen">The surface on which will be drawn, used for dimensions</param>
        private void AntiAliasing(Surface screen)
        {
            if (AAon)
                for (int z = 1; z < screen.height - 1; z++)
                    for (int x = 1; x < screen.width - 1; x++)
                    {
                        int redBucket = ((_pixels[screen.width * z + x - 1] >> 16) + (_pixels[screen.width * z + x] >> 16) +
                                         (_pixels[screen.width * z + x + 1] >> 16) +
                                         (_pixels[screen.width * (z - 1) + x] >> 16) +
                                         (_pixels[screen.width * (z + 1) + x] >> 16)) / 5;
                        int greenBucket = (((_pixels[screen.width * z + x - 1] >> 8) & 0xFF) +
                                           ((_pixels[screen.width * z + x] >> 8) & 0xFF) +
                                           ((_pixels[screen.width * z + x + 1] >> 8) & 0xFF) +
                                           ((_pixels[screen.width * (z - 1) + x] >> 8) & 0xFF) +
                                           ((_pixels[screen.width * (z + 1) + x] >> 8) & 0xFF)) / 5;
                        int blueBucket = ((_pixels[screen.width * z + x - 1] & 0xFF) +
                                          (_pixels[screen.width * z + x] & 0xFF) +
                                          (_pixels[screen.width * z + x + 1] & 0xFF) +
                                          (_pixels[screen.width * (z - 1) + x] & 0xFF) +
                                          (_pixels[screen.width * (z + 1) + x] & 0xFF)) / 5;
                        screen.pixels[screen.width * z + x] = (redBucket << 16) + (greenBucket << 8) + blueBucket;
                    }
            else
                screen.pixels = _pixels;
        }
    }
}