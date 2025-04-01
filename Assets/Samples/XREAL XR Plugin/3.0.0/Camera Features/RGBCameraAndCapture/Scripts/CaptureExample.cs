using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL.Samples
{
#if UNITY_ANDROID && !UNITY_EDITOR
    using GalleryDataProvider = NativeGalleryDataProvider;
#else
    using GalleryDataProvider = MockGalleryDataProvider;
#endif
    public class CaptureExample : MonoBehaviour
    {
        public enum ResolutionLevel
        {
            High,
            Middle,
            Low,
        }
        [SerializeField] private Button m_VideoButton;
        [SerializeField] private Button m_PhotoButton;

        [SerializeField] private Slider m_SliderMic;
        [SerializeField] private Text m_TextMic;
        [SerializeField] private Slider m_SliderApp;
        [SerializeField] private Text m_TextApp;

        [SerializeField] private Dropdown m_QualityDropDown;
        [SerializeField] private Dropdown m_RenderModeDropDown;
        [SerializeField] private Dropdown m_AudioStateDropDown;
        [SerializeField] private Dropdown m_CaptureSideDropDown;
        [SerializeField] private Toggle m_UseGreenBGToggle;

        [SerializeField] private RawImage m_PreviewRawImage;

        List<string> _ResolutionOptions = new List<string>() {
            ResolutionLevel.High.ToString(),
            ResolutionLevel.Middle.ToString(),
            ResolutionLevel.Low.ToString()
        };
        List<string> _RendermodeOptions = new List<string>() {
            BlendMode.Blend.ToString(),
            BlendMode.CameraOnly.ToString(),
            BlendMode.VirtualOnly.ToString()
        };
        List<string> _AudioStateOptions = new List<string>() {
            AudioState.MicAudio.ToString(),
            AudioState.ApplicationAudio.ToString(),
            AudioState.ApplicationAndMicAudio.ToString(),
            AudioState.None.ToString()
        };
        List<string> _CaptureSideOptions = new List<string>() {
            CaptureSide.Single.ToString(),
            CaptureSide.Both.ToString()
        };

        public BlendMode blendMode = BlendMode.Blend;
        public ResolutionLevel resolutionLevel = ResolutionLevel.High;
        public AudioState audioState = AudioState.ApplicationAudio;
        public CaptureSide captureside = CaptureSide.Single;
        public bool useGreenBackGround = false;

        /// <summary> Save the video to Application.persistentDataPath. </summary>
        /// <value> The full pathname of the video save file. </value>
        public string VideoSavePath
        {
            get
            {
                string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
                string filename = string.Format("Xreal_Record_{0}.mp4", timeStamp);
                return Path.Combine(Application.persistentDataPath, filename);
            }
        }

        GalleryDataProvider galleryDataTool;

        /// <summary> The video capture. </summary>
        XREALVideoCapture m_VideoCapture = null;

        /// <summary> The photo capture object. </summary>
        private XREALPhotoCapture m_PhotoCapture;
        /// <summary> The camera resolution. </summary>
        private Resolution m_CameraResolution;
        private bool isOnPhotoProcess = false;

        void Awake()
        {
            m_QualityDropDown.options.Clear();
            m_QualityDropDown.AddOptions(_ResolutionOptions);
            int default_quality_index = 0;
            for (int i = 0; i < _ResolutionOptions.Count; i++)
            {
                if (_ResolutionOptions[i].Equals(resolutionLevel.ToString()))
                {
                    default_quality_index = i;
                }
            }
            m_QualityDropDown.value = default_quality_index;
            m_QualityDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse<ResolutionLevel>(_ResolutionOptions[index],
                    out resolutionLevel);
            });

            m_RenderModeDropDown.options.Clear();
            m_RenderModeDropDown.AddOptions(_RendermodeOptions);
            int default_blendmode_index = 0;
            for (int i = 0; i < _RendermodeOptions.Count; i++)
            {
                if (_RendermodeOptions[i].Equals(blendMode.ToString()))
                {
                    default_blendmode_index = i;
                }
            }
            m_RenderModeDropDown.value = default_blendmode_index;
            m_RenderModeDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse<BlendMode>(_RendermodeOptions[index],
                    out blendMode);
            });

            m_AudioStateDropDown.options.Clear();
            m_AudioStateDropDown.AddOptions(_AudioStateOptions);
            int default_audiostate_index = 0;
            for (int i = 0; i < _AudioStateOptions.Count; i++)
            {
                if (_AudioStateOptions[i].Equals(audioState.ToString()))
                {
                    default_audiostate_index = i;
                }
            }
            m_AudioStateDropDown.value = default_audiostate_index;
            m_AudioStateDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse<AudioState>(_AudioStateOptions[index],
                    out audioState);
            });

            m_CaptureSideDropDown.options.Clear();
            m_CaptureSideDropDown.AddOptions(_CaptureSideOptions);
            int default_captureside_index = 0;
            for (int i = 0; i < _CaptureSideOptions.Count; i++)
            {
                if (_CaptureSideOptions[i].Equals(captureside.ToString()))
                {
                    default_captureside_index = i;
                }
            }
            m_CaptureSideDropDown.value = default_captureside_index;
            m_CaptureSideDropDown.onValueChanged.AddListener((index) =>
            {
                Enum.TryParse<CaptureSide>(_CaptureSideOptions[index],
                    out captureside);
            });

            m_UseGreenBGToggle.isOn = useGreenBackGround;
            m_UseGreenBGToggle.onValueChanged.AddListener((val) =>
            {
                useGreenBackGround = val;
            });

            if (m_SliderMic != null)
            {
                m_SliderMic.maxValue = 5.0f;
                m_SliderMic.minValue = 0.1f;
                m_SliderMic.value = 1;
                m_SliderMic.onValueChanged.AddListener(OnSlideMicValueChange);
            }

            if (m_SliderApp != null)
            {
                m_SliderApp.maxValue = 5.0f;
                m_SliderApp.minValue = 0.1f;
                m_SliderApp.value = 1;
                m_SliderApp.onValueChanged.AddListener(OnSlideAppValueChange);
            }

            m_VideoButton.onClick.AddListener(RecordVideo);
            m_PhotoButton.onClick.AddListener(TakeAPhoto);

            RefreshUIState();
        }

        void OnSlideMicValueChange(float val)
        {
            if (m_VideoCapture != null)
            {
                VideoEncoder encoder = m_VideoCapture.GetContext().GetEncoder() as VideoEncoder;
                if (encoder != null)
                    encoder.AdjustVolume(RecorderIndex.REC_MIC, val);
            }
            RefreshUIState();
        }

        void OnSlideAppValueChange(float val)
        {
            if (m_VideoCapture != null)
            {
                VideoEncoder encoder = m_VideoCapture.GetContext().GetEncoder() as VideoEncoder;
                if (encoder != null)
                    encoder.AdjustVolume(RecorderIndex.REC_APP, val);
            }
            RefreshUIState();
        }

        void CreateVideoCapture(Action callback)
        {
            XREALVideoCapture.CreateAsync(false, delegate (XREALVideoCapture videoCapture)
            {
                Debug.Log("Created VideoCapture Instance!");
                if (videoCapture != null)
                {
                    m_VideoCapture = videoCapture;
                    callback?.Invoke();
                }
                else
                {
                    Debug.LogError("Failed to create VideoCapture Instance!");
                }
            });
        }

        public void RecordVideo()
        {
            if (m_VideoCapture == null)
            {
                CreateVideoCapture(() =>
                {
                    StartVideoCapture();
                });
            }
            else if (m_VideoCapture.IsRecording)
            {
                this.StopVideoCapture();
            }
            else
            {
                this.StartVideoCapture();
            }
        }

        void RefreshUIState()
        {
            var recordText = m_VideoButton.transform.Find("Text");
            if (recordText)
            {
                bool notStarted = m_VideoCapture == null || !m_VideoCapture.IsRecording;
                recordText.GetComponent<Text>().text = notStarted ? "Start Record" : "Stop Record";
            }

            if (m_TextMic != null && m_SliderMic != null)
                m_TextMic.text = m_SliderMic.value.ToString("F1");
            if (m_TextApp != null && m_SliderApp != null)
                m_TextApp.text = m_SliderApp.value.ToString("F1");
        }

        /// <summary> Starts video capture. </summary>
        public void StartVideoCapture()
        {
            if (m_VideoCapture == null || m_VideoCapture.IsRecording)
            {
                Debug.LogWarning("Can not start video capture!");
                return;
            }

            CameraParameters cameraParameters = new CameraParameters();
            Resolution cameraResolution = GetResolutionByLevel(resolutionLevel);
            cameraParameters.cameraType = CameraType.RGB;
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.frameRate = NativeConstants.RECORD_FPS_DEFAULT;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.PNG;
            cameraParameters.blendMode = blendMode;
            // Set audio state, audio record needs the permission of "android.permission.RECORD_AUDIO",
            // Add it to your "AndroidManifest.xml" file in "Assets/Plugin".
            cameraParameters.audioState = audioState;
            cameraParameters.captureSide = captureside;
            cameraParameters.backgroundColor = useGreenBackGround ? Color.green : Color.black;

            m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode, true);
        }

        private Resolution GetResolutionByLevel(ResolutionLevel level)
        {
            var resolutions = XREALVideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height);
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

        /// <summary> Stops video capture. </summary>
        public void StopVideoCapture()
        {
            if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
            {
                Debug.LogWarning("Can not stop video capture!");
                return;
            }

            Debug.Log("Stop Video Capture!");
            m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
        }

        /// <summary> Executes the 'started video capture mode' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStartedVideoCaptureMode(XREALVideoCapture.VideoCaptureResult result)
        {
            if (!result.success)
            {
                Debug.Log("Started Video Capture Mode faild!");
                return;
            }

            Debug.Log("Started Video Capture Mode!");
            if (m_SliderMic != null && m_SliderApp != null)
            {
                float volumeMic = m_SliderMic.value;
                float volumeApp = m_SliderApp.value;
                m_VideoCapture.StartRecordingAsync(VideoSavePath, OnStartedRecordingVideo, volumeMic, volumeApp);
            }
            else
            {
                m_VideoCapture.StartRecordingAsync(VideoSavePath, OnStartedRecordingVideo);
            }

            m_PreviewRawImage.texture = m_VideoCapture.PreviewTexture;
        }

        /// <summary> Executes the 'started recording video' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStartedRecordingVideo(XREALVideoCapture.VideoCaptureResult result)
        {
            if (!result.success)
            {
                Debug.Log("Started Recording Video Faild!");
                return;
            }

            Debug.Log("Started Recording Video!");
            RefreshUIState();
        }

        /// <summary> Executes the 'stopped recording video' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStoppedRecordingVideo(XREALVideoCapture.VideoCaptureResult result)
        {
            if (!result.success)
            {
                Debug.Log("Stopped Recording Video Faild!");
                return;
            }

            Debug.Log("Stopped Recording Video!");
            m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
        }

        /// <summary> Executes the 'stopped video capture mode' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStoppedVideoCaptureMode(XREALVideoCapture.VideoCaptureResult result)
        {
            Debug.Log("Stopped Video Capture Mode!");
            RefreshUIState();

            var encoder = m_VideoCapture.GetContext().GetEncoder() as VideoEncoder;
            string path = encoder.EncodeConfig.outPutPath;
            string filename = string.Format("Xreal_Shot_Video_{0}.mp4", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

            StartCoroutine(DelayInsertVideoToGallery(path, filename, "Record"));

            // Release video capture resource.
            m_VideoCapture.Dispose();
            m_VideoCapture = null;
        }

        void OnDestroy()
        {
            // Release video capture resource.
            m_VideoCapture?.Dispose();
            m_VideoCapture = null;

            // Relase photo capture resource
            m_PhotoCapture?.Dispose();
            m_PhotoCapture = null;
        }

        IEnumerator DelayInsertVideoToGallery(string originFilePath, string displayName, string folderName)
        {
            yield return new WaitForSeconds(0.1f);
            InsertVideoToGallery(originFilePath, displayName, folderName);
        }

        public void InsertVideoToGallery(string originFilePath, string displayName, string folderName)
        {
            Debug.LogFormat("InsertVideoToGallery: {0}, {1} => {2}", displayName, originFilePath, folderName);
            if (galleryDataTool == null)
            {
                galleryDataTool = new GalleryDataProvider();
            }

            galleryDataTool.InsertVideo(originFilePath, displayName, folderName);
        }



        /// <summary> Use this for initialization. </summary>
        void CreatePhotoCapture(Action<XREALPhotoCapture> onCreated)
        {
            if (m_PhotoCapture != null)
            {
                Debug.Log("[TakePicture] CreatePhotoCapture: The XREALPhotoCapture has already been created.");
                return;
            }

            // Create a PhotoCapture object
            XREALPhotoCapture.CreateAsync(false, delegate (XREALPhotoCapture captureObject)
            {
                m_CameraResolution = XREALPhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

                if (captureObject == null)
                {
                    Debug.LogError("Can not get a captureObject.");
                    return;
                }

                m_PhotoCapture = captureObject;

                CameraParameters cameraParameters = new CameraParameters();
                Resolution cameraResolution = GetResolutionByLevel(resolutionLevel);
                cameraParameters.cameraType = CameraType.RGB;
                cameraParameters.hologramOpacity = 0.0f;
                cameraParameters.frameRate = NativeConstants.RECORD_FPS_DEFAULT;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.PNG;
                cameraParameters.blendMode = blendMode;
                cameraParameters.audioState = AudioState.None;
                cameraParameters.captureSide = captureside;
                cameraParameters.backgroundColor = useGreenBackGround ? Color.green : Color.black;

                // Activate the camera
                m_PhotoCapture.StartPhotoModeAsync(cameraParameters, delegate (XREALPhotoCapture.PhotoCaptureResult result)
                {
                    Debug.Log("Start PhotoMode Async");
                    if (result.success)
                    {
                        onCreated?.Invoke(m_PhotoCapture);
                    }
                    else
                    {
                        isOnPhotoProcess = false;
                        this.ClosePhotoCapture();
                        Debug.LogError("[TakePicture] CreatePhotoCapture: Start PhotoMode failed." + result.resultType);
                    }
                }, true);
            });
        }

        /// <summary> Take a photo. </summary>
        void TakeAPhoto()
        {
            if (isOnPhotoProcess)
            {
                Debug.LogWarning("[TakePicture] TakeAPhoto: Currently in the process of taking pictures, Can not take photo .");
                return;
            }


            isOnPhotoProcess = true;

            if (m_PhotoCapture == null)
            {
                this.CreatePhotoCapture((capture) =>
                {
                    capture.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            }
            else
            {
                m_PhotoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);
            }
        }

        /// <summary> Executes the 'captured photo memory' action. </summary>
        /// <param name="result">            The result.</param>
        /// <param name="photoCaptureFrame"> The photo capture frame.</param>
        void OnCapturedPhotoToMemory(XREALPhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            Debug.Log("[TakePicture] OnCapturedPhotoToMemory");
            var targetTexture = new Texture2D(m_CameraResolution.width, m_CameraResolution.height);
            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);

            SaveTextureAsPNG(photoCaptureFrame);
            SaveTextureToGallery(photoCaptureFrame);
            // Release camera resource after capture the photo.
            this.ClosePhotoCapture();
        }

        void SaveTextureAsPNG(PhotoCaptureFrame photoCaptureFrame)
        {
            Debug.Log("[TakePicture] SaveTextureAsPNG");
            if (photoCaptureFrame.TextureData == null)
            {
                Debug.LogError("[TakePicture] SaveTextureAsPNG: TextureData is null");
                return;
            }
            try
            {
                string filename = string.Format("Xreal_Shot_{0}.png", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
                string path = string.Format("{0}/XrealShots", Application.persistentDataPath);
                string filePath = string.Format("{0}/{1}", path, filename);

                byte[] _bytes = photoCaptureFrame.TextureData;
                Debug.LogFormat("[TakePicture] Photo capture: {0}Kb was saved to [{1}]", _bytes.Length / 1024, filePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.WriteAllBytes(string.Format("{0}/{1}", path, filename), _bytes);

            }
            catch (Exception e)
            {
                Debug.LogError($"Save picture failed! {e}");
                throw e;
            }
        }

        /// <summary> Closes this object. </summary>
        void ClosePhotoCapture()
        {
            if (m_PhotoCapture == null)
            {
                Debug.LogError("The XREALPhotoCapture has not been created.");
                return;
            }
            // Deactivate our camera
            m_PhotoCapture.StopPhotoModeAsync(OnStoppedPhotoMode);
        }

        /// <summary> Executes the 'stopped photo mode' action. </summary>
        /// <param name="result"> The result.</param>
        void OnStoppedPhotoMode(XREALPhotoCapture.PhotoCaptureResult result)
        {
            // Shutdown our photo capture resource
            m_PhotoCapture?.Dispose();
            m_PhotoCapture = null;
            isOnPhotoProcess = false;
        }

        public void SaveTextureToGallery(PhotoCaptureFrame photoCaptureFrame)
        {
            Debug.Log("[TakePicture] SaveTextureToGallery");
            if (photoCaptureFrame.TextureData == null)
                return;
            try
            {
                string filename = string.Format("Xreal_Shot_{0}.png", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
                byte[] _bytes = photoCaptureFrame.TextureData;
                Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + filename);
                if (galleryDataTool == null)
                {
                    galleryDataTool = new GalleryDataProvider();
                }

                galleryDataTool.InsertImage(_bytes, filename, "Screenshots");
            }
            catch (Exception e)
            {
                Debug.LogError("[TakePicture] Save picture faild!");
                throw e;
            }
        }
    }
}
