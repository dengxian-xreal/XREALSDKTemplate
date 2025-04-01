using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Marker option for shape selection.
    /// </summary>
    public class MarkerShapeOption : MarkerOption
    {
        public ShapeOptionEvent m_ShapeSelectEvent;
        public Mesh m_Mesh;
        [SerializeField]
        private GameObject m_Highlight;

        /// <summary>
        /// Set the selected status.
        /// </summary>
        /// <param name="selected"></param>
        public override void SetSelected(bool selected)
        {
            if (m_Highlight != null)
            {
                m_Highlight.SetActive(selected);
            }
        }

        /// <summary>
        /// Notify the select event with the selected shape.
        /// </summary>
        public override void NotifySelectEvent()
        {
            m_ShapeSelectEvent.Invoke(m_Mesh);
        }
        [Serializable]
        public class ShapeOptionEvent : UnityEvent<Mesh> { }
    }
}
