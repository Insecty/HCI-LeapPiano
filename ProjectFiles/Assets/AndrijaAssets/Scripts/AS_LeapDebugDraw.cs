using System;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class AS_LeapDebugDraw : MonoBehaviour {

    #region Leap Controller class instance in this class

    [HideInInspector]
    public Leap.Controller LeapController = new Controller();

    #endregion

    #region Debug Draw Variables

    public bool IsDebugDrawingHands = true;
    public Color DebugDrawingHandsLineColor = Color.blue;

    public bool IsDebugDrawingFingers = true;
    public Color DebugDrawingFingertipPositionColor = Color.yellow;
    public float DebugDrawingFingertipPositionScale = 1.0f;

    public Color DebugDrawingFingertipDirectionColor = Color.green;
    public float DebugDrawingFingertipDirectionScale = 1.0f;

    public Color DebugDrawingFingertipVelocityColor = Color.red;
    public float DebugDrawingFingertipVelocityScale = 1.0f;
    public float DebugDrawingFingertipMinTipVelocityToDraw = 10.0f;

    public bool IsDebugDrawingPalms = true;
    public Color DebugDrawingPalmsPositionColor = Color.yellow;
    public float DebugDrawingPalmsPositionScale = 1.0f;

    public Color DebugDrawingPalmsNormalColor = Color.cyan;
    public float DebugDrawingPalmsNormalScale = 1.0f;

    public Color DebugDrawingPalmsVelocityColor = Color.red;
    public float DebugDrawingPalmsVelocityScale = 1.0f;
    public float DebugDrawingPalmsMinVelocityToDraw = 10.0f;

    #endregion

    #region Leap Unity Bridge Related Variables

    public LeapUnityBridge LeapUnityBridge;

    private Vector3 _handParentObjectInitialOffset;

    private Vector3 _handParentObjectCurrentOffset = Vector3.zero;

    #endregion

	// Use this for initialization
	void Start () {
        InitializeLeapUnityBridge();
	}
	
	// Update is called once per frame
	void Update () {
        if (LeapUnityBridge == null)
        {
            Debug.LogError("There is no LeapUnityBridge component in the scene!");
            return;
        }

        UpdateFromUnityBridge();


        Frame currentFrame = LeapController.Frame();

        DebugDrawHands(currentFrame);
        DebugDrawFingers(currentFrame);
        DebugDrawPalms(currentFrame);
	}

    #region Debug Draw Leap Data (Hands, Palms, Fingers) Methods

    /// <summary>
    /// Draws valid hands' fingertip positions as a 3D cross in main world axes, and fingertip directions as a vector
    /// using DebugDrawingFingertipPosition Length and Color, as well as DebugDrawingFingertipDirection Length and Color
    /// </summary>
    void DebugDrawFingers(Frame frame)
    {
        if (IsDebugDrawingFingers)
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsValid)
                {
                    foreach (Finger finger in hand.Fingers)
                    {
                        Vector3 fingerTipPosition = finger.TipPosition.ToUnityTranslated();
                        Vector3 fingerTipVelocity = finger.TipVelocity.ToUnityScaled();
                        Vector3 fingerDirection = finger.Direction.ToUnityScaled();

                        ///////////
                        fingerTipPosition += _handParentObjectCurrentOffset;
                        ///////////

                        DebugDraw3DCross(fingerTipPosition, DebugDrawingFingertipPositionScale, DebugDrawingFingertipPositionColor);

                        Debug.DrawLine(fingerTipPosition, fingerTipPosition + fingerDirection.normalized * DebugDrawingFingertipDirectionScale, DebugDrawingFingertipDirectionColor);
                        
                        if (fingerTipVelocity.magnitude >= DebugDrawingFingertipMinTipVelocityToDraw)
                            Debug.DrawLine(fingerTipPosition, fingerTipPosition + fingerTipVelocity * DebugDrawingFingertipVelocityScale, DebugDrawingFingertipVelocityColor);
                    }
                }
            }
    }

    /// <summary>
    /// Draws valid hands' positions as a 3D cross in main world axes, 
    /// using unity-scaled hand sphere radius as lines length, and DebugDrawingHandsLineColor as color
    /// </summary>
    void DebugDrawHands(Frame frame)
    {
        if (!IsDebugDrawingHands)
            return;

        foreach (Hand hand in frame.Hands)
        {
            if (hand.IsValid)
            {
                Vector3 handPosition = hand.SphereCenter.ToUnityTranslated();

                ///////////
                handPosition += _handParentObjectCurrentOffset;
                ///////////

                float scaledHandSphereRadius = hand.SphereRadius * Leap.UnityVectorExtension.InputScale.x;

                DebugDraw3DCross(handPosition, scaledHandSphereRadius, DebugDrawingHandsLineColor);
            }
        }
    }

    /// <summary>
    /// Draws valid hands' palms positions as a 3D cross in main world axes, 
    /// using unity-scaled hand sphere radius as lines length, and DebugDrawingHandsLineColor as color
    /// </summary>
    void DebugDrawPalms(Frame frame)
    {
        if (IsDebugDrawingPalms)
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsValid)
                {
                    Vector3 palmPosition = hand.PalmPosition.ToUnityTranslated();
                    Vector3 palmNormal = hand.PalmNormal.ToUnity().normalized;
                    Vector3 palmVelocity = hand.PalmVelocity.ToUnityScaled();

                    ///////////
                    palmPosition += _handParentObjectCurrentOffset;
                    ///////////

                    DebugDraw3DCross(palmPosition, DebugDrawingPalmsPositionScale, DebugDrawingPalmsPositionColor);

                    Debug.DrawLine(palmPosition, palmPosition + palmNormal * DebugDrawingPalmsNormalScale, DebugDrawingPalmsNormalColor);

                    if (palmVelocity.magnitude >= DebugDrawingPalmsMinVelocityToDraw)
                        Debug.DrawLine(palmPosition, palmPosition + palmVelocity * DebugDrawingPalmsVelocityScale, DebugDrawingPalmsVelocityColor);
                }
            }
    }

    /// <summary>
    /// Draws a 3D cross in scene view for debug purposes using given parameters
    /// </summary>
    void DebugDraw3DCross(Vector3 position, float halfLength, Color color)
    {
        Debug.DrawLine(position + Vector3.down * halfLength, position + Vector3.up * halfLength, color);
        Debug.DrawLine(position + Vector3.left * halfLength, position + Vector3.right * halfLength, color);
        Debug.DrawLine(position + Vector3.forward * halfLength, position + Vector3.back * halfLength, color);
    }

    #endregion

    #region Unity Bridge Related Methods

    /// <summary>
    /// Acquires a reference to leap unity bridge component and sets initial  _handParentObject offset
    /// </summary>
    void InitializeLeapUnityBridge()
    {
        if (LeapUnityBridge == null)
            LeapUnityBridge = (LeapUnityBridge)GameObject.FindObjectOfType(typeof(LeapUnityBridge));

        if (LeapUnityBridge == null)
        {
            Debug.LogError("There is no LeapUnityBridge component in the scene!");
            return;
        }

        if (LeapUnityBridge.m_InputParent != null)
            _handParentObjectInitialOffset = LeapUnityBridge.m_InputParent.transform.position;
        else
            _handParentObjectInitialOffset = Vector3.zero;
    }

    /// <summary>
    /// Use variables accessible from LeapUnityBridge component
    /// </summary>
    void UpdateFromUnityBridge()
    {
        if (LeapUnityBridge == null)
        {
            Debug.LogError("There is no LeapUnityBridge component in the scene!");
            return;
        }

        if (LeapUnityBridge.m_InputParent != null)
            _handParentObjectCurrentOffset = LeapUnityBridge.m_InputParent.transform.position - _handParentObjectInitialOffset;
        else
            _handParentObjectCurrentOffset = Vector3.zero;
    }

    #endregion
}
