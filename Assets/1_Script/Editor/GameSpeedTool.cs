using UnityEngine;
using UnityEditor;

public class GameSpeedTool : EditorWindow
{
    private float gameSpeed = 1.0f;

    [MenuItem("Tools/Game Speed Tool")]
    public static void ShowWindow()
    {
        GetWindow<GameSpeedTool>("Game Speed");
    }

    private void OnGUI()
    {
        GUILayout.Label("Điều khiển tốc độ Game", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();

        // Thanh kéo điều chỉnh tốc độ
        gameSpeed = EditorGUILayout.Slider("Tốc độ hiện tại", gameSpeed, 0.1f, 10f);

        EditorGUILayout.Space();

        // Các nút nhấn nhanh
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("x1 (Chuẩn)")) gameSpeed = 1f;
        if (GUILayout.Button("x2 (Nhanh)")) gameSpeed = 2f;
        if (GUILayout.Button("x5 (Cực nhanh)")) gameSpeed = 5f;
        if (GUILayout.Button("x10 (Max)")) gameSpeed = 10f;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        if (GUILayout.Button("Dừng hình (Pause)")) gameSpeed = 0f;

        // Cập nhật TimeScale trong thời gian thực nếu đang chơi
        if (Application.isPlaying)
        {
            Time.timeScale = gameSpeed;
        }
        else
        {
            // Nếu không chơi, reset về 1 để tránh lỗi Editor
            gameSpeed = 1.0f;
        }

        Repaint(); // Cập nhật giao diện liên tục
    }
}
