using TMPro;
using UnityEngine;
namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Updates the mesh block information and displays it on the UI.
    /// </summary>
    public class MeshBlockInfo : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_Text;

        [SerializeField]
        MeshFilter m_MeshFilter;

        float m_LastUpdateTime = 0;
        // Update is called once per frame
        void Update()
        {
            if (Time.time - m_LastUpdateTime > 1)
            {
                Debug.Log($"[MeshBlockInfo] {gameObject.name} info exists. vertexCount={m_MeshFilter.mesh.vertexCount}");
                m_LastUpdateTime = Time.time;
                if (m_MeshFilter.mesh == null)
                {
                    m_Text.transform.position = XREALUtility.MainCamera.transform.position;
                    m_Text.text = $"name:{gameObject.name} No mesh";
                    return;
                }
                m_Text.transform.position = m_MeshFilter.mesh.bounds.center + Vector3.up * 2;
                m_Text.text = $"name:{gameObject.name} \n" +
                $"vertexCount={m_MeshFilter.mesh.vertexCount} \n" +
                $"indexCount={m_MeshFilter.mesh.GetIndexCount(0)}";
            }
        }
    }
}