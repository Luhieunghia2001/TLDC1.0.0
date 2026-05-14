using UnityEngine;
using TMPro;

public class DamageTextFly : MonoBehaviour
{
    public float speed = 2f;
    public float lifetime = 1f;

    void Update()
    {
        // Bay lên theo trục Y
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Bạn có thể thêm code làm mờ (Fade out) ở đây nếu muốn
    }
}
