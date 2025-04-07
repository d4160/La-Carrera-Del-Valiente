using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Movimiento : MonoBehaviour
{
    public float rapidez = 5.5f;// Decimales
    public Vector2 direccion;

    // Ejecuta cada frame... se ejecuta 60 por cada segundo
    void Update()
    {
        Mover();
        ActualizarDireccionSprite(direccion);
    }

    void Mover()
    { 
        // d (posicion final) = d0 (posicion actual) + (direccion * rapidez) * t (tiempo)
        Vector2 velocidad = direccion * rapidez;
        Vector2 desplazamiento = velocidad * Time.deltaTime;

        transform.Translate(new Vector3(desplazamiento.x, desplazamiento.y, 0));
        // transform.position = transform.position + new Vector3(desplazamiento.x, desplazamiento.y, 0);
    }

    void ActualizarDireccionSprite(Vector2 direccion)
    {
        Vector3 scale = transform.localScale; // x=-1

        if (direccion.x > 0) // Si direccion en x es 1 (HACIA LA DERECHA)
        {
            scale.x = -1; // Invertir sprite 
        }
        else if (direccion.x < 0)
        {
            scale.x = 1;
        }

        transform.localScale = scale;
    }
}
