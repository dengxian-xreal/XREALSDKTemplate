using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// This class indicates AR map quality by placing quality bars around AR anchors
    /// and estimating mapping quality based on the user's viewpoint.
    /// </summary>
    public class MapQualityIndicator : SingletonMonoBehaviour<MapQualityIndicator>
    {

        public event Action<ARAnchor> OnMappingGuideStart;
        public event Action<ARAnchor> OnMappingGuideStop;
        #region settings
        [SerializeField]
        private IndicatorSettings m_Settings;

        [SerializeField]
        private Camera m_Camera;

        [SerializeField]
        private GameObject m_qualityBarPrefab;

        private ARAnchorManager m_ARAnchorManager;
        private List<MapQualityBar> m_bars;

        /// <summary> Direction of the quality bar ring. </summary>
        private Vector3 m_ringForward;

        /// <summary> Direction from the anchor to the first quality bar. </summary>
        private Vector3 m_startDir;
        #endregion

        #region state

        private bool m_Paused = false;
        private ARAnchor m_currentWorldAnchor;
        private float nextEstimateTime;
        public static IndicatorSettings Settings => Singleton.m_Settings;
        public static ARAnchor CurrentAnchor => Singleton.m_currentWorldAnchor;

        private List<MapQualityBar> bars
        {
            get
            {
                if (m_bars == null || m_bars.Count != Settings.barCount)
                {
                    m_bars = initalizeBars();
                }
                return m_bars;
            }
        }

        /// <summary>
        /// Pose used to estimate mapping quality
        /// </summary>
        private Pose estimatePose
        {
            get
            {
                return new Pose(m_Camera.transform.position, m_Camera.transform.rotation);
            }
        }
        #endregion

        #region public

        /// <summary>
        /// Set the current AR anchor and decide whether to remap.
        /// </summary>
        public static void SetCurrentAnchor(ARAnchor anchor, bool isRemapping = true)
        {
            Debug.Log($"[MapQualityIndicator] SetCurrentAnchor {anchor.sessionId} {isRemapping}");
            if (Singleton.m_currentWorldAnchor == anchor)
            {
                return;
            }
            Singleton.setCurrentAnchor(anchor, isRemapping);
        }

        /// <summary> Show mapping guidance. </summary>
        public static void ShowMappingGuide()
        {
            Singleton.showMappingGuide();
            Singleton.OnMappingGuideStart?.Invoke(CurrentAnchor);
        }

        /// <summary> Pause quality estimation. </summary>
        public static void PauseEstimateQuality()
        {
            Singleton.pauseEstimateQuality();
        }

        /// <summary> Interrupt mapping guidance. </summary>
        public static void InterruptMappingGuide()
        {
            Singleton.interruptMappingGuide();
            Singleton.OnMappingGuideStop?.Invoke(CurrentAnchor);

        }

        /// <summary> Complete mapping guidance. </summary>
        public static void FinishMappingGuide()
        {
            Singleton.finishMappingGuide();
            Singleton.OnMappingGuideStop?.Invoke(CurrentAnchor);
        }
        #endregion

        #region unity messages
        protected override void Awake()
        {
            base.Awake();
            m_Camera = XREALUtility.MainCamera;
            m_ARAnchorManager = XREALUtility.FindAnyObjectByType<ARAnchorManager>();
        }


        protected void Update()
        {
            if (m_currentWorldAnchor == null)
            {
                return;
            }

            if (m_Paused)
            {
                return;
            }

            if (Time.time > nextEstimateTime)
            {

                int index = computeBarIndex();

                if (index < 0 || index >= m_bars.Count)
                {
                    return;
                }

                var visitedBar = bars[index];
                visitedBar.IsVisited = true;
                Debug.Log($"[MapQualityIndicator] GetAnchorQuality");
                XREALAnchorEstimateQuality quality = m_ARAnchorManager.GetAnchorQuality(m_currentWorldAnchor.trackableId, estimatePose);
                visitedBar.QualityState = quality;
                nextEstimateTime = Time.time + Settings.estimateIntervalSeconds;

                Debug.DrawLine(m_currentWorldAnchor.transform.position, visitedBar.transform.position, Color.black, 1);
                Debug.DrawLine(m_currentWorldAnchor.transform.position, estimatePose.position, Color.red, 1);

                Debug.Log($"[MapQualityIndicator] Count Of Good Bars for {m_currentWorldAnchor.sessionId}: {this.CountOfGoodBars}");
            }
        }
        #endregion

        private void setCurrentAnchor(ARAnchor anchor, bool isRemapping)
        {
            interruptMappingGuide();
            m_currentWorldAnchor = anchor;
        }

        private void showMappingGuide()
        {
            if (m_currentWorldAnchor == null)
            {
                Debug.LogWarning($"[MapQualityIndicator] ShowMappingGuide anchor is null!");
                return;
            }

            Debug.Log($"[MapQualityIndicator] ShowMappingGuide {m_currentWorldAnchor.sessionId}");

            recycleBars();
            placeQualityBars();
            nextEstimateTime = Time.time + Settings.estimateIntervalSeconds;
            m_Paused = false;
        }


        private void pauseEstimateQuality()
        {
            Debug.Log($"[MapQualityIndicator] pauseEstimate");
            m_Paused = true;
        }

        private void interruptMappingGuide()
        {
            if (m_currentWorldAnchor != null)
            {
                Debug.Log($"[MapQualityIndicator] interruptMappingGuide {m_currentWorldAnchor.sessionId}");
                m_currentWorldAnchor = null;
            }

            recycleBars();
            m_Paused = false;
        }

        private async void finishMappingGuide()
        {
            if (m_currentWorldAnchor != null)
            {
                m_currentWorldAnchor = null;
            }

            await turnAllBarsGood();

            await Task.Delay(2000);

            recycleBars();
            m_Paused = false;
        }

        private List<MapQualityBar> initalizeBars()
        {
            var list = new List<MapQualityBar>();

            Debug.Log($"[MapQualityIndicator] initalizeBars {Settings.barCount}");

            for (int i = 0; i < Settings.barCount; i++)
            {
                var bar = Instantiate(m_qualityBarPrefab, this.transform).GetComponent<MapQualityBar>();
                bar.gameObject.SetActive(false);
                list.Add(bar);
            }

            return list;
        }

        /// <summary>
        /// Place quality bars around the anchor
        /// z-axis is the forward direction of eye, also the forward direction of the ring
        /// 
        ///               ^z  0 degree
        ///                |
        ///                |
        ///    --------|-------->x  90 degree
        ///     o         |         o     
        ///       o       |       o        
        ///             o | o              
        ///             180 degree              
        /// </summary>
        private void placeQualityBars()
        {
            m_ringForward = (m_currentWorldAnchor.transform.position - estimatePose.position).normalized;
            m_ringForward.y = 0;
            Vector3 upDir = Vector3.up;

            float range = Settings.angleRange;
            int barCount = Settings.barCount;

            float startAngle = normalizeAngle(180 - range * 0.5f);
            float deltaAngle = range / (barCount - 1);
            m_startDir = Quaternion.AngleAxis(startAngle, upDir) * m_ringForward;

            Vector3 midPos = Vector3.Lerp(m_currentWorldAnchor.transform.position, estimatePose.position, 0.3f);
            float y = midPos.y;
            Vector3 initalPos = m_currentWorldAnchor.transform.position;
            initalPos.y = y;
            float radius = Vector3.Distance(initalPos, midPos);

            for (int i = 0; i < barCount; ++i)
            {
                var bar = bars[i];
                bar.gameObject.SetActive(true);
                Quaternion deltaRotation = Quaternion.AngleAxis(normalizeAngle(startAngle + i * deltaAngle), upDir);
                bar.transform.position = initalPos + deltaRotation * m_ringForward * radius;
                bar.transform.up = m_currentWorldAnchor.transform.position - bar.transform.position;
            }
        }

        private int computeBarIndex()
        {
            Vector3 viewRay = getDirectionFromAnchorToEye();
            viewRay.y = 0;
            Vector3 startDir = m_startDir;
            startDir.y = 0;

            float signedAngleFromStartDir = normalizeAngle(Vector3.SignedAngle(viewRay, startDir, Vector3.down));
            float angleStep = Settings.angleRange / (Settings.barCount - 1);
            return Mathf.FloorToInt(signedAngleFromStartDir / angleStep);
        }

        private void recycleBars()
        {
            foreach (MapQualityBar bar in bars)
            {
                bar.Recycle();
            }
        }

        private async Task turnAllBarsGood()
        {
            foreach (MapQualityBar bar in bars)
            {
                if (bar.QualityState == XREALAnchorEstimateQuality.XREAL_ANCHOR_ESTIMATE_QUALITY_GOOD)
                {
                    continue;
                }

                bar.IsVisited = true;
                bar.QualityState = XREALAnchorEstimateQuality.XREAL_ANCHOR_ESTIMATE_QUALITY_GOOD;
                await Task.Delay(100);
            }
        }

        #region helpers
        /// <summary>
        /// Normalize angle range to [0,360)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static float normalizeAngle(float angle)
        {
            float ret = angle % 360f;
            if (ret < 0)
            {
                ret += 360f;
            }
            return ret;
        }

        private Vector3 getDirectionFromAnchorToEye()
        {
            var centerEyePose = estimatePose;
            Vector3 viewRay = centerEyePose.position - m_currentWorldAnchor.transform.position;
            return viewRay.normalized;
        }

        #endregion

        #region debug
        private int CountOfGoodBars
        {
            get
            {
                int count = 0;
                for (int i = 0; i < m_bars.Count; i++)
                {
                    if (m_bars[i].QualityState == XREALAnchorEstimateQuality.XREAL_ANCHOR_ESTIMATE_QUALITY_GOOD)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        #endregion


        [Serializable]
        public class IndicatorSettings
        {
            public bool showIndicator;

            public float angleRange;
            public int barAngleStep;
            /// <summary>
            /// Interval for estimating mapping quality in seconds
            /// </summary>
            public float estimateIntervalSeconds = 0.5f;

            public int barCount => Mathf.FloorToInt(angleRange / barAngleStep);

        }
    }
}
