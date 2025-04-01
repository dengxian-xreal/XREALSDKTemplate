using UnityEngine;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// Draw a line between two points and show the distance.
    /// </summary>
    public class LineConnection : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer m_LineRenderer;

        [SerializeField]
        private TextMesh m_DistanceTextMesh;

        [SerializeField]
        private Transform EndPoint;

        private void Update()
        {
            if (gameObject.activeSelf && EndPoint.gameObject.activeSelf)
            {
                if (!m_LineRenderer.gameObject.activeSelf)
                {
                    m_LineRenderer.gameObject.SetActive(true);
                }
                else
                {
                    m_LineRenderer.material.mainTextureScale = new Vector2(20 * Vector3.Distance(transform.position, EndPoint.position), 1);
                    m_LineRenderer.SetPositions(new Vector3[] {
                    transform.position,
                    EndPoint.position
                });
                }

                if (!m_DistanceTextMesh.gameObject.activeSelf)
                {
                    m_DistanceTextMesh.gameObject.SetActive(true);
                }
                else
                {
                    m_DistanceTextMesh.text = $"{Vector3.Distance(transform.position, EndPoint.position).ToString("F2")}m";
                    m_DistanceTextMesh.transform.position = (transform.position + EndPoint.position) * 0.5f;
                    m_DistanceTextMesh.transform.forward = m_DistanceTextMesh.transform.position - XREALUtility.MainCamera.transform.position;
                }

            }
            else
            {
                if (m_LineRenderer.gameObject.activeSelf)
                {
                    m_LineRenderer.gameObject.SetActive(false);
                }
                if (m_DistanceTextMesh.gameObject.activeSelf)
                {
                    m_DistanceTextMesh.gameObject.SetActive(false);
                }
            }
        }
    }
}