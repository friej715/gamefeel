using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMat : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        if (CameraFollow.includeJuice)
        {
            GetComponent<Renderer>().material = mat;
        }
    }
}
