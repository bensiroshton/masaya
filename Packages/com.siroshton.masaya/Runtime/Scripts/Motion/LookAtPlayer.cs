
using UnityEngine;

namespace Siroshton.Masaya.Motion
{

    public class LookAtPlayer : LookAtTarget
    {
        public enum BodyArea
        {
            Head,
            Torso,
            Feet
        }

        [SerializeField] private BodyArea _bodyArea = BodyArea.Torso;

        protected new void Start()
        {
            base.Start();

            if( _bodyArea == BodyArea.Head ) target = Player.Player.instance.head;
            else if( _bodyArea == BodyArea.Torso ) target = Player.Player.instance.torso;
            else target = Player.Player.instance.transform;
        }
    }

}