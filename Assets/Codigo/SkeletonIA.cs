using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonIA : MonoBehaviour
{
    public EnemigoMovimiento movimiento;


    void Update()
    {
        if (movimiento.enemigoRef.personajeObjetivo != null)
        {
            Vector3 direccionHaciaJugador = movimiento.enemigoRef.personajeObjetivo.transform.position - movimiento.enemigoRef.transform.position;
            Vector2 direccionHaciaJugadorNormalizado = direccionHaciaJugador.normalized; // Magnitud 1

            movimiento.direccion = direccionHaciaJugadorNormalizado;
        }
        else
        {
            movimiento.direccion = Vector2.zero;
        }
    }
}
