using System;
using System.Numerics;

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

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColor(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo, Vector3 sampledPointOnLightSurface)
        {
            Vector3 lightDirection = Vector3.Normalize(sampledPointOnLightSurface - intersectionInfo.IntersectionPoint);
            float distanceFromLight = Vector3.Distance(sampledPointOnLightSurface, intersectionInfo.IntersectionPoint);

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
