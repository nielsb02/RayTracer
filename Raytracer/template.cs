#region Packages

using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

#endregion Packages

// The template provides you with a window which displays a 'linear frame buffer', i.e.
// a 1D array of pixels that represents the graphical contents of the window.

// Under the hood, this array is encapsulated in a 'Surface' object, and copied once per
// frame to an OpenGL texture, which is then used to texture 2 triangles that exactly
// cover the window. This is all handled automatically by the template code.

// Before drawing the two triangles, the template calls the Tick method in MyApplication,
// in which you are expected to modify the contents of the linear frame buffer.

// After (or instead of) rendering the triangles you can add your own OpenGL code.

// We will use both the pure pixel rendering as well as straight OpenGL code in the
// tutorial. After the tutorial you can throw away this template code, or modify it at
// will, or maybe it simply suits your needs.

namespace Raytracer
{
    public class OpenTKApp : GameWindow
    {
        private static int screenID; // unique integer identifier of the OpenGL texture
        private static MyApplication app; // instance of the application
        private static bool terminated; // application terminates gracefully when this is true

        protected override void OnLoad(EventArgs e)
        {
            // called during application initialization
            GL.ClearColor(0, 0, 0, 0);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            ClientSize = new Size(840, 600);
            app = new MyApplication
            {
                Screen = new Surface(ClientSize.Width, ClientSize.Height)
            };
            Sprite.target = app.Screen;
            screenID = app.Screen.GenTexture();
            app.Init();
        }

        protected override void OnUnload(EventArgs e)
        {
            // called upon app close
            GL.DeleteTextures(1, ref screenID);
            Environment.Exit(0); // bypass wait for key on CTRL-F5
        }

        protected override void OnResize(EventArgs e)
        {
            // called upon window resize.
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
            app.Screen = new Surface(ClientSize.Width, ClientSize.Height);
            app.Update = true;
            app.HasResized = true;
        }

        /// <summary>
        ///     Runs every time a new frame is rendered, will keep track of user input
        /// </summary>
        /// <param name="e">Parameters given about the event</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // called once per frame; app logic
            var keyboard = Keyboard.GetState();
            app.Update = true;
            if (keyboard[Key.Escape])
                terminated = true;
            else if (keyboard[Key.T])
                app.DebugMode = true;
            else if (keyboard[Key.R])
                app.DebugMode = false;
            else if (keyboard[Key.W])
                app.Raytracer.MoveCamera("moveForward", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.S])
                app.Raytracer.MoveCamera("moveBackward", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.A])
                app.Raytracer.MoveCamera("moveLeft", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.D])
                app.Raytracer.MoveCamera("moveRight", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.E])
                app.Raytracer.MoveCamera("moveUp", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Q])
                app.Raytracer.MoveCamera("moveDown", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Minus])
                app.Raytracer.MoveCamera("decrease", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Plus])
                app.Raytracer.MoveCamera("increase", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Left])
                app.Raytracer.MoveCamera("turnLeft", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Right])
                app.Raytracer.MoveCamera("turnRight", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Up])
                app.Raytracer.MoveCamera("turnUp", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.Down])
                app.Raytracer.MoveCamera("turnDown", new Surface(ClientSize.Width, ClientSize.Height));
            else if (keyboard[Key.B])
                app.Raytracer.AAon = !app.Raytracer.AAon;
            else if (keyboard[Key.Number1])
                app.Raytracer.Scene = new Scene(0);
            else if (keyboard[Key.Number2])
                app.Raytracer.Scene = new Scene(1);
            else if (keyboard[Key.Number3])
                app.Raytracer.Scene = new Scene(2);
            else if (keyboard[Key.Number4])
                app.Raytracer.Scene = new Scene(3);
            else if (keyboard[Key.Number5])
                app.Raytracer.Scene = new Scene(4);
            else if (keyboard[Key.Number6])
                app.Raytracer.Scene = new Scene(5);
            else
                app.Update = false;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // called once per frame; render
            app.Tick();
            if (terminated)
            {
                Exit();
                return;
            }

            // convert MyApplication.screen to OpenGL texture
            GL.BindTexture(TextureTarget.Texture2D, screenID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                app.Screen.width, app.Screen.height, 0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte, app.Screen.pixels
            );
            // draw screen filling quad
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(-1.0f, -1.0f);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(1.0f, -1.0f);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(1.0f, 1.0f);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(-1.0f, 1.0f);
            GL.End();
            // tell OpenTK we're done rendering
            SwapBuffers();
        }

        public static void Main()
        {
            // entry point
            using (OpenTKApp app = new OpenTKApp())
            {
                app.Run(30.0, 0.0);
            }
        }
    }
}