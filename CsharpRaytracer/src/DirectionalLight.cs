using System;
using System.Numerics;

namespace CsharpRaytracer
{
    public class DirectionalLight : Light
    {
        private Vector3 Source;

        private Vector3 Direction;

        private Vector3 DirectionFromPoint;

        public DirectionalLight(Vector3 source, Vector3 destination, float intensity, Vector3 color)
            : base(constantAttenuation: 1, linearAttenuation: 0, quadraticAttenuation: 0, intensity: intensity, color: color)
        {
            this.Source = source;
            this.Direction = Vector3.Normalize(destination - source);
            this.DirectionFromPoint = Vector3.Normalize(source - destination);
        }

        public override Vector3 GetShadowRayDirection(Vector3 pointFrom)
        {
            return this.DirectionFromPoint;
        }

        public override (Vector3 DiffuseColor, Vector3 SpecularColor) GetDiffuseAndSpecularColor(Vector3 rayOrigin, Vector3 rayDirection, IntersectionInfo intersectionInfo)
        {
            Vector3 lightDirection = this.DirectionFromPoint;
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
