using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform backgroundRect;
    private RectTransform handleRect;

    [Header("Joystick Settings")]
    [SerializeField] private float handleRange = 100f; // Range the handle can move from center

    public Vector2 InputVector { get; private set; }

    private void Awake()
    {
        backgroundRect = GetComponent<RectTransform>();
        
        // Find handle (assuming it's a child object). If not found, create a placeholder logic
        if (transform.childCount > 0)
        {
            handleRect = transform.GetChild(0).GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("VirtualJoystick: No handle found as child. Creating a dummy handle reference.");
            handleRect = backgroundRect; // Fallback, not ideal visually
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = Vector2.zero;

        // Convert screen position to local anchored position
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, eventData.position, eventData.pressEventCamera, out position))
        {
            // Normalize the position to get a -1 to 1 value
            position.x = (position.x / backgroundRect.sizeDelta.x);
            position.y = (position.y / backgroundRect.sizeDelta.y);

            // Set pivot offset depending on where the pivot is (usually 0.5, 0.5)
            float x = (backgroundRect.pivot.x == 1) ? position.x * 2 + 1 : position.x * 2 - 1;
            float y = (backgroundRect.pivot.y == 1) ? position.y * 2 + 1 : position.y * 2 - 1;

            InputVector = new Vector2(x, y);
            InputVector = (InputVector.magnitude > 1.0f) ? InputVector.normalized : InputVector;

            // Move the handle visually
            if (handleRect != null && handleRect != backgroundRect)
            {
                handleRect.anchoredPosition = new Vector2(
                    InputVector.x * (backgroundRect.sizeDelta.x / 2) * (handleRange / 100f),
                    InputVector.y * (backgroundRect.sizeDelta.y / 2) * (handleRange / 100f));
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
        if (handleRect != null && handleRect != backgroundRect)
        {
            handleRect.anchoredPosition = Vector2.zero;
        }
    }
}
