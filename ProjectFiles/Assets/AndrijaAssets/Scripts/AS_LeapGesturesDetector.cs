using System;
using System.Collections.Generic;
using UnityEngine;
using Leap;


public class AS_LeapGesturesDetector : MonoBehaviour
{
    [Flags]
    public enum GestureDirection { None = 0, Left = 1, Right = 2, Up = 4, Down = 8, Forward = 16, Back = 32 };
    
    /// VARIABLES ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Leap Controller class instance in this class

    [HideInInspector] public Leap.Controller LeapController = new Controller();
    
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

    #region Leap Built-in Gesture Variables

    public bool IsEnabledLeapBuiltInGestures = false;

    private bool _isEnabledLeapBuiltInGestures = false;

    public bool IsDebugLogWritingBuiltInGestures = false;

    #endregion

    #region FingerTracking Variables

    private Frame _lastFrame;

    /// <summary>
    /// The distance that is a limit for distance between finger tip position and hand sphere center or hand palm position
    /// below which fingers are disregarded in gestures in this system.
    /// This is introduced because Leap finger detection works faulty for fingers in near fist position - 
    /// it detects them, or at least tries, but we need only the stretched fingers for the gestures, so we
    /// can correctly detect the count of fingers taking part in gestures.
    /// For distance from hand sphere center, 2.5f was appropriate for my hand, 
    /// for distance from hand palm position, around 3.4f to capture thumb (as the closest finger to the palm position) while stretched
    /// </summary>
    public float MinFingerStretchDistanceToTrack = 2.5f;

    public bool IsDisplayingFingerRemovalSpheres = true;

    private List<GameObject> FingerRemovalSpheres = new List<GameObject>();

    private Dictionary<int, FingerEntry> FingerEntries = new Dictionary<int, FingerEntry>();

    /// <summary>
    /// Are we using HandSphere or PalmPosition for finger removal, along with MinFingerStretchDistanceToTrack
    /// </summary>
    public bool IsUsingHandSphereForFingerRemoval = true;

    public bool IsUsingHandSphereForFingerRemovalInUpdate = false;

    public float FixedAmountFromDetectionStartMove = 5;
    public float FixedAmountFromDetectionOneDirection = 7;
    /// <summary>
    /// The amount of distance in one direction a finger is allowed to have to be recognized as finger that 
    /// made a gesture along with the finger that made at least OneDirection distance 
    /// </summary>
    public float FixedAmountFromDetectionSecondDirectionDifference = 0.5f;

    // TODO: enum mode type?

    #endregion

    #region Leap Unity Bridge Related Variables

    public LeapUnityBridge LeapUnityBridge;

    private Vector3 _handParentObjectInitialOffset;

    private Vector3 _handParentObjectCurrentOffset = Vector3.zero;
    
    #endregion

    /// METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Mono Behaviour Methods

    // Use this for initialization
    void Start()
    {
        InitializeLeapUnityBridge();

        CreateHandSpheres();
    }

    // Update is called once per frame
    void Update()
    {
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
        DebugLogLeapBuiltInGesturesOnStop(currentFrame);

        RepositionFingerRemovalSpheres(currentFrame);
        DebugDisplayHandSpheres();

        UpdateFingerEntries(currentFrame);
        CheckFingerEntries();

        // In preparation for the next frame,
        // we store the current frame as last frame,
        // although it's available through LeapController.Frame(1);
        _lastFrame = currentFrame;

        UpdateLeapBuiltinGestures();
    }
    
    #endregion

    #region Finger Removal Spheres Methods

    /// <summary>
    /// Create HandSphere game objects as sphere primitive and remove colliders from them
    /// </summary>
    private void CreateHandSpheres()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject fingerRemovalSphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fingerRemovalSphere1.name = "DebugFingerRemovalSphere";
            Destroy(fingerRemovalSphere1.GetComponent<SphereCollider>());
            FingerRemovalSpheres.Add(fingerRemovalSphere1);
        }
    }

    /// <summary>
    /// Set Enabled of HandSpheres game objects to the value of IsDisplayingHandSpheres
    /// </summary>
    private void DebugDisplayHandSpheres()
    {
        for (int i = 0; i < FingerRemovalSpheres.Count; i++)
            FingerRemovalSpheres[i].gameObject.SetActive(IsDisplayingFingerRemovalSpheres);
    }

    /// <summary>
    /// Repositions finger removal spheres based on hand sphere centers or palm positions
    /// </summary>
    private void RepositionFingerRemovalSpheres(Frame frame)
    {
        int usedHandSphereIndex = 0;

        foreach (Hand hand in frame.Hands)
        {
            if (hand.IsValid)
            {
                Vector3 spherePosition;
                if (IsUsingHandSphereForFingerRemoval)
                {
                    spherePosition = hand.SphereCenter.ToUnityTranslated() + _handParentObjectCurrentOffset;
                }
                else
                {
                    spherePosition = hand.PalmPosition.ToUnityTranslated() + _handParentObjectCurrentOffset;
                }

                FingerRemovalSpheres[usedHandSphereIndex].transform.position = spherePosition;
                FingerRemovalSpheres[usedHandSphereIndex].transform.localScale = MinFingerStretchDistanceToTrack*2.0f*Vector3.one;
                usedHandSphereIndex++;
                if (usedHandSphereIndex >= 2)
                    usedHandSphereIndex = 1;
            }
        }
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

        if(LeapUnityBridge.m_InputParent != null)
            _handParentObjectCurrentOffset = LeapUnityBridge.m_InputParent.transform.position - _handParentObjectInitialOffset;
        else
            _handParentObjectCurrentOffset = Vector3.zero;
    }

    #endregion
   
    #region Debug Draw Leap Data (Hands, Palms, Fingers) Methods

    /// <summary>
    /// Draws valid hands' fingertip positions as a 3D cross in main world axes, and fingertip directions as a vector
    /// using DebugDrawingFingertipPosition Length and Color, as well as DebugDrawingFingertipDirection Length and Color
    /// </summary>
    void DebugDrawFingers(Frame frame)
    {
        if(IsDebugDrawingFingers)
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

                    DebugDraw3DCross(fingerTipPosition, DebugDrawingFingertipPositionScale,DebugDrawingFingertipPositionColor);
                        
                    Debug.DrawLine(fingerTipPosition, fingerTipPosition + fingerDirection.normalized * DebugDrawingFingertipDirectionScale, DebugDrawingFingertipDirectionColor);
                    
                    // Prevent drawing for this finger if its further from hand radius than MinDistance
                    if (!IsFingerAppropriateToTrack(finger)) //((fingerTipPosition - hand.SphereCenter.ToUnityTranslated()).magnitude < MinFingerStretchDistanceToTrack)
                        continue;

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

                float scaledHandSphereRadius = hand.SphereRadius*Leap.UnityVectorExtension.InputScale.x;

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

                    if(palmVelocity.magnitude >= DebugDrawingPalmsMinVelocityToDraw)
                        Debug.DrawLine(palmPosition, palmPosition + palmVelocity * DebugDrawingPalmsVelocityScale, DebugDrawingPalmsVelocityColor);
                }
            }
    }

    /// <summary>
    /// Draws a 3D cross in scene view for debug purposes using given parameters
    /// </summary>
    void DebugDraw3DCross(Vector3 position, float halfLength, Color color)
    {
        Debug.DrawLine(position + Vector3.down*halfLength, position + Vector3.up*halfLength, color);
        Debug.DrawLine(position + Vector3.left*halfLength, position + Vector3.right*halfLength, color);
        Debug.DrawLine(position + Vector3.forward*halfLength, position + Vector3.back*halfLength, color);
    }

    #endregion

    #region Built-in Gestures Related Methods

    /// <summary>
    /// Updates bool flags for built in leap motion gestures, and enables or disables them, based on the flag value
    /// </summary>
    private void UpdateLeapBuiltinGestures()
    {
        if (IsEnabledLeapBuiltInGestures != _isEnabledLeapBuiltInGestures)
        {
            _isEnabledLeapBuiltInGestures = IsEnabledLeapBuiltInGestures;
            SetLeapBuiltInGestures(_isEnabledLeapBuiltInGestures);
        }
    }

    /// <summary>
    /// Enable or disable detection of Leap built in gestures on this controller
    /// </summary>
    /// <param name="isEnabling"></param>
    private void SetLeapBuiltInGestures(bool isEnabling)
    {
        foreach (var enumValue in Enum.GetValues(typeof(Gesture.GestureType)))
        {
            LeapController.EnableGesture((Gesture.GestureType)enumValue,isEnabling);
        }
    }

    /// <summary>
    /// Uses Debug.Log to write the occurences of leap built-in textures on their end
    /// </summary>
    /// <param name="frame"></param>
    private void DebugLogLeapBuiltInGesturesOnStop(Frame frame)
    {
        if(IsDebugLogWritingBuiltInGestures)
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

    #endregion

    #region Custom Gestures Related Methods

    public bool IsFingerAppropriateToTrack(Finger finger)
    {
        Vector3 fingerTipPosition = finger.TipPosition.ToUnityTranslated() + _handParentObjectCurrentOffset;

        if (IsUsingHandSphereForFingerRemoval)
        {
            return (fingerTipPosition - finger.Hand.SphereCenter.ToUnityTranslated()).magnitude >= MinFingerStretchDistanceToTrack;
        }
        else
        {
            return (fingerTipPosition - finger.Hand.PalmPosition.ToUnityTranslated()).magnitude >= MinFingerStretchDistanceToTrack;
        }
    }
    
    private void UpdateFingerEntries(Frame frame)
    {
        List<int> fingerIDsUpdated = new List<int>();

        foreach (Finger finger in frame.Fingers)
            if (finger.IsValid)
            {
                if (!IsUsingHandSphereForFingerRemovalInUpdate || IsFingerAppropriateToTrack(finger))
                {
                    if (!FingerEntries.ContainsKey(finger.Id))
                    {
                        FingerEntry entry = new FingerEntry(finger, _handParentObjectCurrentOffset);
                        FingerEntries.Add(finger.Id, entry);
                        //Debug.Log("Entry "+finger.Id+" added.");
                    }
                    else
                    {
                        FingerEntries[finger.Id].AddEntryData(finger, _handParentObjectCurrentOffset);
                    }
                }
                fingerIDsUpdated.Add(finger.Id);
            }

        // Each entry that was not updated should be removed from entries
        List<int> keysToRemove=new List<int>();

        foreach (int id in FingerEntries.Keys)
        {
            if (!fingerIDsUpdated.Contains(id))
            {
                keysToRemove.Add(id);
                //Debug.Log("Entry " + id + " removed.");
            }
        }

        foreach (int id in keysToRemove)
        {
            FingerEntries.Remove(id);
        }
    }

    /// <summary>
    /// Checks if any of finger entries has tip position that moved for a given
    /// </summary>
    private void CheckFingerEntries()
    {
        foreach (KeyValuePair<int, FingerEntry> keyValuePair in FingerEntries)
        {
            FingerEntry entry = keyValuePair.Value;

            GestureDirection gestureDirection = GetFingerEntryGestureDirection(entry, FixedAmountFromDetectionOneDirection);

            if (gestureDirection != GestureDirection.None)
            {
                List<GestureDirection> gestureDirections = new List<GestureDirection>(){gestureDirection};
                entry.Clear();

                // search for any other that have the smaller minimum component value and quit the search
                foreach (KeyValuePair<int, FingerEntry> keyValuePair2 in FingerEntries)
                {
                    if (keyValuePair.Key != keyValuePair2.Key)
                    {
                        GestureDirection gestureDirection2 = GetFingerEntryGestureDirection(keyValuePair2.Value,
                            FixedAmountFromDetectionOneDirection - FixedAmountFromDetectionSecondDirectionDifference);
                        if(gestureDirection2 != GestureDirection.None)
                        {
                            gestureDirections.Add(gestureDirection2);
                            keyValuePair2.Value.Clear();
                        }
                    }
                }

                Debug.Log("First gesture: " + gestureDirections[0].ToString() + " of " + gestureDirections.Count + " fingers.");

                return;
            }
        }
    }

    private GestureDirection GetFingerEntryGestureDirection(FingerEntry fingerEntry, float minimumComponentValue)
    {
        GestureDirection gestureDirection = GestureDirection.None;

        Vector3 startEnd = fingerEntry.GetStartEndDifference();

        if (startEnd.x > minimumComponentValue)
        {
            gestureDirection = GestureDirection.Right; 
        }
        else if (startEnd.x < -minimumComponentValue)
        {
            gestureDirection = GestureDirection.Left; 
        }
        else if (startEnd.y < -minimumComponentValue)
        {
            gestureDirection = GestureDirection.Down; 
        }
        else if (startEnd.y > minimumComponentValue)
        {
            gestureDirection = GestureDirection.Up; 
        }
        else if (startEnd.z > minimumComponentValue)
        {
            gestureDirection = GestureDirection.Forward; 
        }
        else if (startEnd.z < -minimumComponentValue)
        {
            gestureDirection = GestureDirection.Back; 
        }

        return gestureDirection;
    }

    #endregion

    /// CLASSES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    [Serializable]
    public class FingerEntry
    {
        public const int MaxDataCount = 1000;

        public int ID;
        [SerializeField]
        public List<Vector3> TipPositions;
        [SerializeField]
        public List<Vector3> TipVelocities;
        [SerializeField]
        public List<float> Times; 

        public Vector3 StartTipPosition;
        public Vector3 StartTipVelocity;
        public float StartTime;

        public FingerEntry(Finger finger, Vector3 handParentOffset)
        {
            ID = finger.Id;

            StartTipPosition = finger.TipPosition.ToUnityTranslated() + handParentOffset;
            StartTipVelocity = finger.TipVelocity.ToUnityScaled();
            StartTime = Time.realtimeSinceStartup;

            TipPositions = new List<Vector3>();
            TipVelocities = new List<Vector3>();
            Times = new List<float>();

            AddEntryData(finger,handParentOffset);
        }

        public void AddEntryData(Finger finger, Vector3 handParentOffset)
        {
            TipPositions.Add(finger.TipPosition.ToUnityTranslated()+handParentOffset);
            TipVelocities.Add(finger.TipVelocity.ToUnityScaled());
            Times.Add(Time.realtimeSinceStartup);

            if (TipPositions.Count > MaxDataCount)
            {
                TipPositions.RemoveAt(0);
                TipVelocities.RemoveAt(0);
                Times.RemoveAt(0);
            }
        }

        public float GetStartEndDistance()
        {
            return (StartTipPosition - TipPositions[TipPositions.Count - 1]).magnitude;
        }

        public Vector3 GetStartEndDifference()
        {
            return TipPositions[TipPositions.Count - 1] - StartTipPosition;
        }
        
        public void Clear()
        {
            StartTipPosition = TipPositions[TipPositions.Count - 1];
            StartTipVelocity = TipVelocities[TipVelocities.Count - 1];
            StartTime = Times[Times.Count - 1];
            TipPositions.Clear();
            TipVelocities.Clear();
            Times.Clear();
        }
    }
}
