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

        // sneaky: we're going to artificially speed up gravity just on this object
        // to make it seem like it's falling faster
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
        // if we're still falling out of the sky,
        // pull us downward manually
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
            hasCollided = true;
            // once we've landed, turn regular gravity back on
            rb.useGravity = true;
        }

        // add particle effects
        if (CameraFollow.includeJuice)
        {
            Instantiate(particlePrefab, collision.contacts[0].point, Quaternion.identity);
            camFollow.TriggerScreenshake();
        }
    }
}
