// Lightweight Unity compatibility shims for server build.
// Only implements the minimal types used by server code.
namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct Vector4
    {
        public float x; public float y; public float z; public float w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public struct Rect
    {
        public float x; public float y; public float width; public float height;
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
    }

    public struct Quaternion
    {
        public float x; public float y; public float z; public float w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public struct Ray
    {
        public Vector3 origin; public Vector3 direction;
        public Ray(Vector3 origin, Vector3 direction) { this.origin = origin; this.direction = direction; }
    }

    public struct Plane
    {
        public Vector3 normal; public float distance;
        public Plane(Vector3 normal, float distance) { this.normal = normal; this.distance = distance; }
    }

    public struct Matrix4x4
    {
        // minimal representation: 16 floats
        public float m00,m01,m02,m03,m10,m11,m12,m13,m20,m21,m22,m23,m30,m31,m32,m33;
    }
}
