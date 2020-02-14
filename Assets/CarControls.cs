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

    private bool canMove;
    public bool airborne;

    private CameraFollow camFollow;
    public GameObject playerImpactPrefab;

    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camFollow = GameObject.Find("Main Camera").GetComponent<CameraFollow>();


        if (CameraFollow.includeJuice)
        {
            GetComponent<Renderer>().material = mat;
        }
    }

    private void Update()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > .01f)
        {
            Vector3 rotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            transform.Rotate(rotation);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dist = 1f;
        Vector3 dir = new Vector3(0, -1, 0);
        RaycastHit hit;

        bool lastAirborne = airborne;
        airborne = true;

        if (Physics.Raycast(transform.position, -Vector3.up, out hit, dist))
        {
            airborne = false;
        }

        if (!airborne)
        {
            UpdateSpeed();
        }


        if (lastAirborne == true && airborne == false)
        {
            if (CameraFollow.includeJuice)
            {
                Debug.Log("trigger screenshake");
                StartCoroutine(Chill());
            }

        }

        //Debug.Log(airborne);
    }

    IEnumerator Chill()
    {
        yield return new WaitForSeconds(.05f);
        Instantiate(playerImpactPrefab, transform.position, Quaternion.identity);
        camFollow.TriggerScreenshake();
    }


    void UpdateSpeed()
    {

        float fwd = Input.GetAxis("Vertical");

        if (rb.velocity.z < maxSpeed)
        {
            rb.AddForce(transform.forward * fwd * acceleration, ForceMode.Acceleration);
        }

        curSpeed = rb.velocity.z;

        Debug.Log("current speed: " + curSpeed);
        //rb.position = new Vector3(rb.position.x, rb.position.y, rb.position.z + curSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //camFollow.TriggerScreenshake();
    }
}
