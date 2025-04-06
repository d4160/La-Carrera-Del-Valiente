using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntradaJugador : MonoBehaviour
{
    public Movimiento mov;

    void Update()
    {
        mov.direccion = LeerTeclado();
    }

    Vector2 LeerTeclado()
    {
        // Primera Forma
        //float derecha = Input.GetKey(KeyCode.D);
        //float izquierda = Input.GetKey(KeyCode.A);
        //float arriba = Input.GetKey(KeyCode.W);
        //float abajo = Input.GetKey(KeyCode.S);
        // Segunda forma

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        return new Vector2(horizontal, vertical);
    }
}
