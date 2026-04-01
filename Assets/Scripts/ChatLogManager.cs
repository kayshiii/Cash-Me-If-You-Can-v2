using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ChatLogManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject chatLogRoot;      
    [SerializeField] private TextMeshProUGUI logTextDisplay;
    [SerializeField] private ScrollRect scrollRect;

    private void Awake()
    {
        if (chatLogRoot != null) chatLogRoot.SetActive(false);
        if (logTextDisplay != null) logTextDisplay.text = "";
    }

    public void ToggleLog()
    {
        bool isActive = !chatLogRoot.activeSelf;
        chatLogRoot.SetActive(isActive);

        if (isActive)
        {
            StartCoroutine(ScrollToBottom());
        }
    }

    public void AddLogEntry(string speakerName, string message)
    {
        if (logTextDisplay == null) return;

        logTextDisplay.text += $"<b>{speakerName}:</b> {message}\n\n";

        if (chatLogRoot != null && chatLogRoot.activeInHierarchy)
        {
            StartCoroutine(ScrollToBottom());
        }
    }

    private System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}