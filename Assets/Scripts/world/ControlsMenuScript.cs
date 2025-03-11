using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMenuScript : MonoBehaviour
{
 public RectTransform tabPanel; // Reference to the panel
    public Button toggleButton;    // Reference to the button
    public float slideSpeed = 10f; // Speed of tab movement

    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private bool isExpanded = false; // Track tab state

    void Start()
    {
        float panelWidth = tabPanel.sizeDelta.x; // Get correct width

        // Adjust hidden and visible positions
        hiddenPosition = new Vector2(panelWidth, tabPanel.anchoredPosition.y);
        visiblePosition = new Vector2(0, tabPanel.anchoredPosition.y);

        // Start in hidden position
        tabPanel.anchoredPosition = hiddenPosition;

        // Add button click listener
        toggleButton.onClick.AddListener(ToggleTab);
    }


    public void ToggleTab()
    {
        isExpanded = !isExpanded;
        StopAllCoroutines();
        StartCoroutine(MoveTab(isExpanded ? visiblePosition : hiddenPosition));
    }

    System.Collections.IEnumerator MoveTab(Vector2 targetPos)
    {
        while (Vector2.Distance(tabPanel.anchoredPosition, targetPos) > 0.1f)
        {
            tabPanel.anchoredPosition = Vector2.Lerp(tabPanel.anchoredPosition, targetPos, slideSpeed * Time.deltaTime);
            yield return null;
        }
        tabPanel.anchoredPosition = targetPos;
    }
}
