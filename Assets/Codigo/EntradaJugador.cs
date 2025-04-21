using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class EntradaJugador : MonoBehaviour
{
    public PlatformerDynamicController2D movimiento;

    void Update()
    {
        movimiento.InputDirection = LeerControlHorizontal();

        if (LeerSalto())
        {
            movimiento.Dash(movimiento.InputDirection);
        }
    }

    Vector2 LeerControlHorizontal()
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

    bool LeerSalto()
    {
        return Input.GetButtonDown("Jump");
    }
}
