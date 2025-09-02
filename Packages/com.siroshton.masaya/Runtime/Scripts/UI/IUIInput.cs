using UnityEngine;

namespace Siroshton.Masaya.UI
{
    public interface IUIInput
    {
        public void OnMovePushed(Vector2 direction);
        public void OnMoveUpPushed();
        public void OnMoveDownPushed();
        public void OnMoveLeftPushed();
        public void OnMoveRightPushed();
        public void OnButton1Pushed();
        public void OnButton2Pushed();
        public void OnButton3Pushed();
        public void OnButton4Pushed();
    }

}