using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.XR.XREAL.Samples
{
    public enum ButtonState
    {
        Normal,
        Pressed,
        Hovered,
        Disabled
    }

    /// <summary>
    /// This class listens to button state changes and updates the text color accordingly.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonTextColor : MonoBehaviour
    {
        private Text buttonText;
        private Button button;
        [SerializeField]
        private Color TextNormalColor;
        [SerializeField]
        private Color TextPressColor;
        [SerializeField]
        private Color TextHoverColor;
        [SerializeField]
        private Color TextDisableColor;

        private ButtonStateListener buttonStateListener;

        private void Start()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                buttonStateListener = button.AddOrGetComponent<ButtonStateListener>();
                buttonStateListener.onButtonStateChanged += HandleButtonStateChange;
            }
            if (buttonText == null)
            {
                for (var i = 0; i < transform.childCount; ++i)
                {
                    var text = transform.GetChild(i).GetComponent<Text>();
                    if (text != null)
                    {
                        buttonText = text;
                        break;
                    }
                }
            }
        }
        void HandleButtonStateChange(ButtonState state)
        {
            if (buttonText != null)
            {
                switch (state)
                {
                    case ButtonState.Normal:
                        buttonText.color = TextNormalColor;
                        break;
                    case ButtonState.Pressed:
                        buttonText.color = TextPressColor;
                        break;
                    case ButtonState.Hovered:
                        buttonText.color = TextHoverColor;
                        break;
                    case ButtonState.Disabled:
                        buttonText.color = TextDisableColor;
                        break;
                }
            }
        }

        void OnDestroy()
        {
            buttonStateListener.onButtonStateChanged -= HandleButtonStateChange;
        }
    }

    /// <summary>
    /// Listens to pointer events to determine the state of a button.
    /// </summary>
    [HideInInspector]
    public class ButtonStateListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public delegate void OnButtonStateChanged(ButtonState buttonState);

        /// <summary>
        /// Event triggered when the button state changes.
        /// </summary>
        public event OnButtonStateChanged onButtonStateChanged;

        private Button button;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        void OnEnable()
        {
            UpdateState(button.interactable ? ButtonState.Normal : ButtonState.Disabled);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.interactable)
            {
                UpdateState(ButtonState.Hovered);
                Debug.Log("Button Highlighted (Pointer Enter)");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (button.interactable)
            {
                UpdateState(ButtonState.Normal);
                Debug.Log("Button Normal (Pointer Exit)");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button.interactable)
            {
                UpdateState(ButtonState.Pressed);
                Debug.Log("Button Pressed (Pointer Down)");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (button.interactable)
            {
                UpdateState(ButtonState.Normal);
                Debug.Log("Button Released (Pointer Up)");
            }
        }

        void OnDisable()
        {
            UpdateState(ButtonState.Disabled);
        }

        private void UpdateState(ButtonState newState)
        {
            onButtonStateChanged?.Invoke(newState);
        }
    }
}
