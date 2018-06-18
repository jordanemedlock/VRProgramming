using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class LaserPointer : MonoBehaviour {

	// Use this for initialization
	void Start () {

        VRTK_Pointer pointer = GetComponent<VRTK_Pointer>();
        
        pointer.DestinationMarkerHover += DestinationMarkerHover;
        pointer.DestinationMarkerSet += DestinationMarkerSet;
    }

    void DestinationMarkerHover(object sender, DestinationMarkerEventArgs e)
    {
        Target target = e.target.GetComponent<Target>();
        if (target)
        {
            target.HoverOn(e.destinationPosition, e.distance);
        }
    }
    void DestinationMarkerSet(object sender, DestinationMarkerEventArgs e)
    {
        Target target = e.target.GetComponent<Target>();
        if (target)
        {
            target.MarkerSetOn(e.destinationPosition, e.distance);
        }
    }
    
}
