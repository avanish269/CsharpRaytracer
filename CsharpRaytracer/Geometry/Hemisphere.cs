using CsharpRaytracer.Core;
using CsharpRaytracer.Utilities;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer.Geometry
{
    public class Hemisphere : SceneObject
    {
        private Vector3 Center;

        private Vector3 Normal;

        private float OuterRadius;

        private float InnerRadius;

        public Hemisphere(
            Vector3 center,
            Vector3 normal,
            float outerRadius,
            Material material,
            float thickness)
            : base(material, thickness)
        {
            this.Center = center;
            this.Normal = Vector3.Normalize(normal);
            this.OuterRadius = outerRadius;
            this.InnerRadius = outerRadius - thickness;
        }

        public override bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            intersectionInfo = new IntersectionInfo();

            float closestIntersection = float.PositiveInfinity;

            // Check intersection with the outer sphere
            bool outerHit = this.CheckIntersectionForSphere(
                rayOrigin,
                rayDirection,
                this.OuterRadius,
                isOuter: true,
                out IntersectionInfo outerInfo);

            if (outerHit && outerInfo.TForIntersection < closestIntersection)
            {
                closestIntersection = outerInfo.TForIntersection;
                intersectionInfo.Copy(outerInfo);
            }

            bool innerHit = false;
            IntersectionInfo innerInfo = null;
            if (this.Thickness > 0.0f)
            {
                innerHit = this.CheckIntersectionForSphere(
                    rayOrigin,
                    rayDirection,
                    this.InnerRadius,
                    isOuter: false,
                    out innerInfo);
            }

            if (innerHit && innerInfo.TForIntersection < closestIntersection)
            {
                closestIntersection = innerInfo.TForIntersection;
                intersectionInfo.Copy(innerInfo);
            }

            bool planeHit = this.CheckIntersectionWithPlane(
                rayOrigin,
                rayDirection,
                out IntersectionInfo planeInfo);

            if (planeHit && planeInfo.TForIntersection < closestIntersection)
            {
                closestIntersection = planeInfo.TForIntersection;
                intersectionInfo.Copy(planeInfo);
            }

            return closestIntersection < float.PositiveInfinity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckIntersectionForSphere(Vector3 rayOrigin, Vector3 rayDirection, float r, bool isOuter, out IntersectionInfo info)
        {
            info = null;
            Vector3 oc = rayOrigin - this.Center;
            float a = Vector3.Dot(rayDirection, rayDirection);
            float b = 2.0f * Vector3.Dot(oc, rayDirection);
            float c = Vector3.Dot(oc, oc) - (r * r);
            float discriminant = (b * b) - (4 * a * c);

            if (discriminant < 0)
                return false;
            float sqrtDiscriminant = MathF.Sqrt(discriminant);
            float q = b < 0.0f ? (b - sqrtDiscriminant) * -0.5f : (b + sqrtDiscriminant) * -0.5f;

            float t0 = q / a;
            float t1 = c / q;

            if (t0 > t1)
            {
                (t0, t1) = (t1, t0);
            }

            foreach (float t in new float[] { t0, t1 })
            {
                if (t <= 0.0f)
                    continue;

                Vector3 intersectionPoint = rayOrigin + (t * rayDirection);
                Vector3 ic = intersectionPoint - this.Center;

                if (Vector3.Dot(ic, this.Normal) >= 0.0f)
                {
                    Vector3 normal = Vector3.Normalize(intersectionPoint - this.Center);

                    if (!isOuter)
                        normal = -normal;

                    info = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);
                    return true;
                }
            }

            return false;
        }

        // Ray-plane intersection logic for the flat circular face (the open side of the hemisphere)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckIntersectionWithPlane(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            intersectionInfo = null;

            // We assume the plane equation is (P - Center) · Normal = 0
            float denominator = Vector3.Dot(rayDirection, this.Normal);

            if (MathF.Abs(denominator) < Constants.Epsilon)
                return false;

            float numerator = Vector3.Dot(this.Center - rayOrigin, this.Normal);

            float t = numerator / denominator;

            if (t < 0)
                return false;

            Vector3 intersectionPoint = rayOrigin + (t * rayDirection);

            // Check if the intersection point is within the bounds of the flat circle (hemisphere opening)
            float distFromCenter = Vector3.Distance(intersectionPoint, this.Center);

            if (distFromCenter > this.OuterRadius || (distFromCenter < this.InnerRadius && this.Thickness > Constants.Epsilon))
            {
                return false;
            }

            Vector3 normal = this.Normal;
            if (Vector3.Dot(normal, rayDirection) > 0)
            {
                normal = -normal;
            }

            intersectionInfo = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);
            return true;
        }
    }
}
