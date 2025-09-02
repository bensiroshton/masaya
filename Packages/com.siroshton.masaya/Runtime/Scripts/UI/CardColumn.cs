using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.UI
{
    public class CardColumn : MonoBehaviour, CardManager.ILayoutItemProvider
    {
        [SerializeField] private Transform _center;
        [SerializeField, Range(0, 1)] private float _curve = 0;
        [Tooltip("The vertical spacing between cards on a 1920x1080 canvas.")]
        [SerializeField, Range(0, 100)] private float _spacing = 10;
        [SerializeField, Range(0, 90)] private float _maxRotation = 30;
        [SerializeField, Range(1,5)] private int _maxCards = 1;
        [Tooltip("The size of the card on a 1920x1080 canvas.")]
        [SerializeField] private Vector2 _cardSize = new Vector2(145, 235);

        public CardManager.LayoutItem[] GetLayout()
        {
            CardManager.LayoutItem[] items = new CardManager.LayoutItem[_maxCards];

            Canvas canvas = GetComponentInParent<Canvas>();
            Vector2 canvasSize = new Vector2((canvas.transform as RectTransform).rect.width, (canvas.transform as RectTransform).rect.height);

            RectTransform rt = transform as RectTransform;
            Vector2 scaleXY = new Vector2(canvasSize.x / 1920, canvasSize.y / 1080);
            float scale = Mathf.Min(scaleXY.x, scaleXY.y);

            float cardWidth = _cardSize.x * scale;
            Vector2 cardSize = new Vector2(cardWidth, cardWidth * _cardSize.y / _cardSize.x);
            float spacing = _spacing * scale;

            float step = cardSize.y + spacing;
            Vector2 pos = Vector2.zero;
            float height = cardSize.y * _maxCards + spacing * (_maxCards - 1);
            pos.y = height / 2 - cardSize.y / 2;

            float radius = (rt.localPosition - _center.localPosition).magnitude;
            Vector2 localPosition = new Vector2(rt.localPosition.x, rt.localPosition.y);

            for (int i = 0; i < _maxCards; i++)
            {
                Vector2 posLocal = localPosition + pos * scale;
                Vector2 circlePos = MathUtil.NearestPointOnCircle(_center.localPosition, radius, posLocal);
                posLocal = Vector2.Lerp(posLocal, circlePos, _curve);

                float rotation = Vector2.SignedAngle(new Vector2(-localPosition.x, 0), new Vector2(_center.localPosition.x, _center.localPosition.y) - posLocal);
                float dy = (pos.y - _center.localPosition.y) / (canvasSize.y / 2);
                if (dy < 0) dy *= -1;
                rotation = Mathf.Clamp(rotation, -_maxRotation * dy, _maxRotation * dy);

                posLocal -= localPosition;
                posLocal = rt.localToWorldMatrix * posLocal;
                posLocal += new Vector2(rt.position.x, rt.position.y);

                items[i].position = posLocal;
                items[i].cardSize = cardSize * rt.lossyScale;
                items[i].rotation = rt.rotation.eulerAngles.z + rotation;
                items[i].scale = scale;

                pos.y -= step;
            }

            return items;
        }


        private void OnDrawGizmos()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Vector2 canvasSize = new Vector2((canvas.transform as RectTransform).rect.width, (canvas.transform as RectTransform).rect.height);

            RectTransform rt = transform as RectTransform;
            Gizmos.color = Color.cyan;
            Vector2 scaleXY = new Vector2(canvasSize.x / 1920, canvasSize.y / 1080);
            float scale = Mathf.Min(scaleXY.x, scaleXY.y);

            float cardWidth = _cardSize.x * scale;
            Vector2 cardSize = new Vector2(cardWidth, cardWidth * _cardSize.y / _cardSize.x);
            float spacing = _spacing * scale;

            float step = cardSize.y + spacing;
            Vector2 pos = Vector2.zero;
            float height = cardSize.y * _maxCards + spacing * (_maxCards - 1);
            pos.y = height / 2 - cardSize.y / 2;

            float radius = (rt.localPosition - _center.localPosition).magnitude;
            Vector2 localPosition = new Vector2(rt.localPosition.x, rt.localPosition.y);

            for (int i=0;i<_maxCards;i++)
            {
                Vector2 posLocal = localPosition + pos * scale;
                Vector2 circlePos = MathUtil.NearestPointOnCircle(_center.localPosition, radius, posLocal);
                posLocal = Vector2.Lerp(posLocal, circlePos, _curve);

                float rotation = Vector2.SignedAngle(new Vector2(-localPosition.x, 0), new Vector2(_center.localPosition.x, _center.localPosition.y) - posLocal);
                float dy = (pos.y - _center.localPosition.y) / (canvasSize.y / 2);
                if (dy < 0) dy *= -1;
                rotation = Mathf.Clamp(rotation, -_maxRotation * dy, _maxRotation * dy);

                posLocal -= localPosition;

                Gizmos.matrix = rt.localToWorldMatrix * Matrix4x4.Translate(posLocal) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation));
                Gizmos.DrawWireCube(rt.rect.center, cardSize);
                pos.y -= step;
            }
        }

    }

}