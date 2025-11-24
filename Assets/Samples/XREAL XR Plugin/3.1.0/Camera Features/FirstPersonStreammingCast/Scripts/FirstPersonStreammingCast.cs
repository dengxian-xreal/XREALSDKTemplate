using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.XR.XREAL.Samples.NetWork;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL.Samples
{
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NativeGalleryDataProvider;
#else
    using GalleryDataProvider = MockGalleryDataProvider;
#endif
    /// <summary> A first person streamming cast. </summary>
    public class FirstPersonStreammingCast : MonoBehaviour
    {
        public delegate void OnResponse(bool result);

        [SerializeField]
        private Button m_RecordBtn;
        [SerializeField]
        private Text m_RecordText;
        [SerializeField]
        private Button m_StreamBtn;
        [SerializeField]
        private Text m_StreamText;

        public BlendMode m_BlendMode = BlendMode.Blend;
        public ResolutionLevel m_ResolutionLevel = ResolutionLevel.Middle;
        public LayerMask m_CullingMask = -1;
        public AudioState m_AudioState = AudioState.None;
        public bool useGreenBackGround = false;

        /// <summary> The net worker. </summary>
        private NetWorkBehaviour m_NetWorker;
        /// <summary> The video capture. </summary>
        private XREALVideoCapture m_VideoCapture = null;
        public XREALVideoCapture VideoCapture
        {
            get
            {
                return m_VideoCapture;
            }
        }

        private string m_ServerIP;
        /// <summary> Gets the full pathname of the rtp file. </summary>
        /// <value> The full pathname of the rtp file. </value>
        public string RTPPath
        {
            get
            {
                return string.Format(@"rtp://{0}:5555", m_ServerIP);
            }
        }

        public string VideoSavePath
        {
            get
            {
                string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
                string filename = string.Format("Xreal_Record_{0}.mp4", timeStamp);
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                return filepath;
            }
        }

        private bool m_IsInitialized = false;
        private bool m_IsStreamLock = false;
        private bool m_IsStreamStarted = false;
        private bool m_IsRecordStarted = false;

        private string m_VideoPath;
        private bool m_SavedLocal;
        private GalleryDataProvider galleryDataTool;
        public enum ResolutionLevel
        {
            High,
            Middle,
            Low,
        }

        /// <summary> Starts this object. </summary>
        void Start()
        {
            this.Init();
        }

        /// <summary> Initializes this object. </summary>
        private void Init()
        {
            if (m_IsInitialized)
            {
                return;
            }

            m_StreamBtn.onClick.AddListener(() =>
            {
                Debug.Log("m_StreamBtn.onClick");
                OnStream();
            });

            m_RecordBtn.onClick.AddListener(() =>
            {
                Debug.Log("m_RecordBtn.onClick");
                OnRecord();
            });

            m_IsInitialized = true;
        }

        private void RefreshUI()
        {
            m_RecordText.text = m_IsRecordStarted ? "Stop Record" : "Record";
            m_StreamText.text = m_IsStreamStarted ? "Stop Stream" : "Stream";
            if (m_IsStreamStarted)
            {
                m_RecordBtn.gameObject.SetActive(false);
                m_StreamBtn.gameObject.SetActive(true);
            }
            else if (m_IsRecordStarted)
            {
                m_RecordBtn.gameObject.SetActive(true);
                m_StreamBtn.gameObject.SetActive(false);
            }
            else
            {
                m_RecordBtn.gameObject.SetActive(true);
                m_StreamBtn.gameObject.SetActive(true);
            }
        }

        private void OnRecord()
        {
            if (!m_IsRecordStarted)
            {
                CreateAndStart();
            }
            else
            {
                StopVideoCapture();
            }
            m_IsRecordStarted = !m_IsRecordStarted;
            RefreshUI();
        }

        private void OnStream()
        {
            if (m_IsStreamLock)
            {
                return;
            }

            m_IsStreamLock = true;
            if (m_NetWorker == null)
            {
                m_NetWorker = new NetWorkBehaviour();
                m_NetWorker.Listen();
            }

            if (!m_IsStreamStarted)
            {
                LocalServerSearcher.CreateSingleton().Search((result) =>
                {
                    Debug.LogFormat("[FPStreammingCast] Get the server result:{0} ip:{1}:{2}", result.isSuccess, result.endPoint?.Address, result.endPoint.Port);
                    if (result.isSuccess)
                    {
                        string ip = result.endPoint.Address.ToString();
                        int port = result.endPoint.Port;
                        m_NetWorker.CheckServerAvailable(ip, port, (isAvailable) =>
                        {
                            Debug.LogFormat("[FPStreammingCast] Is the server {0}:{1} ok? {2}", ip, result.endPoint.Port, isAvailable);
                            if (isAvailable)
                            {
                                m_ServerIP = ip;
                                m_IsStreamStarted = true;
                                CreateAndStart();
                                RefreshUI();
                            }
                            m_IsStreamLock = false;
                        });
                    }
                    else
                    {
                        m_IsStreamLock = false;
                        Debug.LogError("[FPStreammingCast] Can not find the server...");
                    }
                });
            }
            else
            {
                StopVideoCapture();
                m_IsStreamStarted = false;
                m_IsStreamLock = false;
                RefreshUI();
            }

        }

        /// <summary> Converts this object to a server. </summary>
        private void CreateAndStart()
        {
            CreateVideoCapture(delegate ()
            {
                Debug.LogFormat("[FPStreammingCast] Start video capture.");
                StartCoroutine(StartVideoCapture());
            });
        }

        private Resolution GetResolutionByLevel(ResolutionLevel level)
        {
            var resolutions = XREALVideoCaptureUtility.SupportedResolutions.OrderByDescending((res) => res.width * res.height);
            Resolution resolution = new Resolution();
            switch (level)
            {
                case ResolutionLevel.High:
                    resolution = resolutions.ElementAt(0);
                    break;
                case ResolutionLevel.Middle:
                    resolution = resolutions.ElementAt(1);
                    break;
                case ResolutionLevel.Low:
                    resolution = resolutions.ElementAt(2);
                    break;
                default:
                    break;
            }
            return resolution;
        }

        #region video capture
        /// <summary> Creates video capture. </summary>
        /// <param name="callback"> The callback.</param>
        private void CreateVideoCapture(Action callback)
        {
            Debug.LogFormat("[FPStreammingCast] Created VideoCapture Instance!");
            if (m_VideoCapture != null)
            {
                callback?.Invoke();
                return;
            }

            XREALVideoCaptureUtility.CreateAsync(false, delegate (XREALVideoCapture videoCapture)
            {
                if (videoCapture != null)
                {
                    m_VideoCapture = videoCapture;

                    callback?.Invoke();
                }
                else
                {
                    Debug.LogError("[FPStreammingCast] Failed to create VideoCapture Instance!");
                }
            });
        }

        /// <summary> Starts video capture. </summary>
        public IEnumerator StartVideoCapture()
        {
            Resolution cameraResolution = GetResolutionByLevel(m_ResolutionLevel);
            Debug.LogFormat("[FPStreammingCast] cameraResolution:" + cameraResolution);

            if (m_VideoCapture != null)
            {
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 1f;
                cameraParameters.frameRate = NativeConstants.RECORD_FPS_DEFAULT;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                cameraParameters.blendMode = m_BlendMode;
                cameraParameters.audioState = m_AudioState;
                cameraParameters.monophonic = true;

                // Send the audioState to server.
                if (m_NetWorker != null)
                {
                    LitJson.JsonData json = new LitJson.JsonData();
                    json["useAudio"] = (cameraParameters.audioState != AudioState.None);
                    m_NetWorker.SendMsg(json, (response) =>
                    {
                        bool result;
                        if (bool.TryParse(response["success"].ToString(), out result) && result)
                        {
                            m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode, true);
                        }
                        else
                        {
                            Debug.LogError("[FPStreammingCast] Can not received response from server.");
                        }
                    });
                }
                else
                {
                    m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode, true);
                }
            }
            else
            {
                Debug.Log("[FPStreammingCast]  VideoCapture object is null...");
            }
            yield return new WaitForEndOfFrame();
        }

        /// <summary> Stops video capture. </summary>
        public void StopVideoCapture()
        {
            Debug.LogFormat("[FPStreammingCast] Stop Video Capture!");
            m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
        }

        /// <summary> Executes the 'started video capture mode' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStartedVideoCaptureMode(XREALVideoCapture.VideoCaptureResult result)
        {
            if (!result.success)
            {
                Debug.LogFormat("[FPStreammingCast] Started Video Capture Mode Faild!");
                return;
            }

            Debug.LogFormat("[FPStreammingCast] Started Video Capture Mode!");
            m_VideoPath = m_IsStreamStarted ? RTPPath : VideoSavePath;
            m_SavedLocal = !m_IsStreamStarted;
            m_VideoCapture.StartRecordingAsync(m_VideoPath, OnStartedRecordingVideo);
            m_VideoCapture.GetContext().GetBehaviour().CaptureCamera.cullingMask = m_CullingMask.value;
            m_VideoCapture.GetContext().GetBehaviour().CaptureCamera.backgroundColor = useGreenBackGround ? Color.green : Color.black;
        }

        /// <summary> Executes the 'stopped video capture mode' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStoppedVideoCaptureMode(XREALVideoCapture.VideoCaptureResult result)
        {
            Debug.LogFormat("[FPStreammingCast] Stopped Video Capture Mode!");
            m_VideoCapture = null;

            if (m_SavedLocal)
            {
                string filename = string.Format("Xreal_Shot_Video_{0}.mp4", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
                StartCoroutine(DelayInsertVideoToGallery(m_VideoPath, filename, "Record"));
                m_SavedLocal = false;
            }
        }
        IEnumerator DelayInsertVideoToGallery(string originFilePath, string displayName, string folderName)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.LogFormat("[FPStreammingCast] InsertVideoToGallery: {0}, {1} => {2}", displayName, originFilePath, folderName);
            if (galleryDataTool == null)
            {
                galleryDataTool = new GalleryDataProvider();
            }

            galleryDataTool.InsertVideo(originFilePath, displayName, folderName);
        }

        /// <summary> Executes the 'started recording video' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStartedRecordingVideo(XREALVideoCapture.VideoCaptureResult result)
        {
            Debug.LogFormat("[FPStreammingCast] Started Recording Video!");
        }

        /// <summary> Executes the 'stopped recording video' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStoppedRecordingVideo(XREALVideoCapture.VideoCaptureResult result)
        {
            Debug.LogFormat("[FPStreammingCast] Stopped Recording Video!");
            m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
            if (m_NetWorker != null)
            {
                m_NetWorker?.Close();
                m_NetWorker = null;
            }
        }
        #endregion
    }
}
