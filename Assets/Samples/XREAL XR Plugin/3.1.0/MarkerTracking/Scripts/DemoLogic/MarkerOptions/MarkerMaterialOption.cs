using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Marker option for material selection.
    /// </summary>
    public class MarkerMaterialOption : MarkerOption
    {
        public MaterialOptionEvent m_MaterialSelectEvent;
        public Material m_Material;
        [SerializeField]
        private Vector3 normalPos;
        [SerializeField]
        private Vector3 highlightPos;
        [SerializeField]
        private Transform m_ArrowTrans;
        [SerializeField]
        private Vector3 m_ArrowEulerAngle;

        /// <summary>
        /// Notify the select event with the selected material.
        /// </summary>
        public override void NotifySelectEvent()
        {
            m_MaterialSelectEvent.Invoke(m_Material);
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
                transform.localPosition = highlightPos;
                m_ArrowTrans.localRotation = Quaternion.Euler(m_ArrowEulerAngle);
            }
            else
            {
                transform.localPosition = normalPos;
            }
        }
        [Serializable]
        public class MaterialOptionEvent : UnityEvent<Material> { }
    }
}
