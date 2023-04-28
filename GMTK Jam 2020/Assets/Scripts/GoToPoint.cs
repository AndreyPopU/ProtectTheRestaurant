using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToPoint : MonoBehaviour
{
    public bool reachable = true;
    
    void OnTriggerEnter2D(Collider2D other) { reachable = false; }
    void OnTriggerExit2D(Collider2D other) { reachable = true; }
}
