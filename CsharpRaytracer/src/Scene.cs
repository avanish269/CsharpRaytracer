using System;
using System.Collections.Generic;
using System.Numerics;

namespace CsharpRaytracer
{
    public class Scene
    {
        private readonly Vector3 ambientColor;

        private readonly List<SceneObject> sceneObjects;

        private readonly List<Light> lights;

        private readonly List<AreaLight> areaLights;

        public Scene()
        {
            this.ambientColor = new Vector3(0.005f, 0.005f, 0.005f);
            this.sceneObjects = new List<SceneObject>();
            this.lights = new List<Light>();
            this.areaLights = new List<AreaLight>();
            this.CreateScene();
        }

        public void CreateScene()
        {
            Material white = new Material(
                new Vector3(1.0f, 1.0f, 1.0f),  // White color for diffuse reflection (albedo of the floor)
                new Vector3(1.0f, 1.0f, 1.0f),  // Full specular reflection (reflectance of water)
                new Vector3(0.0f, 0.0f, 0.0f),  // No ambient reflection
                1000f,  // Mirror-like surface, high specular exponent (water-like)
                1.0f,  // Full reflectivity
                0.0f,  // No transmission (solid, opaque)
                1.33f);  // Refractive index of water

            Material darkBrown = new Material(
                new Vector3(0.396f, 0.263f, 0.129f),  // Dark brown color for diffuse reflection
                new Vector3(0.396f, 0.263f, 0.129f),  // Same color for specular reflection
                new Vector3(0.0f, 0.0f, 0.0f),  // No ambient reflection
                10f,  // Moderate shininess
                0.35f,  // 35% of light is reflected
                0.65f,  // 65% of light passes through
                1.52f);  // Refractive index of glass

            Material silver = new Material(
                new Vector3(0.50754f, 0.50754f, 0.50754f),  // Silver diffuse color
                new Vector3(0.508273f, 0.508273f, 0.508273f),  // Silver specular reflection
                new Vector3(0.19225f, 0.19225f, 0.19225f),  // No ambient reflection
                1000f,  // Highly reflective like polished silver
                0.9f,  // 90% of light is reflected
                0.0f,  // No transmission (solid)
                1.0f);  // Same as air, as it’s a solid object

            Material gold = new Material(
                new Vector3(0.75164f, 0.60648f, 0.22648f),  // Gold color for diffuse reflection
                new Vector3(0.628281f, 0.555802f, 0.366065f),  // Gold-like specular reflection
                new Vector3(0.24725f, 0.19950f, 0.07450f),  // Gold ambient reflection color
                51.2f,  // High reflectivity like gold
                0.9f,  // 90% of light is reflected
                0.0f,  // No transmission
                1.0f);  // Same as air, solid object

            Material glass = new Material(
                new Vector3(0.6588f, 0.8f, 0.8431f),  // Light blue color for the glass
                new Vector3(0.9f, 0.9f, 0.9f),  // High specular reflection (shiny glass)
                new Vector3(0.0f, 0.0f, 0.0f),  // No ambient reflection
                96.0f,  // Highly shiny glass
                0.35f,  // 35% of light is reflected
                0.65f,  // 65% of light passes through
                1.333f);  // Refractive index of glass

            // Left bright yellow light
            this.lights.Add(
                new DirectionalLight(
                    new Vector3(-37.5f, 101f, -187.5f),
                    new Vector3(0f, 26f, -150f),
                    1.0f,
                    new Vector3(1.0f, 1.0f, 0.0f)));

            // Top bright yellow light
            this.lights.Add(
                new DirectionalLight(
                    new Vector3(37.5f, 101f, -187.5f),
                    new Vector3(0f, 26f, -150f),
                    1.0f,
                    new Vector3(1.0f, 1.0f, 0.0f)));

            // Bottom bright cyan light
            this.lights.Add(
                new DirectionalLight(
                    new Vector3(-37.5f, 101f, -112.5f),
                    new Vector3(0f, 26f, -150f),
                    1.0f,
                    new Vector3(0.0f, 1.0f, 1.0f)));

            // Right bright cyan light
            this.lights.Add(
                new DirectionalLight(
                    new Vector3(37.5f, 101f, -112.5f),
                    new Vector3(0f, 26f, -150f),
                    1.0f,
                    new Vector3(0.0f, 1.0f, 1.0f)));

            Vector3 corner1 = new Vector3(-1f, 42f, -151f); // Top-left
            Vector3 corner2 = new Vector3(1f, 42f, -151f);  // Top-right
            Vector3 corner3 = new Vector3(-1f, 42f, -149f); // Bottom-left

            // Center bright white light
            this.areaLights.Add(
                new RectangleAreaLight(
                    corner1,
                    corner2,
                    corner3,
                    1.0f,
                    0.0f,
                    1.0f,
                    1.0f,
                    new Vector3(1.0f, 1.0f, 1.0f)));

            this.sceneObjects.Add(new Sphere(new Vector3(0f, 32f, -150f), 5f, gold));

            this.sceneObjects.Add(new Sphere(new Vector3(0f, 38f, -150f), 1f, gold));

            this.sceneObjects.Add(new Sphere(new Vector3(0f, 38f, -151f), 1f, gold));
        }

        public bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            float nearestDist = float.MaxValue;
            intersectionInfo = new IntersectionInfo();
            bool doesIntersect = false;

            foreach (SceneObject sceneObject in this.sceneObjects)
            {
                if (sceneObject.CheckIntersection(rayOrigin, rayDirection, out IntersectionInfo info)
                    && info.TForIntersection < nearestDist)
                {
                    doesIntersect = true;
                    nearestDist = info.TForIntersection;
                    intersectionInfo.Copy(info);
                }
            }

            return doesIntersect;
        }

        public bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection)
        {
            foreach (SceneObject sceneObject in this.sceneObjects)
            {
                if (sceneObject.CheckIntersection(rayOrigin, rayDirection, out IntersectionInfo _))
                {
                    return true;
                }
            }

            return false;
        }

        public Vector3 RayCast(Vector3 rayOrigin, Vector3 rayDirection, int depth)
        {
            if (depth > Constants.MaxDepth)
                return Constants.Background;

            if (this.CheckIntersection(rayOrigin, rayDirection, out IntersectionInfo intersectionInfo))
            {
                Vector3 ambientIlluminance = this.ambientColor * intersectionInfo.Material.AmbientCoefficient;
                Vector3 diffuseIlluminance = Vector3.Zero;
                Vector3 specularIlluminance = Vector3.Zero;
                Vector3 totalIlluminance = Vector3.Zero;

                float offset = Constants.Offset + intersectionInfo.SceneObject.Thickness;
                Vector3 shadowRayOrigin = intersectionInfo.IntersectionPoint + (intersectionInfo.NormalAtIntersection * offset);

                foreach (Light light in this.lights)
                {
                    Vector3 shadowRayDirection = light.GetShadowRayDirection(shadowRayOrigin);
                    (Vector3 diffuseColor, Vector3 specularColor) = light.GetDiffuseAndSpecularColor(rayOrigin, rayDirection, intersectionInfo);

                    if (this.CheckIntersection(shadowRayOrigin, shadowRayDirection))
                    {
                        diffuseColor *= (1 - Constants.ShadowIntensity);
                        specularColor *= (1 - Constants.ShadowIntensity);
                    }

                    diffuseIlluminance += diffuseColor;
                    specularIlluminance += specularColor;
                }

                foreach (AreaLight areaLight in this.areaLights)
                {
                    Vector3 sampleDiffuseColor = Vector3.Zero;
                    Vector3 sampleSpecularColor = Vector3.Zero;
                    for (int i = 0; i < Constants.NumberOfSamplesForAreaLight; i++)
                    {
                        Vector3 sampledPointOnLightSurface = areaLight.SampleRandomPoint();
                        Vector3 shadowRayDirection = areaLight.GetShadowRayDirection(shadowRayOrigin, sampledPointOnLightSurface);
                        (Vector3 diffuseColor, Vector3 specularColor) = areaLight.GetDiffuseAndSpecularColor(rayOrigin, rayDirection, intersectionInfo, sampledPointOnLightSurface);

                        if (this.CheckIntersection(shadowRayOrigin, shadowRayDirection))
                        {
                            diffuseColor *= (1 - Constants.ShadowIntensity);
                            specularColor *= (1 - Constants.ShadowIntensity);
                        }

                        sampleDiffuseColor += diffuseColor;
                        sampleSpecularColor += specularColor;
                    }
                    diffuseIlluminance += sampleDiffuseColor / Constants.NumberOfSamplesForAreaLight;
                    specularIlluminance += sampleSpecularColor / Constants.NumberOfSamplesForAreaLight;
                }

                totalIlluminance += ambientIlluminance;
                totalIlluminance += diffuseIlluminance;
                totalIlluminance += specularIlluminance;

                if (intersectionInfo.Material.Reflectivity > 0)
                {
                    Vector3 reflectedRayDirection = rayDirection - (2 * Vector3.Dot(rayDirection, intersectionInfo.NormalAtIntersection) * intersectionInfo.NormalAtIntersection);
                    Vector3 reflectedRayOrigin = intersectionInfo.IntersectionPoint + (intersectionInfo.NormalAtIntersection * offset);
                    totalIlluminance += intersectionInfo.Material.Reflectivity * this.RayCast(reflectedRayOrigin, reflectedRayDirection, depth + 1);
                }

                if (intersectionInfo.Material.Transparency > 0)
                {
                    float eta = 1.0f / intersectionInfo.Material.RefractiveIndex;
                    float cosi = -Vector3.Dot(rayDirection, intersectionInfo.NormalAtIntersection);
                    float k = 1 - (eta * eta * (1 - (cosi * cosi)));
                    if (k > 0) // k < 0 means TIR
                    {
                        Vector3 refractedRayDirection =
                            (eta * rayDirection) + (((eta * cosi) - MathF.Sqrt(k)) * intersectionInfo.NormalAtIntersection);
                        Vector3 refractedRayOrigin = intersectionInfo.IntersectionPoint - (intersectionInfo.NormalAtIntersection * offset);
                        totalIlluminance += intersectionInfo.Material.Transparency * this.RayCast(refractedRayOrigin, refractedRayDirection, depth + 1);
                    }
                }

                return totalIlluminance;
            }

            return Constants.Background;
        }
    }
}
