using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Math
{

    public static class MathUtil
    {
        public static Vector2 NearestPointOnCircle(Vector2 center, float radius, Vector2 point)
        {
            // center + direction * radius
            return center + (point - center).normalized * radius;
        }

        public static Vector3 NearestPointOnSphere(Vector3 center, float radius, Vector3 point)
        {
            // center + direction * radius
            return center + (point - center).normalized * radius;
        }

        public static Vector3 GetRandomPointInSphere(Vector3 center, float radius)
        {
            return center + Random.insideUnitSphere * radius;
        }

        public static Vector3 GetRandomPointOnSphere(Vector3 center, float radius)
        {
            return center + Random.onUnitSphere * radius;
        }

        public static Vector3 GetRandomXZDirection(float y = 0)
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            return new Vector3(Mathf.Sin(angle), y, Mathf.Cos(angle));
        }

        public static Vector3 GetRandomPointOnDisc(Vector3 center, Vector3 normal, float radius)
        {
            Vector3 point = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            return center + rotation * point;
        }

        public static Vector3 GetRandomExtent(Vector3 extents)
        {
            return GetRandomExtent(extents.x, extents.y, extents.z);
        }

        public static Vector3 GetRandomExtent(float xExtent, float yExtent, float zExtent)
        {
            return new Vector3(Random.Range(-xExtent, xExtent), Random.Range(-yExtent, yExtent), Random.Range(-zExtent, zExtent));
        }

        public static bool GetRandomBool()
        {
            return Random.value >= 0.5f;
        }

        public static float MapAngle180(float angle)
        {
            return ((angle + 180) % 360) - 180;
        }

        public static Vector2 GetPointAlongSineWave(float normalizedPos, float amplitude)
        {
            float x = 2.0f * Mathf.PI * normalizedPos;
            float y = Mathf.Sin(x) * amplitude;
            return new Vector2(x, y);
        }

        public static Vector2 GetDirectionAlongSineWave(float normalizedPos, float amplitude)
        {
            Vector2 p1 = GetPointAlongSineWave(normalizedPos, amplitude);
            Vector2 p2 = GetPointAlongSineWave(normalizedPos + /* 1 degree */ Mathf.Deg2Rad, amplitude);
            return p2 - p1;
        }

        public static Vector3 GetPointTowards(Vector3 current, Vector3 target, float maxDistance)
        {
            Vector3 dir = target - current;
            if (dir.sqrMagnitude > maxDistance * maxDistance)
            {
                dir = dir.normalized * maxDistance;
            }

            return dir;
        }

        public static SimpleTransform GetPointAndRotationTowards(Vector3 current, Vector3 target, float maxDistance)
        {
            Vector3 dir = GetPointTowards(current, target, maxDistance);
            return new SimpleTransform(current + dir, Quaternion.LookRotation(dir.normalized, Vector3.up));
        }

        /// <summary>
        /// Takes a linear value between [0, 1] and returns an Eased In and Out value between [0, 1].
        /// </summary>
        public static float SmoothNormalizedTime(float normalizedTime)
        {
            return Mathf.Cos(Mathf.Lerp(-Mathf.PI, 0, normalizedTime)) / 2.0f + 0.5f;
        }

        /// <summary>
        /// Determine where value is between min and max and return the normalized position.  value will be clamped to be within [a, b].
        /// </summary>
        /// <param name="a">first value</param>
        /// <param name="b">second value</param>
        /// <param name="value">Value between [a, b]</param>
        /// <returns>normalized position.</returns>
        public static float Normalize(float a, float b, float value)
        {
            float min = Mathf.Min(a, b);
            float max = Mathf.Max(a, b);

            if( value < min ) value = min;
            else if( value > max ) value = max;

            float range = max - min;
            if( a < b ) return (value - min) / range;
            else return 1.0f - (value - min) / range;
        }

        /// <summary>
        /// Calculates the normalized position [0..1] testPoint is in relation to fromPoint and within the band set by inner/outer radius.  Returns
        /// true if the testPoint is within the band otherwise false.  normalizedValue will always be between [0..1].
        /// </summary>
        /// <param name="fromPoint">The point we want test against.</param>
        /// <param name="innerRadius">The inner radius in relation to fromPoint.</param>
        /// <param name="outerRadius">The outer radius in relation to fromPoint.</param>
        /// <param name="testPoint">The point we are testing.</param>
        /// <param name="normalizedValue">An inclusive value between 0 and 1.</param>
        /// <returns>true if the test point is within the band set by inner/outer radius, otherwise false.</returns>
        public static bool GetNormalizedPosition(Vector3 fromPoint, float innerRadius, float outerRadius, Vector3 testPoint, out float normalizedValue)
        {
            float playerDistanceSqr = (testPoint - fromPoint).sqrMagnitude;
            float outerRadiusSqr = outerRadius * outerRadius;
            if (playerDistanceSqr <= outerRadiusSqr)
            {
                float innerRadiusSqr = innerRadius * innerRadius;
                if ( playerDistanceSqr >= innerRadius )
                {
                    normalizedValue = MathUtil.Normalize(innerRadiusSqr, outerRadiusSqr, playerDistanceSqr);
                    return true;
                }
                else
                {
                    normalizedValue = 0;
                    return false;
                }
            }
            else
            {
                normalizedValue = 1;
                return false;
            }
        }

        /// <summary>
        /// Returns a value between 0 and 1 without returning if the testPoint is within the radius band or not.  <see cref="GetNormalizedPosition"/>
        /// </summary>
        public static float GetNormalizedPosition(Vector3 fromPoint, float innerRadius, float outerRadius, Vector3 testPoint)
        {
            float value;
            GetNormalizedPosition(fromPoint, innerRadius, outerRadius, testPoint, out value);
            return value;
        }

        public static float GetDecimal(float value)
        {
            return value - Mathf.Floor(value);
        }

        public static float GetRadius(Bounds bounds)
        {
            return Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z);
        }

        public static Bounds GetBounds(GameObject o)
        {
            bool hasBounds;
            return GetBounds(o, out hasBounds);
        }

        public static Bounds GetChildBounds(Transform transform)
        {
            bool hasBounds;
            return GetChildBounds(transform, out hasBounds);
        }

        public static Bounds GetChildBounds(Transform transform, out bool hasBounds)
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            
            for (int i = 0; i < transform.childCount; i++)
            {
                bool hasChildBounds = false;
                Bounds childBounds = GetBounds(transform.GetChild(i).gameObject, out hasChildBounds);

                if( hasChildBounds ) bounds.Encapsulate(childBounds);
            }

            hasBounds = bounds.extents.magnitude > 0;

            return bounds;
        }

        public static Bounds GetBounds(GameObject o, out bool hasBounds)
        {
            Bounds bounds = new Bounds();
            hasBounds = false;

            if (o.GetComponent<Collider>() is Collider c && c != null)
            {
                bool enabled = c.enabled;
                c.enabled = true;
                bounds = c.bounds;
                c.enabled = enabled;
                hasBounds = true;
                Debug.Log($"{o.name}.Collider: {bounds}");
            }

            if (o.GetComponent<MeshRenderer>() is MeshRenderer r && r != null)
            {
                if ( !hasBounds ) bounds = r.bounds;
                else bounds.Encapsulate(r.bounds);
                hasBounds = true;

                Debug.Log($"{o.name}.MeshRenderer: {bounds}");
            }
            
            if (o.GetComponent<UnityEngine.Light>() is UnityEngine.Light l && l != null && l.type == UnityEngine.LightType.Point )
            {
                float size = l.range * 2;
                Bounds lightBounds = new Bounds(o.transform.position, new Vector3(size, size, size));
                if ( !hasBounds ) bounds = lightBounds;
                else bounds.Encapsulate(lightBounds);
                hasBounds = true;

                Debug.Log($"{o.name}.Light: {bounds}");
            }

            if (o.GetComponent<AudioSource>() is AudioSource a && a != null )
            {
                float size = a.spatialBlend * a.maxDistance * 2;
                Bounds audioBounds = new Bounds(o.transform.position, new Vector3(size, size, size));
                if (!hasBounds) bounds = audioBounds;
                else bounds.Encapsulate(audioBounds);
                hasBounds = true;

                Debug.Log($"{o.name}.AudioSource: {bounds}");
            }

            for (int i = 0; i < o.transform.childCount; i++)
            {
                bool hasChildBounds = false;
                Bounds childBounds = GetBounds(o.transform.GetChild(i).gameObject, out hasChildBounds);

                if (hasBounds && hasChildBounds)
                {
                    bounds.Encapsulate(childBounds);
                }
                else if (!hasBounds && hasChildBounds)
                {
                    bounds = childBounds;
                    hasBounds = true;
                }
            }

            return bounds;
        }



    }

}