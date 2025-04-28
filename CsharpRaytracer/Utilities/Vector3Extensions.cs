using System;
using System.Numerics;

namespace CsharpRaytracer.Utilities
{
    public static class Vector3Extensions
    {
        // Precomputed constants for Uncharted 2 tone mapping
        private const float A = 0.15f;
        private const float B = 0.50f;
        private const float C = 0.10f;
        private const float D = 0.20f;
        private const float E = 0.02f;
        private const float F = 0.30f;
        private const float W = 11.2f; // white point (brightest value expected in scene)
        private static readonly float ScaleFactor = 1.0f / Tonemap(W);

        private static float Tonemap(float x)
        {
            return (((x * ((A * x) + (C * B))) + (D * E)) /
                    ((x * ((A * x) + B)) + (D * F))) - (E / F);
        }

        /// <summary>
        /// Applies Reinhard tone mapping to the vector.
        /// </summary>
        public static Vector3 ApplyReinhardToneMapping(this Vector3 color)
        {
            return color / (Vector3.One + color);
        }

        /// <summary>
        /// Applies gamma correction to the vector.
        /// </summary>
        /// <param name="gamma">The gamma value (default is 2.2).</param>
        public static Vector3 ApplyGammaCorrection(this Vector3 color, float gamma = 2.2f)
        {
            float invGamma = 1.0f / gamma;
            return new Vector3(
                MathF.Pow(color.X, invGamma),
                MathF.Pow(color.Y, invGamma),
                MathF.Pow(color.Z, invGamma)
            );
        }

        /// <summary>
        /// Applies Uncharted 2 tone mapping to the vector.
        /// </summary>
        public static Vector3 ApplyUncharted2Tonemap(this Vector3 color)
        {
            // Apply tone mapping to each component
            return new Vector3(
                Tonemap(color.X),
                Tonemap(color.Y),
                Tonemap(color.Z)
            ) * ScaleFactor;
        }

        /// <summary>
        /// Applies a fast approximation of gamma correction to the vector using square root.
        /// </summary>
        public static Vector3 FastGammaCorrect(this Vector3 color)
        {
            return new Vector3(
                MathF.Sqrt(color.X),
                MathF.Sqrt(color.Y),
                MathF.Sqrt(color.Z)
            );
        }
    }
}
