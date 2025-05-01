using CsharpRaytracer.Core;
using CsharpRaytracer.Utilities;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer.Geometry
{
    public class Cylinder : SceneObject
    {
        private Vector3 BaseCenter;

        private Vector3 TopCenter;

        private Vector3 Direction;

        private readonly float OuterRadius;

        private readonly float InnerRadius;

        private readonly float Height;

        public Cylinder(
            Vector3 baseCenter,
            Vector3 topCenter,
            float outerRadius,
            Material material,
            float thickness)
            : base(material, thickness)
        {
            this.BaseCenter = baseCenter;
            this.TopCenter = topCenter;
            this.Direction = Vector3.Normalize(topCenter - baseCenter);
            this.OuterRadius = outerRadius;
            this.InnerRadius = outerRadius - thickness;
            this.Height = (topCenter - baseCenter).Length();
        }

        public override bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            intersectionInfo = new IntersectionInfo();

            float closestT = float.PositiveInfinity;

            if (this.CheckIntersectionForCylinder(rayOrigin, rayDirection, this.OuterRadius, true, out var outerInfo)
                && outerInfo.TForIntersection < closestT)
            {
                closestT = outerInfo.TForIntersection;
                intersectionInfo.Copy(outerInfo);
            }

            if (this.Thickness > 0.0f
                && this.CheckIntersectionForCylinder(rayOrigin, rayDirection, this.InnerRadius, false, out var innerInfo)
                && innerInfo.TForIntersection < closestT)
            {
                closestT = innerInfo.TForIntersection;
                intersectionInfo.Copy(innerInfo);
            }

            if (this.CheckIntersectionForDisk(rayOrigin, rayDirection, this.OuterRadius, true, out var topInfo)
                && topInfo.TForIntersection < closestT)
            {
                closestT = topInfo.TForIntersection;
                intersectionInfo.Copy(topInfo);
            }

            if (this.CheckIntersectionForDisk(rayOrigin, rayDirection, this.OuterRadius, false, out var bottomInfo)
                && bottomInfo.TForIntersection < closestT)
            {
                closestT = bottomInfo.TForIntersection;
                intersectionInfo.Copy(bottomInfo);
            }

            return closestT < float.PositiveInfinity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckIntersectionForCylinder(Vector3 rayOrigin, Vector3 rayDirection, float radius, bool isOuter, out IntersectionInfo info)
        {
            info = null;
            Vector3 D_parallel = Vector3.Dot(rayDirection, this.Direction) * this.Direction;
            Vector3 D_perpendicular = rayDirection - D_parallel;

            Vector3 localRayOrigin = rayOrigin - this.BaseCenter;
            Vector3 localRayOrigin_parallel = Vector3.Dot(localRayOrigin, this.Direction) * this.Direction;
            Vector3 localRayOrigin_perpendicular = localRayOrigin - localRayOrigin_parallel;

            float a = Vector3.Dot(D_perpendicular, D_perpendicular);

            if (Math.Abs(a) < Constants.Epsilon)
                return false;

            float b = 2.0f * Vector3.Dot(localRayOrigin_perpendicular, D_perpendicular);
            float c = Vector3.Dot(localRayOrigin_perpendicular, localRayOrigin_perpendicular) - (radius * radius);
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

            for (int i = 0; i < 2; i++)
            {
                float t = i == 0 ? t0 : t1;

                if (t <= Constants.Epsilon)
                    continue;

                Vector3 intersectionPoint = rayOrigin + (t * rayDirection);
                float projection = Vector3.Dot(intersectionPoint - this.BaseCenter, this.Direction);

                if (projection < 0.0f || projection > this.Height)
                    continue;

                Vector3 toHit = intersectionPoint - this.BaseCenter;
                Vector3 normal = Vector3.Normalize(toHit - (Vector3.Dot(toHit, this.Direction) * this.Direction));
                if (!isOuter)
                    normal = -normal;

                info = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckIntersectionForDisk(Vector3 rayOrigin, Vector3 rayDirection, float radius, bool isTop, out IntersectionInfo info)
        {
            info = null;
            float denominator = Vector3.Dot(rayDirection, this.Direction);

            if (MathF.Abs(denominator) < Constants.Epsilon)
                return false;

            Vector3 center = isTop ? this.TopCenter : this.BaseCenter;
            float numerator = Vector3.Dot(center - rayOrigin, this.Direction);

            float t = numerator / denominator;

            if (t <= Constants.Epsilon)
                return false;

            Vector3 intersectionPoint = rayOrigin + (t * rayDirection);
            float distanceSquared = (intersectionPoint - center).LengthSquared();

            if (distanceSquared > radius * radius)
                return false;

            if (this.Thickness > Constants.Epsilon
                && distanceSquared < this.InnerRadius * this.InnerRadius)
                return false;

            Vector3 normal = isTop ? this.Direction : -this.Direction;
            info = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);

            return true;
        }
    }
}
