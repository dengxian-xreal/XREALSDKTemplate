using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL.Samples
{
    public class RGBCameraExample : MonoBehaviour
    {
        [SerializeField]
        private Text m_ImageFormatText;
        [SerializeField]
        private RawImage m_YUVImage;
        [SerializeField]
        private Button m_PlayButton;
        [SerializeField]
        private Button m_StopButton;

        private XREALRGBCameraTexture m_RGBCameraTexture;

        void Start()
        {
            Debug.Log($"[RGBCamera] Start");
            m_RGBCameraTexture = XREALRGBCameraTexture.CreateSingleton();
            m_PlayButton.onClick.AddListener(Play);
            m_StopButton.onClick.AddListener(Stop);
            InitUI();
            Play();
        }

        void Update()
        {
            var yuvTextures = m_RGBCameraTexture.GetYUVFormatTextures();
            if (yuvTextures[0] != null)
            {
                m_YUVImage.texture = yuvTextures[0];
                m_YUVImage.material.SetTexture("_UTex", yuvTextures[1]);
                m_YUVImage.material.SetTexture("_VTex", yuvTextures[2]);
            }
        }

        private void OnDestroy()
        {
            Debug.Log($"[RGBCamera] OnDestroy");
            Stop();
        }

        private void InitUI()
        {
            m_ImageFormatText.text = "YUV_420_888";
            m_YUVImage.gameObject.SetActive(true);
        }

        public void Play()
        {
            if (!m_RGBCameraTexture.IsCapturing)
            {
                Debug.Log($"[RGBCamera] Play");
                m_RGBCameraTexture.StartCapture();
            }
        }

        public void Stop()
        {
            if (m_RGBCameraTexture.IsCapturing)
            {
                Debug.Log($"[RGBCamera] Stop");
                m_RGBCameraTexture.StopCapture();
            }
        }
    }
}
