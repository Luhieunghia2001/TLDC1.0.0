using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TimeScaleController : MonoBehaviour
{
    [Header("Game Speed (Slider)")]
    [Range(0.1f, 10f)] 
    public float gameSpeed = 1f;

    void Update()
    {
        // Cập nhật TimeScale
        if (Time.timeScale != gameSpeed)
        {
            Time.timeScale = gameSpeed;
        }

        // Xử lý phím tắt theo New Input System
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) gameSpeed = 1f;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) gameSpeed = 2f;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) gameSpeed = 3f;
        }
#else
        if (Input.GetKeyDown(KeyCode.Alpha1)) gameSpeed = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) gameSpeed = 2f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) gameSpeed = 3f;
#endif
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Time.timeScale = gameSpeed;
        }
    }
}
