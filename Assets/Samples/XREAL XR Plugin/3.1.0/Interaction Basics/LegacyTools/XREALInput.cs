using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Unity.XR.XREAL.Samples
{
    public enum ControllerHandEnum
    {
        None = -1,
        RightHand,
        LeftHand,
    }

    public enum ControllerButton
    {
        TRIGGER = 0x01,
        APP = 0x02,
        HOME = 0x04,
    }

    public enum ButtonEventType
    {
        Down,
        Pressing,
        LongPressDown,
        Up,
        Click,
    }

    /// <summary>
    /// A utility class for managing input compatibility with legacy versions of the NRSdk.
    /// This class ensures backward compatibility with older SDK versions.
    /// 
    /// Note: This class only supports InputSource types of Controller and ControllerAndHands.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class XREALInput : SingletonMonoBehaviour<XREALInput>
    {
        /// <summary> The time interval (in seconds) within which a click is considered valid. </summary>
        public const float ClickInterval = 0.3f;
        /// <summary> The time interval (in seconds) required for a press to be considered a long press. </summary>
        public const float LongPressInterval = 1.0f;

        /// <summary> The current input source type. </summary>
        [ReadOnly]
        [SerializeField]
        private InputSource mInputSourceType = InputSource.Controller;
        public static InputSource CurrentInputSourceType
        {
            get
            {
                if (s_Singleton != null)
                    return Singleton.mInputSourceType;
                return XREALPlugin.GetInputSource();
            }
        }

        /// <summary> The InputActionProperty for the TRIGGER button action. </summary>
        [SerializeField]
        private InputActionProperty m_TriggerActionProperty;
        /// <summary> The InputActionProperty for the HOME button action. </summary>
        [SerializeField]
        private InputActionProperty m_HomeActionProperty;
        /// <summary> The InputActionProperty for the APP button action. </summary>
        [SerializeField]
        private InputActionProperty m_AppActionProperty;
        /// <summary> The InputActionProperty for the touchpad scroll action. </summary>
        [SerializeField]
        private InputActionProperty m_TouchActionProperty;

        /// <summary> The main camera used in the XROrigin setup. </summary>
        public Camera MainCamera => XREALUtility.MainCamera;

        /// <summary> Triggered before the controller is recentered. </summary>
        public static Action OnBeforeControllerRecenter;
        /// <summary> Triggered after the controller has been recentered. </summary>
        public static Action OnControllerRecentered;
        /// <summary> Triggered when an InputSource starts. </summary>
        public static Action<InputSource> OnInputSourceStarted;
        /// <summary> Triggered when an InputSource stops </summary>
        public static Action<InputSource> OnInputSourceStop;

        private Dictionary<ControllerButton, Action>[] mRightListenerArr = new Dictionary<ControllerButton, Action>[Enum.GetValues(typeof(ButtonEventType)).Length];
        private Dictionary<ControllerButton, float> mRightDownTimeDic = new Dictionary<ControllerButton, float>();

        private int mControllerButtonValue = 0;
        private int mControllerLongButtonValue = 0;
        private int mControllerLastButtonValue = 0;
        private int mControllerLastLongButtonValue = 0;

        private bool mControllerTouchpadPressed = false;
        private bool mControllerTouchpadLastPressed = false;
        private Vector2 mControllerTouchPos = Vector2.zero;
        private Vector2 mControllerLastTouchPos = Vector2.zero;
        private Vector2 mControllerTouchDelta = Vector2.zero;
        private Coroutine mAfterUpdateCoroutine = null;

        protected override void Awake()
        {
            base.Awake();
            mInputSourceType = XREALPlugin.GetInputSource();
#if UNITY_EDITOR && UNITY_ANDROID
            mInputSourceType = XREALSettings.GetSettings().InitialInputSource;
#endif
            if (m_TriggerActionProperty.action != null)
            {
                m_TriggerActionProperty.action.performed += TriggerActionPerformed;
                m_TriggerActionProperty.action.canceled += TriggerActionCanceled;
            }
            if (m_HomeActionProperty.action != null)
            {
                m_HomeActionProperty.action.performed += HomeActionPerformed;
                m_HomeActionProperty.action.performed += HomeActionCanceled;
            }
            if (m_AppActionProperty.action != null)
            {
                m_AppActionProperty.action.performed += AppActionPerformed;
                m_AppActionProperty.action.canceled += AppActionCanceled;
            }
            if (m_TouchActionProperty.action != null)
            {
                m_TouchActionProperty.action.performed += TouchActionPerformed;
                m_TouchActionProperty.action.canceled += TouchActionCanceled;
            }
        }

        private void OnEnable()
        {
            if (mAfterUpdateCoroutine != null)
                StopCoroutine(mAfterUpdateCoroutine);
            mAfterUpdateCoroutine = StartCoroutine(SyncAfterUpdate());
        }

        private void Update()
        {
            if (mControllerTouchpadPressed || !mControllerTouchpadLastPressed)
            {
                mControllerTouchDelta = mControllerTouchPos - mControllerLastTouchPos;
            }
            CheckButtonEvent();
        }

        private void OnDisable()
        {
            if (mAfterUpdateCoroutine != null)
            {
                StopCoroutine(mAfterUpdateCoroutine);
                mAfterUpdateCoroutine = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_TriggerActionProperty.action != null)
            {
                m_TriggerActionProperty.action.performed -= TriggerActionPerformed;
                m_TriggerActionProperty.action.canceled -= TriggerActionCanceled;
            }
            if (m_HomeActionProperty.action != null)
            {
                m_HomeActionProperty.action.performed -= HomeActionPerformed;
                m_HomeActionProperty.action.performed -= HomeActionCanceled;
            }
            if (m_AppActionProperty.action != null)
            {
                m_AppActionProperty.action.performed -= AppActionPerformed;
                m_AppActionProperty.action.canceled -= AppActionCanceled;
            }
            if (m_TouchActionProperty.action != null)
            {
                m_TouchActionProperty.action.performed -= TouchActionPerformed;
                m_TouchActionProperty.action.canceled -= TouchActionCanceled;
            }
        }

        private IEnumerator SyncAfterUpdate()
        {
            WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
            while (true)
            {
                yield return frameEnd;
                mControllerLastButtonValue = mControllerButtonValue;
                mControllerLastLongButtonValue = mControllerLongButtonValue;
                mControllerLastTouchPos = mControllerTouchPos;
                mControllerTouchpadLastPressed = mControllerTouchpadPressed;
            }
        }

        /// <summary>
        /// Sets the current input source type.
        /// This method allows you to specify which type of input source is being used, 
        /// </summary>
        /// <param name="inputSourceType"></param>
        /// <returns></returns>
        public static bool SetInputSource(InputSource inputSourceType)
        {
            if (s_Singleton != null)
            {
                Debug.Log(string.Format("[input] xrealInput SetInputSource currentInput = {0} targetInput = {1}", Singleton.mInputSourceType, inputSourceType));
                if (Singleton.mInputSourceType == inputSourceType)
                    return false;
                OnInputSourceStop?.Invoke(Singleton.mInputSourceType);
                XREALPlugin.SetInputSource(inputSourceType);
                Singleton.mInputSourceType = inputSourceType;
                OnInputSourceStarted?.Invoke(inputSourceType);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the touchpad is currently being touched.
        /// </summary>
        /// <returns></returns>
        public static bool IsTouching()
        {
            if (s_Singleton != null)
                return (s_Singleton.mControllerButtonValue & (int)ControllerButton.TRIGGER) != 0;
            return false;
        }

        /// <summary>
        /// Checks if the touchpad has started a scroll interaction.
        /// </summary>
        /// <returns></returns>
        public static bool IsTouchScrollStart()
        {
            if (s_Singleton != null)
                return s_Singleton.mControllerTouchpadPressed && !s_Singleton.mControllerTouchpadLastPressed;
            return false;
        }

        /// <summary>
        /// Checks if the touchpad is currently scrolling.
        /// </summary>
        /// <returns></returns>
        public static bool IsTouchScrolling()
        {
            if (s_Singleton != null)
                return s_Singleton.mControllerTouchpadPressed;
            return false;
        }

        /// <summary>
        /// Checks if the touchpad has stopped scrolling.
        /// </summary>
        /// <returns></returns>
        public static bool IsTouchScrollStop()
        {
            if (s_Singleton != null)
                return s_Singleton.mControllerTouchpadLastPressed && !s_Singleton.mControllerTouchpadPressed;
            return false;
        }

        /// <summary>
        /// Gets the current touch position on the touchpad.
        /// /// </summary>
        /// <returns> values ranging from -1 to 1 for both X and Y axes </returns>
        public static Vector2 GetTouch()
        {
            if (s_Singleton != null)
            {
                if (s_Singleton.mControllerTouchpadPressed || !s_Singleton.mControllerTouchpadLastPressed)
                    return s_Singleton.mControllerTouchPos;
                else if (s_Singleton.mControllerTouchpadLastPressed)
                    return s_Singleton.mControllerLastTouchPos;
            }
            return Vector2.zero;
        }

        /// <summary>
        /// Gets the change in touch position on the touchpad since the last frame.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetDeltaTouch()
        {
            if (s_Singleton != null)
                return s_Singleton.mControllerTouchDelta;
            return Vector2.zero;
        }

        /// <summary>
        /// Gets the current position of the controller in 3D space.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetPosition()
        {
            if (CurrentInputSourceType == InputSource.Controller || CurrentInputSourceType == InputSource.ControllerAndHands)
            {
                Vector3 position = Vector3.zero;
                InputDevice inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out position);
                return position;
            }

            return Vector3.one;
        }

        /// <summary>
        /// Gets the current rotation of the controller in 3D space.
        /// </summary>
        /// <returns></returns>
        public static Quaternion GetRotation()
        {
            if (CurrentInputSourceType == InputSource.Controller || CurrentInputSourceType == InputSource.ControllerAndHands)
            {
                Quaternion rotation = Quaternion.identity;
                InputDevice inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);
                return rotation;
            }
            return Quaternion.identity;
        }

        /// <summary>
        /// Checks if the controller button was pressed down in the current frame.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetButtonDown(ControllerButton button)
        {
            if (s_Singleton != null)
                return ((s_Singleton.mControllerButtonValue & (int)button) != 0) && ((s_Singleton.mControllerLastButtonValue & (int)button) == 0);
            return false;
        }

        /// <summary>
        /// Checks if the controller button was was released in the current frame.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetButtonUp(ControllerButton button)
        {
            if (s_Singleton != null)
                return ((s_Singleton.mControllerLastButtonValue & (int)button) != 0) && ((s_Singleton.mControllerButtonValue & (int)button) == 0);
            return false;
        }

        /// <summary>
        /// Checks if the controller button is currently pressed.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetButton(ControllerButton button)
        {
            if (s_Singleton != null)
                return (s_Singleton.mControllerButtonValue & (int)button) != 0;
            return false;
        }

        /// <summary>
        /// Triggers a haptic vibration on the controller.
        /// </summary>
        /// <param name="durationSeconds"></param>
        /// <param name="amplitude"></param>
        /// <returns></returns>
        public static bool TriggerHapticVibration(float durationSeconds = 0.1f, float amplitude = 0.8f)
        {
#if UNITY_EDITOR
            return false;
#else
            return XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, amplitude, durationSeconds);
#endif
        }

        /// <summary>
        /// Triggers a haptic vibration on the controller.
        /// </summary>
        /// <param name="durationSeconds"></param>
        /// <param name="amplitude"></param>
        /// <returns></returns>
        public static bool TriggerHapticVibration(ControllerHandEnum hand, float durationSeconds = 0.1f, float amplitude = 0.8f)
        {
#if UNITY_EDITOR
            return false;
#else
            return XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, amplitude, durationSeconds);
#endif
        }

        /// <summary>
        /// Adds a listener for a specific controller button event.
        /// </summary>
        /// <param name="buttonEventType">The type of button event to listen for, represented by the <see cref="ButtonEventType"/> </param>
        /// <param name="button">The controller button to listen for, represented by the <see cref="ControllerButton"/> </param>
        /// <param name="action">The <see cref="Action"/> to execute when the specified button event occurs.</param>
        public static void AddButtonListener(ButtonEventType buttonEventType, ControllerButton button, Action action)
        {
            if (s_Singleton != null)
            {
                Debug.Log("[input] AddButtonListener " + buttonEventType + "  " + button);
                int btnEventID = (int)buttonEventType;
                if (Singleton.mRightListenerArr[btnEventID] == null)
                    Singleton.mRightListenerArr[btnEventID] = new Dictionary<ControllerButton, Action>();

                var buttonListernerArr = Singleton.mRightListenerArr;
                if (buttonListernerArr[btnEventID].ContainsKey(button))
                {
                    buttonListernerArr[btnEventID][button] += action;
                }
                else
                {
                    buttonListernerArr[btnEventID].Add(button, action);
                }
            }
        }

        /// <summary>
        /// Removes a listener for a specific controller button event.
        /// </summary>
        /// <param name="buttonEventType"></param>
        /// <param name="button"></param>
        /// <param name="action"></param>
        public static void RemoveButtonListener(ButtonEventType buttonEventType, ControllerButton button, Action action)
        {
            if (s_Singleton != null)
            {
                int btnEventID = (int)buttonEventType;
                if (Singleton.mRightListenerArr[btnEventID] == null)
                    Singleton.mRightListenerArr[btnEventID] = new Dictionary<ControllerButton, Action>();

                var buttonListernerArr = Singleton.mRightListenerArr;
                if (buttonListernerArr[btnEventID].ContainsKey(button) && buttonListernerArr[btnEventID][button] != null)
                {
                    buttonListernerArr[btnEventID][button] -= action;
                    if (buttonListernerArr[btnEventID][button] == null)
                    {
                        buttonListernerArr[btnEventID].Remove(button);
                    }
                }
            }
        }

        /// <summary>
        /// Re-centers the controller's orientation
        /// </summary>
        public static void RecenterController()
        {
            if (CurrentInputSourceType == InputSource.Controller || CurrentInputSourceType == InputSource.ControllerAndHands)
            {
                OnBeforeControllerRecenter?.Invoke();
                XREALPlugin.RecenterController();
                OnControllerRecentered?.Invoke();
            }
        }

        private void CheckButtonEvent()
        {
            if (mControllerButtonValue == 0 && mControllerLastButtonValue == 0)
                return;

            foreach (ControllerButton button in Enum.GetValues(typeof(ControllerButton)))
            {
                //down event
                int buttonNum = (int)button;
                if ((mControllerButtonValue & buttonNum) != 0 && !mRightDownTimeDic.ContainsKey(button))
                {
                    TryInvokeListener(ButtonEventType.Down, button);
                    mRightDownTimeDic[button] = Time.unscaledTime;
                }
                //press event
                if ((mControllerButtonValue & buttonNum) != 0)
                {
                    TryInvokeListener(ButtonEventType.Pressing, button);
                    if (Time.unscaledTime - mRightDownTimeDic[button] >= LongPressInterval)
                    {
                        mControllerLongButtonValue |= buttonNum;
                        if ((mControllerLastLongButtonValue & buttonNum) == 0)
                        {
                            TryInvokeListener(ButtonEventType.LongPressDown, button);
                        }
                    }
                }
                //up event
                if ((mControllerLastButtonValue & buttonNum) != 0 && (mControllerButtonValue & buttonNum) == 0)
                {
                    mControllerLongButtonValue &= ~buttonNum;

                    TryInvokeListener(ButtonEventType.Up, button);
                    if (Time.unscaledTime - mRightDownTimeDic[button] <= ClickInterval)
                        TryInvokeListener(ButtonEventType.Click, button);
                    mRightDownTimeDic.Remove(button);
                }
            }
        }

        private void TryInvokeListener(ButtonEventType buttonEventType, ControllerButton button)
        {
            var buttonListernerArr = mRightListenerArr;
            int btnEventID = (int)buttonEventType;
            if (buttonListernerArr == null || buttonListernerArr[btnEventID] == null)
                return;
            if (buttonListernerArr[btnEventID].ContainsKey(button) && buttonListernerArr[btnEventID][button] != null)
                buttonListernerArr[btnEventID][button].Invoke();
        }

        private void AppActionPerformed(InputAction.CallbackContext context)
        {
            mControllerButtonValue |= (int)ControllerButton.APP;
        }

        private void AppActionCanceled(InputAction.CallbackContext context)
        {
            mControllerButtonValue &= ~(int)ControllerButton.APP;
        }

        private void HomeActionPerformed(InputAction.CallbackContext context)
        {
            if (context.interaction is PressInteraction)
                mControllerButtonValue |= (int)ControllerButton.HOME;
        }

        private void HomeActionCanceled(InputAction.CallbackContext context)
        {
            mControllerButtonValue &= ~(int)ControllerButton.HOME;
        }

        private void TriggerActionPerformed(InputAction.CallbackContext context)
        {
            mControllerButtonValue |= (int)ControllerButton.TRIGGER;
        }

        private void TriggerActionCanceled(InputAction.CallbackContext context)
        {
            mControllerButtonValue &= ~(int)ControllerButton.TRIGGER;
        }

        private void TouchActionPerformed(InputAction.CallbackContext context)
        {
            mControllerTouchPos = context.ReadValue<Vector2>();
            mControllerTouchpadPressed = true;
        }

        private void TouchActionCanceled(InputAction.CallbackContext context)
        {
            mControllerTouchPos = Vector2.zero;
            mControllerTouchpadPressed = false;
        }
    }
}
