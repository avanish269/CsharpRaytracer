using System;
using System.Numerics;

namespace CsharpRaytracer
{
    public class Scene
    {
        private Vector3 ambientColor;

        private static readonly Random random = new Random();

        public void CreateScene()
        {
            this.ambientColor = new Vector3(0.24725f, 0.19950f, 0.07450f);
        }

        public bool CheckIntersection(Vector3 rayOrigin, Vector3 rayDirection)
        {
            return random.Next(100) == 0;
        }

        public Vector3 RayCast(Vector3 rayOrigin, Vector3 rayDirection, int depth)
        {
            if (depth > Constants.MaxDepth)
                return Constants.Background;

            if (this.CheckIntersection(Vector3.One, Vector3.One))
            {
                return this.ambientColor;
            }

            return Constants.Background;
        }
    }
}
