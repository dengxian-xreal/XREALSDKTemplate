using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// This class is used to adjust the focus plane of the XR display subsystem.
    /// </summary>
    public class FocusPlanePanel : MonoBehaviour
    {
        public bool ajustFocusPlaneNormal = false;

        [SerializeField]
        Text m_FocusDistance;
        [SerializeField]
        Text m_FocusPoint;
        [SerializeField]
        Text m_FocusNormal;

        Transform m_Camera;
        RaycastHit m_HitResult;

        void Start()
        {
            m_Camera = XREALUtility.MainCamera.transform;
        }

        void Update()
        {
            if (Physics.Raycast(new Ray(m_Camera.position, m_Camera.forward), out m_HitResult, 100))
            {
                Vector3 focusPoint = m_Camera.InverseTransformPoint(m_HitResult.point);
                Vector3 normal = ajustFocusPlaneNormal ? m_Camera.InverseTransformDirection(m_HitResult.normal) : Vector3.back;

                if (m_FocusDistance != null)
                    m_FocusDistance.text = focusPoint.magnitude.ToString("F2");
                if (m_FocusPoint != null)
                    m_FocusPoint.text = focusPoint.ToString();
                if (m_FocusNormal != null)
                    m_FocusNormal.text = normal.ToString();

                SetFocusPlane(focusPoint, normal);
            }
        }

        void SetFocusPlane(Vector3 point, Vector3 normal)
        {
            XREALUtility.GetLoadedSubsystem<XRDisplaySubsystem>()?.SetFocusPlane(point, normal, Vector3.zero);
        }

        void OnDrawGizmos()
        {
            if (m_HitResult.collider != null)
            {
                Gizmos.DrawSphere(m_HitResult.point, 0.05f);
                Gizmos.DrawLine(m_Camera.position, m_HitResult.point);
            }
        }
    }
}
