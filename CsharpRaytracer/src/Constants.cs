using System.Numerics;

namespace CsharpRaytracer
{
    public static class Constants
    {
        public const int MaxDepth = 4;

        public const float ShadowIntensity = 1.0f;

        public const float Epsilon = 1e-6f;

        public const float Offset = 5.5e-3f;

        public static readonly Vector3 Background = Vector3.Zero;
    }
}
