using UnityEngine;
using System.Collections;

public class TweenerAnimation : MonoBehaviour
{
    [Header("位移、旋转、缩放设置开关及曲线")]
    public bool[] bEnable = new bool[3];
    public AnimationCurve[] mCurve = new AnimationCurve[3];
    [Header("对应轴控制开关")]
    public bool[] mXyz = new bool[3];
    [Header("位移系数")]
    public Vector3 moveFactor = Vector3.one;
    public bool[] rXyz = new bool[3];
    public bool[] sXyz = new bool[3];
    [Header("缩放系数")]
    public Vector3 scaleFactor = Vector3.one;
    private float[] f = new float[3];
    private Vector3 position,rotation,scale;
    private void Start()
    {
        position = transform.position;
        rotation = transform.rotation.eulerAngles;
        scale = transform.localScale;
    }
    void Update()
    {
        if (bEnable[0])
        {
            f[0] = mCurve[0].Evaluate(Time.time);
            transform.position = new Vector3(mXyz[0] ? f[0] * moveFactor.x : position.x, mXyz[1] ? f[0] * moveFactor.y : position.y, mXyz[2] ? f[0] * moveFactor.z : position.z);
        }

        if (bEnable[1])
        {
            f[1] = mCurve[1].Evaluate(Time.time) * 360;
            transform.eulerAngles = new Vector3(rXyz[0] ? f[1] : rotation.x, rXyz[1] ? f[1] : rotation.y, rXyz[2] ? f[1] : rotation.z);
        }

        if (bEnable[2])
        {
            f[2] = mCurve[2].Evaluate(Time.time);
            transform.localScale = new Vector3(sXyz[0] ? f[2] * scaleFactor.x : scale.x, sXyz[1] ? f[2] * scaleFactor.y : scale.y, sXyz[2] ? f[2] * scaleFactor.z : scale.z);
        }
    }
}
