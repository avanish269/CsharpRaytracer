using System;
using System.Numerics;

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

        public override Vector3 GetShadowRayDirection(Vector3 pointFrom)
        {
            return Vector3.Normalize(this.Source - pointFrom);
        }

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColor(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo)
        {
            Vector3 lightDirection = Vector3.Normalize(this.Source - intersectionInfo.IntersectionPoint);
            float distanceFromLight = Vector3.Distance(this.Source, intersectionInfo.IntersectionPoint);
            float attenuatedIntensity = this.GetAttenuatedIntensity(distanceFromLight);

            float NdotL = Vector3.Dot(intersectionInfo.NormalAtIntersection, lightDirection);
            float lambertianTerm = MathF.Max(0.0f, NdotL);

            Vector3 diffuseColor = (attenuatedIntensity * lambertianTerm) * (intersectionInfo.Material.DiffuseCoefficient * this.Color);

            Vector3 halfVector = Vector3.Normalize(lightDirection - rayDirection);
            float NdotH = Vector3.Dot(intersectionInfo.NormalAtIntersection, halfVector);
            float specularTerm = MathF.Max(0.0f, NdotH);

            Vector3 specularColor = (attenuatedIntensity * MathF.Pow(specularTerm, intersectionInfo.Material.SpecularExponent)) * (intersectionInfo.Material.SpecularCoefficient * this.Color);

            return (diffuseColor, specularColor);
        }
    }
}
