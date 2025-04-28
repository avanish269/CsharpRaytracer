using System;
using System.Numerics;

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

            if (this.Thickness == 0.0f)
            {
                intersectionInfo = outerInfo;
                return outerHit;
            }

            // Check intersection with the inner sphere
            bool innerHit = this.CheckIntersectionForSphere(
                rayOrigin,
                rayDirection,
                this.InnerRadius,
                1,
                out IntersectionInfo innerInfo);

            if (!outerHit && !innerHit)
                return false;

            if (innerHit)
                innerInfo.NormalAtIntersection = Vector3.Negate(innerInfo.NormalAtIntersection);

            if (outerHit && (!innerHit || outerInfo.TForIntersection < innerInfo.TForIntersection))
            {
                intersectionInfo = outerInfo;
            }
            else if (innerHit)
            {
                intersectionInfo = innerInfo;
            }

            return true;
        }

        private bool CheckIntersectionForSphere(Vector3 rayOrigin, Vector3 rayDirection, float r, int clipSign, out IntersectionInfo info)
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
    }
}
