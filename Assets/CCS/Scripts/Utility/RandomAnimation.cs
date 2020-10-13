using UnityEngine;
using System.Collections;

public class RandomAnimation : MonoBehaviour
{

    // Use this for initialization
    Animator ani;

    public float fMin = 0.5f;
    public float fMax = 1.5f;

    void Start()
    {
        ani = this.GetComponent<Animator>();
        ani.speed = 0;
        StartCoroutine(StartPlayer());
    }
    IEnumerator StartPlayer()
    {
        yield return new WaitForSeconds(Random.Range(fMin, fMax));
        ani.speed = 1;
        Destroy(this);
    }
}
