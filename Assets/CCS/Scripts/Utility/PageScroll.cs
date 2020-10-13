using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class PageScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect rect;

    //页面：0，1，2  索引从0开始
    //每夜占的比列：0/2=0  1/2=0.5  2/2=1
    public int Num = 1;
    float[] page;
    //滑动速度
    public float smooting = 4;

    //滑动的起始坐标
    float targethorizontal = 0;

    //是否拖拽结束
    bool isDrag = false;

    // Use this for initialization
    void Start()
    {
        //rect = transform.GetComponent<ScrollRect>();
        //page = new float[Num];
        //if (Num > 1)
        //{
        //    for (int i = 0; i < Num; i++)
        //    {
        //        float a =(float)i / (Num - 1);
        //        page[i] = a;
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //如果不判断。当在拖拽的时候要也会执行插值，所以会出现闪烁的效果
        //这里只要在拖动结束的时候。在进行插值
        if (!isDrag)
        {
            rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, targethorizontal, Time.deltaTime * smooting);
        }
    }

    public void GoPage(int num)
    {
        rect = transform.GetComponent<ScrollRect>();
        page = new float[Num];
        if (Num > 1)
        {
            for (int i = 0; i < Num; i++)
            {
                float a = (float)i / (Num - 1);
                page[i] = a;
            }
        }
        targethorizontal = page[num - 1];
        rect.horizontalNormalizedPosition = page[num - 1];
    }

    /// <summary>
    /// 拖动开始
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
    }

    /// <summary>
    /// 拖拽结束
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;

        //    //拖动停止滑动的坐标 
        //    Vector2 f = rect.normalizedPosition;
        //    //水平  开始值是0  结尾值是1  [0,1]
        //    float h = rect.horizontalNormalizedPosition;
        //    //垂直
        //    float v = rect.verticalNormalizedPosition;

        float posX = rect.horizontalNormalizedPosition;
        int index = 0; 
        //假设离第一位最近
        float offset = Mathf.Abs(page[index] - posX);
        for (int i = 1; i < page.Length; i++)
        {
            float temp = Mathf.Abs(page[i] - posX);
            if (temp < offset)
            {
                index = i;
                //保存当前的偏移量
                offset = temp;
            }
        }
        targethorizontal = page[index];
    }
}