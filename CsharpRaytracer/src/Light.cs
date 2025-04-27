using System.Numerics;

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

        public abstract (Vector3 shadowRayOrigin, Vector3 shadowRayDirection) GetShadowRay(IntersectionInfo intersectionInfo);

        public abstract (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColor(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo);
    }
}
