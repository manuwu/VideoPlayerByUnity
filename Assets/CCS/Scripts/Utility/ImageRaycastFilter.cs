using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using CCS;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class ImageRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    public bool reversed;

    private Image image;

    void OnEnable()
    {
        image = GetComponent<Image>();
        image.type = Image.Type.Simple;
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!enabled)
            return true;

        Sprite sprite = image.overrideSprite;
        if (sprite == null)
            return true;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform, screenPoint, eventCamera, out local);

        Rect rect = image.rectTransform.rect;

        // Convert to have lower left corner as reference point.
        local.x += image.rectTransform.pivot.x * rect.width;
        local.y += image.rectTransform.pivot.y * rect.height;

        float u = local.x / rect.width;
        float v = local.y / rect.height;

        try
        {
            if (!reversed)
                return sprite.texture.GetPixelBilinear(u, v).a != 0;
            else
                return sprite.texture.GetPixelBilinear(u, v).a == 0;
        }
        catch (UnityException e)
        {
            GameLogger.LogError(e);
        }

        return true;
    }
}