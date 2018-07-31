using UnityEngine;
using System.Collections;

public class SpawnPrefabOnKeyPlay : MonoBehaviour
{
    public GameObject[] ObjectToSpawns;
    public int effectid;
    public GameObject ObjectToSpawn;
    public Vector3 OffsetFromTransform;
    public bool IsUsingTransformRotation = true;
    public Vector3 CustomEulerRotation = Vector3.zero;

    public bool IsUsingTimeout = false;
    public float Timeout = 1;
    private float _currentTimeout = 0;

	// Use this for initialization
	void Start () {
        ObjectToSpawn = ObjectToSpawns[effectid];
	}
	
	// Update is called once per frame
	void Update () {
	    if(IsUsingTimeout && _currentTimeout <= Timeout)
	        _currentTimeout += Time.deltaTime;
	}

    public void changeEffect(int i)
    {
        effectid = i;
        ObjectToSpawn = ObjectToSpawns[i];
    }

    void OnMouseDown()
    {
        if (!IsUsingTimeout || _currentTimeout >= Timeout)
        {
            GameObject.Instantiate(ObjectToSpawn, transform.position + OffsetFromTransform,
                                   (IsUsingTransformRotation)
                                       ? transform.rotation
                                       : Quaternion.Euler(CustomEulerRotation));
            _currentTimeout = 0;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<Collider>().bounds.max.x > collision.contacts[0].point.x && GetComponent<Collider>().bounds.min.x < collision.contacts[0].point.x
           && GetComponent<Collider>().bounds.min.y < collision.contacts[0].point.y)
        if (!IsUsingTimeout || _currentTimeout >= Timeout)
        {
            GameObject.Instantiate(ObjectToSpawn, transform.position + OffsetFromTransform,
                                   (IsUsingTransformRotation)
                                       ? transform.rotation
                                       : Quaternion.Euler(CustomEulerRotation));
            _currentTimeout = 0;
        }
    }
}
