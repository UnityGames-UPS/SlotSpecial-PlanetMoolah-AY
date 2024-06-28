using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITrailController : MonoBehaviour
{
    public RectTransform uiElement; // Reference to the UI element's RectTransform
    public Vector2 startPoint; // Starting point of the scaling
    public Vector2 endPoint; // Ending point of the scaling

    void Start()
    {
        ScaleUIElement();
    }

    void ScaleUIElement()
    {
        // Calculate the distance between startPoint and endPoint
        float distance = Vector2.Distance(startPoint, endPoint);

        // Set the scale of the UI element based on the distance
        float scaleX = distance / uiElement.rect.width;
        uiElement.localScale = new Vector3(uiElement.localScale.x, scaleX, uiElement.localScale.z);

        // Set the position of the UI element to startPoint (optional)
        uiElement.position = startPoint;

        Debug.Log("UI element scaled instantly!");
    }
}
