using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer
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
            intersectionInfo = null;

            // Check intersection with the outer sphere
            bool outerHit = this.CheckIntersectionForSphere(
                rayOrigin,
                rayDirection,
                this.OuterRadius,
                1,
                out IntersectionInfo outerInfo);

            bool innerHit = false;
            IntersectionInfo innerInfo = null;
            if (this.Thickness > 0.0f)
            {
                innerHit = this.CheckIntersectionForSphere(
                    rayOrigin,
                    rayDirection,
                    this.InnerRadius,
                    1,
                    out innerInfo);

                if (innerHit)
                    innerInfo.NormalAtIntersection = Vector3.Negate(innerInfo.NormalAtIntersection);
            }

            bool planeHit = this.CheckIntersectionWithPlane(
                rayOrigin,
                rayDirection,
                out IntersectionInfo planeInfo);

            if (outerHit && (!innerHit || (outerInfo.TForIntersection < planeInfo.TForIntersection && outerInfo.TForIntersection < innerInfo.TForIntersection)))
            {
                intersectionInfo = outerInfo;
            }
            else if (innerHit && innerInfo.TForIntersection < planeInfo.TForIntersection)
            {
                intersectionInfo = innerInfo;
            }
            else if (planeHit)
            {
                intersectionInfo = planeInfo;
            }

            if (intersectionInfo == null)
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckIntersectionForSphere(Vector3 rayOrigin, Vector3 rayDirection, float r, int clipSign, out IntersectionInfo info)
        {
            info = new IntersectionInfo();
            Vector3 oc = rayOrigin - this.Center;
            float a = Vector3.Dot(rayDirection, rayDirection);
            float b = 2.0f * Vector3.Dot(oc, rayDirection);
            float c = Vector3.Dot(oc, oc) - (r * r);
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

            foreach (float t in new float[] { t0, t1 })
            {
                if (t <= 0.0f)
                    continue;

                Vector3 intersectionPoint = rayOrigin + (t * rayDirection);
                Vector3 ic = intersectionPoint - this.Center;
                float hemisphereDot = Vector3.Dot(ic, this.Normal);

                bool isPointOnCorrectHemiphere = (clipSign > 0) ? (hemisphereDot >= 0.0f) : (hemisphereDot <= 0.0f);

                if (isPointOnCorrectHemiphere)
                {
                    Vector3 normal = Vector3.Normalize(intersectionPoint - this.Center);
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
            intersectionInfo = new IntersectionInfo();

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
                normal = Vector3.Negate(normal);
            }

            intersectionInfo = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);
            return true;
        }
    }
}
