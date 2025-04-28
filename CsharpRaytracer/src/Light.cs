using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer
{
    public abstract class Light
    {
        protected float ConstantAttenuation;

        protected float LinearAttenuation;

        protected float QuadraticAttenuation;

        protected float Intensity;

        protected Vector3 Color;

        protected Light(
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetAttenuatedIntensity(float distance)
        {
            float attenuation = this.ConstantAttenuation +
                (this.LinearAttenuation * distance) +
                (this.QuadraticAttenuation * distance * distance);

            return this.Intensity / attenuation;
        }

        public abstract Vector3 GetShadowRayDirection(Vector3 pointFrom);

        public abstract (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorBlinnPhongShading(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo);

        public abstract (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorCelShading(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo);
    }
}
