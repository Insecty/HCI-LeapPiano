using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecalUpdater : MonoBehaviour {

    // expected 10 projectors, 1 for each finger
    public List<GameObject> Projectors;

    private Dictionary<int, GameObject> _projectorIndex_FingerGO; 

    // Use this for initialization
	void Start () 
    {
	    _projectorIndex_FingerGO = new Dictionary<int, GameObject>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        // check all dictionary entries and remove ones with value null
        // get all unused projectors in a list
        List<int> unused = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            if(!_projectorIndex_FingerGO.ContainsKey(i))
                unused.Add(i);
        }
        
    	// find all game objects tagged as fingers
	    List<GameObject> fingersList = new List<GameObject>(GameObject.FindGameObjectsWithTag("FingerTip"));

        // remove those that are already in dictionary as values
        for(int i=fingersList.Count-1; i>=0; i--)
            if (_projectorIndex_FingerGO.ContainsValue(fingersList[i]))
            {
                fingersList.RemoveAt(i);
            }

	    // add those that aren't along with free projectors in dictionary
        for (int i = 0; i < fingersList.Count; i++)
        {
            _projectorIndex_FingerGO.Add(unused[unused.Count-1],fingersList[i]);
            Projectors[unused[unused.Count-1]].SetActive(true);
            unused.RemoveAt(unused.Count-1);
        }

	    // reposition all projectors' x and z to match its finger values in dictionary
	    foreach (KeyValuePair<int, GameObject> keyValuePair in _projectorIndex_FingerGO)
	    {
	        Vector3 fingerPosition = keyValuePair.Value.transform.position;
	        fingerPosition.y = Projectors[keyValuePair.Key].transform.position.y;
	        Projectors[keyValuePair.Key].transform.position = fingerPosition;
	    }
        //Edelweiss.DecalSystem.de

        // disable projectors on unused
        for(int i=0; i<10; i++)
        {
            Projectors[i].SetActive(_projectorIndex_FingerGO[i].GetComponent<SphereCollider>().enabled);
        }

    }
}
