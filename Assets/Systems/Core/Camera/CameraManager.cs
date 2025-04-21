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

        // Obtener la c�mara principal
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
    /// Inicia el efecto de vibraci�n de la c�mara.
    /// </summary>
    /// <param name="duration">Duraci�n total de la vibraci�n.</param>
    /// <param name="magnitude">Intensidad de la vibraci�n.</param>
    public void ShakeCamera(float duration, float magnitude)
    {
        if (cameraTransform == null) return; // No hacer nada si no hay c�mara

        // Detener cualquier vibraci�n anterior
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            cameraTransform.localPosition = originalPosition; // Asegurar que vuelva a la original
        }

        // Guardar la posici�n original si no est� ya en la original (por si acaso)
        // Podr�as querer que la c�mara siga al jugador, as� que 'originalPosition' podr�a necesitar actualizarse
        // o el shake podr�a aplicarse relativo a la posici�n actual del seguidor de c�mara.
        // Para este ejemplo, asumimos una posici�n base fija o que se resetea antes del shake.
        originalPosition = cameraTransform.localPosition; // Actualizar por si la c�mara se movi�

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

            // Aplicar offset a la posici�n original
            cameraTransform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            // Esperar al siguiente frame
            yield return null;
        }

        // Asegurar que la c�mara vuelve exactamente a su posici�n original al terminar
        cameraTransform.localPosition = originalPosition;
        shakeCoroutine = null; // Marcar que la corutina ha terminado
    }

    // Opcional: M�todo para resetear la c�mara si es necesario externamente
    public void ResetCameraPosition()
    {
        if (cameraTransform != null)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            cameraTransform.localPosition = originalPosition;
            shakeCoroutine = null;
        }
    }

    // Llama a esto si tu c�mara sigue al jugador y quieres que el shake
    // se aplique relativo a la posici�n actual del jugador
    public void UpdateOriginalPosition(Vector3 newPos)
    {
        // Solo actualiza si no est� temblando
        if (shakeCoroutine == null)
        {
            originalPosition = newPos;
        }
    }
}