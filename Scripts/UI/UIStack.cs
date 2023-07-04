using System.Collections.Generic;
using UnityEngine;

namespace Parang.UI
{
    public class UICanvas : MonoBehaviour
    {
        public float Alpha
        {
            get { return group.alpha; }
            set { group.alpha = value; }
        }
        public bool Interactable
        {
            get { return group.interactable; }
            set { group.interactable = value; }
        }

        protected CanvasGroup group;

        protected void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }
    }

    public class UIStack : UICanvas
    {
        public bool Empty => stack.Count == 0;
        public int Count => stack.Count;
        public UIStackBehaviour First => !Empty ? stack.Peek() : null;

        private readonly Stack<UIStackBehaviour> stack = new();

        public void Push(UIStackBehaviour next)
        {
            if (stack.Count > 0)
            {
                var head = stack.Peek();
                head.gameObject.SetActive(next is not UIRootBehaviour);
                head.Store(next);
            }
            stack.Push(next);
            next.Push();
        }

        public UIStackBehaviour Pop(bool forcePop = false, bool forceRestore = false)
        {
            if (stack.Count == 0) return null;
            var prev = stack.Peek();
            if (prev is UIPopupBehaviour)
            {
                // 팝업일때
                if (!forcePop)
                {
                    if (!prev.Pop()) return null;
                }
                var popup = prev as UIPopupBehaviour;
                // 스택에서 뺴주고 결과값을 넣어야 중복 Close를 막을수 있다.
                stack.Pop();
                popup.Result(PopupResult.Cancel);
            }
            else
            {
                if (!forcePop)
                {
                    if (!prev.Pop()) return null;
                }
                stack.Pop();
            }
            if (stack.Count == 0) return prev;
            var head = stack.Peek();
            head.gameObject.SetActive(true);
            if (!forceRestore)
            {
                head.Restore(prev);
            }
            return prev;
        }
    }
}
