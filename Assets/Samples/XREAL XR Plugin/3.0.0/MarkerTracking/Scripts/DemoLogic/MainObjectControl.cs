using UnityEngine;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Control the transform, material and shape of the main object.
    /// </summary>
    public class MainObjectControl : MonoBehaviour
    {
        const string TAG = "MainObjectControl";
        [SerializeField]
        private Renderer m_Renderer;
        [SerializeField]
        private MeshFilter m_MeshFilter;

        /// <summary>
        /// Change the transform of the main object.
        /// </summary>
        /// <param name="target"></param>
        public void GoToTarget(Transform target)
        {
            Debug.Log($"[{TAG}] GoToTarget {target.position} {target.rotation.eulerAngles}");
            transform.position = target.position;
            transform.rotation = target.rotation;
            transform.localScale = target.localScale;
        }

        /// <summary>
        /// Change the material of the main object.
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterial(Material material)
        {
            m_Renderer.material = material;
        }

        /// <summary>
        /// Change the shape of the main object.
        /// </summary>
        /// <param name="mesh"></param>
        public void SetShape(Mesh mesh)
        {
            Debug.Log($"[{TAG}] SetShape {mesh.name}");
            m_MeshFilter.mesh = mesh;
        }
    }
}
