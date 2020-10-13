using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EffectController : MonoBehaviour
{
    public bool PlayOnAwake = true;
    public float Duration;
    private Action m_CallFunc;
    private bool m_IsPlay = false;
    private float m_CurTime = 0;
    private List<ParticleSystem> m_ParticleList = new List<ParticleSystem>();

    public void Play(Action func)
    {
        m_CallFunc = func;
        m_IsPlay = true;
        for (int i = 0; i < m_ParticleList.Count; ++i)
            m_ParticleList[i].Play();
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

        if (PlayOnAwake)
            Play();
        else
        {
            for (int i = 0; i < m_ParticleList.Count; ++i)
            {
                m_ParticleList[i].playOnAwake = false;
                m_ParticleList[i].Stop();
            }
        }
    }
	
	void Update ()
    {
        if (!m_IsPlay)
            return;
        m_CurTime += Time.deltaTime;
        if (m_CurTime >= Duration)
        {
            if (m_CallFunc != null)
            {
                m_CallFunc();
                m_CallFunc = null;
            }
            GameObject.Destroy(gameObject);
        }
    }
}
