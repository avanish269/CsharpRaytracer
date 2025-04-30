using CsharpRaytracer.Core;
using CsharpRaytracer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CsharpRaytracer.Geometry
{
    public class Cuboid : SceneObject
    {
        private Vector3[] Corners;

        private Vector3 Center;

        private float Width;

        private float Height;

        private float Depth;

        private Vector3 Right;

        private Vector3 Up;

        private Vector3 Forward;

        private Matrix4x4 RotationMartrix;

        private readonly Dictionary<Vector3, HashSet<Vector3>> Adjacency;

        private readonly Matrix4x4 invRotation;

        private readonly Vector3 boxMin;

        private readonly Vector3 boxMax;

        public Cuboid(
            Vector3 corner1,
            Vector3 corner2,
            Vector3 corner3,
            Vector3 corner4,
            Vector3 corner5,
            Vector3 corner6,
            Vector3 corner7,
            Vector3 corner8,
            float width,
            float height,
            float depth,
            Material material)
            : base(material, thickness: 0.0f)
        {
            this.Corners = new Vector3[] { corner1, corner2, corner3, corner4, corner5, corner6, corner7, corner8 };
            this.Center = (corner1 + corner2 + corner3 + corner4 + corner5 + corner6 + corner7 + corner8) / 8.0f;
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
            this.Adjacency = new Dictionary<Vector3, HashSet<Vector3>>();

            this.BuildEdges();
            this.BuildRotationMatrix();

            Matrix4x4.Invert(this.RotationMartrix, out this.invRotation);

            this.boxMin = new Vector3(-this.Width / 2, -this.Height / 2, -this.Depth / 2);
            this.boxMax = new Vector3(this.Width / 2, this.Height / 2, this.Depth / 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildEdges()
        {
            float widthSquared = this.Width * this.Width;
            float heightSquared = this.Height * this.Height;
            float depthSquared = this.Depth * this.Depth;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == j)
                        continue;

                    float distanceSquared = (this.Corners[i] - this.Corners[j]).LengthSquared();

                    if (ApproximatelyEquals(distanceSquared, widthSquared)
                        || ApproximatelyEquals(distanceSquared, heightSquared)
                        || ApproximatelyEquals(distanceSquared, depthSquared))
                    {
                        this.AddEdge(this.Corners[i], this.Corners[j]);
                        this.AddEdge(this.Corners[j], this.Corners[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ApproximatelyEquals(float a, float b)
        {
            return MathF.Abs(a - b) < Constants.Offset1e3f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddEdge(Vector3 point1, Vector3 point2)
        {
            bool exists = this.Adjacency.TryGetValue(point1, out HashSet<Vector3> adjacentCorners);

            if (!exists)
            {
                adjacentCorners = new HashSet<Vector3> { point2 };
                this.Adjacency.Add(point1, adjacentCorners);
            }
            else
            {
                adjacentCorners.Add(point2);
                this.Adjacency[point1] = adjacentCorners;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildRotationMatrix()
        {
            Vector3 p0 = this.Corners[0], p1, p2;
            HashSet<Vector3> adjacentCorners = this.Adjacency[p0];

            Vector3[] twoValues = adjacentCorners.Take(2).ToArray();
            p1 = twoValues[0];
            p2 = twoValues[1];

            this.Right = Vector3.Normalize(p1 - p0);
            this.Up = Vector3.Normalize(p2 - p0);
            this.Forward = Vector3.Normalize(Vector3.Cross(this.Right, this.Up));

            this.RotationMartrix = new Matrix4x4(
                this.Right.X, this.Up.X, this.Forward.X, 0.0f,
                this.Right.Y, this.Up.Y, this.Forward.Y, 0.0f,
                this.Right.Z, this.Up.Z, this.Forward.Z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public override bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo)
        {
            intersectionInfo = null;

            Vector3 localOrigin = Vector3.Transform(rayOrigin - this.Center, this.invRotation);
            Vector3 localDirection = Vector3.TransformNormal(rayDirection, this.invRotation);

            float tMin = float.NegativeInfinity;
            float tMax = float.PositiveInfinity;

            // Axis - X
            {
                float invD = 1f / localDirection.X;
                float t1 = (this.boxMin.X - localOrigin.X) * invD;
                float t2 = (this.boxMax.X - localOrigin.X) * invD;
                if (invD < 0f) (t1, t2) = (t2, t1);
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax) return false;
            }

            // Axis - Y
            {
                float invD = 1f / localDirection.Y;
                float t1 = (this.boxMin.Y - localOrigin.Y) * invD;
                float t2 = (this.boxMax.Y - localOrigin.Y) * invD;
                if (invD < 0f) (t1, t2) = (t2, t1);
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax) return false;
            }

            // Axis - Z
            {
                float invD = 1f / localDirection.Z;
                float t1 = (this.boxMin.Z - localOrigin.Z) * invD;
                float t2 = (this.boxMax.Z - localOrigin.Z) * invD;
                if (invD < 0f) (t1, t2) = (t2, t1);
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax) return false;
            }

            if (tMax < 0)
                return false;

            float t = tMin >= 0 ? tMin : tMax;

            Vector3 localIntersectionPoint = localOrigin + t * localDirection;

            Vector3 localNormal;
            if (MathF.Abs(localIntersectionPoint.X - this.boxMin.X) < Constants.Offset1e4f)
                localNormal = new Vector3(-1, 0, 0);
            else if (MathF.Abs(localIntersectionPoint.X - this.boxMax.X) < Constants.Offset1e4f)
                localNormal = new Vector3(1, 0, 0);
            else if (MathF.Abs(localIntersectionPoint.Y - this.boxMin.Y) < Constants.Offset1e4f)
                localNormal = new Vector3(0, -1, 0);
            else if (MathF.Abs(localIntersectionPoint.Y - this.boxMax.Y) < Constants.Offset1e4f)
                localNormal = new Vector3(0, 1, 0);
            else if (MathF.Abs(localIntersectionPoint.Z - this.boxMin.Z) < Constants.Offset1e4f)
                localNormal = new Vector3(0, 0, -1);
            else
                localNormal = new Vector3(0, 0, 1);

            Vector3 intersectionPoint = Vector3.Transform(localIntersectionPoint, this.RotationMartrix) + this.Center;
            Vector3 normal = Vector3.Normalize(Vector3.TransformNormal(localNormal, this.RotationMartrix));

            if (Vector3.Dot(normal, rayDirection) > 0)
                normal = -normal;

            intersectionInfo = new IntersectionInfo(t, intersectionPoint, normal, this.Material, this);

            return true;
        }
    }
}
