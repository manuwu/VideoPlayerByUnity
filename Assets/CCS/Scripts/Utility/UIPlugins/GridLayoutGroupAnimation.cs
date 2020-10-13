using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class GridLayoutGroupAnimation : MonoBehaviour
{
    public Vector3 v3wai = new Vector3(10000, 0, 0);
    public float duration = 0.5f;
    public float delay = 1;
    public float delayDuration = 0.06f;

    GridLayoutGroup glg;
    List<Vector3> v3List = new List<Vector3>();
    List<Transform> tranList = new List<Transform>();
    
    void OnEnable()
    {
        glg = this.GetComponent<GridLayoutGroup>();
        glg.enabled = false;
        Transform t;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            t = this.transform.GetChild(i);
            v3List.Add(t.localPosition);
            tranList.Add(t);
            t.localPosition = t.localPosition + v3wai;
        }
        int j = this.transform.childCount - 1;
        for (int i = 0; i < j; i++)
        {
            tranList[i].DOLocalMove(v3List[i], duration, true).SetDelay(delay + i * delayDuration);
        }
        tranList[j].DOLocalMove(v3List[j], duration, true).SetDelay(delay + j * delayDuration).OnComplete(() =>
        {
            glg.enabled = true;
            this.enabled = false;
            v3List.Clear();
            tranList.Clear();
        });
    }
}
