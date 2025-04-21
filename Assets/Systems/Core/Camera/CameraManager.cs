using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private Transform cameraTransform;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Opcional: DontDestroyOnLoad(gameObject); si quieres que persista entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Obtener la cámara principal
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            originalPosition = cameraTransform.localPosition;
        }
        else
        {
            Debug.LogError("Main Camera not found. CameraManager needs a camera tagged 'MainCamera'.");
        }
    }

    /// <summary>
    /// Inicia el efecto de vibración de la cámara.
    /// </summary>
    /// <param name="duration">Duración total de la vibración.</param>
    /// <param name="magnitude">Intensidad de la vibración.</param>
    public void ShakeCamera(float duration, float magnitude)
    {
        if (cameraTransform == null) return; // No hacer nada si no hay cámara

        // Detener cualquier vibración anterior
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            cameraTransform.localPosition = originalPosition; // Asegurar que vuelva a la original
        }

        // Guardar la posición original si no está ya en la original (por si acaso)
        // Podrías querer que la cámara siga al jugador, así que 'originalPosition' podría necesitar actualizarse
        // o el shake podría aplicarse relativo a la posición actual del seguidor de cámara.
        // Para este ejemplo, asumimos una posición base fija o que se resetea antes del shake.
        originalPosition = cameraTransform.localPosition; // Actualizar por si la cámara se movió

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Calcular offset aleatorio (solo en X e Y para 2D)
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Aplicar offset a la posición original
            cameraTransform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            // Esperar al siguiente frame
            yield return null;
        }

        // Asegurar que la cámara vuelve exactamente a su posición original al terminar
        cameraTransform.localPosition = originalPosition;
        shakeCoroutine = null; // Marcar que la corutina ha terminado
    }

    // Opcional: Método para resetear la cámara si es necesario externamente
    public void ResetCameraPosition()
    {
        if (cameraTransform != null)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            cameraTransform.localPosition = originalPosition;
            shakeCoroutine = null;
        }
    }

    // Llama a esto si tu cámara sigue al jugador y quieres que el shake
    // se aplique relativo a la posición actual del jugador
    public void UpdateOriginalPosition(Vector3 newPos)
    {
        // Solo actualiza si no está temblando
        if (shakeCoroutine == null)
        {
            originalPosition = newPos;
        }
    }
}