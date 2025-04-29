using CsharpRaytracer.Geometry;
using System.Numerics;

namespace CsharpRaytracer.Core
{
    public class IntersectionInfo
    {
        public float TForIntersection { get; set; }

        public Vector3 IntersectionPoint { get; set; }

        public Vector3 NormalAtIntersection { get; set; }

        public Material Material { get; set; }

        public SceneObject SceneObject { get; set; }

        public IntersectionInfo()
        {
            this.TForIntersection = float.PositiveInfinity;
            this.IntersectionPoint = default;
            this.NormalAtIntersection = default;
            this.Material = null;
            this.SceneObject = null;
        }

        public IntersectionInfo(float tForIntersection, Vector3 intersectionPoint, Vector3 normalAtIntersection, Material material, SceneObject sceneObject)
        {
            this.TForIntersection = tForIntersection;
            this.IntersectionPoint = intersectionPoint;
            this.NormalAtIntersection = normalAtIntersection;
            this.Material = material;
            this.SceneObject = sceneObject;
        }

        public void Copy(IntersectionInfo other)
        {
            this.TForIntersection = other.TForIntersection;
            this.IntersectionPoint = other.IntersectionPoint;
            this.NormalAtIntersection = other.NormalAtIntersection;
            this.Material = other.Material;
            this.SceneObject = other.SceneObject;
        }
    }
}
