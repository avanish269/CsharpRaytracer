using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer
{
    public class PointLight : Light
    {
        private Vector3 Source;

        public PointLight(
            Vector3 source,
            float constantAttenuation,
            float linearAttenuation,
            float quadraticAttenuation,
            float intensity,
            Vector3 color)
            : base(constantAttenuation,
                linearAttenuation,
                quadraticAttenuation,
                intensity,
                color)
        {
            this.Source = source;
            this.ConstantAttenuation = constantAttenuation;
            this.LinearAttenuation = linearAttenuation;
            this.QuadraticAttenuation = quadraticAttenuation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetAttenuatedIntensity(float distance)
        {
            float attenuation = this.ConstantAttenuation +
                (this.LinearAttenuation * distance) +
                (this.QuadraticAttenuation * distance * distance);

            return this.Intensity / attenuation;
        }

        public override (Vector3 shadowRayOrigin, Vector3 shadowRayDirection) GetShadowRay(IntersectionInfo intersectionInfo)
        {
            float offset = Constants.Offset + intersectionInfo.SceneObject.Thickness;
            Vector3 intersectionPointWithOffset = intersectionInfo.IntersectionPoint + (intersectionInfo.NormalAtIntersection * offset);
            Vector3 shadowRayOrigin = intersectionPointWithOffset;
            Vector3 shadowRayDirection = Vector3.Normalize(this.Source - intersectionPointWithOffset);
            return (shadowRayOrigin, shadowRayDirection);
        }

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColor(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo)
        {
            Vector3 lightDirection = Vector3.Normalize(this.Source - intersectionInfo.IntersectionPoint);
            float distanceFromLight = Vector3.Distance(this.Source, intersectionInfo.IntersectionPoint);

            float NdotL = Vector3.Dot(intersectionInfo.NormalAtIntersection, lightDirection);
            float lambertianTerm = MathF.Max(0.0f, NdotL);

            Vector3 diffuseColor = (this.GetAttenuatedIntensity(distanceFromLight) * lambertianTerm) * (intersectionInfo.Material.DiffuseCoefficient * this.Color);

            Vector3 halfVector = Vector3.Normalize(lightDirection - rayDirection);
            float NdotH = Vector3.Dot(intersectionInfo.NormalAtIntersection, halfVector);
            float specularTerm = MathF.Max(0.0f, NdotH);

            Vector3 specularColor = (this.GetAttenuatedIntensity(distanceFromLight) * MathF.Pow(specularTerm, intersectionInfo.Material.SpecularExponent)) * (intersectionInfo.Material.SpecularCoefficient * this.Color);

            return (diffuseColor, specularColor);
        }
    }
}
