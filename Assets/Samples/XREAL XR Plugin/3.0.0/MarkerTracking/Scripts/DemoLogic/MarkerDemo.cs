using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.XR.XREAL.Samples
{
    public class MarkerDemo : MonoBehaviour
    {
        [SerializeField]
        private MarkerImageExampleController m_Controller;

        [SerializeField]
        private List<MarkerOptionCollection> m_OptionCollectionList;

        private void Awake()
        {
            m_Controller.OnImageLoaded += OnImageLoaded;
            m_Controller.OnImageLost += OnImageLost;
        }

        /// <summary>
        /// Called when an image is tracked.
        /// </summary>
        /// <param name="image"></param>
        private void OnImageLoaded(ARTrackedImage image)
        {
            int index = (int)image.trackableId.subId2 & 0xFF;
            Debug.Log($"[MarkerDemo] OnImageLoaded {index}");
            foreach (var collect in m_OptionCollectionList)
            {
                if (collect.IsRelatedTo(index))
                {
                    collect.AttachTo(image);
                    collect.Select(index);
                    return;
                }
            }
        }

        /// <summary>
        /// Called when an image is lost.
        /// </summary>
        /// <param name="image"></param>
        private void OnImageLost(ARTrackedImage image)
        {
            int index = (int)image.trackableId.subId2 & 0xFF;
            Debug.Log($"[MarkerDemo] OnImageLost {index}");

            foreach (var collect in m_OptionCollectionList)
            {
                if (collect.IsRelatedTo(index))
                {
                    collect.AttachTo(null);
                    collect.Deselect(index);
                    return;
                }
            }
        }
    }
}
