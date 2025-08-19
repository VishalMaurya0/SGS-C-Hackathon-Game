using UnityEngine;
using UnityEngine.UI;

public class InfiniteUIScroll : MonoBehaviour
{
    public RectTransform background;
    public float scrollSpeedX = 50f; // pixels per second
    public float scrollSpeedY = 50f; // pixels per second

    private Vector2 startPos;
    private float width;
    private float height;

    void Start()
    {
        startPos = background.anchoredPosition;
        width = background.rect.width;
        height = background.rect.height;
    }

    void Update()
    {
        float newX = Mathf.Repeat(Time.time * scrollSpeedX, width);
        float newY = Mathf.Repeat(Time.time * scrollSpeedY, height);
        background.anchoredPosition = startPos + new Vector2(-newX, -newY);
    }
}
