using System.Numerics;

namespace CsharpRaytracer
{
    public abstract class SceneObject
    {
        public Material Material { get; set; }

        public readonly float Thickness;

        protected SceneObject(Material material, float thickness)
        {
            this.Material = material;
            this.Thickness = thickness;
        }

        public abstract bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection, out IntersectionInfo intersectionInfo);
    }
}
