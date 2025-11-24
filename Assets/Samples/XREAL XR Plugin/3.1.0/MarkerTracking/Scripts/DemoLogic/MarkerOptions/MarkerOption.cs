using UnityEngine;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Base class for marker options.
    /// A marker option is a UI element that show which marker is selected.
    /// When a marker is selected, it will send a select event to the listener.
    /// </summary>
    public abstract class MarkerOption : MonoBehaviour
    {

        const string TAG = "MarkerOption";
        #region events
        public abstract void NotifySelectEvent();
        #endregion

        #region settings
        [SerializeField]
        private int m_Index;
        #endregion

        public int Index => m_Index;

        public virtual void SetSelected(bool selected)
        {
        }

        public void Select()
        {
            Debug.Log($"[{TAG}] Select index={m_Index}");
            SetSelected(true);
            NotifySelectEvent();
        }

        public void Deselect()
        {
            Debug.Log($"[{TAG}] Deselect index={m_Index}");
            SetSelected(false);
        }

        #region unity messages

        private void Awake()
        {
            SetSelected(false);
        }
        #endregion

    }
}
