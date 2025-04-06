using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonIA : MonoBehaviour
{
    public EnemigoMovimiento movimiento;
    public float rapidez;


    void Update()
    {
        Vector3 direccionHaciaJugador = movimiento.personaje.transform.position - movimiento.esteEnemigo.transform.position;
        Vector2 direccionMagnitud1 = direccionHaciaJugador.normalized;

        Vector3 desplazaiento = direccionMagnitud1 * rapidez * Time.deltaTime;

        transform.position = transform.position + desplazaiento;
    }
}
