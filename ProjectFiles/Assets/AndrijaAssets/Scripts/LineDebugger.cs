using UnityEngine;
using System.Collections;

public class LineDebugger : MonoBehaviour {

    public float LineLength = 1;
    public Color LineColor = Color.white;
    
	void Update () 
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward * LineLength, LineColor);	
	}
}
