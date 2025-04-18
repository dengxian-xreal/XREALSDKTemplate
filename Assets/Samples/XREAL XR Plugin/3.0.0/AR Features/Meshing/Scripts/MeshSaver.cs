using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// It provides methods for saving meshes generated by the ARMeshManager.
    /// </summary>
    public class MeshSaver : MonoBehaviour
    {
        [SerializeField]
        ARMeshManager m_MeshManager;
        [SerializeField]
        MeshClassificationFracking m_MeshClassificationFracking;
        [SerializeField]
        GameObject m_SaveMeshDialog;
        [SerializeField]
        GameObject m_SaveSuccessDialog;

        /// <summary>
        /// Save all meshes generated by the ARMeshManager.
        /// </summary>
        public void Save()
        {
            var allMeshes = m_MeshManager.meshes;
            Dictionary<ulong, Mesh> idMeshDict = new Dictionary<ulong, Mesh>();
            foreach (var mesh in allMeshes)
            {
                string idStr = mesh.gameObject.name.Split(" ")[1];
                TrackableId id = new TrackableId(idStr);
                idMeshDict.Add(id.subId2, mesh.mesh);
            }
            MeshSaveUtility.Save(idMeshDict);
            m_SaveMeshDialog.SetActive(false);
            m_SaveSuccessDialog.SetActive(true);
        }

        /// <summary>
        /// Save all classified meshes.
        /// </summary>
        public void SaveClassifiedMeshes()
        {
            var meshFrackingMap = m_MeshClassificationFracking.MeshFrackingMap;
            MeshSaveUtility.SaveFrackingMesh(meshFrackingMap);
            m_SaveMeshDialog.SetActive(false);
            m_SaveSuccessDialog.SetActive(true);
        }

        public void OnConfirmClick()
        {
            m_SaveSuccessDialog.SetActive(false);
            m_SaveMeshDialog.SetActive(true);
        }
    }
}
