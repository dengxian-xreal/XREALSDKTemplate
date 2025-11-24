using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// This sample demonstrates how to switch between different tracking modes and input sources.
    /// </summary>
    public class HelloMR : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_TextCurrentMode;
        [SerializeField]
        Toggle m_Toggle0Dof;
        [SerializeField]
        Toggle m_Toggle0DofStable;
        [SerializeField]
        Toggle m_Toggle3Dof;
        [SerializeField]
        Toggle m_Toggle6Dof;
        [SerializeField]
        Button m_ButtonHandInput;

        private void Start()
        {
            m_TextCurrentMode.text = $"Current Mode: {XREALPlugin.GetTrackingType()}";

            XREALPlugin.OnTrackingTypeChanged += OnTrackingTypeChanged;
            m_Toggle0Dof.onValueChanged.AddListener(On0DofToggleChanged);
            m_Toggle0DofStable.onValueChanged.AddListener(On0DofStableToggleChanged);
            m_Toggle3Dof.onValueChanged.AddListener(On3DofToggleChanged);
            m_Toggle6Dof.onValueChanged.AddListener(On6DofToggleChanged);

            InitDofUI();
            m_ButtonHandInput.interactable = XREALPlugin.IsHMDFeatureSupported(XREALSupportedFeature.XREAL_FEATURE_PERCEPTION_HEAD_TRACKING_POSITION);
        }

        private void OnDestroy()
        {
            XREALPlugin.OnTrackingTypeChanged -= OnTrackingTypeChanged;
            m_Toggle0Dof.onValueChanged.RemoveListener(On0DofToggleChanged);
            m_Toggle0DofStable.onValueChanged.RemoveListener(On0DofStableToggleChanged);
            m_Toggle3Dof.onValueChanged.RemoveListener(On3DofToggleChanged);
            m_Toggle6Dof.onValueChanged.RemoveListener(On6DofToggleChanged);
        }

        private void InitDofUI()
        {
            switch (XREALPlugin.GetTrackingType())
            {
                case TrackingType.MODE_0DOF:
                    m_Toggle0Dof.SetIsOnWithoutNotify(true);
                    break;
                case TrackingType.MODE_0DOF_STAB:
                    m_Toggle0DofStable.SetIsOnWithoutNotify(true);
                    break;
                case TrackingType.MODE_3DOF:
                    m_Toggle3Dof.SetIsOnWithoutNotify(true);
                    break;
                case TrackingType.MODE_6DOF:
                    m_Toggle6Dof.SetIsOnWithoutNotify(true);
                    break;
            }
        }

        private void On6DofToggleChanged(bool on)
        {
            if (on)
            {
                _ = XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_6DOF, OnTrackingTypeChanged);
            }
        }

        private void On3DofToggleChanged(bool on)
        {
            if (on)
            {
                _ = XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_3DOF, OnTrackingTypeChanged);
            }
        }

        private void On0DofStableToggleChanged(bool on)
        {
            if (on)
            {
                _ = XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_0DOF_STAB, OnTrackingTypeChanged);
            }
        }

        private void On0DofToggleChanged(bool on)
        {
            if (on)
            {
                _ = XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_0DOF, OnTrackingTypeChanged);
            }
        }

        /// <summary>
        /// Changes the input source to controller.
        /// </summary>
        public void ChangeToControllerInput()
        {
            XREALPlugin.SetInputSource(InputSource.Controller);
        }

        /// <summary>
        /// Changes the input source to hand.
        /// </summary>
        public void ChangeToHandInput()
        {
            XREALPlugin.SetInputSource(InputSource.Hands);
        }

        /// <summary>
        /// Vibrates the controller.
        /// </summary>
        public void Vibrate()
        {
            if (XREALVirtualController.Singleton != null)
                XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, 0.25f, 0.1f);
        }

        private void OnTrackingTypeChanged(bool result, TrackingType targetTrackingType)
        {
            var currentTrackingType = XREALPlugin.GetTrackingType();
            m_TextCurrentMode.text = $"Current Mode: {currentTrackingType}";
        }
    }
}
