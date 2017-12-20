using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Description:
 * Script which manages the game state. The primary use of which in this game is
 * the holder of static variables which other funcitons need to reference.
 */

public class GameState : MonoBehaviour
{
    //Boundry of the arena
    public static Rect Boundry = new Rect(-40,-40,80,80); 
}
