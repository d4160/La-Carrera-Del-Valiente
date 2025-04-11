using UnityEngine;

public class EnemigoPersiguiendo : Estado
{
    public EnemigoMovimiento movimiento;

    public EnemigoPersiguiendo(EnemigoMovimiento movimiento)
    {
        this.movimiento = movimiento; // this representa a esta clase, para acceder a variables globales
    }

    public override void EjecutarCadaFrame()
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
