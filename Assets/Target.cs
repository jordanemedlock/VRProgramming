using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

	public void HoverOn(Vector3 worldPosition, float distance)
    {
        Debug.Log("HoverOn() "+worldPosition+" "+distance);
    }

    public void MarkerSetOn(Vector3 worldPosition, float distance)
    {
        Debug.Log("MarkerSetOn() " + worldPosition + " " + distance);

    }
}
