using UnityEditor;
using UnityEngine;

// Asegúrate de que este script esté dentro de una carpeta llamada "Editor" en tu proyecto.
[CustomEditor(typeof(PlatformerDynamicController2D))]
public class PlatformerDynamicController2DEditor : Editor
{
    private PlatformerDynamicController2D _controller;
    private Rigidbody2D _rb; // Referencia al Rigidbody para mostrar info extra

    // Variables para los controles del editor
    private Vector2 _customDashDir = Vector2.right;
    private Vector2 _customGravityDir = Vector2.down;

    // Se llama cuando el objeto es seleccionado o el editor se recompila
    private void OnEnable()
    {
        // Obtener las referencias una vez
        _controller = (PlatformerDynamicController2D)target;
        // Intentar obtener el Rigidbody asociado
        _rb = _controller.GetComponent<Rigidbody2D>();
    }

    public override void OnInspectorGUI()
    {
        // Dibuja los campos públicos/serializados por defecto (como moveSpeed, gravityStrength, etc.)
        DrawDefaultInspector();

        // Separador visual
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("▶ Runtime Debug Info", EditorStyles.boldLabel);

        // Solo mostrar información y controles si la aplicación está en Play Mode
        if (Application.isPlaying && _controller != null && _rb != null)
        {
            // Usar BeginDisabledGroup para mostrar info como solo lectura visualmente
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Is Grounded", _controller.IsGrounded);
            EditorGUILayout.Toggle("Is Dashing", _controller.IsDashing);
            EditorGUILayout.Vector2Field("Gravity Direction", _controller.GravityDirection);
            EditorGUILayout.Vector2Field("Input Direction", _controller.InputDirection);
            EditorGUILayout.Vector2Field("Rigidbody Velocity", _rb.linearVelocity); // Usar _rb.velocity directamente
            EditorGUILayout.FloatField("Rigidbody Drag", _rb.linearDamping); // Mostrar el drag actual
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("▶ Dash Control", EditorStyles.boldLabel);

            // Campo para definir la dirección del dash personalizado
            _customDashDir = EditorGUILayout.Vector2Field("Custom Dash Direction", _customDashDir);
            // Botón para ejecutar el dash
            if (GUILayout.Button("Dash!"))
            {
                // Llamar a la función Dash del script del controlador
                _controller.Dash(_customDashDir);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("▶ Gravity Control", EditorStyles.boldLabel);

            // Campo para definir la dirección de la gravedad personalizada
            _customGravityDir = EditorGUILayout.Vector2Field("Custom Gravity Direction", _customGravityDir);
            // Botón para aplicar la nueva dirección de gravedad
            if (GUILayout.Button("Apply Gravity Direction"))
            {
                // Llamar a la función SetGravityDirection del script del controlador
                _controller.SetGravityDirection(_customGravityDir);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Presets:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            // Botones rápidos para direcciones de gravedad comunes
            if (GUILayout.Button("⬇ Down")) _controller.SetGravityDirection(Vector2.down);
            if (GUILayout.Button("⬆ Up")) _controller.SetGravityDirection(Vector2.up);
            if (GUILayout.Button("⬅ Left")) _controller.SetGravityDirection(Vector2.left);
            if (GUILayout.Button("➡ Right")) _controller.SetGravityDirection(Vector2.right);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("▶ Debug Utilities", EditorStyles.boldLabel);

            // Botón para resetear la posición y opcionalmente la velocidad
            if (GUILayout.Button("Reset Position & Velocity (0,0)"))
            {
                _controller.transform.position = Vector3.zero;
                _rb.linearVelocity = Vector2.zero; // Resetear también la velocidad
                _rb.angularVelocity = 0f; // Resetear velocidad angular si acaso
            }
        }
        else
        {
            // Mensaje si no estamos en Play Mode
            EditorGUILayout.HelpBox("Debug tools and controls available only in Play Mode.", MessageType.Info);
        }

        // Es buena práctica llamar a esto al final si has modificado propiedades serializadas
        // aunque en este caso no estamos modificando directamente las SerializedProperties
        // fuera del DrawDefaultInspector().
        // serializedObject.ApplyModifiedProperties();
    }
}