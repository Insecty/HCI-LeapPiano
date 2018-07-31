using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class restartForNote : MonoBehaviour {

    // Update is called once per frame
    public void OnDestroy()
    {
        // restart
        GameObject.FindGameObjectWithTag("restart").GetComponent<practice>().showRestart();
    }
}
