using UnityEngine;

[System.Serializable]

public class Bezier : System.Object
{
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public float ti = 0f;

    private Vector3 b0 = Vector3.zero;
    private Vector3 b1 = Vector3.zero;
    private Vector3 b2 = Vector3.zero;
    private Vector3 b3 = Vector3.zero;

    private float Ax;
    private float Ay;
    private float Az;
    private float Bx;
    private float By;
    private float Bz;
    private float Cx;
    private float Cy;
    private float Cz;

    public Bezier()
    {
    }
    // Init function v0 = 1st point, v1 = handle of the 1st point , v2 = handle of the 2nd point, v3 = 2nd point
    // handle1 = v0 + v1
    // handle2 = v3 + v2
    public Bezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.p0 = v0;
        this.p1 = v1;
        this.p2 = v2;
        this.p3 = v3;
    }

    // 0.0 >= t <= 1.0
    public Vector3 GetPointAtTime(float t)
    {
        this.CheckConstant();
        float t2 = t * t;
        float t3 = t * t * t;
        float x = this.Ax * t3 + this.Bx * t2 + this.Cx * t + p0.x;
        float y = this.Ay * t3 + this.By * t2 + this.Cy * t + p0.y;
        float z = this.Az * t3 + this.Bz * t2 + this.Cz * t + p0.z;
        return new Vector3(x, y, z);
    }
    public Vector3 GetPointAtTime2(float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t) * (1 - t) * (1 - t);
        float t2 = (1 - t) * (1 - t) * t;
        float t3 = t * t * (1 - t);
        float t4 = t * t * t;
        B = p0 * t1 + 3 * t2 * p1 + 3 * t3 * p2 + p3 * t4;
        return B;
    }


    private void SetConstant()
    {
        this.Cx = 3f * ((this.p0.x + this.p1.x) - this.p0.x);
        this.Bx = 3f * ((this.p3.x + this.p2.x) - (this.p0.x + this.p1.x)) - this.Cx;
        this.Ax = this.p3.x - this.p0.x - this.Cx - this.Bx;
        this.Cy = 3f * ((this.p0.y + this.p1.y) - this.p0.y);
        this.By = 3f * ((this.p3.y + this.p2.y) - (this.p0.y + this.p1.y)) - this.Cy;
        this.Ay = this.p3.y - this.p0.y - this.Cy - this.By;
        this.Cz = 3f * ((this.p0.z + this.p1.z) - this.p0.z);
        this.Bz = 3f * ((this.p3.z + this.p2.z) - (this.p0.z + this.p1.z)) - this.Cz;
        this.Az = this.p3.z - this.p0.z - this.Cz - this.Bz;

    }

    // Check if p0, p1, p2 or p3 have changed
    private void CheckConstant()
    {
        if (this.p0 != this.b0 || this.p1 != this.b1 || this.p2 != this.b2 || this.p3 != this.b3)
        {
            this.SetConstant();
            this.b0 = this.p0;
            this.b1 = this.p1;
            this.b2 = this.p2;
            this.b3 = this.p3;
        }
    }

    /// <summary>  
    /// 线性贝赛尔曲线  
    /// </summary>  
    /// <param name="P0"></param>  
    /// <param name="P1"></param>  
    /// <param name="t"> 0.0 >= t <= 1.0 </param>  
    /// <returns></returns>  
    public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t);
        B = t1 * P0 + P1 * t;
        //B.y = t1*P0.y + P1.y*t;  
        //B.z = t1*P0.z + P1.z*t;  
        return B;
    }

    /// <summary>  
    ///   
    /// </summary>  
    /// <param name="P0"></param>  
    /// <param name="P1"></param>  
    /// <param name="P2"></param>  
    /// <param name="t">0.0 >= t <= 1.0 </param>  
    /// <returns></returns>  
    public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t) * (1 - t);
        float t2 = t * (1 - t);
        float t3 = t * t;
        B = P0 * t1 + 2 * t2 * P1 + t3 * P2;
        //B.y = P0.y*t1 + 2*t2*P1.y + t3*P2.y;  
        //B.z = P0.z*t1 + 2*t2*P1.z + t3*P2.z;  
        return B;
    }

    /// <summary>  
    ///   
    /// </summary>  
    /// <param name="P0"></param>  
    /// <param name="P1"></param>  
    /// <param name="P2"></param>  
    /// <param name="P3"></param>  
    /// <param name="t">0.0 >= t <= 1.0 </param>  
    /// <returns></returns>  
    public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t) * (1 - t) * (1 - t);
        float t2 = (1 - t) * (1 - t) * t;
        float t3 = t * t * (1 - t);
        float t4 = t * t * t;
        B = P0 * t1 + 3 * t2 * P1 + 3 * t3 * P2 + P3 * t4;
        //B.y = P0.y*t1 + 3*t2*P1.y + 3*t3*P2.y + P3.y*t4;  
        //B.z = P0.z*t1 + 3*t2*P1.z + 3*t3*P2.z + P3.z*t4;  
        return B;
    }
}