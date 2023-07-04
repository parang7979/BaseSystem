using UnityEngine;

namespace Parang
{
    public class PoolBehaviour : MonoBehaviour
    {
        public string Path { get; set; }
        public long TimeStamp { get; set; }
        public bool IsReleased { get; set; }

        private Vector3 localPosition;
        private Quaternion localRotation;
        private Vector3 localScale;

        virtual public void OnLoadPool()
        {

        }

        virtual public void SaveTransform()
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
        }

        virtual public void CopyFrom(PoolBehaviour source)
        {
            localPosition = source.localPosition;
            localRotation = source.localRotation;
            localScale = source.localScale;
        }

        virtual public void ResetTransform()
        {
            gameObject.SetActive(true);
            transform.ResetLocal();
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;
        }
    }
}