using UnityEngine;

/// <summary>
/// 커서를 따라다니는 스프라이틑 조정해주는 스크립트
/// </summary>
public class CustomCursor : MonoBehaviour
{
    RectTransform _rect;
    public float followSpeed = 25f;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector2 targetPos = new Vector2(
            mouseScreenPos.x - Screen.width * 0.5f,
            mouseScreenPos.y - Screen.height * 0.5f
        );

        _rect.anchoredPosition = Vector2.Lerp(
            _rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * followSpeed); // timescale이 0이어도 작동
    }

    void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}