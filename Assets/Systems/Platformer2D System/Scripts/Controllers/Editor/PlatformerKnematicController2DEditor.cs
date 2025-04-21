using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlatformerKinematicController2D))]
public class PlatformerKnematicController2DEditor : Editor
{
    private PlatformerKinematicController2D _controller;
    private Vector2 _customDashDir = Vector2.right;
    private Vector2 _customGravityDir = Vector2.down;

    public override void OnInspectorGUI()
    {
        // Base GUI
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("▶ Runtime Debug Info", EditorStyles.boldLabel);

        _controller = (PlatformerKinematicController2D)target;

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Is Grounded", _controller.IsGrounded.ToString());
            EditorGUILayout.LabelField("Is Dashing", _controller.IsDashing.ToString());
            EditorGUILayout.Vector2Field("Gravity", _controller.Gravity);
            EditorGUILayout.Vector2Field("Input Direction", _controller.InputDirection);
            EditorGUILayout.Vector2Field("Velocity", _controller.Velocity);
            EditorGUILayout.Vector2Field("Delta", _controller.Delta);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("▶ Dash Control", EditorStyles.boldLabel);

            _customDashDir = EditorGUILayout.Vector2Field("Custom Dash Direction", _customDashDir);
            if (GUILayout.Button("Dash!"))
            {
                _controller.Dash(_customDashDir);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("▶ Gravity Control", EditorStyles.boldLabel);

            _customGravityDir = EditorGUILayout.Vector2Field("Custom Gravity Direction", _customGravityDir);
            if (GUILayout.Button("Apply Gravity"))
            {
                _controller.SetGravityDirection(_customGravityDir);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Presets:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⬇ Down")) _controller.SetGravityDirection(Vector2.down);
            if (GUILayout.Button("⬆ Up")) _controller.SetGravityDirection(Vector2.up);
            if (GUILayout.Button("⬅ Left")) _controller.SetGravityDirection(Vector2.left);
            if (GUILayout.Button("➡ Right")) _controller.SetGravityDirection(Vector2.right);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("▶ Debug Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset Position (0,0)"))
            {
                _controller.transform.position = Vector3.zero;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Debug tools disponibles solo en Play Mode.", MessageType.Info);
        }
    }
}
