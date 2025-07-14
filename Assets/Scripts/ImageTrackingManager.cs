using UnityEngine;
using Vuforia;
using TMPro;

[RequireComponent(typeof(ObserverBehaviour))]
public class ImageTrackingManager : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Όνομα του Image Target όπως φαίνεται στο Vuforia. " +
             "Αν μείνει κενό, θα χρησιμοποιηθεί αυτόματα το TargetName του Observer.")]
    public string targetImageName = "";

    [Header("UI References")]
    [Tooltip("Το UI Panel που περιέχει την ερώτηση (να είναι disabled αρχικά)")]
    public GameObject questionPanel;

    [Tooltip("Το TextMeshProUGUI για το αρχικό μήνυμα")]
    public TextMeshProUGUI instructionText;

    [Header("Optional")]
    [Tooltip("Προαιρετικό offset για να υψωθεί το UI πάνω από το marker")]
    public float heightOffset = 0f;

    private ObserverBehaviour observer;

    void Awake()
    {
        observer = GetComponent<ObserverBehaviour>();
        observer.OnTargetStatusChanged += OnTargetStatusChanged;

        // Αν το πεδίο είναι κενό, πάρε το όνομα από το target
        if (string.IsNullOrEmpty(targetImageName))
            targetImageName = observer.TargetName;

        if (questionPanel) questionPanel.SetActive(false);

        if (instructionText)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text =
                $"Για να αποκαλυφθεί η ερώτηση πρέπει να βρεις τον {targetImageName}";
        }

        if (heightOffset != 0 && questionPanel)
            questionPanel.transform.localPosition += Vector3.up * heightOffset;
    }

    void OnDestroy()
    {
        if (observer != null)
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool tracked = status.Status == Status.TRACKED ||
                       status.Status == Status.EXTENDED_TRACKED;

        if (!tracked || behaviour.TargetName != targetImageName)
            return;

        // 🔻 Απενεργοποίησε ΟΛΟ το container του μηνύματος
        if (instructionText && instructionText.transform.parent != null)
            instructionText.transform.parent.gameObject.SetActive(false);

        // Ενεργοποίησε το panel με την ερώτηση
        if (questionPanel) questionPanel.SetActive(true);

        // Προαιρετικά σταματάμε να ακούμε περαιτέρω status
        observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

}
