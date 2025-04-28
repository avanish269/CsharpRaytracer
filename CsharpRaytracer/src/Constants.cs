using System.Numerics;

namespace CsharpRaytracer
{
    public static class Constants
    {
        public const int MaxDepth = 4;

        public const int NumberOfSamplesForAreaLight = 4;

        public const float ShadowIntensity = 1.0f;

        public const float Epsilon = 1e-6f;

        public const float Offset = 5.5e-3f;

        public const float Offset1e3f = 1e-3f;

        public const float Offset1e4f = 1e-4f;

        public static readonly Vector3 Background = Vector3.Zero;
    }
}
