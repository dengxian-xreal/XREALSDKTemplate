#if UNITY_6000_0_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.XREAL;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    public partial class Anchors
    {
        void AddListener()
        {
            m_AnchorManager.trackablesChanged.AddListener(OnAnchrsChangeHandle);
        }

        private void OnAnchrsChangeHandle(ARTrackablesChangedEventArgs<ARAnchor> args)
        {
            var added = args.added;
            var updated = args.updated;
            var removed = args.removed;
            foreach (var item in added)
            {
                Debug.Log($"[Anchors] OnAnchorsChanged Add {item.sessionId}");
            }
            foreach (var item in updated)
            {
                Debug.Log($"[Anchors] OnAnchorsChanged Updated {item.sessionId}");
            }
            foreach (var item in removed)
            {
                Debug.Log($"[Anchors] OnAnchorsChanged Removed {item.Key} {item.Value.sessionId}");
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
        public async Task<bool> EraseAnchor(TrackableId trackableId)
        {
            if (m_SavedAnchorEntryDict.TryGetValue(trackableId, out var anchorItem))
            {
                var result = await m_AnchorManager.TryEraseAnchorAsync(anchorItem.PersistentGuid);
                return result.IsSuccess();
            }
            return false;
        }

        private async void LoadAllAnchors()
        {
            Debug.Log($"[Anchors] LoadAllAnchors");
            var result = await m_AnchorManager.TryGetSavedAnchorIdsAsync(Unity.Collections.Allocator.Temp);
            if (result.status.IsSuccess())
            {
                var savedAnchorIds = result.value;
                foreach (var id in savedAnchorIds)
                {
                    Debug.Log($"[Anchors] Try Load {id}");
                    var loadResult = await m_AnchorManager.TryLoadAnchorAsync(id);
                    if (loadResult.status.IsSuccess())
                    {
                        var anchor = loadResult.value;
                        Debug.Log($"[Anchors] Try Load {id} success");
                        AddSavedAnchorEntry(anchor.trackableId, id);
                        anchor.transform.position = new Vector3(9999, 9999, 9999); // send anchor to far away before it is located
                    }
                    else
                    {
                        Debug.Log($"[Anchors] Try Load {id} fail");
                    }
                }
            }
            Debug.Log($"[Anchors] LoadAllAnchors Finished");
        }

        private async void EraseAllAnchors()
        {
            Debug.Log($"[Anchors] EraseAllAnchors");
            var savedAnchorIds = await m_AnchorManager.TryGetSavedAnchorIdsAsync( Collections.Allocator.Persistent);
            foreach (var persistentGuid in savedAnchorIds.value)
            {
                Debug.Log($"[Anchors] TryEraseAnchorAsync {persistentGuid}");
                _ = m_AnchorManager.TryEraseAnchorAsync(persistentGuid);
            }
            Debug.Log($"[Anchors] EraseAllAnchors Finished");
        }
    }
}
#endif
