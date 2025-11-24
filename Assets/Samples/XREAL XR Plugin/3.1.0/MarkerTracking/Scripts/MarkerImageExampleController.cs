using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.XR.XREAL.Samples
{
    /// <summary> 
    /// Controller for TrackingImage example. 
    /// </summary>
    [HelpURL("https://developer.xreal.com/develop/unity/image-tracking")]
    public class MarkerImageExampleController : MonoBehaviour
    {
        public event Action<ARTrackedImage> OnImageLoaded;
        public event Action<ARTrackedImage> OnImageLost;

        public ARTrackedImageManager m_TrackedImageManager;
        /// <summary> 
        /// A prefab for visualizing an TrackingImage. 
        /// </summary>
        public MarkerImageVisualizer MarkerImageVisualizerPrefab;

        private Dictionary<int, MarkerImageVisualizer> m_Visualizers
            = new Dictionary<int, MarkerImageVisualizer>();

        private List<ARTrackedImage> m_TempTrackingImages = new List<ARTrackedImage>();

        private void Start()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
#if UNITY_EDITOR
            MockTrackableImageFactory.CreateSingleton().OnTrackablesChanged += OnMockTrackablesChanged;
#endif
        }



        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var removed in args.removed)
            {
                OnImageLost?.Invoke(removed);
            }

            foreach (var added in args.added)
            {
                OnImageLoaded?.Invoke(added);
            }

        }

#if UNITY_EDITOR
        private void OnMockTrackablesChanged(List<ARTrackedImage> removedList, List<ARTrackedImage> addedList)
        {
            foreach (var removed in removedList)
            {
                OnImageLost?.Invoke(removed);
            }
            foreach (var added in addedList)
            {
                OnImageLoaded?.Invoke(added);
            }

        }
#endif
    }
}
