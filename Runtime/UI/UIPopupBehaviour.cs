using Cysharp.Threading.Tasks;

namespace Parang.UI
{
    public enum PopupResult
    {
        Ok,
        Cancel,
    }

    public class UIPopupBehaviour : UIStackBehaviour
    {
        private UniTaskCompletionSource<PopupResult> cs;

        public override void Push()
        {
            cs = new UniTaskCompletionSource<PopupResult>();
            base.Push();
        }

        public UniTask<PopupResult> Wait()
        {
            return cs.Task;
        }

        virtual public void Result(PopupResult result)
        {
            if (cs != null)
            {
                cs.TrySetResult(result);
                cs = null;
            }
        }
    }
}