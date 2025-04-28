using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer
{
    public class RectangleAreaLight : AreaLight
    {
        private Vector3 corner1; // One corner of the rectangle

        private Vector3 corner2; // Diagonal corner of the rectangle

        private Vector3 corner3; // Third corner (used for the normal)

        private Vector3 edge1;

        private Vector3 edge2;

        public RectangleAreaLight(
            Vector3 corner1,
            Vector3 corner2,
            Vector3 corner3,
            float constantAttenuation,
            float linearAttenuation,
            float quadraticAttenuation,
            float intensity,
            Vector3 color)
            : base(constantAttenuation, linearAttenuation, quadraticAttenuation, intensity, color)
        {
            this.corner1 = corner1;
            this.corner2 = corner2;
            this.corner3 = corner3;
            this.edge1 = corner2 - corner1;
            this.edge2 = corner3 - corner1;
        }

        public override Vector3 SampleRandomPoint()
        {
            float u = this.random.NextSingle();
            float v = this.random.NextSingle();

            return this.corner1 + (u * this.edge1) + (v * this.edge2);
        }

        public override Vector3 GetShadowRayDirection(Vector3 pointFrom, Vector3 sampledPointOnLightSurface)
        {
            return Vector3.Normalize(sampledPointOnLightSurface - pointFrom);
        }

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorBlinnPhong(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo, Vector3 sampledPointOnLightSurface)
        {
            Vector3 lightDirection = sampledPointOnLightSurface - intersectionInfo.IntersectionPoint;
            float distanceFromLight = lightDirection.Length();
            lightDirection = Vector3.Normalize(lightDirection);
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

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColorCelShading(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo, Vector3 sampledPointOnLightSurface)
        {
            Vector3 lightDirection = sampledPointOnLightSurface - intersectionInfo.IntersectionPoint;
            float distanceFromLight = lightDirection.Length();
            lightDirection = Vector3.Normalize(lightDirection);
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
