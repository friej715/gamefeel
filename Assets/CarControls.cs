using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControls : MonoBehaviour
{
    public float acceleration;
    public float maxSpeed;
    public float curSpeed;
    public float friction;

    private Rigidbody rb;

    public bool airborne;

    private CameraFollow camFollow;
    public GameObject playerImpactPrefab;

    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camFollow = GameObject.Find("Main Camera").GetComponent<CameraFollow>();

        // using juice? add a material
        if (CameraFollow.includeJuice)
        {
            GetComponent<Renderer>().material = mat;
        }
    }

    private void Update()
    {
        // controls left/right rotation
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > .01f)
        {
            Vector3 rotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            transform.Rotate(rotation);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // check to see if we're in the air (flying off the ramp)
        float dist = 1f;
        Vector3 dir = new Vector3(0, -1, 0);
        RaycastHit hit;

        bool lastAirborne = airborne;
        airborne = true;

        if (Physics.Raycast(transform.position, -Vector3.up, out hit, dist))
        {
            airborne = false;
        }

        // if we're not airborne, update our speed
        if (!airborne)
        {
            UpdateSpeed();
        }

        // if we WERE airborne, but are not anymore, get ready to trigger screenshake
        if (lastAirborne == true && airborne == false)
        {
            if (CameraFollow.includeJuice)
            {
                StartCoroutine(Chill());
            }

        }
    }

    // screenshake here looks more natural if we pause before triggering it
    IEnumerator Chill()
    {
        yield return new WaitForSeconds(.05f);
        Instantiate(playerImpactPrefab, transform.position, Quaternion.identity);
        camFollow.TriggerScreenshake();
    }


    void UpdateSpeed()
    {
        // add forward force
        float fwd = Input.GetAxis("Vertical");

        if (rb.velocity.z < maxSpeed)
        {
            rb.AddForce(transform.forward * fwd * acceleration, ForceMode.Acceleration);
        }
        curSpeed = rb.velocity.z;
    }
}
