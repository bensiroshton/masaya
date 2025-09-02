
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Entity
{
    [RequireComponent(typeof(SphereCollider))]
    public class WorldPerceptorCollider : MonoBehaviour
    {
        private UnityEvent<IBullet> _onBulletEnter = new UnityEvent<IBullet>();

        public UnityEvent<IBullet> onBulletEnter { get => _onBulletEnter; set => _onBulletEnter = value; }

        static public WorldPerceptorCollider Create(Transform parent, float radius)
        {
            GameObject o = new GameObject("WorldPerceptorCollider");
            o.transform.SetParent(parent);

            WorldPerceptorCollider me = o.AddComponent<WorldPerceptorCollider>();

            SphereCollider c = me.GetComponent<SphereCollider>();
            c.isTrigger = true;
            c.radius = radius;

            Rigidbody rb = o.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            o.layer = GameLayers.bulletTriggers;
            o.transform.localPosition = Vector3.zero;
            o.transform.localRotation = Quaternion.identity;

            return me;
        }

        private void Awake()
        {
            gameObject.layer = GameLayers.enemies;
        }

        private void OnTriggerEnter(Collider other)
        {
            if( other.gameObject.layer != GameLayers.bullets ) return;

            IBullet bullet = other.gameObject.GetComponent<IBullet>();
            if( bullet != null ) _onBulletEnter.Invoke(bullet);
        }


    }

}