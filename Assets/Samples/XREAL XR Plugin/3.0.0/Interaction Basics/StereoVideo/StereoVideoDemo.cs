using UnityEngine;
using UnityEngine.Video;

namespace Unity.XR.XREAL.Samples
{
    public class StereoVideo : MonoBehaviour
    {
        [SerializeField]
        VideoPlayer m_VideoPlayer;
        [SerializeField]
        MeshRenderer m_Screen;

        void Start()
        {
            m_VideoPlayer.prepareCompleted += (VideoPlayer source) =>
            {
                m_Screen.sharedMaterial.SetTexture("_MainTex", source.texture);
            };
        }
    }
}
