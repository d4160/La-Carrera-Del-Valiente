using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // d (posicion final) = d0 (posicion actual) + direccion * v (rapidez) * t (tiempo)
        Vector2 velocidad = direccion * rapidez;
        Vector2 desplazamiento = velocidad * Time.deltaTime;

        transform.position = transform.position + new Vector3(desplazamiento.x, desplazamiento.y, 0);
    }

    void ActualizarDireccionSprite(Vector2 direccion)
    {
        // ERROR EN ASIGNAR VALORES
        //transform.localScale *= new Vector3(-1, 1, 1);
        Vector3 scale = transform.localScale; // x=-1

        if (direccion.x > 0) // Si direccion en x es 1 (HACIA LA DERECHA)
        {
            scale.x = -1;
        }
        else if (direccion.x < 0)
        {
            scale.x = 1;
        }

        transform.localScale = scale;
    }

}
