using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : Personaje
{
    public Personaje personajeObjetivo; // Puede ser nulo
    public EnemigoMovimiento movimientoRef;
    public MaquinaDeEstados enemigoMaquina;

    public EnemigoPersiguiendo estadoPersiguiendo;

    // Defino valores iniciales
    void Start()
    {
        estadoPersiguiendo = new EnemigoPersiguiendo(movimientoRef);

        enemigoMaquina.CambiarEstado(estadoPersiguiendo);
    }
}
