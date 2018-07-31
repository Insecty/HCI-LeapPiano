using UnityEngine;
using System.Collections;

public class DestroyAfter : MonoBehaviour {

    public float DestroyDelay = 1;

	// Use this for initialization
	void Start () {
        Destroy(this.gameObject, DestroyDelay);
	}
}
