using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CCS
{
    public class PanelBase : View
    {
        public GameObject skin;
        public object[]   args;

        public bool isShow = false;
        public bool isInit = false;

        public virtual void Init(params object[] args)
        {
            this.args = args;
        }

        public virtual void OnShowing()   { }
        public virtual void OnShowed()    { }
        public virtual void Update()      { }
        public virtual void OnCloseing()  { }
        public virtual void OnClosed()    { }

        public virtual void AddEvent()    { }
        public virtual void RemoveEvent() { }

        public virtual void OnDestroy()   { }
    }
}
