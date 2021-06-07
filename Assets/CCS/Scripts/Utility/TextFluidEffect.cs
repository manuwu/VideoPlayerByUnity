using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public static class TextFontExternal
{
    public static int CalculateLengthOfText(this Text mTextComponent, string name)
    {
        int totalLength = 0;
        Font myFont = mTextComponent.font;  //chatText is my Text component
        myFont.RequestCharactersInTexture(name, mTextComponent.fontSize, mTextComponent.fontStyle);
        CharacterInfo characterInfo = new CharacterInfo();
        char[] arr = name.ToCharArray();

        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out characterInfo, mTextComponent.fontSize);
            totalLength += characterInfo.advance;
        }

        return totalLength;
    }
}

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(ContentSizeFitter))]
public class TextFluidEffect : MonoBehaviour
{
   
    private string mText;
    public string text
    {
        get
        {
            return mText;
        }
        set
        {
            mText = value;
            InitWadget();
            mTextComponent.text = mText;
        }
    }
    private bool inited = false;
    private Text mTextComponent;
    private int truelysize;
    private float widgetsize;
    private float Speed = 40;
    public bool _CanRoll = false;
    public bool CheckY = false;
    private bool autoRoll = false;//不论文本长短均自动滚动
    private float RollDelay;//滚动延迟

    void Start()
    {
        InitWadget();
    }
    void Update()
    {
        if(_CanRoll)
        {
            if ((RollDelay -= Time.deltaTime) > 0f) return;
            if(AutoRoll)
            {
                Vector3 _pos = mTextComponent.rectTransform.localPosition;
                float x = _pos.x;
                x -= Time.deltaTime * Speed;
                _pos.x = x;
                mTextComponent.rectTransform.localPosition = _pos;
                if (_pos.x <= -(mTextComponent.rectTransform.sizeDelta.x))
                {
                    Vector3 pos = Vector3.zero;
                    if (CheckY)
                    {
                        pos = new Vector3(0f, _pos.y, 0f);
                    }
                    pos.x = widgetsize;

                    mTextComponent.rectTransform.localPosition = pos;
                }
            }
            else
            {
                if (widgetsize < mTextComponent.rectTransform.sizeDelta.x)
                {
                    Vector3 _pos = mTextComponent.rectTransform.localPosition;
                    float x = _pos.x;
                    x -= Time.deltaTime * Speed;
                    _pos.x = x;
                    mTextComponent.rectTransform.localPosition = _pos;
                    if (_pos.x <= -(mTextComponent.rectTransform.sizeDelta.x))
                    {
                        Vector3 pos = new Vector3(0f, _pos.y, 0f);
                        pos.x = widgetsize;

                        mTextComponent.rectTransform.localPosition = pos;
                    }
                }
            }
        }
    }

    public bool CanRoll
    {
        set
        {
            _CanRoll = value;
            if(!value)
            {
                InitWadget();
                Vector3 pos = Vector3.zero;
                if (CheckY)
                {
                    pos = new Vector3(0f, mTextComponent.rectTransform.localPosition.y, 0f);
                }
                mTextComponent.rectTransform.localPosition = pos;
            }
            else
            {
                RollDelay = 0.5f;
            }
        }
    }

    /// <summary>
    /// 不论文本长短是否自动滚动
    /// </summary>
    public bool AutoRoll
    {
        get
        {
            return autoRoll;
        }

        set
        {
            autoRoll = value;
        }
    }

    public void UpdateText(string  tt)
    {
        text = tt;
        widgetsize = mTextComponent.rectTransform.parent.GetComponent<RectTransform>().sizeDelta.x;
        Vector3 pos = Vector3.zero;
        if (CheckY)
        {
            pos = new Vector3(0f, mTextComponent.rectTransform.localPosition.y, 0f);
        }
        mTextComponent.rectTransform.localPosition = pos;
    }

    private void InitWadget()
    {
        if (inited)
            return;

        mTextComponent = GetComponent<Text>();
        var mask = mTextComponent.transform.parent.GetComponent<RectMask2D>();
        if (mask == null)
            mask = mTextComponent.transform.parent.gameObject.AddComponent<RectMask2D>();

        widgetsize = mTextComponent.rectTransform.parent.GetComponent<RectTransform>().sizeDelta.x;
        
        inited = true;
    }
#if UNITY_EDITOR
    public string m_Text;
    private void OnValidate()
    {
        mText = m_Text;
        InitWadget();
        mTextComponent.text = mText;
        //LogTool.Log(mTextComponent.rectTransform.sizeDelta.x);
    }
#endif

}
