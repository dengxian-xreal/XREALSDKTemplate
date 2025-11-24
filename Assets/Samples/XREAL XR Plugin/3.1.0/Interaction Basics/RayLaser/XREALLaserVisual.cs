using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.XR.XREAL.Samples
{
    public class XREALLaserVisual : MonoBehaviour
    {
        /// <summary> The raycaster. </summary>
        [SerializeField]
        private XREALLaser m_Laser;
        /// <summary> The line renderer. </summary>
        [SerializeField]
        private LineRenderer m_LineRenderer;
        /// <summary> Serialized field to control whether the line should be forcibly hidden. </summary>
        [SerializeField]
        private bool m_ForceHideLine = false;

        /// <summary> Public property to get or set the force hide line flag. </summary>
        public bool ForceHideLine
        {
            get => m_ForceHideLine;
            set => m_ForceHideLine = value;
        }

        /// <summary> True to show, false to hide the on hit only. </summary>
        public bool showOnHitOnly;
        /// <summary> The default distance. </summary>
        public float defaultDistance = 1.2f;

        private void Awake()
        {
            if (m_LineRenderer == null)
                m_LineRenderer = GetComponentInChildren<LineRenderer>(true);
        }

        protected virtual void LateUpdate()
        {
            if (m_ForceHideLine)
            {
                m_LineRenderer.enabled = false;
                return;
            }

            if (m_Laser != null)
            {
                bool hitResult = m_Laser.GetCurrentRaycast(out RaycastHit hit, out RaycastResult raycastResult, out bool isUIHitClosest);
                Vector3 startPoint = m_Laser.RayOriginTransform != null ? m_Laser.RayOriginTransform.position : Vector3.zero;
                Vector3 endPoint = Vector3.forward;

                if (hitResult)
                {
                    if (isUIHitClosest)
                    {
                        endPoint = raycastResult.worldPosition;
                    }
                    else
                    {
                        endPoint = hit.point;
                    }
                }
                else if (m_Laser.RayOriginTransform != null && m_Laser.AttachTransform != null)
                {
                    endPoint = m_Laser.RayOriginTransform.position + m_Laser.AttachTransform.forward * defaultDistance;
                }

                if (showOnHitOnly && !hitResult)
                {
                    m_LineRenderer.enabled = false;
                    return;
                }
                m_LineRenderer.enabled = true;
                m_LineRenderer.useWorldSpace = false;
                m_LineRenderer.positionCount = 2;
                m_LineRenderer.SetPosition(0, transform.InverseTransformPoint(startPoint));
                m_LineRenderer.SetPosition(1, transform.InverseTransformPoint(endPoint));
            }
        }

        protected virtual void OnDisable()
        {
            m_LineRenderer.enabled = false;
        }
    }

}
