using UnityEngine;
using System.Collections.Generic;
 
using CCS;
using System;

public class ParticleSystemController : MonoBehaviour
{
    public bool PlayOnAwake = true;
    //当持久播放的粒子需要手动销毁
    public bool DestoryByHand = false;
    //持续时间
    public float Duration = 1.0f;
    //private LuaFunction m_CallFunc;
    private bool m_IsPlay = false;
    private float m_CurTime = 0;
    private List<ParticleSystem> m_ParticleList = new List<ParticleSystem>();
    private int m_ParticleListCount = 0;
    /// <summary>
    /// 开始例子播放
    /// </summary>
    /// <param name="func">例子播放指定时间结束后返回处理函数</param>
    public void Play(Action  callBack)
    {
        //m_CallFunc = callBack;
        m_IsPlay = true;
        for (int i = 0; i < m_ParticleListCount; ++i)
            m_ParticleList[i].Play();
    }
    //暂停粒子特效
    public void Pause()
    {
        for (int i = 0; i < m_ParticleListCount; ++i)
            m_ParticleList[i].Pause();
    }
    public void Play()
    {
        Play(null);
    }

    void Awake()
    {
        ParticleSystem p = transform.GetComponent<ParticleSystem>();
        if (p != null)
            m_ParticleList.Add(p);
        m_ParticleList.AddRange(transform.GetComponentsInChildren<ParticleSystem>());
        m_ParticleListCount = m_ParticleList.Count;
        if (PlayOnAwake)
            Play();
        else
        {
            for (int i = 0; i < m_ParticleListCount; ++i)
            {
                m_ParticleList[i].playOnAwake = false;
                m_ParticleList[i].Stop();
            }
        }
    }

    void Update()
    {
        if (Duration <= 0)
        {
            return;
        }
        if (!m_IsPlay)
            return;
        m_CurTime += Time.deltaTime;
        if (m_CurTime >= Duration)
        {
            //if (m_CallFunc != null)
            //{
            //    m_CallFunc.Call();
            //    m_CallFunc = null;
            //}
            if (DestoryByHand == false)
            {
                GODestroy();
            }
        }
    }
    /// <summary>
    /// 粒子系统销毁，当粒子自动永久播放时，不需要时调用该方法删除
    /// </summary>
    public void GODestroy()
    {
        for (int i = 0; i < m_ParticleListCount; ++i)
        {
            m_ParticleList[i].Stop();
        }
        GameObject.Destroy(gameObject);
    }
}
