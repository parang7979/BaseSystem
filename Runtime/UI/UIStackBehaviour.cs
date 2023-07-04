using System.Collections.Generic;

namespace Parang.UI
{
    public class UIStackBehaviour : PoolBehaviour
    {
        private List<LocalizeTMP> _localizeTexts;

        virtual protected void Awake()
        {
            _localizeTexts = new List<LocalizeTMP>();
            transform.GetComponentsRecursively(ref _localizeTexts);
        }

        virtual public void Localize()
        {
            foreach (var l in _localizeTexts)
                l.Localize();
        }

        virtual public void Push()
        {
        }

        virtual public void Store(UIStackBehaviour next)
        {
        }

        virtual public void Restore(UIStackBehaviour prev)
        {
        }

        virtual public bool Pop()
        {
            return true;
        }
    }
}