using CsharpRaytracer.Core;
using CsharpRaytracer.Utilities;
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

            const float fieldOfView = 45.0f;
            Vector3 cameraLooksAt = new Vector3(0, 25.5f, -150);
            Vector3 worldUp = new Vector3(0, 1, 0);
            float latitudeAngle = 5 * MathF.PI / 180;
            float longitudeAngle = 30 * MathF.PI / 180;

            const float heightOfBoundingBox = 151.0f;
            const float widthOfBoundingBox = 100.0f;
            const float depthOfBoundingBox = 100.0f;
            const float halfWidth = widthOfBoundingBox / 2.0f;
            const float halfHeight = heightOfBoundingBox / 2.0f;
            const float halfDepth = depthOfBoundingBox / 2.0f;
            float cornerDistance = MathF.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight) + (halfDepth * halfDepth));
            float radiusOfCameraSphere = cornerDistance / MathF.Sin(fieldOfView * MathF.PI / 360.0f);

            var nativeWindowSettings = new NativeWindowSettings()
            {
                // WGA
                //ClientSize = new Vector2i(640, 480),

                // XGA
                ClientSize = new Vector2i(1024, 768),

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
                byte[] data = new byte[window.ClientSize.X * window.ClientSize.Y * 3];

                double previousTime = 0.0;
                int frameCount = 0;

                float cosLat = MathF.Cos(latitudeAngle);
                float sinLat = MathF.Sin(latitudeAngle);
                float cosLong = MathF.Cos(longitudeAngle);
                float sinLong = MathF.Sin(longitudeAngle);

                window.Load += () =>
                {
                    // Initialize OpenGL state
                    GL.ClearColor(0.5f, 1.0f, 1.0f, 0.0f);
                    Console.WriteLine($"Width = {window.ClientSize.X}, Height = {window.ClientSize.Y}");
                    openGLDraw.Init(window.ClientSize.X, window.ClientSize.Y);
                    data = new byte[window.ClientSize.X * window.ClientSize.Y * 3];
                };

                window.Resize += (ResizeEventArgs e) =>
                {
                    GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y);
                    Console.WriteLine($"Width = {window.ClientSize.X}, Height = {window.ClientSize.Y}");
                    openGLDraw.Init(window.ClientSize.X, window.ClientSize.Y);
                    data = new byte[window.ClientSize.X * window.ClientSize.Y * 3];
                };

                window.UpdateFrame += (FrameEventArgs e) =>
                {
                    if (window.KeyboardState.IsKeyDown(Keys.Escape))
                    {
                        window.Close();
                    }

                    if (window.KeyboardState.IsKeyDown(Keys.Left))
                    {
                        longitudeAngle -= 15.0f * MathF.PI / 180.0f;
                        longitudeAngle %= 2 * MathF.PI;
                        Console.WriteLine(longitudeAngle);
                    }

                    if (window.KeyboardState.IsKeyDown(Keys.Right))
                    {
                        longitudeAngle += 15.0f * MathF.PI / 180.0f;
                        longitudeAngle %= 2 * MathF.PI;
                        Console.WriteLine(longitudeAngle);
                    }

                    if (window.KeyboardState.IsKeyDown(Keys.Up))
                    {
                        latitudeAngle += 15.0f * MathF.PI / 180.0f;
                        latitudeAngle %= 2 * MathF.PI;
                        Console.WriteLine(latitudeAngle);
                    }

                    if (window.KeyboardState.IsKeyDown(Keys.Down))
                    {
                        latitudeAngle -= 15.0f * MathF.PI / 180.0f;
                        latitudeAngle %= 2 * MathF.PI;
                        Console.WriteLine(latitudeAngle);
                    }
                };

                window.RenderFrame += (FrameEventArgs e) =>
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    cosLat = MathF.Cos(latitudeAngle);
                    sinLat = MathF.Sin(latitudeAngle);
                    cosLong = MathF.Cos(longitudeAngle);
                    sinLong = MathF.Sin(longitudeAngle);
                    Vector3 conversionMatrix = new Vector3(cosLat * sinLong, sinLat, cosLat * cosLong);
                    Vector3 cameraAt = cameraLooksAt + (radiusOfCameraSphere * conversionMatrix);
                    Camera camera = new Camera(
                        cameraAt,
                        cameraLooksAt,
                        worldUp,
                        fieldOfView,
                        window.ClientSize.Y,
                        window.ClientSize.X);

                    Render(data, window.ClientSize.Y, window.ClientSize.X, scene, camera);

                    // Draw to screen
                    openGLDraw.Draw(data, window.ClientSize.X, window.ClientSize.Y, 0, 0);

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
