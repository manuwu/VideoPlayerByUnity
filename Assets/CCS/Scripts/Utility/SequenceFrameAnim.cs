using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 序列帧动画播放
/// </summary>
public class SequenceFrameAnim : MonoBehaviour
{

    public Sprite[] Source;

    public int mFrame = 12; //每秒帧数

    //是否播放
    public bool playingAnim = true;
    //是否一轮动画播放完成后停止播放一下（诸如眨眼睛，一开始要停一下）
    public bool needpause = false;

    //下一轮开始停止时间
    public float paseTimes = 3.0f;
    //当前时间
    public float curPaseTimes = 0f;

    private Image mImg;
    private float mCurTime = 0;
    private float mSpace;
    private int mIndex = 0;

    public void Awake()
    {
        mImg = GetComponent<Image>();
    }
    /// <summary>
    /// 序列帧播放和停止控制
    /// </summary>
    /// <param name="bol"></param>
    public void PlayOrStop(bool bol)
    {
        playingAnim = bol;
    }

    public void FixedUpdate()
    {
        if (playingAnim == false)
        {
            return;
        }
        if (Source == null || Source.Length == 0)
            return;
        if (needpause == true)
        {
            curPaseTimes += Time.deltaTime;
            if (curPaseTimes < paseTimes)
            {
                return;
            }
        }
        mSpace = 1.0f / mFrame;
        mCurTime -= Time.deltaTime;
        if (mCurTime <= 0)
        {
            mImg.sprite = Source[mIndex];
            mCurTime = mSpace;
            mIndex += 1;
            if (mIndex == Source.Length)
            {
                mIndex = 0;
                if (needpause == true)
                {
                    curPaseTimes = 0f;
                }
            }
        }
    }
    public void GODestroy()
    {
        playingAnim = false;
        GameObject.Destroy(gameObject);
    }
}
