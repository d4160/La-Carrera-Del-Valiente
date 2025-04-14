using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Movimiento de enemigos
public class EnemigoMovimiento : Movimiento
{
    public Enemigo EnemigoRef => personajeRef as Enemigo;
}
