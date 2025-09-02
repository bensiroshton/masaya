
using System;
using UnityEngine;
using Siroshton.Masaya.Extension;

namespace Siroshton.Masaya.Math
{
    [Serializable]
    public struct IntervalFloat
    {
        public float a;
        public float b;

        public IntervalFloat(float a, float b)
        {
            this.a = a;
            this.b = b;
        }

        public float min { get => Mathf.Min(a, b); }
        public float max { get => Mathf.Max(a, b); }
        public float length { get => max - min; }
        public float random { get => UnityEngine.Random.Range(a, b); }
        public float center { get => a * 0.5f + b * 0.5f; }

        public float Lerp(float pos)
        {
            return Mathf.Lerp(a, b, pos);
        }

        public static IntervalFloat operator * (IntervalFloat me, float scaler)
        {
            return new IntervalFloat(me.a * scaler, me.b * scaler);
        }

        public override string ToString()
        {
            return $"[{a}...{b}]";
        }
    }

    [Serializable]
    public struct IntervalInt
    {
        public int a;
        public int b;

        public IntervalInt(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public int min { get => Mathf.Min(a, b); }
        public int max { get => Mathf.Max(a, b); }
        public int length { get => max - min; }
        public int random { get => UnityEngine.Random.Range(a, b + 1); }
        public int center { get => (int)((float)a * 0.5f + (float)b * 0.5f); }

        public int Lerp(float pos)
        {
            return (int)Mathf.Lerp(a, b, pos);
        }

        public static IntervalInt operator * (IntervalInt me, int scaler)
        {
            return new IntervalInt(me.a * scaler, me.b * scaler);
        }

        public override string ToString()
        {
            return $"[{a}...{b}]";
        }
    }

    [Serializable]
    public struct IntervalVec3
    {
        public Vector3 a;
        public Vector3 b;

        public IntervalVec3(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        public float minX { get => Mathf.Min(a.x, b.x); }
        public float maxX { get => Mathf.Max(a.x, b.x); }
        public float minY { get => Mathf.Min(a.y, b.y); }
        public float maxY { get => Mathf.Max(a.y, b.y); }
        public float minZ { get => Mathf.Min(a.z, b.z); }
        public float maxZ { get => Mathf.Max(a.z, b.z); }
        public float length { get => (a - b).magnitude; }
        public Vector3 center { get => a * 0.5f + b * 0.5f; }

        public Vector3 random 
        { 
            get
            {
                return new Vector3(
                    UnityEngine.Random.Range(a.x, b.x),
                    UnityEngine.Random.Range(a.y, b.y),
                    UnityEngine.Random.Range(a.z, b.z)
                );
            }
        }

        public Vector3 randomExtents
        {
            get
            {
                return new Vector3(
                    UnityEngine.Random.Range(a.x, b.x) * (MathUtil.GetRandomBool() ? 1 : -1),
                    UnityEngine.Random.Range(a.y, b.y) * (MathUtil.GetRandomBool() ? 1 : -1),
                    UnityEngine.Random.Range(a.z, b.z) * (MathUtil.GetRandomBool() ? 1 : -1)
                );
            }
        }

        public Vector3 randomLerp{ get => Lerp(UnityEngine.Random.Range(0.0f, 1.0f)); }
        public Vector3 randomSlerp { get => Slerp(UnityEngine.Random.Range(0.0f, 1.0f)); }

        public Vector3 Lerp(float pos)
        {
            return Vector3.Lerp(a, b, pos);
        }

        public Vector3 Slerp(float pos)
        {
            return Vector3.Slerp(a, b, pos);
        }

        public static IntervalVec3 operator * (IntervalVec3 me, float scaler)
        {
            return new IntervalVec3(me.a * scaler, me.b * scaler);
        }

        public override string ToString()
        {
            return $"[{a}...{b}]";
        }
    }

}