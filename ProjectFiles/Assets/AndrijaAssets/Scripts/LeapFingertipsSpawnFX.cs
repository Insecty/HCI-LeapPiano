using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Leap;

public class LeapFingertipsSpawnFX : MonoBehaviour {

    [HideInInspector]
    public Leap.Controller LeapController = new Controller();

    public float MinTipVelocityMagnitudeToDebugLine = 10;

    public float MinSphereSpawnDeltaTime = 0.2f;
    private float _currentSphereSpawnTime = 0;

    public int FramesToAverageVelocityCount = 5;

    public float ScaleOfSpawnedSphereVelocity = 0.2f;

    public float StartSphereSpawnOffset = 2;

    public GameObject SpawningPrefab;

    public GameObject SpawningPrefabLighting;

    private Frame _lastFrame;

    private const int MaxGameObjects = 200;
    private List<GameObject> _pool = new List<GameObject>();

    private bool isFiringOn = false;

    public bool isLightningModeOn = true;
    private bool _lastIsLightingModeOn = true;

	// Use this for initialization
	void Start ()
	{
	    _currentSphereSpawnTime = MinSphereSpawnDeltaTime;

	    foreach (var enumValue in Enum.GetValues(typeof(Gesture.GestureType)))
	    {
            LeapController.EnableGesture((Gesture.GestureType)enumValue);
	    }
	}
	
	// Update is called once per frame
    void Update()
    {
        Frame frame = LeapController.Frame();

        if (_lastFrame == null)
        {
            _lastFrame = frame;
            return;
        }

        // DebugFrameHandsFingers(frame);

        // DebugFrameGesturesStop(frame);

        if (_currentSphereSpawnTime < MinSphereSpawnDeltaTime)
            _currentSphereSpawnTime += Time.deltaTime;
        else 
        {
            Finger fingerToAverage = frame.Fingers[0];

            Vector3 fingerTipPosition = fingerToAverage.TipPosition.ToUnityTranslated();
            Debug.DrawLine(fingerTipPosition, fingerTipPosition + StartSphereSpawnOffset * fingerToAverage.Direction.ToUnityScaled().normalized, Color.white);
            Debug.DrawLine(fingerTipPosition, Camera.main.transform.position, Color.white);

            int count = 0;
            Vector3 average = new Vector3();

            for (int i = 0; i < FramesToAverageVelocityCount; i++)
            {
                Finger fingerFromFrame = LeapController.Frame(i).Finger(fingerToAverage.Id);
                if (fingerFromFrame.IsValid)
                {
                    average += (fingerFromFrame.TipVelocity.ToUnityScaled());
                    count++;
                }
            }

            if (count != 0)
            {
                average /= count;
                if (average.magnitude > MinTipVelocityMagnitudeToDebugLine)
                    if (Vector3.Dot(average, Vector3.Normalize(fingerToAverage.TipPosition.ToUnityTranslated() - Camera.main.transform.position)) > 0.25f)
                    {
                        isFiringOn = !isFiringOn;
                        if (!isFiringOn)
                        {
                            foreach (GameObject o in _pool)
                            {
                                Destroy(o);
                            }
                            _pool.Clear();
                        }
                        _currentSphereSpawnTime = 0;
                    }
            }

            if (isFiringOn)
            {
                if (SpawningPrefab != null)
                    // if (average.magnitude > MinTipVelocityMagnitudeToDebugLine)
                {
                    if (!isLightningModeOn)
                    {
                        // Destroy all the lightning
                        if (_lastIsLightingModeOn != isLightningModeOn)
                        {
                            foreach (GameObject o in _pool)
                            {
                                Destroy(o);
                            }
                            _pool.Clear();
                        }

                        LeapFinger[] fingers = (LeapFinger[]) GameObject.FindObjectsOfType(typeof (LeapFinger));
                        foreach (LeapFinger leapFinger in fingers)
                        {
                            if (leapFinger.gameObject.transform.childCount == 0 &&
                                leapFinger.transform.parent.gameObject.name != "Unknown Hand")
                            {
                                GameObject go = (GameObject) Instantiate(SpawningPrefab);
                                //go.transform.rotation = leapFinger.gameObject.transform.rotation;
                                go.transform.position = Vector3.zero;
                                go.transform.rotation = Quaternion.identity;
                                go.transform.parent = leapFinger.gameObject.transform;
                                go.transform.localPosition = Vector3.zero;
                                go.transform.rotation = Quaternion.identity;
                                _pool.Add(go);
                            }

                            if (leapFinger.transform.parent.gameObject.name == "Unknown Hand" &&
                                leapFinger.gameObject.transform.childCount == 1)
                            {
                                GameObject go = leapFinger.gameObject.transform.GetChild(0).gameObject;
                                _pool.Remove(go);
                                Destroy(go);
                            }
                        }
                        _currentSphereSpawnTime = 0.0f;
                        return;
                    }

                    // destroy all the flames
                    foreach (LeapFinger leapFinger in (LeapFinger[])GameObject.FindObjectsOfType(typeof(LeapFinger)))
                    {
                        if (leapFinger.gameObject.transform.childCount == 1)
                        {
                            GameObject go = leapFinger.gameObject.transform.GetChild(0).gameObject;
                            _pool.Remove(go);
                            Destroy(go);
                        }
                    }

                    for(int i=0; i<frame.Hands[0].Fingers.Count; i++)
                    // if (Vector3.Dot(average, Vector3.Normalize(fingerToAverage.TipPosition.ToUnityTranslated() - Camera.main.transform.position)) > 0.25f)
                    {
                        fingerToAverage = frame.Hands[0].Fingers[i];
                        fingerTipPosition = fingerToAverage.TipPosition.ToUnityTranslated();


                        GameObject go = (GameObject) Instantiate(SpawningPrefabLighting);
                            //GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        go.transform.position = fingerTipPosition +
                                                fingerToAverage.Direction.ToUnityScaled().normalized*
                                                StartSphereSpawnOffset;
                        go.transform.rotation =
                            Quaternion.LookRotation(fingerToAverage.Direction.ToUnityScaled().normalized, Vector3.up);
                        //  go.AddComponent<Rigidbody>();
                        //  go.rigidbody.velocity = fingerToAverage.Direction.ToUnityScaled().normalized * ScaleOfSpawnedSphereVelocity;
                        // average*ScaleOfSpawnedSphereVelocity;

                        _currentSphereSpawnTime = 0.0f;

                        if (_pool.Count == MaxGameObjects)
                        {
                            Destroy(_pool[0]);
                            _pool.RemoveAt(0);
                        }
                        _pool.Add(go);
                    }
                    Debug.DrawLine(fingerTipPosition, fingerTipPosition + fingerToAverage.TipVelocity.ToUnityScaled(),
                                   Color.yellow);
                }
            }
        }


        _lastFrame = frame;
        _lastIsLightingModeOn = isLightningModeOn;
    }

    void DebugFrameHandsFingers(Frame frame)
    {
        foreach (Hand hand in frame.Hands)
        {
            if (hand.IsValid)
            {
                Vector3 handPosition = hand.SphereCenter.ToUnityTranslated();
                Debug.Log(hand.SphereCenter);
                
                Debug.DrawLine(handPosition + Vector3.down,handPosition + Vector3.up,Color.blue);
                Debug.DrawLine(handPosition + Vector3.left, handPosition + Vector3.right, Color.blue);
                Debug.DrawLine(handPosition + Vector3.forward, handPosition + Vector3.back, Color.blue);

                foreach (Finger finger in hand.Fingers)
                {
                    Vector3 fingerTipPosition = finger.TipPosition.ToUnityTranslated();

                    Debug.DrawLine(fingerTipPosition, fingerTipPosition + 50 * finger.Direction.ToUnityScaled(), Color.blue);
                    Debug.DrawLine(fingerTipPosition, fingerTipPosition + 10 * Vector3.up, Color.blue);

                    if (finger.TipVelocity.ToUnityScaled().magnitude > MinTipVelocityMagnitudeToDebugLine)
                        Debug.DrawLine(fingerTipPosition, fingerTipPosition + finger.TipVelocity.ToUnityScaled(), Color.red);
                }
                //break;
            }
        }
    }

    void DebugFrameGesturesStop(Frame frame)
    {
        foreach (Gesture gesture in frame.Gestures())
            if (gesture.State == Gesture.GestureState.STATESTOP)
            {
                switch (gesture.Type)
                {
                    case (Gesture.GestureType.TYPECIRCLE):
                        {
                            Debug.Log("Circle gesture recognized.");
                            break;
                        }
                    case (Gesture.GestureType.TYPEINVALID):
                        {
                            Debug.Log("Invalid gesture recognized.");
                            break;
                        }
                    case (Gesture.GestureType.TYPEKEYTAP):
                        {
                            Debug.Log("Key Tap gesture recognized.");
                            break;
                        }
                    case (Gesture.GestureType.TYPESCREENTAP):
                        {
                            Debug.Log("Screen tap gesture recognized.");
                            break;
                        }
                    case (Gesture.GestureType.TYPESWIPE):
                        {
                            Debug.Log("Swipe gesture recognized.");
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
    }
}
