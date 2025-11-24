using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    public partial class Anchors
    {
#if !UNITY_6000_0_OR_NEWER
        void AddListener()
        {
            m_AnchorManager.anchorsChanged += OnAnchorsChanged;
        }

        private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
        {
            var added = args.added;
            var updated = args.updated;
            var removed = args.removed;
            foreach (var item in added)
            {
                Debug.Log($"[Anchors] OnAnchorsChanged Add {item.sessionId} {item.trackableId}");
            }
            foreach (var item in updated)
            {
                Debug.Log($"[Anchors] OnAnchorsChanged Updated {item.sessionId}");
            }
            foreach (var item in removed)
            {
                Debug.Log($"[Anchors] OnAnchorsChanged Removed {item.sessionId}");
            }
        }

        /// <summary>
        /// Asynchronously erases an anchor identified by the given <paramref name="trackableId"/>.
        /// </summary>
        /// <param name="trackableId">The ID of the trackable anchor to be erased.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value indicating 
        /// whether the anchor was successfully erased (true) or not (false).
        /// </returns>
        public Task<bool> EraseAnchor(TrackableId trackableId)
        {
            if (m_SavedAnchorEntryDict.TryGetValue(trackableId, out var anchorItem))
            {
                var result = m_AnchorManager.TryEraseAnchorAsync(anchorItem.PersistentGuid);
                m_SavedAnchorEntryDict.Remove(trackableId);
                return result;
            }
            return Task.FromResult(false);
        }

        private void LoadAllAnchors()
        {
            Debug.Log($"[Anchors] LoadAllAnchors");
            var savedAnchorIds = m_AnchorManager.TryGetSavedAnchorIds();
            foreach (var persistentGuid in savedAnchorIds)
            {
                if (m_AnchorManager.TryLoadAnchor(persistentGuid, out XRAnchor anchor))
                {
                    Debug.Log($"[Anchors] Try Load {persistentGuid} success");
                    AddSavedAnchorEntry(anchor.trackableId, persistentGuid);
                }
                else
                {
                    Debug.Log($"[Anchors] Try Load {persistentGuid} fail");
                }
            }
            Debug.Log($"[Anchors] LoadAllAnchors Finished");
        }

        private void EraseAllAnchors()
        {
            Debug.Log($"[Anchors] EraseAllAnchors");
            var savedAnchorIds = m_AnchorManager.TryGetSavedAnchorIds();
            foreach (var persistentGuid in savedAnchorIds)
            {
                Debug.Log($"[Anchors] TryEraseAnchorAsync {persistentGuid}");
                m_AnchorManager.TryEraseAnchorAsync(persistentGuid);
            }
            Debug.Log($"[Anchors] EraseAllAnchors Finished");
        }
#endif
    }
}
