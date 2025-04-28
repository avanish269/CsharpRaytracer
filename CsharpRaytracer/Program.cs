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
    internal static class Program
    {
        static void Render(byte[] data, int height, int width, Scene scene, Camera camera)
        {
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, height, parallelOptions, j =>
            {
                int rowOffset = (height - j - 1) * width;

                for (int i = 0; i < width; i++)
                {
                    camera.GetRayAtPixel(i, j, out Vector3 rayOrigin, out Vector3 rayDirection);

                    Vector3 color = scene.RayCast(rayOrigin, rayDirection, depth: 0);

                    color = color.ApplyReinhardToneMapping();
                    color = color.FastGammaCorrect();

                    color *= 255.0f; // Scale color to 0-255 range

                    // Calculate the position in framebuffer
                    int pixelIndex = i + rowOffset;

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

            Scene scene = new Scene();

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

                    float fieldOfView = 45.0f;
                    float angle = 30 * MathF.PI / 180;
                    Vector3 source = new Vector3(
                        -120.0f,
                        32.0f + (150.0f * MathF.Sin(angle)),
                        -150.0f * (1 - MathF.Cos(angle)));
                    Vector3 destination = new Vector3(0, 32, -150);
                    Vector3 cameraUp = new Vector3(0, 1, 0);
                    Camera camera = new Camera(
                        source,
                        destination,
                        cameraUp,
                        fieldOfView,
                        window.Size.Y,
                        window.Size.X);

                    Render(data, window.Size.Y, window.Size.X, scene, camera);

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
