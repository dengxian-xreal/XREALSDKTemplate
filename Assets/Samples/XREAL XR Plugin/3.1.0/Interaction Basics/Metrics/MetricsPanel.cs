using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// This class is used to display the metrics of the XR display subsystem.
    /// </summary>
    public class MetricsPanel : MonoBehaviour
    {
        [SerializeField]
        Text m_MotionToPhotonText, m_DroppedFrameText, m_FramePresentText, m_DisplayRefreshRateText,
             m_ExtendedFrameText, m_EarlyFrameText, m_TearedFrameText;

        XRDisplaySubsystem m_DisplaySubsystem;
        bool m_EnableTearedFrameCount = false;
        bool m_EnableRenderBackColor = false;

        void Start()
        {
            m_DisplaySubsystem = XREALUtility.GetLoadedSubsystem<XRDisplaySubsystem>();
            if (m_DisplaySubsystem == null)
                enabled = false;
        }

        void Update()
        {
            if (m_DisplaySubsystem.TryGetMotionToPhoton(out float motionToPhoton))
            {
                m_MotionToPhotonText.text = string.Format("{0:F2}ms", motionToPhoton * 1000);
            }

            if (m_DisplaySubsystem.TryGetDroppedFrameCount(out int droppedFrameCount))
            {
                m_DroppedFrameText.text = droppedFrameCount.ToString();
            }

            if (m_DisplaySubsystem.TryGetFramePresentCount(out int framePresentCount))
            {
                m_FramePresentText.text = framePresentCount.ToString();
            }

            if (m_DisplaySubsystem.TryGetDisplayRefreshRate(out float displayRefreshRate))
            {
                m_DisplayRefreshRateText.text = displayRefreshRate.ToString();
            }

            if (UnityEngine.XR.Provider.XRStats.TryGetStat(m_DisplaySubsystem, "ExtendedFrameCount", out float extendedFrameCount))
            {
                m_ExtendedFrameText.text = extendedFrameCount.ToString();
            }

            if (UnityEngine.XR.Provider.XRStats.TryGetStat(m_DisplaySubsystem, "EarlyFrameCount", out float earlyFrameCount))
            {
                m_EarlyFrameText.text = earlyFrameCount.ToString();
            }

            if (UnityEngine.XR.Provider.XRStats.TryGetStat(m_DisplaySubsystem, "TearedFrameCount", out float tearedFrameCount))
            {
                m_TearedFrameText.text = tearedFrameCount.ToString();
            }
        }

        /// <summary>
        /// Enable or disable the teared frame count.
        /// </summary>
        public void EnableTearedFrameCount()
        {
            m_EnableTearedFrameCount = !m_EnableTearedFrameCount;
            m_DisplaySubsystem.EnableTearedFrameCount(m_EnableTearedFrameCount);
        }

        /// <summary>
        /// Enable or disable the render back color.
        /// </summary>
        public void EnableRenderBackColor()
        {
            m_EnableRenderBackColor = !m_EnableRenderBackColor;
            m_DisplaySubsystem.EnableRenderBackColor(m_EnableRenderBackColor);
        }
    }
}
