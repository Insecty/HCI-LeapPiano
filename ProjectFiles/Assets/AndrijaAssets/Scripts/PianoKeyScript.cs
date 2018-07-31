using UnityEngine;
using System.Collections;

public class PianoKeyScript : MonoBehaviour {
      
    public MusicTone MusicTone;
    public int Octave = 3;

    public float ResetPlayTime = 0.5f;
    private float currentResetPlayTime = 0;
    
    public float MaxZRotation = 6.0f;

    private Vector3 initialPosition;
    
    public float ZRotationResetVelocity = -6;

    public float MouseClickDownRotation = 600;
    public float MouseClickSmoothTime = 0.5f;
    private Vector3 smoothVelocity = Vector3.zero;

	// Use this for initialization
	void Start () {
        currentResetPlayTime = ResetPlayTime;
        initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (transform.rotation.eulerAngles.z > 0.1f)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, Time.deltaTime * ZRotationResetVelocity));
            transform.GetComponent<Rigidbody>().angularDrag = 0;
            transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        if (transform.rotation.eulerAngles.z > MaxZRotation)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, MaxZRotation);
            transform.GetComponent<Rigidbody>().angularDrag = 0;
            transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        if (transform.rotation.eulerAngles.z < 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            transform.GetComponent<Rigidbody>().angularDrag = 0;
            transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        if (currentResetPlayTime < ResetPlayTime)
            currentResetPlayTime += Time.deltaTime;

        transform.position = initialPosition;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;               
	}
       
    public void OnMouseDown()
    {
        if (currentResetPlayTime >= ResetPlayTime)
        {
            currentResetPlayTime = 0.0f;

            TonePlayer.Instance.PlayTone(MusicTone, Octave);
        
            transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(transform.rotation.eulerAngles,
                transform.rotation.eulerAngles + new Vector3(0, 0, MouseClickDownRotation), ref smoothVelocity, MouseClickSmoothTime));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Contact point: " + collision.contacts[0].point);
        //Debug.Log("Collider center: " + collider.bounds.center);
        //if(collider.bounds.center.y < collision.contacts[0].point.y)
        Debug.Log("Collision of me, " + gameObject.name + " with " + collision.gameObject.name);
        
        Debug.Log("cbmaxy: " + GetComponent<Collider>().bounds.max.y + " c.trans.pos.y " + collision.transform.position.y);
        // this prevents from accidental playing, but does not work in normal cases sometimes
        //if (collider.bounds.max.y < collision.transform.position.y)

        if(GetComponent<Collider>().bounds.max.x > collision.contacts[0].point.x && GetComponent<Collider>().bounds.min.x < collision.contacts[0].point.x
            && GetComponent<Collider>().bounds.min.y<collision.contacts[0].point.y)
        {
            if (currentResetPlayTime >= ResetPlayTime)
            {
                currentResetPlayTime = 0.0f;

                TonePlayer.Instance.PlayTone(MusicTone, Octave);

                transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(transform.rotation.eulerAngles,
                transform.rotation.eulerAngles + new Vector3(0, 0, MouseClickDownRotation), ref smoothVelocity, MouseClickSmoothTime));
            }
        }
    }
}
