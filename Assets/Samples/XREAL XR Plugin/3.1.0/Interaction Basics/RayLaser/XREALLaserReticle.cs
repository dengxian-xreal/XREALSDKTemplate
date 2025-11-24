using UnityEngine;
using UnityEngine.EventSystems;
namespace Unity.XR.XREAL.Samples
{
    public class XREALLaserReticle : MonoBehaviour
    {
        /// <summary> Values that represent reticle states. </summary>
        public enum ReticleState
        {
            /// <summary> An enum constant representing the hide option. </summary>
            Hide,
            /// <summary> An enum constant representing the normal option. </summary>
            Normal,
            /// <summary> An enum constant representing the hover option. </summary>
            Hover,
        }
        /// <summary> The raycaster. </summary>
        [SerializeField]
        private XREALLaser m_Laser;
        /// <summary> The default visual. </summary>
        [SerializeField]
        private GameObject m_DefaultVisual;
        /// <summary> The hover visual. </summary>
        [SerializeField]
        private GameObject m_HoverVisual;
        /// <summary> Flag to indicate whether the reticle object should be forcibly hidden </summary>
        [SerializeField]
        private bool m_ForceHideReticle = false;
        /// <summary> The hit target. </summary>
        private GameObject m_HitTarget;
        /// <summary> True if is visible, false if not. </summary>
        private bool m_IsVisible = true;

        /// <summary> The default distance. </summary>
        public float defaultDistance = 2.5f;
        /// <summary> The reticle size ratio. </summary>
        public float reticleSizeRatio = 0.02f;

        [SerializeField]
        private Camera m_MainCamera;
        /// <summary> Gets the main camera. </summary>
        private Camera MainCamera
        {
            get => m_MainCamera;
        }

        public bool ForceHideReticle
        {
            get => m_ForceHideReticle;
            set => m_ForceHideReticle = value;
        }

        private void Awake()
        {
            if (m_MainCamera == null)
                m_MainCamera = Camera.main;

            SwitchReticleState(ReticleState.Hide);
        }

        void Start()
        {
            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
            }
        }

        protected virtual void LateUpdate()
        {
            if (m_ForceHideReticle || m_Laser == null || m_Laser.RayOriginTransform == null || m_Laser.AttachTransform == null)
            {
                SwitchReticleState(ReticleState.Hide);
                return;
            }

            Vector3 position = m_Laser.RayOriginTransform.position + m_Laser.AttachTransform.forward * defaultDistance;
            Quaternion rotation = m_Laser.AttachTransform.rotation;
            var result = m_Laser.GetCurrentRaycast(out RaycastHit hit, out RaycastResult raycastResult, out bool isUIHitClosest);
            if (result)
            {
                SwitchReticleState(ReticleState.Hover);
                if (isUIHitClosest)
                {
                    position = raycastResult.worldPosition;
                    rotation = Quaternion.LookRotation(raycastResult.worldNormal, m_Laser.AttachTransform.forward);
                }
                else
                {
                    position = hit.point;
                    rotation = Quaternion.LookRotation(hit.normal, m_Laser.AttachTransform.forward);
                }
            }
            else
            {
                SwitchReticleState(ReticleState.Normal);
            }
            transform.position = position;
            transform.rotation = rotation;

            if (MainCamera)
                transform.localScale = Vector3.one * reticleSizeRatio * (transform.position - MainCamera.transform.position).magnitude;
        }

        private void OnDisable()
        {
            SwitchReticleState(ReticleState.Hide);
        }

        /// <summary> Switch reticle state. </summary>
        /// <param name="state"> The state.</param>
        private void SwitchReticleState(ReticleState state)
        {
            switch (state)
            {
                case ReticleState.Hide:
                    m_DefaultVisual.SetActive(false);
                    m_HoverVisual.SetActive(false);
                    break;
                case ReticleState.Normal:
                    m_DefaultVisual.SetActive(true);
                    m_HoverVisual.SetActive(false);
                    break;
                case ReticleState.Hover:
                    m_DefaultVisual.SetActive(false);
                    m_HoverVisual.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        /// <summary> Sets a visible. </summary>
        /// <param name="isVisible"> True if is visible, false if not.</param>
        public void SetVisible(bool isVisible)
        {
            this.m_IsVisible = isVisible;
        }
    }
}
