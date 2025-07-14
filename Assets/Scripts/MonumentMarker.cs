using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class MonumentMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Marker Components")]
    public Image markerImage;
    public TextMeshProUGUI markerText;
    public Button markerButton;
    public GameObject lockIcon;
    
    [Header("Marker States")]
    public Color unlockedColor = Color.yellow;
    public Color lockedColor = Color.gray;
    public Color completedColor = Color.green;
    public Color hoverColor = Color.white;
    
    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float animationDuration = 0.3f;
    public float pulseScale = 1.2f;
    public float pulseSpeed = 1f;
    
    private int monumentIndex;
    private MapController mapController;
    private bool isCompleted = false;
    private bool isUnlocked = false;
    private Vector3 originalScale;
    private Color originalColor;
    private Tween pulseTween;
    
    public void Initialize(int index, MapController controller)
    {
        monumentIndex = index;
        mapController = controller;
        //originalScale = transform.localScale;
        originalScale = Vector3.one;

        if (markerImage != null)
            originalColor = markerImage.color;
        
        // Setup button if it exists
        if (markerButton != null)
        {
            markerButton.onClick.AddListener(OnMarkerClicked);
        }
        
        // Set initial marker text
        if (markerText != null)
        {
            markerText.text = (monumentIndex + 1).ToString();
        }
        
        // Initialize marker state
        UpdateMarkerState();
    }
    
    public void SetMarkerState(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateMarkerState();
    }
    
    public void SetCompleted(bool completed)
    {
        isCompleted = completed;
        UpdateMarkerState();
    }
    
    private void UpdateMarkerState()
    {
        if (markerImage != null)
        {
            Color targetColor;
            
            if (isCompleted)
            {
                targetColor = completedColor;
            }
            else if (isUnlocked)
            {
                targetColor = unlockedColor;
            }
            else
            {
                targetColor = lockedColor;
            }
            
            markerImage.color = targetColor;
            originalColor = targetColor;
        }
        
        // Update button interactability
        if (markerButton != null)
        {
            markerButton.interactable = isUnlocked;
        }
        
        // Show/hide lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }
        
        // Start pulse animation for unlocked but not completed markers
        if (isUnlocked && !isCompleted)
        {
            StartPulseAnimation();
        }
        else
        {
            StopPulseAnimation();
        }
    }
    
    private void StartPulseAnimation()
    {
        StopPulseAnimation();
        
        pulseTween = markerImage.gameObject.transform.DOScale(originalScale * pulseScale, pulseSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void StopPulseAnimation()
    {
        if (pulseTween != null)
        {
            pulseTween.Kill();
            pulseTween = null;
        }
        
        transform.localScale = originalScale;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mapController != null)
        {
            mapController.OnMarkerHover(monumentIndex, true);
        }
        
        // Stop pulse animation during hover
        StopPulseAnimation();
        if(!isCompleted && isUnlocked)
        {
            // Hover animation
            transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(Ease.OutQuart);
        }
        
        // Change color on hover if unlocked
        if (isUnlocked && !isCompleted && markerImage != null)
        {
            markerImage.DOColor(hoverColor, animationDuration);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (mapController != null)
        {
            mapController.OnMarkerHover(monumentIndex, false);
        }
        
        // Return to original scale
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuart);
        
        // Return to original color
        if (markerImage != null)
        {
            markerImage.DOColor(originalColor, animationDuration);
        }
        
        // Resume pulse animation if needed
        if (isUnlocked && !isCompleted)
        {
            StartPulseAnimation();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnMarkerClicked();
    }
    
    private void OnMarkerClicked()
    {
        if (mapController != null)
        {
            mapController.OnMarkerClicked(monumentIndex);
        }
    }
    
    public bool IsCompleted()
    {
        return isCompleted;
    }
    
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
    
    public int GetMonumentIndex()
    {
        return monumentIndex;
    }
    
    // Method to manually trigger click animation
    public void PlayClickAnimation()
    {
        StopPulseAnimation();
        
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0.5f).OnComplete(() =>
        {
            if (isUnlocked && !isCompleted)
            {
                StartPulseAnimation();
            }
        });
    }
    
    // Method to play unlock animation
    public void PlayUnlockAnimation()
    {
        if (markerImage != null)
        {
            // Flash effect
            markerImage.DOColor(Color.white, 0.1f).SetLoops(6, LoopType.Yoyo).OnComplete(() =>
            {
                markerImage.color = originalColor;
            });
        }
        
        // Scale bounce
        transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 1, 0.5f);
        
        // Hide lock icon with animation
        if (lockIcon != null && lockIcon.activeInHierarchy)
        {
            lockIcon.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                lockIcon.SetActive(false);
                lockIcon.transform.localScale = Vector3.one;
            });
        }
    }
    
    private void OnDestroy()
    {
        // Clean up tweens
        if (pulseTween != null)
        {
            pulseTween.Kill();
        }
        
        transform.DOKill();
        
        if (markerImage != null)
        {
            markerImage.DOKill();
        }
    }
}