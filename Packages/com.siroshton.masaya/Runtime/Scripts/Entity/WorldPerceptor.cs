using Siroshton.Masaya.Weapon;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Entity
{
    [RequireComponent(typeof(Collider))]
    public class WorldPerceptor : MonoBehaviour
    {
        [SerializeField] private float _perceptionRange = 3.0f;
        //[SerializeField] private float _updateInterval = 0.25f;

        private Collider _collider;
        private WorldPerceptorCollider _perceptorCollider;
        private Player.Player _player;
        private float _timeSinceUpdate;

        public struct BulletInfo
        {
            public IBullet bullet;
            public Vector3 evadeDirection;
            public bool mightHit;
            public RaycastHit mightHitInfo;
        }

        private UnityEvent<BulletInfo> _onBulletEnteredPerception = new UnityEvent<BulletInfo>();

        public Vector3 playerPosition { get => _player.transform.position; }
        public Vector3 directionToPlayer { get => (playerPosition - transform.position).normalized; }

        public UnityEvent<BulletInfo> onBulletEnteredPerception { get => _onBulletEnteredPerception; set => _onBulletEnteredPerception = value; }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _perceptorCollider = WorldPerceptorCollider.Create(transform, _perceptionRange);
            _perceptorCollider.onBulletEnter.AddListener(OnBulletEnter);

            _player = Player.Player.instance;
            //_timeSinceUpdate = _updateInterval;
        }

        private void OnBulletEnter(IBullet bullet)
        {
            Behaviour bulletObj = bullet as Behaviour;
            Vector3 bulletDir = (bulletObj.transform.position - bullet.startPos).normalized;
            if (bulletDir.sqrMagnitude == 0) return;

            BulletInfo info = new BulletInfo();
            info.bullet = bullet;
            info.evadeDirection = Vector3.Cross(bulletDir, Vector3.up);
            Plane plane = new Plane(info.evadeDirection, bulletObj.transform.position);
            float d1 = plane.GetDistanceToPoint(transform.position + info.evadeDirection);
            float d2 = -plane.GetDistanceToPoint(transform.position - info.evadeDirection);
            if (d2 > d1) info.evadeDirection = -info.evadeDirection;

            RaycastHit hit;
            Ray bulletRay = new Ray(bullet.startPos, bulletDir);
            if( _collider.Raycast(bulletRay, out hit, _perceptionRange) )
            {
                info.mightHit = true;
                info.mightHitInfo = hit;
            }

            _onBulletEnteredPerception.Invoke(info);
        }


    }
}
