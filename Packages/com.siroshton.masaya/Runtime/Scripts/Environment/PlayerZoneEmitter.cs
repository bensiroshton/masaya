using Siroshton.Masaya.Core;
using Siroshton.Masaya.Entity;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Motion;
using Siroshton.Masaya.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{
    public class PlayerZoneEmitter : CircleZoneEmitter
    {
        protected new void Start()
        {
            base.Start();
            zone = Player.Player.instance.circleZone;
        }
    }
}
