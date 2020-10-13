using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupSwitch : MonoBehaviour
{
    private Toggle m_toggle = null;

    private void Start()
    {
        m_toggle = transform.GetComponent<Toggle>();
        if (m_toggle != null)
        {
            m_toggle.onValueChanged.AddListener((b) =>
            {
               if (b)
                {
                    transform.Find("Background/Checkmark").gameObject.SetActive(true);
                    transform.Find("Label").gameObject.SetActive(false);
                }
                else
                {
                    transform.Find("Background/Checkmark").gameObject.SetActive(false);
                    transform.Find("Label").gameObject.SetActive(true);
                }
            });
        }
    }
}
