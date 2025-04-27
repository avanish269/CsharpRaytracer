using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace CsharpRaytracer
{
    public class Scene
    {
        private readonly Vector3 ambientColor;

        private readonly List<SceneObject> sceneObjects;

        private readonly List<Light> lights;

        public Scene()
        {
            this.ambientColor = new Vector3(0.005f, 0.005f, 0.005f);
            this.sceneObjects = new List<SceneObject>();
            this.lights = new List<Light>();
            this.CreateScene();
        }

        public void CreateScene()
        {
            Material gold = new Material(
                new Vector3(0.75164f, 0.60648f, 0.22648f),  // Gold color for diffuse reflection  
                new Vector3(0.628281f, 0.555802f, 0.366065f),  // Gold-like specular reflection  
                new Vector3(0.24725f, 0.19950f, 0.07450f),  // Gold ambient reflection color  
                51.2f,  // High reflectivity like gold  
                0.9f,  // 90% of light is reflected  
                0.0f,  // No transmission  
                1.0f);  // Same as air, solid object  

            this.sceneObjects.Add(new Sphere(new Vector3(0f, 32f, -150f), 5f, gold));

            this.lights.Add(new PointLight(new Vector3(0f, 42f, -150f), 1.0f, 0.0f, 1.0f, 2.0f, new Vector3(0f, 1f, 1f)));
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

                foreach (Light light in  this.lights)
                {
                    (Vector3 shadowRayOrigin, Vector3 shadowRayDirection) = light.GetShadowRay(intersectionInfo);
                    if (this.CheckIntersection(shadowRayOrigin, shadowRayDirection))
                    {
                        continue; // Skip shading if the shadow ray intersects with any object

                        //TODO: if shadowIntersection is not None and shadowIntersection.object != light:
                        // Apply shadow (reduce light intensity)
                        //color *= (1 - SHADOW_INTENSITY)
                    }

                    (Vector3 diffuseColor, Vector3 specularColor) = light.GetDiffuseAndSpecularColor(rayOrigin, rayDirection, intersectionInfo);
                    diffuseIlluminance += diffuseColor;
                    specularIlluminance += specularColor;
                }

                totalIlluminance += ambientIlluminance;
                totalIlluminance += diffuseIlluminance;
                totalIlluminance += specularIlluminance;

                totalIlluminance += this.RayCast(rayOrigin, rayDirection, depth + 1);
                return totalIlluminance;
            }

            return Constants.Background;
        }
    }
}
