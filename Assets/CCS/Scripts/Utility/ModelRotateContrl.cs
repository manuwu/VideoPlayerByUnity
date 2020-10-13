using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using CCS;

[ExecuteInEditMode]
public class ModelRotateContrl : MonoBehaviour, IDragHandler
{
    public Transform model;
    public float speed = 250;
    public float resetTime = 1.0f;
    public float delayTime = 3.0f;
    private float X_before;
    private float X_after;

    public void OnDrag(PointerEventData eventData)
    {
        if (!model) return;

        X_after = eventData.position.x;
        if (X_before - X_after <= 0)
        {
            model.Rotate(Vector3.down * Time.deltaTime * speed);
        }
        else
        {
            model.Rotate(Vector3.up * Time.deltaTime * speed);
        }
        X_before = X_after;
        CancelInvoke("ResetRotation");
        Invoke("ResetRotation", delayTime);
    }

    [ContextMenu("ResetRotation")]
    public void ResetRotation()
    {
        model.DOLocalRotate(Vector3.zero, resetTime);
    }
}
