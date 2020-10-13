//using MogoEngine.Utils;
using CCS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 数据列表渲染组件，Item缓存，支持无限循环列表，即用少量的Item实现大量的列表项显示
/// </summary>
public class DataGrid : MonoBehaviour
{
    public int maxLength = 100;
    public bool useLoopItems = true;           //是否使用无限循环列表，对于列表项中OnDataSet方法执行消耗较大时不宜使用，因为OnDataSet方法会在滚动的时候频繁调用
    public GameObject ItemCell;
    public bool useClickEvent = false;           //列表项是否监听点击事件
    public bool autoSelectFirst = false;         //创建时是否自动选中第一个对象
    private Action<int> onItemSelectedFunc = null;
    public void AddOnItemSelectedFunc(Action<int> callBack)
    {
        onItemSelectedFunc = callBack;
    }

    private RectTransform m_content;
    private ToggleGroup m_toggleGroup;
    private GameObject m_goItemRender;
    private Type m_itemRenderType;
    private readonly List<ItemRender> m_items = new List<ItemRender>();
    private int m_selectedData;
    private LayoutGroup m_LayoutGroup;
    private RectOffset m_oldPadding;

    //下面的属性会需要父对象上有ScrollRect组件
    private ScrollRect m_scrollRect;    //父对象上的，不一定存在
    private RectTransform m_tranScrollRect;
    private int m_itemSpace;          //每个Item的空间
    private int m_viewItemCount;        //可视区域内Item的数量（向上取整）
    private bool m_isVertical;          //是否是垂直滚动方式，否则是水平滚动
    private int m_startIndex;           //数据数组渲染的起始下标
    private string m_itemClickSound = "";//AudioConst.btnClick;

    public float verticalPos
    {
        get { return m_scrollRect.verticalNormalizedPosition; }
        set { m_scrollRect.verticalNormalizedPosition = value; }
    }

    public float horizonPos
    {
        get { return m_scrollRect.horizontalNormalizedPosition; }
        set { m_scrollRect.horizontalNormalizedPosition = value; }
    }

    //内容长度
    private float ContentSpace
    {
        get
        {
            return m_isVertical ? m_content.sizeDelta.y : m_content.sizeDelta.x;
        }
    }
    //可见区域长度
    private float ViewSpace
    {
        get
        {
            //return m_isVertical ? m_tranScrollRect.sizeDelta.y : m_tranScrollRect.sizeDelta.x;
            return m_isVertical ? m_tranScrollRect.rect.height : m_tranScrollRect.rect.width;
        }
    }
    //约束常量（固定的行（列）数）
    private int ConstraintCount
    {
        get
        {
            return m_LayoutGroup == null ? 1 : ((m_LayoutGroup is GridLayoutGroup) ? (m_LayoutGroup as GridLayoutGroup).constraintCount : 1);
        }
    }

    //缓存数量
    private int CacheCount
    {
        get
        {
            return ConstraintCount + maxLength % ConstraintCount;
        }
    }
    //缓存单元的行（列）数
    private int CacheUnitCount
    {
        get
        {
            return m_LayoutGroup == null ? 1 : Mathf.CeilToInt((float)CacheCount / ConstraintCount);
        }
    }
    //数据单元的行（列）数
    private int DataUnitCount
    {
        get
        {
            return m_LayoutGroup == null ? maxLength : Mathf.CeilToInt((float)maxLength / ConstraintCount);
        }
    }
    public int MaxLength
    {
        set
        {
            maxLength = value;
            UpdateView();
        }
    }

    void Awake()
    {
        var go = gameObject;
        var trans = transform;
        m_toggleGroup = GetComponent<ToggleGroup>();
        m_LayoutGroup = GetComponentInChildren<LayoutGroup>(true);
        m_content = m_LayoutGroup.gameObject.GetComponent<RectTransform>();
        if (m_LayoutGroup != null)
            m_oldPadding = m_LayoutGroup.padding;

        m_scrollRect = trans.GetComponentInParent<ScrollRect>();
        if (m_scrollRect != null && m_LayoutGroup != null)
        {
            m_scrollRect.gameObject.layer = LayerMask.NameToLayer("UI");
            m_scrollRect.decelerationRate = 0.2f;
            m_tranScrollRect = m_scrollRect.GetComponent<RectTransform>();
            m_isVertical = m_scrollRect.vertical;
            var layoutgroup = m_LayoutGroup as GridLayoutGroup;
            if (layoutgroup != null)
            {
                m_itemSpace = (int)(m_isVertical ? (layoutgroup.cellSize.y + layoutgroup.spacing.y) : (layoutgroup.cellSize.x + layoutgroup.spacing.x));
                m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
            }
        }
        else
        {
            Util.LogError("scrollRect is null or verticalLayoutGroup is null");
        }
        SetItemRender(ItemCell);
        if (m_scrollRect != null)
        {
            if (useLoopItems)
                m_scrollRect.onValueChanged.AddListener(OnScroll);
            if (m_toggleGroup != null)
                m_toggleGroup.allowSwitchOff = useLoopItems;
        }
        ResetScrollPosition();
    }

    /// <summary>
    /// 设置渲染项
    /// </summary>
    /// <param name="goItemRender"></param>
    /// <param name="itemRenderType"></param>
    public void SetItemRender(GameObject goItemRender)
    {
        if (goItemRender == null)
            return;
        m_goItemRender = goItemRender;
        m_itemRenderType = typeof(ItemRender);
        LayoutElement le = goItemRender.GetComponent<LayoutElement>();
        var layoutGroup = m_LayoutGroup as HorizontalOrVerticalLayoutGroup;
        if (le != null && layoutGroup != null)
        {
            if (m_tranScrollRect == null)
            {
                m_scrollRect = transform.GetComponentInParent<ScrollRect>();
                m_tranScrollRect = m_scrollRect.GetComponent<RectTransform>();
            }
            m_itemSpace = (int)(le.preferredHeight + (int)layoutGroup.spacing);
            m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
        }
    }

    public void SetItemClickSound(string sound)
    {
        //m_itemClickSound = sound;
    }

    public ItemRender[] getItemRenders()
    {
        return m_items.ToArray();
    }

    /// <summary>
    /// 当前选择的数据项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public int SelectedData()
    {
        return m_selectedData;
    }

    /// <summary>
    /// 重置滚动位置，
    /// </summary>
    /// <param name="top">true则跳转到顶部，false则跳转到底部</param>
    public void ResetScrollPosition(bool top = true)
    {
        int index = top ? 0 : maxLength;
        ResetScrollPosition(index);
    }

    /// <summary>
    /// 重置滚动位置，如果同时还要赋值新的Data，请在赋值之前调用本方法
    /// </summary>
    public void ResetScrollPosition(int index)
    {
        var unitIndex = Mathf.Clamp(index / ConstraintCount, 0, DataUnitCount - m_viewItemCount > 0 ? DataUnitCount - m_viewItemCount : 0);
        var value = (unitIndex * m_itemSpace) / (Mathf.Max(ViewSpace, ContentSpace - ViewSpace));
        value = Mathf.Clamp01(value);

        //特殊处理无法使指定条目置顶的情况——拉到最后
        if (unitIndex != index / ConstraintCount)
            value = 1;

        if (m_scrollRect)
        {
            if (m_isVertical)
                m_scrollRect.verticalNormalizedPosition = 1 - value;
            else
                m_scrollRect.horizontalNormalizedPosition = value;
        }

        m_startIndex = unitIndex * ConstraintCount;
        UpdateView();
    }

    /// <summary>
    /// 更新视图
    /// </summary>
    public void UpdateView()
    {
        if (useLoopItems)
        {
            m_startIndex = Mathf.Max(0, Mathf.Min(m_startIndex / ConstraintCount, DataUnitCount - m_viewItemCount - CacheUnitCount)) * ConstraintCount;
            var frontSpace = m_startIndex / ConstraintCount * m_itemSpace;
            var behindSpace = Mathf.Max(0, m_itemSpace * (DataUnitCount - CacheUnitCount) - frontSpace - (m_itemSpace * m_viewItemCount));
            if (m_isVertical)
                m_LayoutGroup.padding = new RectOffset(m_oldPadding.left, m_oldPadding.right, frontSpace, behindSpace);
            else
                m_LayoutGroup.padding = new RectOffset(frontSpace, behindSpace, m_oldPadding.top, m_oldPadding.bottom);
        }
        else
            m_startIndex = 0;

        if (m_goItemRender == null || m_itemRenderType == null || m_content == null)
            return;

        int itemLength = useLoopItems ? m_viewItemCount * ConstraintCount + CacheCount : maxLength;
        itemLength = Mathf.Min(itemLength, maxLength);
        for (int i = itemLength; i < m_items.Count; i++)
        {
            Destroy(m_items[i].gameObject);
            m_items[i] = null;
        }
        for (int i = m_items.Count - 1; i >= 0; i--)
        {
            if (m_items[i] == null)
                m_items.RemoveAt(i);
        }

        for (int i = 0; i < itemLength; i++)
        {
            var index = m_startIndex + i;
            if (index >= maxLength || index < 0)
                continue;
            if (i < m_items.Count)
            {
                m_items[i].SetData(index);

                if (useClickEvent || autoSelectFirst)
                    SetToggle(i, m_selectedData == index);
            }
            else
            {
                var go = Instantiate(m_goItemRender) as GameObject;
                go.name = m_goItemRender.name + i;
                go.transform.SetParent(m_content, false);
                go.SetActive(true);
                var script = go.AddComponent(m_itemRenderType) as ItemRender;
                if (!go.activeInHierarchy)
                    script.Awake();
                script.SetData(index);
                script.m_owner = this;
                script.gameObj = go;
                if (useClickEvent)
                    UGUIClickHandler.Get(go, m_itemClickSound).onPointerClick += OnItemClick;
                if (m_toggleGroup != null)
                {
                    var toggle = go.GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        toggle.group = m_toggleGroup;

                        //使用循环模式的话不能用渐变效果，否则视觉上会出现破绽
                        if (useLoopItems)
                            toggle.toggleTransition = Toggle.ToggleTransition.None;
                    }
                }
                m_items.Add(script);
            }
        }
    }

    private void OnScroll(Vector2 data)
    {
        var value = (ContentSpace - ViewSpace) * (m_isVertical ? data.y : 1 - data.x);
        var start = ContentSpace - value - ViewSpace;
        var startIndex = Mathf.FloorToInt(start / m_itemSpace) * ConstraintCount;
        startIndex = Mathf.Max(0, startIndex);

        if (startIndex != m_startIndex)
        {
            m_startIndex = startIndex;
            UpdateView();
        }
    }

    private void SelectItem(int renderData)
    {
        m_selectedData = renderData;
        if (null != onItemSelectedFunc)
        {
            onItemSelectedFunc(m_selectedData);
        }
    }
    private void OnItemClick(GameObject target, BaseEventData baseEventData)
    {
        var renderData = target.GetComponent<ItemRender>().m_renderData;
        if (useLoopItems && renderData == m_selectedData)
        {
            var toggle = target.GetComponent<Toggle>();
            if (toggle)
                toggle.isOn = true;
        }
        SelectItem(renderData);
    }

    private void SetToggle(int index, bool value)
    {
        if (index < m_items.Count)
        {
            var toggle = m_items[index].GetComponent<Toggle>();
            if (toggle)
                toggle.isOn = value;
        }
    }

    public void Destroy()
    {
        if (onItemSelectedFunc != null)
        {
            onItemSelectedFunc = null;
        }
        for (int i = 0; i < m_items.Count; i++)
        {
            Destroy(m_items[i].gameObject);
        }
        m_items.Clear();
    }

    /// <summary>
    /// 选择指定项
    /// </summary>
    /// <param name="index"></param>
    public void Select(int index)
    {
        if (index >= maxLength)
            return;

        if (index != m_selectedData)
            SelectItem(index);

        UpdateView();
    }

    /// <summary>
    /// 开启或关闭某一项的响应
    /// </summary>
    /// <param name="index"></param>
    public void Enable(int index, bool isEnable = true)
    {
        if (index < m_items.Count)
        {
            var toggle = m_items[index].GetComponent<Toggle>();
            if (toggle)
            {
                toggle.isOn = isEnable;
                toggle.enabled = isEnable;
                if (isEnable)
                {
                    UGUIClickHandler.Get(toggle.gameObject, m_itemClickSound).onPointerClick += OnItemClick;
                }
                else
                {
                    UGUIClickHandler.Get(toggle.gameObject, m_itemClickSound).RemoveAllHandler();
                }
            }
        }
    }
}