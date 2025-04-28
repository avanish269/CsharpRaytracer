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

        public override Vector3 GetShadowRayDirection(Vector3 pointFrom)
        {
            return Vector3.Normalize(this.Source - pointFrom);
        }

        // TODO: FresnelSchlick approximation
        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorBlinnPhongShading(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo)
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

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorCelShading(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo)
        {
            Vector3 lightDirection = Vector3.Normalize(this.Source - intersectionInfo.IntersectionPoint);
            float distanceFromLight = Vector3.Distance(this.Source, intersectionInfo.IntersectionPoint);
            float attenuatedIntensity = this.GetAttenuatedIntensity(distanceFromLight);

            float diffuseIntensity = Vector3.Dot(intersectionInfo.NormalAtIntersection, lightDirection);
            diffuseIntensity = MathF.Max(0.0f, diffuseIntensity);

            float quantizedDiffuse = this.Quantize(diffuseIntensity, 3);
            Vector3 diffuseColor = attenuatedIntensity * (quantizedDiffuse * intersectionInfo.Material.DiffuseCoefficient) * this.Color;

            Vector3 halfVector = Vector3.Normalize(lightDirection - rayDirection);
            float NdotH = Vector3.Dot(intersectionInfo.NormalAtIntersection, halfVector);
            float specularTerm = MathF.Max(0.0f, NdotH);
            specularTerm = MathF.Pow(specularTerm, intersectionInfo.Material.SpecularExponent);
            specularTerm = this.Quantize(specularTerm, 2);
            Vector3 specularColor = attenuatedIntensity * (specularTerm * intersectionInfo.Material.SpecularCoefficient) * this.Color;

            return (diffuseColor, specularColor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Quantize(float value, int levels)
        {
            float step = 1.0f / levels;
            return MathF.Floor(value / step) * step;
        }
    }
}
