using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.XR.XREAL.Samples
{

    /// <summary>
    /// Marker option for transform selection.
    /// </summary>
    public class MarkerTransformOption : MarkerOption
    {
        public TransformOptionEvent m_TransformSelectEvent;
        public Transform m_Transform;

        public MeshFilter m_PlaneMeshFilter;
        public Mesh m_PlaneMesh;

        /// <summary>
        /// Notify the select event with the selected transform.
        /// </summary>
        public override void NotifySelectEvent()
        {
            m_TransformSelectEvent.Invoke(m_Transform);
        }

        /// <summary>
        /// Set the selected status.
        /// </summary>
        /// <param name="selected"></param>
        public override void SetSelected(bool selected)
        {
            base.SetSelected(selected);
            if (selected)
            {
                m_PlaneMeshFilter.mesh = m_PlaneMesh;
            }
        }

        [Serializable]
        public class TransformOptionEvent : UnityEvent<Transform> { }
    }
}
