using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ReporterGUI : MonoBehaviour
{
	Reporter reporter;
    EventSystem eventSystem;
    void Awake()
	{
		reporter = gameObject.GetComponent<Reporter>();
        eventSystem = FindObjectOfType(typeof(EventSystem)) as EventSystem;
        if (eventSystem != null)
        {
            eventSystem.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (eventSystem != null)
        {
            eventSystem.gameObject.SetActive(true);
        }
    }

	void OnGUI()
	{
		reporter.OnGUIDraw();
	}
}
