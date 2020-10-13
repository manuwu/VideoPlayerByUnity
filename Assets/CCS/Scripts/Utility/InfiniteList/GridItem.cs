 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CCS
{

    public class GridItem : MonoBehaviour
    {
        public int mCurrentIndex = 0;
        public delegate void mDeletgate(int u);
        public event mDeletgate mHandler;

        private InfiniteList mManger = null;
        public void InitItem(InfiniteList manager)
        {
            mManger = manager;
            mHandler = Refreshthis;
            mManger.mInfiniteList.AddLast(this);

        }

        public void Refresh(int index)
        {
            if (mHandler!=null)
                mHandler.Invoke(index);
        }

        private void Refreshthis(int index)
        {

            mCurrentIndex = index;
            gameObject.name = mCurrentIndex.ToString();
            mManger.SetPos(index, this.gameObject);
            

            //if (mManger.onItemFunc != null)
            //    mManger.onItemFunc.Call(transform, mCurrentIndex);
            //else
            //    Util.LogWarning("第" + index +"个Item没有注册事件活事件在InitList之后");
        }

        void OnDestroy()
        {
            mHandler = null;
        }
    }
}
