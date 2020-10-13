using UnityEngine;
using UnityEngine.UI;
 
/// <summary>
/// 序列帧动画播放 单次播放
/// </summary>
public class SingleSequenceFrameAnim : MonoBehaviour
{

    public Sprite[] Source;

    public int mFrame = 12; //每秒帧数

    //是否播放
    public bool playingAnim = true;
    
    //当前时间
    private float curPaseTimes = 0f;

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
                mImg.sprite = Source[mIndex];
                playingAnim = false;
            }
        }
    }
    public void GODestroy()
    {
        playingAnim = false;
        GameObject.Destroy(gameObject);
    }
}
