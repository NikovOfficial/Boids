using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtAttractor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(Attractor.POS);
	}
}
