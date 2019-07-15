using UnityEngine;

namespace CCS
{
    public class Main : MonoBehaviour
    {
        public GameObject ReporterObj = null;

        void Start()
        {
#if DEBUG_A
            Reporter reporter = FindObjectOfType(typeof(Reporter)) as Reporter;
            if (reporter == null)
            {
                GameObject.Instantiate(ReporterObj);
            }
#endif

#if DEBUG_TEST
            Util.ShowLog(true);
            Util.PrintLogToFile(true);
#else
            Util.ShowLog(true);
            Util.PrintLogToFile(true);
#endif
            AppFacade.Instance.StartUp();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Screen.fullScreen = false;  //退出全屏         
            }
        }
    }
}