using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    ///  Updates the mesh change state information and displays it on the UI.
    /// </summary>
    public class MeshChangeStateInfo : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_ChangeStateText;
        [SerializeField]
        MeshClassificationFracking m_MeshFracking;

        private List<ulong> added = new List<ulong>();
        private List<ulong> updated = new List<ulong>();
        private List<ulong> removed = new List<ulong>();
        private List<ulong> unchanged = new List<ulong>();

        private float m_LastUpdateTime;
        private void Update()
        {
            if (Time.time - m_LastUpdateTime > 1)
            {
                UpdateChangeInfo();
                m_LastUpdateTime = Time.time;
            }
        }

        private void UpdateChangeInfo()
        {
            added.Clear();
            updated.Clear();
            removed.Clear();
            unchanged.Clear();

            foreach (var key in m_MeshFracking.MeshFrackingMap.Keys.ToList())
            {
                var state = m_MeshFracking.GetMeshChangeState(key);
                switch (state)
                {
                    case UnityEngine.XR.MeshChangeState.Added:
                        added.Add(key.subId2);
                        break;
                    case UnityEngine.XR.MeshChangeState.Updated:
                        updated.Add(key.subId2);
                        break;
                    case UnityEngine.XR.MeshChangeState.Removed:
                        removed.Add(key.subId2);
                        break;
                    case UnityEngine.XR.MeshChangeState.Unchanged:
                        unchanged.Add(key.subId2);
                        break;
                }
            }

            m_ChangeStateText.text = $"Added: {string.Join(", ", added)}\n"
             + $"Updated: {string.Join(", ", updated)}\n"
            + $"Removed: {string.Join(", ", removed)} \n"
            + $"Unchanged: {string.Join(", ", unchanged)}";

        }
    }
}
