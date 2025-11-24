using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    /// <summary>
    /// This sample shows how to display RGBCamera image provided by the ARCameraManager.
    /// </summary>
    public class ARFoundationCameraSample : MonoBehaviour
    {
        Texture2D m_TextureY;
        Texture2D m_TextureV;
        Texture2D m_TextureU;

        [SerializeField]
        ARCameraManager m_CameraManager;

        [SerializeField]
        RawImage m_RawCameraImage;

        void OnEnable()
        {
            if (m_CameraManager != null)
                m_CameraManager.frameReceived += OnCameraFrameReceived;
        }

        void OnDisable()
        {
            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= OnCameraFrameReceived;
        }

        void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            UpdateCameraImage();
        }

        void UpdateCameraImage()
        {
            if (m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {

                if (m_TextureY == null)
                {
                    int Width = image.dimensions.x;
                    int Height = image.dimensions.y;
                    m_TextureY = new Texture2D(Width, Height, TextureFormat.Alpha8, false);
                    m_TextureU = new Texture2D(Width / 2, Height / 2, TextureFormat.Alpha8, false);
                    m_TextureV = new Texture2D(Width / 2, Height / 2, TextureFormat.Alpha8, false);

                    var material = m_RawCameraImage.material;
                    m_RawCameraImage.texture = m_TextureY;
                    material.SetTexture("_UTex", m_TextureU);
                    material.SetTexture("_VTex", m_TextureV);
                }

                var planeY = image.GetPlane(0);
                var planeV = image.GetPlane(1);
                var planeU = image.GetPlane(2);

                m_TextureY.LoadRawTextureData(planeY.data);
                m_TextureV.LoadRawTextureData(planeV.data);
                m_TextureU.LoadRawTextureData(planeU.data);

                m_TextureY.Apply();
                m_TextureU.Apply();
                m_TextureV.Apply();

                image.Dispose();
            }
        }
    }
}
