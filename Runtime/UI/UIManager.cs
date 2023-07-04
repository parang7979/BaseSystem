using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Parang.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourcePath : Attribute
    {
        public string Path { get; private set; }

        public ResourcePath(string path)
        {
            Path = path;
        }
    }

    public class UIManager<U> : Pool<U> where U : MonoBehaviour
    {
        public CanvasGroup Underlay;
        public UIStack Normal;
        public CanvasGroup Fixed;
        public UIStack Overlay;

        public UIStackBehaviour Current => Normal.First;

        protected T Push<T>(UIStack stack, string path) where T : UIStackBehaviour
        {
            var ui = Instantiate<T>(path, stack.transform);
            if (ui != null)
                stack.Push(ui);
            else
                Release(ui);
            return ui;
        }

        protected void Pop(UIStack stack, bool force)
        {
            var ui = stack.Pop(force);
            if (ui == null) return;
            Release(ui);
        }

        protected void Clear(UIStack stack)
        {
            while (true)
            {
                var ui = stack.Pop(true, true);
                if (ui == null) return;
                Release(ui);
            }
        }

        protected void Clear<T>(UIStack stack) where T : UIStackBehaviour
        {
            while (true)
            {
                var ui = stack.Pop(true, true);
                if (ui == null) return;
                if (stack.First is T)
                {
                    stack.First.Restore(ui);
                    Release(ui);
                    return;
                }
                Release(ui);
            }
        }

        public async UniTask<T> Push<T>() where T : UIStackBehaviour
        {
            var attr = (ResourcePath[])typeof(T).GetCustomAttributes(typeof(ResourcePath), false);
            if (attr.Length <= 0) return null;
            return await Push<T>(attr[0].Path);
        }

        /// <summary>
        /// 일반 UI를 적재합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<T> Push<T>(string path) where T : UIStackBehaviour
        {
            await LoadPool(path);
            return Push<T>(Normal, path);
        }

        /// <summary>
        /// 스택에서 UI를 하나 제거합니다.
        /// </summary>
        virtual public void Pop(bool force = false)
        {
            Pop(Normal, force);
        }

        /// <summary>
        /// 스택 가장 처음이 팝업이라면 Result를 전달합니다.
        /// </summary>
        /// <param name="result"></param>
        public void Result(PopupResult result)
        {
            var popup = Normal.First as UIPopupBehaviour;
            if (popup != null) popup.Result(result);
        }

        /// <summary>
        /// 스택을 초기화 합니다
        /// </summary>
        public void Clear()
        {
            Clear(Normal);
        }

        /// <summary>
        /// 특정 타입의 UI 까지 스택을 초기화 합니다
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>() where T : UIStackBehaviour
        {
            Clear<T>(Normal);
        }

        public async UniTask<T> PushOverlay<T>() where T : UIStackBehaviour
        {
            var attr = (ResourcePath[])typeof(T).GetCustomAttributes(typeof(ResourcePath), false);
            if (attr.Length <= 0) return null;
            return await PushOverlay<T>(attr[0].Path);
        }

        /// <summary>
        /// 다른 UI 위에 오버레이되는 특수한 UI 입니다.
        /// 한번에 하나씩만 적재할수 있습니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        protected async UniTask<T> PushOverlay<T>(string path) where T : UIStackBehaviour
        {
            if (Overlay.Empty)
            {
                await LoadPool(path);
                return Push<T>(Overlay, path);
            }
            return Overlay.First as T;
        }

        public void PopOverlay<T>(bool force = false) where T : UIStackBehaviour
        {
            if (Overlay.First is T)
                Pop(Overlay, force);
        }

        protected void ClearOverlay()
        {
            Clear(Overlay);
        }

        public void ClearAll()
        {
            Clear(Normal);
            Clear(Overlay);
        }

        public void Lock(bool _lock)
        {
            Normal.Interactable = !_lock;
            Underlay.interactable = !_lock;
        }

        virtual protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Normal.First != null)
                {
                    Pop();
                }
            }
        }
    }
}
