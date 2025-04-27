using System.Numerics;

namespace CsharpRaytracer
{
    public record Material(
        Vector3 DiffuseCoefficient,
        Vector3 SpecularCoefficient,
        Vector3 AmbientCoefficient,
        float SpecularExponent,
        float Reflectivity,
        float Transparency,
        float RefractiveIndex
    );
}
