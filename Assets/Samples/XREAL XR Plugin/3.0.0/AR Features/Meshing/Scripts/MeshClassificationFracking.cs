using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// This class handles mesh classification by breaking up an ARMesh into multiple submeshes based on semantic labels.
    /// </summary>
    public class MeshClassificationFracking : MonoBehaviour
    {

        const string TAG = "MeshClassificationFracking";
        /// <summary>
        /// The number of mesh classifications detected.
        /// </summary>
        const int k_NumClassifications = 12;

        /// <summary>
        /// The mesh manager for the scene.
        /// </summary>
        public ARMeshManager m_MeshManager;


        /// <summary>
        /// List for all classified meshfilter prefabs
        /// </summary>
        [SerializeField]
        private LabelMeshFilterPair[] m_ClassifiedMeshFilterPrefabs;

        private NRMeshingVertexSemanticLabel[] m_AvailableLabels = null;
        private NRMeshingVertexSemanticLabel[] AvailableLabels
        {
            get
            {
                if (m_AvailableLabels == null || m_AvailableLabels.Length == 0)
                {
                    m_AvailableLabels = new NRMeshingVertexSemanticLabel[m_ClassifiedMeshFilterPrefabs.Length];
                    for (int i = 0; i < m_ClassifiedMeshFilterPrefabs.Length; ++i)
                    {
                        m_AvailableLabels[i] = m_ClassifiedMeshFilterPrefabs[i].label;
                    }
                }
                return m_AvailableLabels;
            }
        }

        /// <summary>
        /// A mapping from tracking ID to instantiated mesh filters.
        /// </summary>
        readonly Dictionary<TrackableId, Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>> m_MeshFrackingMap = new Dictionary<TrackableId, Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>>();

        public Dictionary<TrackableId, Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>> MeshFrackingMap => m_MeshFrackingMap;

        private Dictionary<TrackableId, MeshChangeState> m_MeshChangeStateMap = new Dictionary<TrackableId, MeshChangeState>();

        /// <summary>
        /// An array to store the triangle vertices of the base mesh.
        /// </summary>
        readonly List<int> m_BaseTriangles = new List<int>();

        /// <summary>
        /// An array to store the colors of the base mesh.
        /// </summary>
        readonly List<Color32> m_BaseColors = new List<Color32>();

        readonly Dictionary<NRMeshingVertexSemanticLabel, List<int>> m_LabelClassifiedTrianglesDict = new Dictionary<NRMeshingVertexSemanticLabel, List<int>>();
        /// <summary>
        /// An array to store the triangle vertices of the classified mesh.
        /// </summary>
        readonly List<int> m_ClassifiedTriangles = new List<int>();

        /// <summary>
        /// On enable, subscribe to the meshes changed event.
        /// </summary>
        void OnEnable()
        {
            Debug.Assert(m_MeshManager != null, "mesh manager cannot be null");
            m_MeshManager.meshesChanged += OnMeshesChanged;
        }

        /// <summary>
        /// On disable, unsubscribe from the meshes changed event.
        /// </summary>
        void OnDisable()
        {
            Debug.Assert(m_MeshManager != null, "mesh manager cannot be null");
            m_MeshManager.meshesChanged -= OnMeshesChanged;
        }

        /// <summary>
        /// Retrieves the current change state of a mesh associated with the given trackable ID.
        /// </summary>
        /// <param name="trackableId">The ID of the trackable object whose mesh change state is to be retrieved.</param>
        /// <returns>
        /// The <see cref="MeshChangeState"/> of the specified trackable ID:
        /// - If the trackable ID exists in the internal map, the associated state is returned.
        /// - If the trackable ID is not found, <see cref="MeshChangeState.Removed"/> is returned by default.
        /// </returns>
        public MeshChangeState GetMeshChangeState(TrackableId trackableId)
        {
            if (m_MeshChangeStateMap.TryGetValue(trackableId, out var state))
            {
                return state;
            }

            return MeshChangeState.Removed;
        }

        /// <summary>
        /// When the meshes change, update the scene meshes.
        /// </summary>
        void OnMeshesChanged(ARMeshesChangedEventArgs args)
        {
            foreach (var key in m_MeshChangeStateMap.Keys.ToList())
            {
                m_MeshChangeStateMap[key] = MeshChangeState.Unchanged;
            }

            if (args.added != null)
            {
                args.added.ForEach(BreakupMesh);
            }

            if (args.updated != null)
            {
                args.updated.ForEach(UpdateMesh);
            }

            if (args.removed != null)
            {
                args.removed.ForEach(RemoveMesh);
            }
        }

        /// <summary>
        /// Parse the trackable ID from the mesh filter name.
        /// </summary>
        /// <param name="meshFilterName">The mesh filter name containing the trackable ID.</param>
        /// <returns>
        /// The trackable ID parsed from the string.
        /// </returns>
        TrackableId ExtractTrackableId(string meshFilterName)
        {
            string[] nameSplit = meshFilterName.Split(' ');
            return new TrackableId(nameSplit[1]);
        }

        /// <summary>
        /// Break up a single mesh with multiple face classifications into submeshes, each with an unique and uniform mesh
        /// classification.
        /// </summary>
        /// <param name="meshFilter">The mesh filter for the base mesh with multiple face classifications.</param>
        void BreakupMesh(MeshFilter meshFilter)
        {
            XRMeshSubsystem meshSubsystem = m_MeshManager.subsystem as XRMeshSubsystem;
            if (meshSubsystem == null)
            {
                return;
            }

            var meshId = ExtractTrackableId(meshFilter.name);
            m_MeshChangeStateMap[meshId] = MeshChangeState.Added;
            var faceClassifications = meshSubsystem.GetFaceClassifications(meshId, Allocator.Persistent);
            Debug.Log($"[{TAG}] BreakupMesh {meshId} labels.Count={faceClassifications.Length}");

            if (!faceClassifications.IsCreated)
            {
                return;
            }

            using (faceClassifications)
            {
                if (faceClassifications.Length <= 0)
                {
                    return;
                }

                var parent = meshFilter.transform.parent;

                Dictionary<NRMeshingVertexSemanticLabel, MeshFilter> meshFilters = new Dictionary<NRMeshingVertexSemanticLabel, MeshFilter>();
                for (int i = 0; i < m_ClassifiedMeshFilterPrefabs.Length; ++i)
                {
                    var pair = m_ClassifiedMeshFilterPrefabs[i];
                    var filter = (pair.meshFilter == null) ? null : Instantiate(pair.meshFilter, parent);
                    filter.gameObject.name = $"{meshId}_{pair.label}";
                    meshFilters[pair.label] = filter;
                }
                m_MeshFrackingMap[meshId] = meshFilters;

                var baseMesh = meshFilter.sharedMesh;
                ExtractClassifiedMesh(baseMesh, faceClassifications, meshFilters);
            }
        }

        /// <summary>
        /// Given a base mesh, the face classifications for all faces in the mesh, 
        /// extract into new meshes , each of which only has faces that have the selected face classification.
        /// </summary>
        /// <param name="baseMesh">The original base mesh. </param>
        /// <param name="faceClassifications">The array of face classifications for each triangle in the mesh. </param>
        /// <param name="meshFilters"></param>
        private void ExtractClassifiedMesh(Mesh baseMesh, NativeArray<NRMeshingVertexSemanticLabel> faceClassifications, Dictionary<NRMeshingVertexSemanticLabel, MeshFilter> meshFilters)
        {
            m_BaseTriangles.Clear();
            baseMesh.GetTriangles(m_BaseTriangles, 0);
            baseMesh.GetColors(m_BaseColors);

            Debug.Assert(m_BaseTriangles.Count == (faceClassifications.Length * 3),
                        "unexpected mismatch between triangle count and face classification count");

            // Renew s_LabelClassifiedTrianglesDict data
            m_LabelClassifiedTrianglesDict.Clear();
            for (int i = 0; i < AvailableLabels.Length; ++i)
            {
                var label = AvailableLabels[i];
                var classifiedTriangles = new List<int>();
                classifiedTriangles.Capacity = m_BaseTriangles.Count;
                m_LabelClassifiedTrianglesDict.Add(label, classifiedTriangles);
            }

            // Iterate through each triangle face and assign it to the list of triangle faces for a certain label.
            for (int i = 0; i < m_BaseTriangles.Count / 3; i++)
            {
                int idx_0 = m_BaseTriangles[i * 3];
                int idx_1 = m_BaseTriangles[i * 3 + 1];
                int idx_2 = m_BaseTriangles[i * 3 + 2];

#if false
                    NRMeshingVertexSemanticLabel[] sl = new NRMeshingVertexSemanticLabel[] {
                    (NRMeshingVertexSemanticLabel)m_BaseColors[idx_0].r,
                    (NRMeshingVertexSemanticLabel)m_BaseColors[idx_1].r,
                    (NRMeshingVertexSemanticLabel)m_BaseColors[idx_2].r
                };

#else
                NRMeshingVertexSemanticLabel[] sl = new NRMeshingVertexSemanticLabel[] {
                    faceClassifications[idx_0],
                    faceClassifications[idx_1],
                    faceClassifications[idx_2]
                };

#endif

                NRMeshingVertexSemanticLabel[] selectedLabels = new NRMeshingVertexSemanticLabel[] { sl[0] };

                for (int j = 0; j < selectedLabels.Length; ++j)
                {
                    NRMeshingVertexSemanticLabel selectedLabel = selectedLabels[j];
                    try
                    {
                        var classifiedTriangles = m_LabelClassifiedTrianglesDict[selectedLabel];
                        classifiedTriangles.Add(idx_0);
                        classifiedTriangles.Add(idx_1);
                        classifiedTriangles.Add(idx_2);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[{TAG}] {ex.Message} {selectedLabel}");
                    }
                }
            }

            // Setup classified mesh for each label
            for (int i = 0; i < AvailableLabels.Length; ++i)
            {
                try
                {
                    NRMeshingVertexSemanticLabel selectedLabel = AvailableLabels[i];
                    var classifiedTriangles = m_LabelClassifiedTrianglesDict[selectedLabel];
                    Mesh classifiedMesh = meshFilters[selectedLabel].mesh;

                    classifiedMesh.Clear();
                    classifiedMesh.SetVertices(baseMesh.vertices);
                    classifiedMesh.SetNormals(baseMesh.normals);
                    classifiedMesh.SetTriangles(classifiedTriangles, 0);
                    //WirframeMeshComputor.ComputeWireframeMeshData(baseMesh.vertices, baseMesh.normals, classifiedTriangles, classifiedMesh);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[{TAG}] {ex.Message}");
                }
            }

        }

        /// <summary>
        /// Update the submeshes corresponding to the single mesh with multiple face classifications into submeshes.
        /// </summary>
        /// <param name="meshFilter">The mesh filter for the base mesh with multiple face classifications.</param>
        void UpdateMesh(MeshFilter meshFilter)
        {
            XRMeshSubsystem meshSubsystem = m_MeshManager.subsystem as XRMeshSubsystem;
            if (meshSubsystem == null)
            {
                return;
            }

            var meshId = ExtractTrackableId(meshFilter.name);
            m_MeshChangeStateMap[meshId] = MeshChangeState.Updated;

            var faceClassifications = meshSubsystem.GetFaceClassifications(meshId, Allocator.Persistent);
            Debug.Log($"[{TAG}] UpdateMesh {meshId} labels.Count={faceClassifications.Length}");

            if (!faceClassifications.IsCreated)
            {
                return;
            }

            using (faceClassifications)
            {
                if (faceClassifications.Length <= 0)
                {
                    return;
                }

                var meshFilters = m_MeshFrackingMap[meshId];

                var baseMesh = meshFilter.sharedMesh;
                ExtractClassifiedMesh(baseMesh, faceClassifications, meshFilters);
            }
        }

        /// <summary>
        /// Remove the submeshes corresponding to the single mesh.
        /// </summary>
        /// <param name="meshFilter">The mesh filter for the base mesh with multiple face classifications.</param>
        void RemoveMesh(MeshFilter meshFilter)
        {
            var meshId = ExtractTrackableId(meshFilter.name);
            m_MeshChangeStateMap[meshId] = MeshChangeState.Removed;

            var meshFilters = m_MeshFrackingMap[meshId];
            foreach (var kv in meshFilters)
            {
                var label = kv.Key;
                var classifiedMeshFilter = kv.Value;
                if (classifiedMeshFilter != null)
                {
                    Destroy(classifiedMeshFilter);
                }
            }

            m_MeshFrackingMap.Remove(meshId);
        }

        [System.Serializable]
        public class LabelMeshFilterPair
        {
            public NRMeshingVertexSemanticLabel label;
            public MeshFilter meshFilter;
        }
    }
}
