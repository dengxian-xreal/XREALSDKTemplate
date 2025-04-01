using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{

    /// <summary>
    /// Visualizes a MarkerImage by displaying marker index.
    /// </summary>
    public class MarkerImageVisualizer : MonoBehaviour
    {
        /// <summary> 
        /// The TrackingImage to visualize. 
        /// </summary>
        public ARTrackedImage Image;

        public TextMesh Title;

        public void Init(ARTrackedImage image, int dataBaseIndex)
        {
            Image = image;
            transform.parent = transform;
            if (Title != null)
            {
                Title.text = $"{dataBaseIndex}";
            }
        }

        /// <summary> 
        /// Updates this object. 
        /// </summary>
        public void Update()
        {
            if (Image == null || Image.trackingState != TrackingState.Tracking)
            {
                return;
            }

            var dir = Title.transform.position - XREALUtility.MainCamera.transform.position;
            dir.y = 0;
            Title.transform.forward = dir;
        }

    }
}
