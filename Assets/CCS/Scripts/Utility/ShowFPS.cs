using UnityEngine;
using System.Collections;

public class ShowFPS : MonoBehaviour
{
    public float f_UpdateInterval = 0.5F;
    private float f_LastInterval;
    private int i_Frames = 0;
    public float f_Fps;
    public bool show = false;
    public string f_Ver;

    void OnGUI()
    {
#if UNITY_EDITOR || _jjyAndroidtest|| _jjyIOStest
        if (this.show)
        {
            GUI.Label(new Rect(120, 0, 100, 30), "Fps=");
            GUI.Label(new Rect(160, 0, 100, 30), f_Fps.ToString("f2"));
            GUI.Label(new Rect(200, 0, 100, 30), "QLv=");
            GUI.Label(new Rect(240, 0, 100, 30), QualitySettings.GetQualityLevel().ToString());
            GUI.Label(new Rect(280, 0, 100, 30), "Ver=");
            GUI.Label(new Rect(320, 0, 100, 30), f_Ver);
        }
#endif
    }

    void Start()
    {
        f_LastInterval = Time.realtimeSinceStartup;

        i_Frames = 0;
    }

    void Update()
    {
        ++i_Frames;

        if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval)
        {
            f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);

            i_Frames = 0;

            f_LastInterval = Time.realtimeSinceStartup;
        }
    }
}
