using System;
using System.Numerics;

namespace CsharpRaytracer.Core
{
    public class Camera
    {
        private Vector3 source;

        private Vector3 destination;

        private Vector3 camUp;

        private Vector3 camRight;

        private Vector3 camDir;

        private float height;

        private float width;

        private float fieldOfView;

        private float halfTanFov;

        private float aspectRatio;


        public Camera(Vector3 source, Vector3 destination, Vector3 camUp, float fov, float height, float width)
        {
            this.source = source;
            this.destination = destination;
            this.camUp = camUp;
            this.fieldOfView = fov * MathF.PI / 180.0f;
            this.height = height;
            this.width = width;

            this.camDir = Vector3.Normalize(destination - source);
            this.camRight = Vector3.Normalize(Vector3.Cross(this.camDir, camUp));
            this.camUp = Vector3.Normalize(Vector3.Cross(this.camRight, this.camDir));
            this.halfTanFov = MathF.Tan(this.fieldOfView / 2);
            this.aspectRatio = width / height;
        }

        public void GetRayAtPixel(float x, float y, out Vector3 rayOrigin, out Vector3 rayDirection)
        {
            float xOffset = ((2 * (x + 0.5f) / this.width) - 1) * this.aspectRatio * this.halfTanFov;
            float yOffset = (1 - (2 * (y + 0.5f) / this.height)) * this.halfTanFov;
            rayDirection = Vector3.Normalize(this.camDir + (xOffset * this.camRight) + (yOffset * this.camUp));
            rayOrigin = this.source;
        }
    }
}
