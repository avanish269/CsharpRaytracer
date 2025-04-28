using CsharpRaytracer.Core;
using CsharpRaytracer.Utilities;
using System;
using System.Numerics;

namespace CsharpRaytracer.Geometry
{
    public class Plane : SceneObject
    {
        private Vector3 Center;

        private Vector3 Normal;

        private Vector3 u; // Vector u (span of the plane)

        private Vector3 v; // Vector v (span of the plane)

        public Plane(Vector3 center, Vector3 normal, Vector3 u, Vector3 v, Material material)
            : base(material, thickness: 0)
        {
            this.Center = center;
            this.Normal = Vector3.Normalize(normal);
            // Any point on plane can be written as
            // P = P0 + u * s + v * t
            // where s and t are between -1.0 and 1.0
            this.u = u;
            this.v = v;
        }

        public override bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            intersectionInfo = new IntersectionInfo();
            float denominator = Vector3.Dot(rayDirection, this.Normal);

            if (MathF.Abs(denominator) < Constants.Epsilon)
                return false;

            float numerator = Vector3.Dot(this.Center - rayOrigin, this.Normal);

            float t = numerator / denominator;

            if (t < 0)
                return false;

            Vector3 intersectionPoint = rayOrigin + (t * rayDirection);

            // Check if the intersection point is within the bounds of the plane
            Vector3 w = intersectionPoint - this.Center;
            float uDotu = Vector3.Dot(this.u, this.u);
            float uDotV = Vector3.Dot(this.u, this.v);
            float vDotv = Vector3.Dot(this.v, this.v);

            float wDotu = Vector3.Dot(w, this.u);
            float wDotv = Vector3.Dot(w, this.v);

            float determinant = (uDotu * vDotv) - (uDotV * uDotV);
            float s_coordinate = ((vDotv * wDotu) - (uDotV * wDotv)) / determinant;
            float t_coordinate = ((uDotu * wDotv) - (uDotV * wDotu)) / determinant;

            if (s_coordinate < -1.0f || s_coordinate > 1.0f || t_coordinate < -1.0f || t_coordinate > 1.0f)
                return false;

            Vector3 normal = this.Normal;

            if (Vector3.Dot(normal, rayDirection) > 0)
            {
                normal = Vector3.Negate(normal);
            }

            intersectionInfo = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);

            return true;
        }
    }
}
