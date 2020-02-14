using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraFollow : MonoBehaviour
{
    public static bool includeJuice = true;

    public GameObject car;
    public GameObject big;
    private Camera myCam;
    private float fieldOfView;

    private CarControls carControls;

    private float minFOV = 60;
    private float maxFOV = 120;

    private float slowY = 3;
    private float fastY = .5f;

    private float maxScreenShake = 3f;
    private float totalScreenShakeTime = 1.5f;
    private float curScreenShakeTime;

    private float minDistortion = 0;
    private float maxDistortion = -70;

    public bool isShaking;
    
    public BoxCollider ps;

    PostProcessVolume volume;
    LensDistortion lensDistortion;
    ChromaticAberration abberation;
    Vignette vignette;

    private float maxLookAt = .09f;
    private float minLookAt = .01f;
    private float curLookAt;


    // Start is called before the first frame update
    void Start()
    {
        // get all our references
        big = GameObject.Find("Big");
        myCam = GetComponent<Camera>();
        carControls = car.GetComponent<CarControls>();
        volume = GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out lensDistortion);
        volume.profile.TryGetSettings(out abberation);
        volume.profile.TryGetSettings(out vignette);
        curLookAt = maxLookAt;

        // if we're using juice, also use lens flare and shadows
        if (!includeJuice)
        {
            GameObject.Find("Sun").GetComponent<Light>().shadows = LightShadows.None;
            GetComponent<FlareLayer>().enabled = false;
        }
        else
        {
            GameObject.Find("Sun").GetComponent<Light>().shadows = LightShadows.Soft;
            GetComponent<FlareLayer>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // this camera code is pretty crummy and just for showing off this one thing!
        // making good game cameras is actually pretty hard.
        // there's a gdc talk on all the pitfalls here: https://www.youtube.com/watch?v=C7307qRmlMI
        Vector3 point;
        float dist = Vector3.Distance(car.transform.position, big.transform.position);

        if (includeJuice)
        {
            if (dist > 100)
            {
                if (curLookAt < maxLookAt)
                {
                    curLookAt += .005f;
                }
                point = Vector3.Lerp(car.transform.position, big.transform.position, curLookAt);
            }
            else
            {
                if (curLookAt > minLookAt)
                {
                    curLookAt -= .005f;
                }
                point = Vector3.Lerp(car.transform.position, big.transform.position, curLookAt);

            }
        }
        else
        {
            point = car.transform.position;
        }

        transform.LookAt(point);

        // are we currently shaking? if so, deduct from the screenshake time we have left
        if (isShaking && includeJuice)
        {
            curScreenShakeTime -= Time.deltaTime;
        }

        // are we using juice? if so, make sure postprocessing, fog, FOV, etc. are enabled.
        if (includeJuice)
        {
            volume.enabled = true;
            RenderSettings.fog = true;
            UpdateJuice();
        }
        else {

            volume.enabled = false;
            RenderSettings.fog = false;
            Vector3 newPos = car.transform.position - (car.transform.forward * 5);
            transform.position = Vector3.Lerp(transform.position, new Vector3(newPos.x, car.transform.position.y + 1, newPos.z), 0.9f);
        }



    }

    void UpdateJuice()
    {
        // the basic numbers we're working with
        // (car min speed is assumed to be 0
        float carCurSpeed = carControls.curSpeed;
        float carMaxSpeed = carControls.maxSpeed;

        // FOV
        myCam.fieldOfView = Mathf.Clamp(Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, minFOV, maxFOV), minFOV, maxFOV);

        // new camera position and y-height
        float newHeight = Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, slowY, fastY);
        Vector3 newPos = car.transform.position - (car.transform.forward * 5);
        transform.position = Vector3.Lerp(transform.position, new Vector3(newPos.x, car.transform.position.y + newHeight, newPos.z), 0.9f);

        // screenshake
        if (curScreenShakeTime > 0)
        {
            float curShakePrc = curScreenShakeTime / totalScreenShakeTime;
            float curShake = maxScreenShake * curShakePrc;

            Vector3 randomPoint = Random.onUnitSphere * curShake;

            transform.position += new Vector3(randomPoint.x, randomPoint.y, 0);

        } else {
            isShaking = false;
        }

        // postprocessing: distortion, abberation, vignette
        float distortion = Mathf.Clamp(Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, minDistortion, maxDistortion), maxDistortion, minDistortion);
        lensDistortion.intensity.value = distortion;
        abberation.intensity.value = Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, 0, 1);
        vignette.intensity.value = Mathf.Clamp(Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, 0, .45f), 0, .45f);

    }

    // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void TriggerScreenshake()
    {
        if (isShaking == false)
        {
            isShaking = true;
            curScreenShakeTime = totalScreenShakeTime;
        }
    }
}
