using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class TitleScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform titleLabel;
    [SerializeField] private Button newGenButton;
    [SerializeField] private Button loadGenButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationHeight = 10f;
    [SerializeField] private float animationDuration = 2f;

    // Events (Unity's equivalent of Godot signals)
    public UnityEvent OnNewGenerationPressed = new UnityEvent();
    public UnityEvent OnLoadGenerationPressed = new UnityEvent();

    private float originalY;
    private float animationTimer;
    private bool movingUp = true;

    private UIMain _uiMain;
    
    public void Initialize(UIMain uiMain)
    {
        _uiMain = uiMain;
    }

    void Start()
    {
        // Store original position
        if (titleLabel != null)
        {
            originalY = titleLabel.anchoredPosition.y;
        }

        // Connect button events
        if (newGenButton != null)
        {
            newGenButton.onClick.AddListener(HandleNewGenButtonPressed);
        }

        if (loadGenButton != null)
        {
            loadGenButton.onClick.AddListener(HandleLoadGenButtonPressed);
        }
    }

    void Update()
    {
        // Animate title
        if (titleLabel != null)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationDuration)
            {
                animationTimer = 0;
                movingUp = !movingUp;
            }

            float t = animationTimer / animationDuration;
            float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f); // Sine easing

            float targetY = movingUp ? 
                originalY + animationHeight : 
                originalY - animationHeight;

            float currentY = Mathf.Lerp(
                movingUp ? originalY - animationHeight : originalY + animationHeight,
                targetY,
                smoothT
            );

            titleLabel.anchoredPosition = new Vector2(
                titleLabel.anchoredPosition.x,
                currentY
            );
        }
    }

    private void HandleNewGenButtonPressed()
    {
        _uiMain?.OnMenuItemPressed(0);
    }
    
    private void HandleLoadGenButtonPressed()
    {
        _uiMain?.OnMenuItemPressed(2);
    }
}