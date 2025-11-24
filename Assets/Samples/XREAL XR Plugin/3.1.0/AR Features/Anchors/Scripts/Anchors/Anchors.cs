using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SerializableGuid = UnityEngine.XR.ARSubsystems.SerializableGuid;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Represents an entry for a saved anchor, containing a TrackableId and a PersistentGuid.
    /// </summary>
    public struct SavedAnchorEntry
    {
        /// <summary>
        /// The unique identifier for the trackable anchor.
        /// </summary>
        public TrackableId TrackableId;

        /// <summary>
        /// The persistent GUID associated with the anchor.
        /// </summary>
        public SerializableGuid PersistentGuid;
    }

    /// <summary>
    /// Manages AR anchors within the sample, including creation, deletion, and loading of anchors.
    /// </summary>
    public partial class Anchors : MonoBehaviour
    {
        #region settings
        [SerializeField]
        [Tooltip("Prefab for the AR anchor.")]
        GameObject m_ARAnchorPrafab;

        [SerializeField]
        [Tooltip("Manager for handling AR anchors.")]
        ARAnchorManager m_AnchorManager;

        [SerializeField]
        [Tooltip("Control panel for user interactions.")]
        GameObject m_ControlPanel;

        private Camera m_Camera;

        private Dictionary<TrackableId, SavedAnchorEntry> m_SavedAnchorEntryDict = new Dictionary<TrackableId, SavedAnchorEntry>();

        private InputDevice m_RightController;

        #endregion

        private void Start()
        {
            m_Camera = XREALUtility.MainCamera;
            AddListener();
            MapQualityIndicator.Singleton.OnMappingGuideStop += OnMappingGuideStop;
        }

        private void OnMappingGuideStop(ARAnchor anchor)
        {
            ShowControlPanel(true);
        }

        /// <summary>
        /// Adds a saved anchor entry to the dictionary.
        /// </summary>
        /// <param name="trackableId">The unique identifier for the trackable anchor.</param>
        /// <param name="persistentGuid">The persistent GUID associated with the anchor.</param>
        public void AddSavedAnchorEntry(TrackableId trackableId, SerializableGuid persistentGuid)
        {
            if (m_SavedAnchorEntryDict.TryGetValue(trackableId, out var savedAnchorEntry))
            {
                savedAnchorEntry.PersistentGuid = persistentGuid;
            }
            else
            {
                m_SavedAnchorEntryDict.Add(trackableId, new SavedAnchorEntry
                {
                    TrackableId = trackableId,
                    PersistentGuid = persistentGuid
                });
            }
        }

        /// <summary>
        /// Shows or hides the control panel.
        /// </summary>
        /// <param name="show">True to show the control panel, false to hide it.</param>
        public void ShowControlPanel(bool show)
        {
            m_ControlPanel.SetActive(show);
        }

        /// <summary>
        /// Handles the creation of a new anchor when the corresponding button is clicked.
        /// </summary>
        public async void OnNewButtonClick()
        {
            var targetPos = m_Camera.transform.position + m_Camera.transform.forward * 1f;
            var targetRotation = m_Camera.transform.rotation;
            var anchorObj = Instantiate(m_ARAnchorPrafab, targetPos, targetRotation);
            var anchorInfo = anchorObj.GetComponent<AnchorInfo>();
            anchorInfo.WaitForLocating();
            ShowControlPanel(false);
            await anchorInfo.WaitUntilLocated();
            ConfirmDialog.Instance.Show(ConfirmDialog.ANCHORS_USER_VIEW_GUIDE);
            await ConfirmDialog.Instance.WaitUntilClosed();

            if (anchorInfo != null)
            {
                anchorInfo.ShowPanel();
            }
            MapQualityIndicator.SetCurrentAnchor(anchorInfo.Anchor, false);
            MapQualityIndicator.ShowMappingGuide();
            Debug.Log($"[Anchors] CreateAnchor at {targetPos}");
        }

        /// <summary>
        /// Handles the erasure of all anchors when the corresponding button is clicked.
        /// </summary>
        public void OnEraseAllButtonClick()
        {
            EraseAllAnchors();
        }

        /// <summary>
        /// Handles the loading of all anchors when the corresponding button is clicked.
        /// </summary>
        public void OnLoadAllButtonClick()
        {
            LoadAllAnchors();
        }
    }
}
