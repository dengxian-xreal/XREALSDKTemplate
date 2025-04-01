using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The ConfirmDialog class provides functionality to display a confirmation dialog with a guide message.
/// </summary>
namespace Unity.XR.XREAL.Samples
{
    public class ConfirmDialog : MonoBehaviour
    {
        public static readonly string ANCHORS_USER_VIEW_GUIDE = "To ensure successful anchor loading, view the anchor from different angles and turn as many indicator bars green as possible before saving.";

        [SerializeField]
        private TMP_Text m_GuideText;

        [SerializeField]
        private Button m_ConfirmButton;

        [SerializeField]
        private GameObject m_Panel;

        private void Awake()
        {
            instance = this;
            m_Panel.SetActive(false);
        }

        private static ConfirmDialog instance;

        /// <summary>
        /// Gets the singleton instance of the ConfirmDialog.
        /// </summary>
        public static ConfirmDialog Instance => instance;

        /// <summary>
        /// Displays the confirmation dialog with the specified message.
        /// </summary>
        /// <param name="msg">The message to display in the guide text.</param>
        public void Show(string msg)
        {
            m_GuideText.text = msg;
            m_Panel.SetActive(true);
        }

        /// <summary>
        /// Waits asynchronously until the dialog is closed.
        /// </summary>
        /// <returns>A task that completes when the dialog is closed.</returns>
        public async Task WaitUntilClosed()
        {
            m_WaitCloseDialogTask = new TaskCompletionSource<bool>();
            await m_WaitCloseDialogTask.Task;
        }

        private TaskCompletionSource<bool> m_WaitCloseDialogTask = null;

        /// <summary>
        /// Closes the confirmation dialog and signals the waiting task.
        /// </summary>
        public void CloseDialog()
        {
            this.m_Panel.SetActive(false);

            if (m_WaitCloseDialogTask != null)
            {
                m_WaitCloseDialogTask.SetResult(true);
            }
        }
    }
}
