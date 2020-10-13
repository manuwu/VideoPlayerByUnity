/*
 * Description:具有MonoBehaviour特性的单例类
 */
using UnityEngine;

public class SingletonMB<T> : MonoBehaviour where T : Component
{
    private static T Instance;

    public static T GetInstance()
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<T>();
            if(Instance == null)
            {
                GameObject go = new GameObject();
                Instance = go.AddComponent<T>();
                go.name = Instance.ToString();
            }
            DontDestroyOnLoad(Instance.gameObject);
        }
        return Instance;
    }
}