using System;
using System.Numerics;

namespace CsharpRaytracer
{
    public class Sphere : SceneObject
    {
        private readonly Vector3 Center;

        private readonly float Radius;

        public Sphere(Vector3 center, float radius, Material material)
            : base(material, thickness: 0.0f)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public override bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            intersectionInfo = new IntersectionInfo();
            Vector3 oc = rayOrigin - this.Center;
            float a = Vector3.Dot(rayDirection, rayDirection);
            float b = 2.0f * Vector3.Dot(oc, rayDirection);
            float c = Vector3.Dot(oc, oc) - (this.Radius * this.Radius);
            float discriminant = (b * b) - (4 * a * c);
            if (discriminant < 0)
                return false;
            float sqrtDiscriminant = MathF.Sqrt(discriminant);
            float q = (b < 0.0f) ? (b - sqrtDiscriminant) * -0.5f : (b + sqrtDiscriminant) * -0.5f;

            float t0 = q / a;
            float t1 = c / q;

            if (t0 > t1)
            {
                (t0, t1) = (t1, t0);
            }

            float t = t0;

            if (t0 < 0)
            {
                t = t1;
                if (t1 < 0)
                {
                    return false;
                }
            }

            Vector3 intersectionPoint = rayOrigin + (t * rayDirection);
            Vector3 normal = Vector3.Normalize(intersectionPoint - this.Center);

            if (Vector3.Dot(normal, rayDirection) > 0)
            {
                normal = Vector3.Negate(normal);
            }

            intersectionInfo = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);
            return true;
        }
    }
}
