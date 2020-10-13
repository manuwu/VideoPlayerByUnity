 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CCS
{
    public class InfiniteList : MonoBehaviour
    {
        /// <summary>
        /// 当前的顺序
        /// </summary>
        private int CURRENTINDEX = 0;
        public RectTransform mContent = null;

        private int mTotalDataCount = 0;
        public ScrollRect mScroll = null;
        public GridLayoutGroup mLayoutGroup = null;
        public RectTransform mViewPort = null;
        public GameObject mItemClone = null;
        //public LuaFunction onItemFunc = null;
        public LinkedList<GridItem> mInfiniteList = null;
        private List<GameObject> mObjList = new List<GameObject>();
        private int mStarIndex = 0;
        private int mEndIndex = 0;
        /// <summary>
        /// 最多需要多少个这个Item
        /// </summary>
        private int mMaxNum = 0;

        /// <summary>
        /// content的pos
        /// </summary>
        private float mContentValue = 0;
        /// <summary>
        /// 间距两个item之间
        /// </summary>
        private float mSpaceing = 0;
        /// <summary>
        /// 物体的size
        /// </summary>
        private float mCellSize = 0;
        /// <summary>
        /// mContentCellSize
        /// </summary>
        private float mContentCellSize = 0;

        /// <summary>
        /// this.transfrom.recttrans;
        /// </summary>
        private float mViewPortRect = 0;

        /// <summary>
        /// 超过这个值的倍数 就变换
        /// </summary>
        private float mBaseoffsetValue = 0;

        /// <summary>
        /// 规划化scoll数值 初始化content相对位置0-1
        /// </summary>
        private float mNormalValue = 0;

        /// <summary>
        /// 0是横 1 是竖 
        /// </summary>
        private int mCurrentType = 0;

        private void GetCurrentType()
        {
            if (mScroll.horizontal)
            {
                mCurrentType = 0;

            }
            else if (mScroll.vertical)
            {
                mCurrentType = 1;
            }
        }

        private void InitOriginData()
        {
            GetCurrentType();
            if (mScroll.horizontal)
            {
                mContentValue = mContent.anchoredPosition3D.x;
                mSpaceing = mLayoutGroup.spacing.x;
                mCellSize = mLayoutGroup.cellSize.x;
                mViewPortRect = mViewPort.rect.width;

            }
            else if (mScroll.vertical)
            {

                mContentValue = mContent.anchoredPosition3D.y;
                mSpaceing = mLayoutGroup.spacing.y;
                mCellSize = mLayoutGroup.cellSize.y;
                mViewPortRect = mViewPort.rect.height;

            }
            else
                GameLogger.LogError("InfiniteList ScrollView only accept horizontal or vertical");
            var space = mSpaceing + mCellSize;
            mContentCellSize = mTotalDataCount * space - mSpaceing;

            var Delta = mContentCellSize - mViewPortRect;
            mBaseoffsetValue = (mSpaceing + mCellSize) / (Delta);
            if (mBaseoffsetValue < 0)
            {
                mBaseoffsetValue = 1;
            }
            var LayoutSpacing = mSpaceing + mCellSize;
            mMaxNum = (int)(mViewPortRect / LayoutSpacing) + 2;
            if (mMaxNum >= mTotalDataCount)
            {
                mMaxNum = mTotalDataCount;
            }
        }

        /// <summary>
        /// 获取 content的SizeDalta
        /// </summary>
        private void SetContentSizeDelta()
        {
            var sizeDelta = mContent.sizeDelta;
            if (mScroll.horizontal)
            {
                sizeDelta.x = mContentCellSize;
                sizeDelta.y = mLayoutGroup.cellSize.y ;
            }
            else if (mScroll.vertical)
            {
                sizeDelta.y = mContentCellSize;
                sizeDelta.x = mLayoutGroup.cellSize.x ;
            }
            else
            {
                GameLogger.LogError("InfiniteList ScrollView only accept horizontal or vertical");
            }
            mContent.sizeDelta = sizeDelta;
            
        }

        private void SetNormalize()
        {
            if (mScroll.horizontal)
                mNormalValue = mScroll.normalizedPosition.x;
            else if (mScroll.vertical)
                mNormalValue = mScroll.normalizedPosition.y;
            if (mNormalValue == 1)
            {
                mNormalValue = 0;
            }
            GameLogger.Log("mNormalValue:" + mNormalValue);
        }

        public void SetPos(int index, GameObject obj)
        {
            var Value = 0.0f;
            index = mTotalDataCount - index;
            if (index == 0)
            {
                Value = -(mContentCellSize - mCellSize) / 2;
            }
            else
            {
                Value = (-(mContentCellSize - mCellSize) / 2) + (index) * (mCellSize + mSpaceing);
            }
            if (mScroll.horizontal)
            {
                var offset = mLayoutGroup.cellSize.y - mContent.sizeDelta.y;
                //obj.transform.rectTransform().anchoredPosition3D = new Vector3(Value, offset, 0);
            }
            else if (mScroll.vertical)
            {
                var offset = mLayoutGroup.cellSize.x - mContent.sizeDelta.x;
                //obj.transform.rectTransform().anchoredPosition3D = new Vector3(offset, Value, 0);
                //Debug.Log(obj.transform.localPosition);
                //Debug.Log("mContentCellSize:" + mContentCellSize + "mCellSize:" + mCellSize);
                //Debug.Log("Value:" + Value);
                //Debug.Log("index" + index);
            }
        }

        private int GetStarIndex(float normalize)
        {
            var Value = mTotalDataCount - (int)(normalize / mBaseoffsetValue);
            if (Value - mMaxNum + 1 <= 0)
            {
                Value = mMaxNum;
            }
            if (Value > mTotalDataCount)
            {
                Value = mTotalDataCount;
            }
            return Value;
        }

        private void InitTrans()
        {
            mContent.anchorMax = Vector2.one / 2;
            mContent.anchorMin = Vector2.one / 2;
        }

        public void RefreshList(int TOTALCOUNT)
        {
            InitTrans();
            mTotalDataCount = TOTALCOUNT;
            if (mTotalDataCount < 0) { mInfiniteList = null; return; }
            InitOriginData();
            SetContentSizeDelta();
            SetNormalize();
            SetOriginPos(CURRENTINDEX);
            mInfiniteList = new LinkedList<GridItem>();
            //Debug.Log("mNormalValue:" + mNormalValue);
            //Debug.Log("mBaseoffsetValue:" + mBaseoffsetValue);
            mStarIndex = GetStarIndex(mNormalValue);
            mEndIndex = mStarIndex - mMaxNum + 1;
            CURRENTINDEX = mStarIndex;
            mLayoutGroup.enabled = false;
            mObjList.Sort((T1,T2) => { if (!T1.activeSelf || !T2.activeSelf) { return -1; }  else if (T1.GetComponent<GridItem>().mCurrentIndex > T2.GetComponent<GridItem>().mCurrentIndex) { return 1; }                
                else { return -1; } });
            for (int i = 0; i < mObjList.Count; i++)
            {
                if(i<mTotalDataCount)
                    mObjList[i].SetActive(true);
                else
                    mObjList[i].SetActive(false);
            }
            for (int i = 0; i < mMaxNum; i++)
            {
                GameObject obj = null;
                GridItem grid = null;
                if (i < mObjList.Count)
                {
                    obj = mObjList[i];
                    grid = obj.GetComponent<GridItem>();                    
                }
                else
                {
                    obj = GameObject.Instantiate(mItemClone);
                    obj.transform.SetParent(mContent.transform);
                    obj.transform.localScale = Vector3.one;
                    obj.SetActive(true);
                    grid = obj.AddComponent<GridItem>();
                    //obj.transform.rectTransform().anchorMax = Vector2.one / 2;
                    //obj.transform.rectTransform().anchorMin = Vector2.one / 2;
                    //obj.transform.rectTransform().sizeDelta = mLayoutGroup.cellSize;
                    mObjList.Add(obj);
                }
                var Pos = mStarIndex - i;
                grid.InitItem(this);
                grid.Refresh(Pos);
            }
            
            mScroll.onValueChanged.RemoveAllListeners();
            mScroll.onValueChanged.AddListener((vec) =>
            {
                UpdateView(vec);
                //Debug.Log(vec);
            });
            
        }
        //public void AddOnItemFunc(LuaFunction luac)
        //{
        //    onItemFunc = luac;
        //}


        public void GoToItem(int gotoIndex)
        {
            SetOriginPos(gotoIndex);
            //RefreshList(mTotalDataCount);
        }

        private void SetOriginPos(int gotoIndex)
        {
            var Value = 0;
            if (mTotalDataCount / 2 > gotoIndex)
            {
                Value = mTotalDataCount - gotoIndex + 1;
            }
            else
            {
                Value = mTotalDataCount - gotoIndex;
            }
            if (mScroll.vertical)
            {
                mScroll.normalizedPosition = new Vector2(mScroll.normalizedPosition.x, (Value / (float)mTotalDataCount));
                GameLogger.Log(mScroll.normalizedPosition);
            }
            else if (mScroll.horizontal)
            {
                mScroll.normalizedPosition = new Vector2((Value / (float)mTotalDataCount), mScroll.normalizedPosition.y);
            }
            mStarIndex = GetStarIndex(mNormalValue);
            mEndIndex = mStarIndex - mMaxNum + 1;
            CURRENTINDEX = mStarIndex;
            UpdateView(mScroll.normalizedPosition);
        }

        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.F1))
        //    {
        //        mTotalDataCount = mTotalDataCount - 1;
        //        //Debug.Log(mTotalDataCount);
        //        RefreshList(mTotalDataCount);
        //    }
        //    if (Input.GetKeyDown(KeyCode.F2))
        //    {
        //        //Debug.Log("CURRENTINDEX" + (5 / (float)mTotalDataCount));
        //        SetOriginPos(mTotalDataCount);
        //    }
        //    if (Input.GetKeyDown(KeyCode.F3))
        //    {
        //        mTotalDataCount = mTotalDataCount + 100;
        //        //Debug.Log(mTotalDataCount);
        //        RefreshList(mTotalDataCount);
        //        //SetOriginPos(mTotalDataCount);
        //    }
        //    if (Input.GetKeyDown(KeyCode.F4))
        //    {
        //        //Debug.Log("CURRENTINDEX" + (5 / (float)mTotalDataCount));
        //        //RefreshList(mTotalDataCount);
        //        SetOriginPos(20);
                
        //    }
        //}

        //void Start()
        //{
        //    RefreshList(mTotalDataCount);
        //}


        void UpdateView(Vector2 vec2)
        {
            var Value = mCurrentType == 0 ? vec2.x : vec2.y;
            if (mInfiniteList == null || mInfiniteList.Count == 0) { return; }
            if (Value < 0 || Value > 1) { if (Value < 0) Value = 0; else if (Value > 1) Value = 1; }
            if (mTotalDataCount <= mMaxNum) { return; }
            var mCurretIndex = GetStarIndex(Value);
            //Debug.Log("mCurretIndex:" + mCurretIndex);
            var tempValue = mCurretIndex;
            if (mStarIndex != mCurretIndex)
            {
                var temp = 1;
                while (mCurretIndex != CURRENTINDEX)
                {
                    if (Mathf.Abs(mCurretIndex - CURRENTINDEX) > mMaxNum)
                    {
                        if ((mCurretIndex - CURRENTINDEX) > 0)
                        {
                            temp = Mathf.Abs(mCurretIndex - CURRENTINDEX - mMaxNum);
                        }
                        else if (mCurretIndex - CURRENTINDEX < 0)
                        {
                            temp = Mathf.Abs(mCurretIndex - CURRENTINDEX + mMaxNum);
                        }
                        
                    }
                    else
                    {
                        temp = 1;
                    }
                    if (mCurretIndex > CURRENTINDEX && mStarIndex < mTotalDataCount)
                    {
                        mCurretIndex -= temp;
                        SetNew(ref mStarIndex, ref mEndIndex, temp);
                    }
                    else if (mCurretIndex < CURRENTINDEX && mEndIndex > 1)
                    {
                        mCurretIndex += temp;
                        SetNew(ref mStarIndex, ref mEndIndex, -temp);
                    }
                    else
                    {
                        break;
                    }
                }               
                CURRENTINDEX = tempValue;
            }

        }

        private void SetNew(ref int star,ref int end, int i)
        {
            star = star + i;
            end = end + i;
            if (i < 0)
            {
                var First = mInfiniteList.First;
                mInfiniteList.RemoveFirst();
                mInfiniteList.AddLast(First);
                mInfiniteList.Last.Value.Refresh(end);
            }
            else if (i > 0)
            {
                var Last = mInfiniteList.Last;
                mInfiniteList.RemoveLast();
                mInfiniteList.AddFirst(Last);              
                mInfiniteList.First.Value.Refresh(star);                
            }
        }

        void OnDestroy()
        {
            //if (onItemFunc != null)
            //{
            //    onItemFunc.EndPCall();
            //    onItemFunc = null;
            //}
        }
    }
}
