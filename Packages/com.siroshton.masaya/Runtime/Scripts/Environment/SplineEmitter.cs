using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Siroshton.Masaya.Environment
{

    public class SplineEmitter : Emitter
    {

        [SerializeField] private SplineContainer _spline;

        protected override void OnObjectEmitted(GameObject o)
        {
            SplineAnimate sa = o.GetComponent<SplineAnimate>();
            if( sa != null )
            {
                float3 p = _spline.EvaluatePosition(0);
                o.transform.position = _spline.transform.TransformPoint(new Vector3(p.x, p.y, p.z));
                sa.Container = _spline;
                sa.Play();
            }
        }
    }

}