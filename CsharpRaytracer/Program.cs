using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Threading.Tasks;
using Vector3 = System.Numerics.Vector3;

namespace CsharpRaytracer
{
    public class OpenGLDraw
    {
        private int width, height;
        private int fbo;

        public void Init(int W, int H)
        {
            int texture;
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            this.width = W;
            this.height = H;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, W, H, 0,
                PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

#pragma warning disable S125 // Sections of code should not be commented out
                            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.GenFramebuffers(1, out this.fbo);
#pragma warning restore S125 // Sections of code should not be commented out
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, this.fbo);
            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }

        public void Draw(byte[] image, int W = 0, int H = 0, int atx = 0, int aty = 0)
        {
            if (H == 0) H = this.height;
            if (W == 0) W = this.width;

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, atx, aty, W, H, PixelFormat.Rgb, PixelType.UnsignedByte, image);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, this.fbo);
            GL.BlitFramebuffer(0, 0, W, H, atx, aty, W, H,
                ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }
    }

    internal static class Program
    {
        static void Render(byte[] data, int height, int width)
        {
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, height, parallelOptions, j =>
            {
                for (int i = 0; i < width; i++)
                {
                    Vector3 color = Vector3.One * 255;

                    // Calculate the position in framebuffer
                    int pixelIndex = i + ((height - j - 1) * width);

                    // Write directly to data array (3 bytes per pixel)
                    int dataIndex = pixelIndex * 3;
                    data[dataIndex] = (byte)color.X;
                    data[dataIndex + 1] = (byte)color.Y;
                    data[dataIndex + 2] = (byte)color.Z;
                }
            });
        }

        static void Main(string[] args)
        {
            OpenGLDraw openGLDraw = new OpenGLDraw();

            var nativeWindowSettings = new NativeWindowSettings()
            {
                // WGA
                //ClientSize = new Vector2i(640, 480),

                // XGA
                ClientSize = new Vector2i(1280, 720),

                // WXGA
                //ClientSize = new Vector2i(1280, 720),

                // FHD
                //ClientSize = new Vector2i(1920, 1080),

                // QHD
                //ClientSize = new Vector2i(2560, 1440),

                // UHD
                //ClientSize = new Vector2i(3840, 2160),

                Title = "Recursive Ray Tracer",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core,
                Vsync = VSyncMode.On,
            };

            using (var window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                byte[] data = new byte[window.Size.X * window.Size.Y * 3];

                double previousTime = 0.0;
                int frameCount = 0;

                window.Load += () =>
                {
                    // Initialize OpenGL state
                    GL.ClearColor(0.5f, 1.0f, 1.0f, 0.0f);
                    Console.WriteLine($"Width = {window.Size.X}, Height = {window.Size.Y}");
                    openGLDraw.Init(window.Size.X, window.Size.Y);
                    data = new byte[window.Size.X * window.Size.Y * 3];
                };

                window.Resize += (ResizeEventArgs e) =>
                {
                    GL.Viewport(0, 0, window.Size.X, window.Size.Y);
                    Console.WriteLine($"Width = {window.Size.X}, Height = {window.Size.Y}");
                    openGLDraw.Init(window.Size.X, window.Size.Y);
                    data = new byte[window.Size.X * window.Size.Y * 3];
                };

                window.UpdateFrame += (FrameEventArgs e) =>
                {
                    if (window.KeyboardState.IsKeyDown(Keys.Escape))
                    {
                        window.Close();
                    }
                };

                window.RenderFrame += (FrameEventArgs e) =>
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    Render(data, window.Size.Y, window.Size.X);

                    // Draw to screen
                    openGLDraw.Draw(data, window.Size.X, window.Size.Y, 0, 0);

                    frameCount++;
                    double currentTime = GLFW.GetTime();
                    if (currentTime - previousTime >= 2.0)
                    {
                        window.Title = $"Recursive Ray Tracer [{frameCount / (currentTime - previousTime):F2} FPS]";
                        frameCount = 0;
                        previousTime = currentTime;
                    }

                    window.SwapBuffers();
                };

                window.Run();
            }
        }
    }
}
