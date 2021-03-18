using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    public Player player;
    public EnemiesController enemies;
    public void DoTurn()
    {
        Debug.Log("Next turn");
        enemies.MoveEnemies();               
    }
}
