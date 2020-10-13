using UnityEngine;
using UnityEngine.UI;
using CCS;
using DG.Tweening;
public class PlayAnim : MonoBehaviour
{
    public Animation anim = null;
    public Transform GridNode = null;
    public void Play(string name)
    {
        if(anim == null)
            anim = GetComponent<Animation>();
        anim.Play(name);
    }

    public void Stop()
    {
        if (anim != null)
            anim.Stop();
    }

   
}
