using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer
{
    public abstract class AreaLight
    {
        protected float ConstantAttenuation;

        protected float LinearAttenuation;

        protected float QuadraticAttenuation;

        protected float Intensity;

        protected Vector3 Color;

        protected readonly Random random;

        protected AreaLight(
            float constantAttenuation,
            float linearAttenuation,
            float quadraticAttenuation,
            float intensity,
            Vector3 color)
        {
            this.ConstantAttenuation = constantAttenuation;
            this.LinearAttenuation = linearAttenuation;
            this.QuadraticAttenuation = quadraticAttenuation;
            this.Intensity = intensity;
            this.Color = color;
            this.random = new Random();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetAttenuatedIntensity(float distance)
        {
            float attenuation = this.ConstantAttenuation +
                (this.LinearAttenuation * distance) +
                (this.QuadraticAttenuation * distance * distance);

            return this.Intensity / attenuation;
        }

        public abstract Vector3 SampleRandomPoint();

        public abstract Vector3 GetShadowRayDirection(Vector3 pointFrom, Vector3 sampledPointOnLightSurface);

        public abstract (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorBlinnPhong(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo, Vector3 sampledPointOnLightSurface);

        public abstract (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorCelShading(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo, Vector3 sampledPointOnLightSurface);
    }
}
