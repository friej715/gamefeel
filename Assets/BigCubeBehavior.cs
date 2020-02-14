using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCubeBehavior : MonoBehaviour
{

    private CameraFollow camFollow;
    private bool hasCollided = false;
    private Rigidbody rb;

    private float curFallSpeed;
    private float customGravity = 9.8f;

    public GameObject particlePrefab;
    public Material mat;
        
    // Start is called before the first frame update
    void Start()
    {
        camFollow = GameObject.Find("Main Camera").GetComponent<CameraFollow>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        if (CameraFollow.includeJuice)
        {
            GetComponent<Renderer>().material = mat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.useGravity == false)
        {
            curFallSpeed += customGravity * Time.deltaTime;
            rb.position -= new Vector3(0, curFallSpeed, 0);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {

        if (hasCollided == false)
        {
            camFollow.TriggerScreenshake();
            hasCollided = true;
            rb.useGravity = true;
        }
        if (CameraFollow.includeJuice)
        {
            Instantiate(particlePrefab, collision.contacts[0].point, Quaternion.identity);
        }
        

    }
}
