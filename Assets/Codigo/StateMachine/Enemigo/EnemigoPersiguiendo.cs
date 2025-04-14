using UnityEngine;

public class EnemigoPersiguiendo : State
{
    public EnemigoMovimiento movimiento;

    public EnemigoPersiguiendo(EnemigoMovimiento movimiento)
    {
        this.movimiento = movimiento; // this representa a esta clase, para acceder a variables globales
    }

    public override void Update()
    {
        if (movimiento.EnemigoRef.personajeObjetivo != null)
        {
            Vector3 direccionHaciaJugador = movimiento.EnemigoRef.personajeObjetivo.transform.position - movimiento.EnemigoRef.transform.position;
            Vector2 direccionHaciaJugadorNormalizado = direccionHaciaJugador.normalized; // Magnitud 1

            movimiento.direccion = direccionHaciaJugadorNormalizado;
        }
        else
        {
            movimiento.direccion = Vector2.zero;
        }
        
    }
}
