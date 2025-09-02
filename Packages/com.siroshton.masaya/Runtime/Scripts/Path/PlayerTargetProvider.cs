using UnityEngine;

namespace Siroshton.Masaya.Path
{
    public class PlayerTargetProvider : TargetProvider
    {
        public enum Direction
        {
            TowardPlayer,
            AwayFromPlayer
        }

        [SerializeField] private Direction _direction = Direction.TowardPlayer;
        [SerializeField, Range(0, 10)] private float _awayFromPlayerStepSize = 1.0f;

        public override bool GetTarget(Transform current, out Vector3 target, float timeSinceLastCall)
        {
            target = Player.Player.instance.transform.position;

            if( _direction == Direction.AwayFromPlayer )
            {
                Vector3 dir = current.position - target;
                target = current.position + dir.normalized * _awayFromPlayerStepSize;
            }

            return true;
        }

        public override string ToString()
        {
            return $"PlayerTargetProvider({_direction})";
        }

    }
}
