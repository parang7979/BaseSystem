using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Parang.UI
{
    public interface IScrollListener
    {
        void OnScrolled(int itemCount, int index);
    }

    public class UILoop : MonoBehaviour
    {
        [SerializeField]
        protected bool m_InitOnStart = true;

        public delegate void UILoopCallBack(int index, GameObject go);
        public delegate long UILoopCallBackIdentify(int index);

        private enum Direction
        {
            Horizontal,
            Vertical
        }

        public const long UNDEFINED = 0;

        [SerializeField]
        protected RectTransform m_Cell;

        [SerializeField]
        protected Vector2 m_CellGap;

        [SerializeField]
        protected Vector2 m_Page;

        [SerializeField]
        private Direction m_Direction = Direction.Vertical;

        [SerializeField, Range(0, 10)]
        private int m_BufferNo = 1;

        [SerializeField]
        private int m_ItemsCount = 0;

        [SerializeField]
        protected float m_StartOffset = 0f;

        [SerializeField]
        protected float m_EndOffset = 0f;

        [SerializeField]
        protected GameObject m_EmptyGo;

        [SerializeField]
        protected bool m_AlignCenter = false;

        public Action onInitFinished;
        public UILoopCallBack onUpdate;
        public UILoopCallBack onCreate;
        public UILoopCallBack onRemove;
        public UILoopCallBack onCreateInSort;
        public UILoopCallBack onRemoveInSort;
        private UILoopCallBackIdentify onIdentify = null;

        public Vector2 CellRect { get { return m_Cell != null ? m_Cell.rect.size + m_CellGap : new Vector2(100, 100); } }

        private float m_PrevPos = 0;
        private List<RectTransform> m_InstantiateItems = new List<RectTransform>();
        private int m_CurrentIndex;
        private IScrollListener scrollListener;
        private Vector2 m_InstantiateSize = Vector2.zero;
        private ScrollRect m_ScrollRect;
        private RectTransform m_Rect;
        private Vector2 m_gridItemCount;
        private float m_LeftPadding = 0f;

        private Vector2 instantiateSize
        {
            get
            {
                if (m_InstantiateSize == Vector2.zero)
                {
                    float rows, cols;
                    if (m_Direction == Direction.Horizontal)
                    {
                        rows = m_Page.x;
                        cols = m_Page.y + m_BufferNo;
                    }
                    else
                    {
                        rows = m_Page.x + m_BufferNo;
                        cols = m_Page.y;
                    }
                    m_InstantiateSize = new Vector2(rows, cols);
                }
                return m_InstantiateSize;
            }
        }
        private Vector2 gridItemCount
        {
            get
            {
                if (m_gridItemCount == Vector2.zero)
                {
                    m_gridItemCount.x = GetGridItemCount(m_Cell.rect.size.x, GetViewportWidth(), m_CellGap.x);
                    m_gridItemCount.y = GetGridItemCount(m_Cell.rect.size.y, GetViewportHeight(), m_CellGap.y);
                    if (m_Direction == Direction.Horizontal)
                        m_gridItemCount.x++;
                    else
                        m_gridItemCount.y++;
                }
                return m_gridItemCount;
            }
        }
        private int PageCount { get { return (int)m_Page.x * (int)m_Page.y; } }
        private int PageScale { get { return m_Direction == Direction.Horizontal ? (int)m_Page.x : (int)m_Page.y; } }
        private int InstantiateCount { get { return (int)instantiateSize.x * (int)instantiateSize.y; } }
        protected float scale { get { return m_Direction == Direction.Horizontal ? 1f : -1f; } }
        private bool IsIdentifiable { get { return onIdentify != null; } }
        protected float CellScale { get { return m_Direction == Direction.Horizontal ? CellRect.x : CellRect.y; } }
        private float DirectionPos { get { return m_Direction == Direction.Horizontal ? m_Rect.anchoredPosition.x : m_Rect.anchoredPosition.y; } }

        private void Awake()
        {
            if (m_ScrollRect == null)
            {
                m_ScrollRect = GetComponentInParent<ScrollRect>();

            }
            if (m_ScrollRect != null)
            {
                m_ScrollRect.horizontal = m_Direction == Direction.Horizontal;
                m_ScrollRect.vertical = m_Direction == Direction.Vertical;
            }

            m_Rect = GetComponent<RectTransform>();
            m_Rect.anchoredPosition = Vector2.zero;
            m_Cell.gameObject.SetActive(false);
        }

        public void Start()
        {
            if (m_InitOnStart) Init();
        }

        public void SetScrollListener(IScrollListener listener)
        {
            scrollListener = listener;
        }

        private int GetGridItemCount(float contentSize, float size, float spacing)
        {
            int itemCount = 1;
            for (int i = 0; i < 100; ++i)
            {
                var length = (contentSize * itemCount) + (spacing * (itemCount - 1));
                if (length > size)
                {
                    itemCount--;
                    break;
                }
                itemCount++;
            }
            return Mathf.Max(1, itemCount);
        }

        private void ResetPage()
        {
            m_gridItemCount = Vector2.zero;
            m_Page = Vector2.zero;
            m_Page.x = m_Page.x == 0 ? gridItemCount.y : m_Page.x;
            m_Page.y = m_Page.y == 0 ? gridItemCount.x : m_Page.y;
            if (m_AlignCenter)
                m_LeftPadding = Mathf.Max(0f, (GetViewportWidth() - (m_Cell.sizeDelta.x * m_Page.y + (m_Page.y - 1) * m_CellGap.x)) * 0.5f);
            else
                m_LeftPadding = 0;
        }

        public void Init()
        {
            ResetPage();

            if (m_InstantiateItems.Count == 0)
            {
                for (int i = 0; i < InstantiateCount; i++)
                {
                    CreateItem(i);
                }
            }

            HideAllItem();

            m_InstantiateItems.Sort(delegate (RectTransform rect1, RectTransform rect2)
            {
                return int.Parse(rect1.gameObject.name).CompareTo(int.Parse(rect2.gameObject.name));
            });

            // start이전에 StartIndex를 지정하거나, itemCount를 지정했을 때를 위해 호출
            SetSizeContent();
            //MoveToIndex(0);

            if (m_ItemsCount > InstantiateCount)
            {
                for (int i = 0; i < InstantiateCount; i++)
                {
                    ShowItem(i);
                    MoveItemToIndex(i, m_InstantiateItems[i]);
                }
            }
            else
            {
                for (int i = 0; i < m_ItemsCount; i++)
                {
                    ShowItem(i);
                    MoveItemToIndex(i, m_InstantiateItems[i]);
                }
            }

            // 현재 아이템 목록으로 초기화
            previousIds = GetCurrentItemIds();
            onInitFinished?.Invoke();

        }

        private bool IsRowMaxIndex(int i)
        {
#if ASYNC_PER_ROW
        int countPerFrame = 0;
        if (direction == Direction.Vertical)
            countPerFrame = Mathf.RoundToInt(m_Page.y);
        if (direction == Direction.Horizontal)
            countPerFrame = Mathf.RoundToInt(m_Page.x);

        return countPerFrame <= 1 || i % countPerFrame == countPerFrame - 1;
#else
            return true;
#endif
        }

        public void InitLoop()
        {
            HideAllItem();
            Reset();
            // 현재 아이템 목록으로 초기화
            previousIds = GetCurrentItemIds();
        }

        public void SetSizeContent()
        {
            SetBound(GetRectByNum(m_ItemsCount));
        }

        private void ShowItem(int index)
        {
            m_InstantiateItems[index].gameObject.SetActive(true);
        }

        private void HideItem(int index)
        {
            m_InstantiateItems[index].gameObject.SetActive(false);
        }

        private void HideAllItem()
        {
            for (int i = 0; i < InstantiateCount; i++)
            {
                HideItem(i);
            }
        }

        private Vector2 GetChildAnchoredPosition(int index)
        {
            if (m_Direction == Direction.Horizontal)
                return new Vector2(m_StartOffset + m_LeftPadding + Mathf.Floor(index / instantiateSize.x) * CellRect.x, -(index % instantiateSize.x) * CellRect.y);
            else
                return new Vector2(m_LeftPadding + (index % instantiateSize.y) * CellRect.x, -m_StartOffset - Mathf.Floor(index / instantiateSize.y) * CellRect.y);
        }

        private void CreateItem(int index)
        {
            RectTransform item = GameObject.Instantiate(m_Cell);
            item.SetParent(transform, false);
            item.pivot = Vector2.up;
            item.name = $"{index}";
            item.anchoredPosition = GetChildAnchoredPosition(index);
            m_InstantiateItems.Add(item);
            item.gameObject.SetActive(true);

            onCreate?.Invoke(index, item.gameObject);
        }

        protected void RemoveItem(int index)
        {
            RectTransform item = m_InstantiateItems[index];
            m_InstantiateItems.Remove(item);
            RectTransform.Destroy(item.gameObject);

            onRemove?.Invoke(index, item.gameObject);
        }

        public void ClearAll()
        {
            if (m_Rect == null) return;//如果没有被初始化，则不需要清除
            foreach (Transform trans in transform)
            {
                if (trans != m_Cell)
                    Destroy(trans.gameObject);
            }
            m_InstantiateItems = new List<RectTransform>();
            m_Rect.anchoredPosition = Vector2.zero;
        }

        public void Reset()
        {
            if (m_Rect == null) return;
            m_Rect.anchoredPosition = Vector2.zero;
        }

        private Vector2 GetRectByNum(int num)
        {
            return m_Direction == Direction.Horizontal ?
                new Vector2(m_Page.x, Mathf.CeilToInt(num / m_Page.x)) :
                new Vector2(Mathf.CeilToInt(num / m_Page.y), m_Page.y);

        }

        private void SetBound(Vector2 bound)
        {
            if (m_Rect != null)
            {
                if (m_Rect.anchorMin == Vector2.zero && m_Rect.anchorMax == Vector2.one)
                {
                    if (m_Direction == Direction.Horizontal)
                        m_Rect.sizeDelta = new Vector2(bound.y * CellRect.x + m_CellGap.x - GetViewportWidth() + m_EndOffset, 0);
                    else
                        m_Rect.sizeDelta = new Vector2(0, bound.x * CellRect.y + m_CellGap.y - GetViewportHeight() + m_EndOffset);
                }
                else
                {
                    if (m_Direction == Direction.Horizontal)
                        m_Rect.sizeDelta = new Vector2(bound.y * CellRect.x + m_CellGap.x + m_EndOffset, bound.x * CellRect.y + m_CellGap.y);
                    else
                        m_Rect.sizeDelta = new Vector2(bound.y * CellRect.x + m_CellGap.x, bound.x * CellRect.y + m_CellGap.y + m_EndOffset);
                }
            }

        }

        private float GetViewportWidth()
        {
            return m_ScrollRect.viewport.rect.width;
        }

        private float GetViewportHeight()
        {
            return m_ScrollRect.viewport.rect.height;
        }

        protected float MaxPrevPos
        {
            get
            {
                float result;
                Vector2 max = GetRectByNum(m_ItemsCount);
                if (m_Direction == Direction.Horizontal)
                {
                    result = max.y - m_Page.y;
                }
                else
                {
                    result = max.x - m_Page.x;
                }
                return result * CellScale;
            }
        }

        private void Update()
        {
            if (m_ItemsCount == 0) return;

            while (scale * DirectionPos - m_PrevPos < -CellScale)
            {
                if (m_PrevPos <= -MaxPrevPos) return;

                m_PrevPos -= CellScale;

                List<RectTransform> range = m_InstantiateItems.GetRange(0, PageScale);
                m_InstantiateItems.RemoveRange(0, PageScale);
                m_InstantiateItems.AddRange(range);
                for (int i = 0; i < range.Count; i++)
                {
                    MoveItemToIndex(m_CurrentIndex * PageScale + m_InstantiateItems.Count + i, range[i]);
                }
                m_CurrentIndex++;
            }

            while (scale * DirectionPos - m_PrevPos > 0)
            {
                if (Mathf.RoundToInt(m_PrevPos) >= 0) return;

                m_PrevPos += CellScale;

                m_CurrentIndex--;

                if (m_CurrentIndex < 0) return;

                List<RectTransform> range = m_InstantiateItems.GetRange(m_InstantiateItems.Count - PageScale, PageScale);
                m_InstantiateItems.RemoveRange(m_InstantiateItems.Count - PageScale, PageScale);
                m_InstantiateItems.InsertRange(0, range);
                for (int i = 0; i < range.Count; i++)
                {
                    MoveItemToIndex(m_CurrentIndex * PageScale + i, range[i]);
                }
            }
        }

        protected void MoveItemToIndex(int index, RectTransform item)
        {
            item.anchoredPosition = getPosByIndex(index);
            UpdateItem(index, item.gameObject);
            if (IsIdentifiable)
            {
                var id = IdentifyItem(index);
                if (id != UNDEFINED)
                    previousIds[id] = index;
            }
        }

        // id -> index map
        private Dictionary<long, int> previousIds = new Dictionary<long, int>();

        private Dictionary<long, int> GetCurrentItemIds()
        {
            var newIds = new Dictionary<long, int>();
            if (IsIdentifiable)
            {
                for (int i = 0; i < m_InstantiateItems.Count; i++)
                {
                    var id = IdentifyItem(m_CurrentIndex * PageScale + i);
                    if (id != UNDEFINED)
                        newIds[id] = m_CurrentIndex * PageScale + i;
                }
            }
            return newIds;
        }

        // 현재 가지고있는 리스트들을 리프레시
        public void RefreshItem()
        {
            var newIds = GetCurrentItemIds();

            // 제거된 아이템 인덱스
            var removedIndexes = previousIds.Where(p => !newIds.ContainsKey(p.Key)).Select(p => p.Value).ToArray();

            for (int i = 0; i < m_InstantiateItems.Count; i++)
            {
                var index = m_CurrentIndex * PageScale + i;

                long id = UNDEFINED;
                if (IsIdentifiable)
                    id = IdentifyItem(index);

                // 제거된 경우
                if (removedIndexes.Contains(index))
                {
                    // 사라진 아이템
                    var destryGo = Instantiate(m_InstantiateItems[i].gameObject, m_InstantiateItems[i].parent);
                    Destroy(destryGo);
                }

                UpdateItem(index, m_InstantiateItems[i].gameObject);

                if (id != UNDEFINED)
                {
                    // 위치가 변경된 경우
                    if (previousIds.TryGetValue(id, out int oldIndex))
                    {
                        m_InstantiateItems[i].anchoredPosition = getPosByIndex(oldIndex);
                    }
                }
            }

            previousIds = newIds;
        }

        public Vector2 getPosByIndex(int index)
        {
            float x, y;
            Vector2 offset = Vector2.zero;
            if (m_Direction == Direction.Horizontal)
            {
                x = index % m_Page.x;
                y = Mathf.FloorToInt(index / m_Page.x);
                offset.x += m_StartOffset;
            }
            else
            {
                x = Mathf.FloorToInt(index / m_Page.y);
                y = index % m_Page.y;

                offset.y -= m_StartOffset;
            }
            return new Vector2(y * CellRect.x + m_LeftPadding, -x * CellRect.y) + offset;
        }

        private void UpdateItem(int index, GameObject item)
        {
            item.SetActive(index < m_ItemsCount);

            if (item.activeSelf)
            {
                onUpdate?.Invoke(index, item);
                scrollListener?.OnScrolled(GetItemCount(), index);
            }
        }

        private long IdentifyItem(int index)
        {
            if (onIdentify == null) return UNDEFINED;
            if (index >= m_ItemsCount) return UNDEFINED;

            return onIdentify(index);
        }

        protected int GetFirstItemCell()
        {
            return int.Parse(m_InstantiateItems[0].name);
        }

        /// <summary>
        /// 通过总的index得到现在它在哪个编号的格子中
        /// </summary>
        /// <returns></returns>
        public int GetCellIndexByItemIndex(int index)
        {
            if (index < m_CurrentIndex * PageScale)
            {
                return -1;
            }
            else if (index >= m_CurrentIndex * PageScale + InstantiateCount)
            {
                return -1;
            }
            else
            {
                return GetFirstItemCell() + index - m_CurrentIndex * PageScale;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetRealCellIndexByCurCellIndex(int curCell)
        {
            int firtItemCell = GetFirstItemCell();
            int realCell;//实际总的index

            if (curCell >= firtItemCell)
            {
                realCell = m_CurrentIndex * (int)m_Page.y + curCell - firtItemCell;
            }
            else
            {
                realCell = m_CurrentIndex * (int)m_Page.y + InstantiateCount - firtItemCell + curCell;
            }

            return realCell;
        }

        // 루프될 아이템 총 갯수
        public void SetItemCount(int count)
        {
            m_ItemsCount = count;
            SetSizeContent();
            if (m_EmptyGo != null)
                m_EmptyGo.SetActive(count <= 0);
        }

        public int GetItemCount()
        {
            return m_ItemsCount;
        }

        public void MoveToIndex(float index, bool snap = true)
        {
            if (m_ItemsCount > 1)
            {
                if (m_ScrollRect != null)
                {
                    if (m_Direction == Direction.Horizontal)
                        m_ScrollRect.horizontalNormalizedPosition = GetItemNormalizedPos(index, snap);
                    else
                        m_ScrollRect.verticalNormalizedPosition = GetItemNormalizedPos(index, snap);
                }
            }
        }

        private float GetItemNormalizedPos(float index, bool snap)
        {
            if (m_Direction == Direction.Horizontal)
            {
                var itemIndex = snap ? Mathf.FloorToInt(index / m_Page.x) : index / m_Page.x;
                float totalSize;
                if (m_Rect.anchorMin == Vector2.zero && m_Rect.anchorMax == Vector2.one)
                    totalSize = m_Rect.sizeDelta.x;
                else
                    totalSize = m_Rect.sizeDelta.x - GetViewportWidth();
                return Mathf.Clamp01(((CellRect.x * itemIndex) - m_StartOffset) / totalSize);

            }
            else
            {
                var itemIndex = snap ? Mathf.FloorToInt(index / m_Page.y) : index / m_Page.y;
                float totalSize;
                if (m_Rect.anchorMin == Vector2.zero && m_Rect.anchorMax == Vector2.one)
                    totalSize = m_Rect.sizeDelta.y;
                else
                    totalSize = m_Rect.sizeDelta.y - GetViewportHeight();
                return Mathf.Clamp01((totalSize - (CellRect.y * itemIndex) - m_StartOffset) / totalSize);
            }
        }

        public int GetCurrentFirstIndex()
        {
            return (int)(-DirectionPos / CellScale);
        }

        public IEnumerable<RectTransform> GetVisibleItems()
        {
            return m_InstantiateItems.Where(x => x.gameObject.activeSelf);
        }

        public void RefreshChildItems()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                ResetPage();
                for (int i = 0; i < m_InstantiateItems.Count; ++i)
                {
                    var child = m_InstantiateItems[i];
                    child.anchoredPosition = GetChildAnchoredPosition(i);
                }
            }
#endif
        }
    }
}