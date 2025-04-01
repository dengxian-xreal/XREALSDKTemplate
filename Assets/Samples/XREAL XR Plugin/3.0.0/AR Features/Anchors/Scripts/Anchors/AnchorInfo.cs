using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// The AnchorInfo class manages AR anchor-related information. It also provides functions to save, remove, erase, and remap anchors for persistent storage 
    /// </summary>
    public class AnchorInfo : MonoBehaviour
    {
        #region settings
        [SerializeField]
        ARAnchorManager m_AnchorManager;
        [SerializeField]
        GameObject m_Panel;
        [SerializeField]
        Anchors m_Anchors;
        private ARAnchor m_Anchor;
        public ARAnchor Anchor
        {
            get
            {
                if (m_Anchor == null)
                {
                    m_Anchor = GetComponent<ARAnchor>();
                }
                return m_Anchor;
            }
        }
        [SerializeField]
        TMP_Text m_InfoText;
        [SerializeField]
        Button m_BtnSave;
        [SerializeField]
        Button m_BtnRemove;
        [SerializeField]
        Button m_BtnErase;
        [SerializeField]
        Button m_BtnRemap;

        [SerializeField]
        InputActionReference m_RightControllerPositionAction;
        [SerializeField]
        InputActionReference m_RightControllerRotationAction;
        [SerializeField]
        InputActionReference m_RightControllerPressAction;
        #endregion

        #region states
        private bool m_Located = true;
        public bool Located => m_Located;
        #endregion

        private void Awake()
        {
            m_AnchorManager = XREALUtility.FindAnyObjectByType<ARAnchorManager>();
            m_Anchors = XREALUtility.FindAnyObjectByType<Anchors>();
            m_RightControllerPressAction.action.performed += PressAction_performed;
        }

        public void WaitForLocating()
        {
            m_Panel.SetActive(false);
            m_Located = false;
        }

        private void PressAction_performed(InputAction.CallbackContext obj)
        {
            if (!m_Located)
            {
                m_Located = true;
                if (m_Anchor == null)
                {
                    m_Anchor = gameObject.AddComponent<ARAnchor>();
                }
                Anchor.enabled = true;
            }
        }

        void Start()
        {
            m_BtnSave.onClick.AddListener(OnSaveClick);
            m_BtnRemove.onClick.AddListener(OnRemoveClick);
            m_BtnErase.onClick.AddListener(OnEraseClick);
            m_BtnRemap.onClick.AddListener(OnRemapClick);
        }

        public void ShowPanel()
        {
            m_Panel.SetActive(true);
        }

        private void OnRemapClick()
        {
            Debug.Log($"[AnchorInfo] OnRemapClick begin");
            if (m_AnchorManager.subsystem is XREALAnchorSubsystem subsystem)
            {
                bool success = m_AnchorManager.TryRemap(Anchor.trackableId);
                MapQualityIndicator.SetCurrentAnchor(Anchor, true);
                MapQualityIndicator.ShowMappingGuide();
                Debug.Log($"[AnchorInfo] OnRemapClick result={success}");
            }
        }
        private async void OnEraseClick()
        {
            bool result = await m_Anchors.EraseAnchor(Anchor.trackableId);
            Debug.Log($"[AnchorInfo] OnEraseClick result={result}");

        }

        private void OnRemoveClick()
        {
            GameObject.Destroy(gameObject);
            MapQualityIndicator.InterruptMappingGuide();
        }

        private async void OnSaveClick()
        {

            MapQualityIndicator.PauseEstimateQuality();
#if UNITY_6000_0_OR_NEWER
        Debug.Log($"[AnchorInfo] OnSaveClick begin");
        var result = await m_AnchorManager.TrySaveAnchorAsync(m_Anchor);
        Debug.Log($"[AnchorInfo] OnSaveClick result={result.status.statusCode} guid={result.value} ");
        if (result.status.statusCode == UnityEngine.XR.ARSubsystems.XRResultStatus.StatusCode.UnqualifiedSuccess)
        {
            MapQualityIndicator.FinishMappingGuide();
        }
        else
        {
            GameObject.Destroy(gameObject);
            MapQualityIndicator.InterruptMappingGuide();
        }
#else
            var result = await m_AnchorManager.TrySaveAnchorAsync(Anchor);
            Debug.Log($"[AnchorInfo] OnSaveClick result={result.success} {result.value}");
            if (result.success)
            {
                m_Anchors.AddSavedAnchorEntry(Anchor.trackableId, result.value);
                MapQualityIndicator.FinishMappingGuide();
            }
            else
            {
                GameObject.Destroy(gameObject);
                MapQualityIndicator.InterruptMappingGuide();
            }
#endif
        }

        void Update()
        {
            if (!m_Located)
            {
                XROrigin xrOrig = XREALUtility.FindAnyObjectByType<XROrigin>();
                var pos = m_RightControllerPositionAction.action.ReadValue<Vector3>();
                var rot = m_RightControllerRotationAction.action.ReadValue<Quaternion>();
                transform.position = xrOrig.CameraFloorOffsetObject.transform.TransformPoint(pos + rot * Vector3.forward * 1.5f);
                transform.rotation = rot;

            }
            else
            {
                if (Anchor != null)
                {
                    m_InfoText.text = ($" Status {Anchor.trackingState}\n Coord {Anchor.transform.position}\n sessionID {Anchor.sessionId}\n TrackableID {Anchor.trackableId}");
                }
            }
        }
        private void OnDestroy()
        {
            Debug.Log($"[AnchorInfo] Destroyed");
        }

        internal async Task WaitUntilLocated()
        {
            await Task.Run(() =>
            {
                while (!m_Located)
                {
                    Thread.Sleep(100);
                }
            });
        }
    }
}
