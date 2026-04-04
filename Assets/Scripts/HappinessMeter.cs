using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HappinessMeter : MonoBehaviour
{
    [SerializeField] private Image meterImage;
    [SerializeField] private Sprite sadSprite;
    [SerializeField] private Sprite mehSprite;
    [SerializeField] private Sprite happySprite;

    private int lastHappiness = int.MinValue;

    private void Awake()
    {
        if (meterImage == null)
            meterImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void OnEnable()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        int current = GameManager.Instance.GetHappinessPercent();

        if (current != lastHappiness)
        {
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        if (meterImage == null)
        {
            Debug.LogWarning("[HappinessMeter] meterImage is not assigned.", this);
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[HappinessMeter] GameManager.Instance is null.", this);
            return;
        }

        int happiness = GameManager.Instance.GetHappinessPercent();
        lastHappiness = happiness;

        if (happiness <= 30)
        {
            meterImage.sprite = sadSprite;
            Debug.Log($"[HappinessMeter] Updated to SAD. Happiness = {happiness}", this);
        }
        else if (happiness <= 70)
        {
            meterImage.sprite = mehSprite;
            Debug.Log($"[HappinessMeter] Updated to MEH. Happiness = {happiness}", this);
        }
        else
        {
            meterImage.sprite = happySprite;
            Debug.Log($"[HappinessMeter] Updated to HAPPY. Happiness = {happiness}", this);
        }
    }
}
