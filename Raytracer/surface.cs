#region Packages

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

#endregion Packages

namespace Raytracer
{
    public class Surface
    {
        private static bool fontReady;
        private static Surface font;
        private static int[] fontRedir;
        public int[] pixels;
        public int width, height;

        // Surface constructor
        public Surface(int w, int h)
        {
            width = w;
            height = h;
            pixels = new int[w * h];
        }

        // Surface constructor using a file
        public Surface(string fileName)
        {
            Bitmap bmp = new Bitmap(fileName);
            width = bmp.Width;
            height = bmp.Height;
            pixels = new int[width * height];
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            Marshal.Copy(data.Scan0, pixels, 0, width * height);
            bmp.UnlockBits(data);
        }

        // Create an OpenGL texture
        public int GenTexture()
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            return id;
        }

        // Clear the surface
        public void Clear(int c)
        {
            for (int s = width * height, p = 0; p < s; p++) pixels[p] = c;
        }

        // Copy the surface to another surface
        public void CopyTo(Surface target, int x = 0, int y = 0)
        {
            int src = 0;
            int dst = 0;
            int srcwidth = width;
            int srcheight = height;
            int dstwidth = target.width;
            int dstheight = target.height;
            if (srcwidth + x > dstwidth) srcwidth = dstwidth - x;

            if (srcheight + y > dstheight) srcheight = dstheight - y;

            if (x < 0)
            {
                src -= x;
                srcwidth += x;
                x = 0;
            }

            if (y < 0)
            {
                src -= y * width;
                srcheight += y;
                y = 0;
            }

            if (srcwidth > 0 && srcheight > 0)
            {
                dst += x + dstwidth * y;
                for (int v = 0; v < srcheight; v++)
                {
                    for (int u = 0; u < srcwidth; u++) target.pixels[dst + u] = pixels[src + u];

                    dst += dstwidth;
                    src += width;
                }
            }
        }

        // Draw a rectangle
        public void Box(int x1, int y1, int x2, int y2, int c)
        {
            int dest = y1 * width;
            for (int y = y1; y <= y2; y++, dest += width)
            {
                pixels[dest + x1] = c;
                pixels[dest + x2] = c;
            }

            int dest1 = y1 * width;
            int dest2 = y2 * width;
            for (int x = x1; x <= x2; x++)
            {
                pixels[dest1 + x] = c;
                pixels[dest2 + x] = c;
            }
        }

        // Draw a solid bar
        public void Bar(int x1, int y1, int x2, int y2, int c)
        {
            int dest = y1 * width;
            for (int y = y1; y <= y2; y++, dest += width)
                for (int x = x1; x <= x2; x++)
                    pixels[dest + x] = c;
        }

        // Helper function for line clipping
        private int OUTCODE(int x, int y)
        {
            int xmin = 0, ymin = 0, xmax = width - 1, ymax = height - 1;
            return (x < xmin ? 1 : x > xmax ? 2 : 0) + (y < ymin ? 4 : y > ymax ? 8 : 0);
        }

        // Draw a line, clipped to the window
        public void Line(int x1, int y1, int x2, int y2, int c)
        {
            int xmin = 0, ymin = 0, xmax = width - 1, ymax = height - 1;
            int c0 = OUTCODE(x1, y1), c1 = OUTCODE(x2, y2);
            bool accept = false;
            while (true)
                if (c0 == 0 && c1 == 0)
                {
                    accept = true;
                    break;
                }
                else if ((c0 & c1) > 0)
                {
                    break;
                }
                else
                {
                    int x = 0, y = 0;
                    int co = c0 > 0 ? c0 : c1;
                    if ((co & 8) > 0)
                    {
                        x = x1 + (x2 - x1) * (ymax - y1) / (y2 - y1);
                        y = ymax;
                    }
                    else if ((co & 4) > 0)
                    {
                        x = x1 + (x2 - x1) * (ymin - y1) / (y2 - y1);
                        y = ymin;
                    }
                    else if ((co & 2) > 0)
                    {
                        y = y1 + (y2 - y1) * (xmax - x1) / (x2 - x1);
                        x = xmax;
                    }
                    else if ((co & 1) > 0)
                    {
                        y = y1 + (y2 - y1) * (xmin - x1) / (x2 - x1);
                        x = xmin;
                    }

                    if (co == c0)
                    {
                        x1 = x;
                        y1 = y;
                        c0 = OUTCODE(x1, y1);
                    }
                    else
                    {
                        x2 = x;
                        y2 = y;
                        c1 = OUTCODE(x2, y2);
                    }
                }

            if (!accept) return;

            if (Math.Abs(x2 - x1) >= Math.Abs(y2 - y1))
            {
                if (x2 < x1)
                {
                    int h = x1;
                    x1 = x2;
                    x2 = h;
                    h = y2;
                    y2 = y1;
                    y1 = h;
                }

                int l = x2 - x1;
                if (l == 0) return;

                int dy = (y2 - y1) * 8192 / l;
                y1 *= 8192;
                for (int i = 0; i < l; i++)
                {
                    pixels[x1++ + y1 / 8192 * width] = c;
                    y1 += dy;
                }
            }
            else
            {
                if (y2 < y1)
                {
                    int h = x1;
                    x1 = x2;
                    x2 = h;
                    h = y2;
                    y2 = y1;
                    y1 = h;
                }

                int l = y2 - y1;
                if (l == 0) return;

                int dx = (x2 - x1) * 8192 / l;
                x1 *= 8192;
                for (int i = 0; i < l; i++)
                {
                    pixels[x1 / 8192 + y1++ * width] = c;
                    x1 += dx;
                }
            }
        }

        // Plot a single pixel
        public void Plot(int x, int y, int c)
        {
            if (x >= 0 && y >= 0 && x < width && y < height) pixels[x + y * width] = c;
        }

        // Print a string
        public void Print(string t, int x, int y, int c)
        {
            if (!fontReady)
            {
                font = new Surface("../../assets/font.png");
                string ch =
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+={}[];:<>,.?/\\ ";
                fontRedir = new int[256];
                for (int i = 0; i < 256; i++) fontRedir[i] = 0;

                for (int i = 0; i < ch.Length; i++)
                {
                    int l = ch[i];
                    fontRedir[l & 255] = i;
                }

                fontReady = true;
            }

            for (int i = 0; i < t.Length; i++)
            {
                int f = fontRedir[t[i] & 255];
                int dest = x + i * 12 + y * width;
                int src = f * 12;
                for (int v = 0; v < font.height; v++, src += font.width, dest += width)
                    for (int u = 0; u < 12; u++)
                        if ((font.pixels[src + u] & 0xffffff) != 0)
                            pixels[dest + u] = c;
            }
        }
    }

    public class Sprite
    {
        public static Surface target;
        private readonly Surface bitmap;
        private readonly int textureID;

        // Sprite constructor
        public Sprite(string fileName)
        {
            bitmap = new Surface(fileName);
            textureID = bitmap.GenTexture();
        }

        // Draw a sprite with scaling
        public void Draw(float x, float y, float scale = 1.0f)
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Begin(PrimitiveType.Quads);
            float u1 = (x * 2 - 0.5f * scale * bitmap.width) / target.width - 1;
            float v1 = 1 - (y * 2 - 0.5f * scale * bitmap.height) / target.height;
            float u2 = (x + 0.5f * scale * bitmap.width) * 2 / target.width - 1;
            float v2 = 1 - (y + 0.5f * scale * bitmap.height) * 2 / target.height;
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(u1, v2);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(u2, v2);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(u2, v1);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(u1, v1);
            GL.End();
            GL.Disable(EnableCap.Blend);
        }
    }
}