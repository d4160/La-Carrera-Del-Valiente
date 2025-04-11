using UnityEngine;

public class MaquinaDeEstados : MonoBehaviour
{
    public Estado estadoActivo; // camelCase

    void Update()
    {
        estadoActivo.EjecutarCadaFrame();
    }

    public void CambiarEstado(Estado nuevoEstado)
    {
        estadoActivo = nuevoEstado;
    }
}
