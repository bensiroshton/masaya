

using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Util
{
    static public class PhysicsUtil
    {

        /// <summary>
        /// Get all points on the XZ plane around a point with a given spacing at a set radius that optionally is in contact with a collider.
        /// </summary>
        /// <param name="surface">The collider to get points on, if null then all points are returned.</param>
        /// <param name="aroundPosition">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="spacing">Spacing to check at along the circles path.</param>
        /// <returns></returns>
        static public List<Vector3> GetCircleContactPointsXZ(Collider surface, Vector3 aroundPosition, float radius, float spacing, float rotationOffset = 0)
        {
            float circumference = 2 * Mathf.PI * radius;
            int numPoints = (int)(circumference / (spacing * 2));
            float step = 2 * Mathf.PI / (float)numPoints;
            List<Vector3> contacts = new List<Vector3>();
            RaycastHit hit;

            rotationOffset = Mathf.Deg2Rad * rotationOffset;

            for (int i = 0; i < numPoints; i++)
            {
                Vector3 point = new Vector3(
                    aroundPosition.x + Mathf.Sin(rotationOffset + step * (float)i) * radius,
                    aroundPosition.y,
                    aroundPosition.z + Mathf.Cos(rotationOffset + step * (float)i) * radius);

                if( surface != null )
                {
                    Ray ray = new Ray(point + new Vector3(0, 10, 0), Vector3.down);

                    if (surface.Raycast(ray, out hit, 20))
                    {
                        contacts.Add(hit.point);
                    }
                }
                else
                {
                    contacts.Add(point);
                }
            }

            return contacts;
        }
    }

}