using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.FPS.Gameplay;

public class UIGazeSelection : MonoBehaviour
{
    [Tooltip("Drag your SelectionInputManager here")]
    public SelectionInputManager selectionManager;
    public EyeGaze eyeGaze;
    private GameObject lastHoveredButton;

    void OnEnable()
    {
        if (selectionManager != null)
        {
            selectionManager.OnObjectSelected += HandleSelection;
        }
        if(eyeGaze != null)
        {
            eyeGaze.OnGazeObjectChanged += HandleObjectHover;
        }
    }

    void OnDisable()
    {
        if (selectionManager != null)
        {
            selectionManager.OnObjectSelected -= HandleSelection;
        }
        if(eyeGaze != null)
        {
            eyeGaze.OnGazeObjectChanged -= HandleObjectHover;
        }
        

    }

    private void HandleObjectHover(GameObject hoveredObject)
    {
        // Reset previous hover
        if (lastHoveredButton != null && lastHoveredButton != hoveredObject)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerExitHandler>(
                lastHoveredButton,
                pointerData,
                ExecuteEvents.pointerExitHandler
            );
        }

        // Trigger hover for new object
        if (hoveredObject != null)
        {
            Button parentButton = hoveredObject.GetComponentInParent<Button>();
            if (parentButton != null)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute<IPointerEnterHandler>(
                    parentButton.gameObject,
                    pointerData,
                    ExecuteEvents.pointerEnterHandler
                );

                lastHoveredButton = parentButton.gameObject;
            }
        }

    }
    private void HandleSelection(GameObject selectedObject)
    {
        if (selectedObject == null) return;

        // Look for a Button in the parent hierarchy
        Button parentButton = selectedObject.GetComponentInParent<Button>();
        if (parentButton != null)
        {
            // Trigger the Button's OnClick
            parentButton.onClick.Invoke();
            Debug.Log(parentButton.name);

            // Optional: visual feedback (flash pressed color)
            StartCoroutine(FlashButton(parentButton));
        }
    }

    private System.Collections.IEnumerator FlashButton(Button button)
    {
        if (button == null) yield break;

        Image image = button.GetComponent<Image>();
        if (image == null) yield break;

        ColorBlock colors = button.colors;
        image.color = colors.pressedColor;
        yield return new WaitForSeconds(0.1f);
        image.color = colors.normalColor;
    }
}