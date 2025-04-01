using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Collection of marker options.
    /// A marker option collection is a collection of marker options.
    /// </summary>
    public class MarkerOptionCollection : MonoBehaviour
    {
        [SerializeField]
        private List<MarkerOption> m_RelatedOptions = new List<MarkerOption>();
        [SerializeField]
        private Dictionary<int, MarkerOption> m_IndexOptionDict = new Dictionary<int, MarkerOption>();
        private int m_Index;

        private HashSet<int> m_SelectedIndex = new HashSet<int>();
        #region unity messages
        private void Awake()
        {
            m_RelatedOptions.AddRange(GetComponentsInChildren<MarkerOption>());
            foreach (var option in m_RelatedOptions)
            {
                m_IndexOptionDict.Add(option.Index, option);
            }
            gameObject.SetActive(false);
        }
        #endregion

        /// <summary>
        /// Check if the collection is related to the index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsRelatedTo(int index)
        {
            return m_IndexOptionDict.ContainsKey(index);
        }


        /// <summary>
        /// Select the option with the index.
        /// </summary>
        /// <param name="index"></param>
        public void Select(int index)
        {
            if (m_IndexOptionDict.TryGetValue(m_Index, out MarkerOption option))
            {
                option.Deselect();
                m_SelectedIndex.Remove(m_Index);
            }

            if (m_IndexOptionDict.TryGetValue(index, out option))
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
                m_Index = index;
                m_SelectedIndex.Add(index);
                option.Select();
            }
        }

        /// <summary>
        /// Deselect the option with the index.
        /// </summary>
        /// <param name="index"></param>
        public void Deselect(int index)
        {
            if (m_IndexOptionDict.TryGetValue(index, out MarkerOption option))
            {
                option.Deselect();
                m_SelectedIndex.Remove(index);
            }
            if (m_SelectedIndex.Count == 0)
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        internal void AttachTo(ARTrackedImage image)
        {
            if (image == null)
            {
                transform.SetParent(null);
                return;
            }
            transform.SetParent(image.transform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
