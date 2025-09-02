using UnityEngine;

namespace Siroshton.Masaya.Core
{

    public static class GameLayers
    {
        public static int area = LayerMask.NameToLayer("Area");
        public static int areaMask = 1 << area;

        public static int bullets = LayerMask.NameToLayer("Bullets");
        public static int bulletsMask = 1 << bullets;

        public static int bulletTriggers = LayerMask.NameToLayer("BulletTriggers");
        public static int bulletTriggersMask = 1 << bulletTriggers;

        public static int cameraFocus = LayerMask.NameToLayer("CameraFocus");
        public static int cameraFocusMask = 1 << cameraFocus;

        public static int enemies = LayerMask.NameToLayer("Enemies");
        public static int enemiesMask = 1 << enemies;

        public static int noFireZone = LayerMask.NameToLayer("NoFireZone");
        public static int noFireZoneMask = 1 << noFireZone;

        public static int player = LayerMask.NameToLayer("Player");
        public static int playerMask = 1 << player;

        public static int triggers = LayerMask.NameToLayer("Triggers");
        public static int triggersMask = 1 << triggers;

        public static int water = LayerMask.NameToLayer("Water");
        public static int waterMask = 1 << water;
    }

}