using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.XR.XREAL.Samples
{
    public class XREALLaser : MonoBehaviour
    {
        private const float StillTime = 3.0f;

        [SerializeField]
        private Transform m_InteractorTransform;
        [SerializeField]
        private Transform m_AttachTransform;
        public Transform AttachTransform { get => m_AttachTransform; }

        [SerializeField]
        private Transform m_RayOriginTransform;
        public Transform RayOriginTransform { get => m_RayOriginTransform; }

        [SerializeField]
        private XREALLaserVisual m_LaserVisual;
        [SerializeField]
        private XREALLaserReticle m_Reticle;
        [SerializeField]
        private InputActionProperty m_TriggerAction;
        [SerializeField]
        private LayerMask m_InteractionLayerMask = 1;
        [SerializeField]
        private float m_MaxRaycastDistance = 10;

        XRUIInputModule m_InputModule;
        IUIInteractor m_Interactor;

        private float m_NoMovementTimer = 0;
        private float m_MovementThreshold = 0.3f;
        private Quaternion m_MovementRotation = Quaternion.identity;
        private bool m_Moveable = true;
        private bool m_TriggerTouched = false;

        private RaycastHit m_RaycastHit;
        private RaycastResult m_RaycastResult;
        private bool m_RaycastValid = false;
        private bool m_IsUIHitClosest;

        private IEnumerator Start()
        {
            float timer = 0;
            while (timer < 5.0f && m_InteractorTransform == null)
            {
                yield return new WaitForSeconds(0.5f);
                timer += 0.5f;
                GameObject controller = GameObject.Find("Right Controller");
                if (controller != null)
                {
                    m_InteractorTransform = controller.transform.Find("Ray Interactor");
                    if (m_InteractorTransform == null)
                    {
                        m_InteractorTransform = controller.transform.Find("Near-Far Interactor");
                    }
                }
            }

            if (m_InteractorTransform != null)
            {
                if (m_AttachTransform == null)
                    m_AttachTransform = m_InteractorTransform.transform;
                if (m_RayOriginTransform == null)
                    m_RayOriginTransform = m_InteractorTransform.transform;
                m_Interactor = m_InteractorTransform.GetComponent<IUIInteractor>();
            }

            var eventSystem = EventSystem.current;
            if (!eventSystem.TryGetComponent(out m_InputModule))
                Debug.LogError("[XREAL] not find XRUIInputModule");

            if (m_TriggerAction.action != null)
            {
                m_TriggerAction.action.performed += TriggerActionPerformed;
                m_TriggerAction.action.canceled += TriggerActionCanceled;
            }
        }

        private void OnDisable()
        {
            if (m_TriggerAction.action != null)
            {
                m_TriggerAction.action.performed -= TriggerActionPerformed;
                m_TriggerAction.action.canceled -= TriggerActionCanceled;
            }
        }
        private void Update()
        {
            bool interactorObjActive = m_InteractorTransform != null ? m_InteractorTransform.gameObject.activeInHierarchy : false;
            if (!interactorObjActive)
            {
                m_Moveable = true;
                m_NoMovementTimer = 0;
            }

            bool isEnable = m_Moveable && interactorObjActive;
            if (m_LaserVisual != null)
                m_LaserVisual.gameObject.SetActive(isEnable);
            if (m_Reticle != null)
                m_Reticle.gameObject.SetActive(isEnable);

            if (m_Interactor != null && m_InputModule != null)
            {
                m_InputModule.GetTrackedDeviceModel(m_Interactor, out TrackedDeviceModel model);
                m_RaycastResult = model.currentRaycast;
                m_RaycastValid = model.currentRaycast.isValid;
            }
            else
            {
                m_RaycastResult = default;
                m_RaycastValid = false;
            }

            if (m_RayOriginTransform == null || m_AttachTransform == null)
            {
                m_Moveable = false;
                m_RaycastHit = default;
                m_IsUIHitClosest = true;
                return;
            }

            bool raycastValid = Physics.Raycast(m_RayOriginTransform.position, m_AttachTransform.forward, out m_RaycastHit, m_MaxRaycastDistance, m_InteractionLayerMask);
            if (raycastValid)
            {
                if (m_RaycastValid)
                {
                    m_IsUIHitClosest = m_RaycastResult.distance <= m_RaycastHit.distance;
                }
                else
                {
                    m_IsUIHitClosest = false;
                }
            }
            else
            {
                m_IsUIHitClosest = true;
            }
            m_RaycastValid |= raycastValid;

            if (Quaternion.Angle(m_AttachTransform.rotation, m_MovementRotation) >= m_MovementThreshold || m_TriggerTouched)
            {
                m_MovementRotation = m_AttachTransform.rotation;
                m_Moveable = true;
                m_NoMovementTimer = 0;
            }
            else
            {
                if (m_Moveable)
                {
                    m_NoMovementTimer += Time.deltaTime;
                    if (m_NoMovementTimer >= StillTime)
                    {
                        m_Moveable = false;
                        m_MovementRotation = m_AttachTransform.rotation;
                    }
                }
                else if (Time.frameCount % 30 == 0)
                {
                    m_MovementRotation = m_AttachTransform.rotation;
                }
            }
        }

        public bool GetCurrentRaycast(out RaycastHit raycastHit, out RaycastResult raycastResult, out bool isUIHitClosest)
        {
            raycastHit = m_RaycastHit;
            raycastResult = m_RaycastResult;
            isUIHitClosest = m_IsUIHitClosest;
            return m_RaycastValid;
        }

        private void TriggerActionPerformed(InputAction.CallbackContext context)
        {
            m_TriggerTouched = true;
        }

        private void TriggerActionCanceled(InputAction.CallbackContext context)
        {
            m_TriggerTouched = false;
        }
    }
}