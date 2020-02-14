using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class CameraFollow : MonoBehaviour
{
    public static bool includeJuice = true;
    private bool isLockedOnToBig = true;

    public GameObject car;
    public GameObject big;
    private Camera myCam;

    private CarControls carControls;

    private float minFOV = 60;
    private float maxFOV = 120;

    private float slowY = 1f;
    private float fastY = .25f;

    private float maxScreenShake = 3f;
    private float totalScreenShakeTime = 1.5f;
    private float curScreenShakeTime;

    private float minDistortion = 0;
    private float maxDistortion = -70;

    public bool isShaking;

    PostProcessVolume volume;
    LensDistortion lensDistortion;
    ChromaticAberration abberation;
    Vignette vignette;

    private float maxLookAt = .09f;
    private float minLookAt = .01f;
    private float curLookAt;

    public Text lockOnText;

    // Start is called before the first frame update
    void Start()
    {
        // get all our references
        FindObjectsAndComponents();

        if (includeJuice)
        {
            FindAndEnableJuiceSettings();
        }
    }

    void FindObjectsAndComponents()
    {
        big = GameObject.Find("Big");
        myCam = GetComponent<Camera>();
        carControls = car.GetComponent<CarControls>();
    }

    void FindAndEnableJuiceSettings()
    {
        volume = GetComponent<PostProcessVolume>();
        volume.enabled = true;
        volume.profile.TryGetSettings(out lensDistortion);
        volume.profile.TryGetSettings(out abberation);
        volume.profile.TryGetSettings(out vignette);
        curLookAt = maxLookAt;

        RenderSettings.fog = true;

        GameObject.Find("Sun").GetComponent<Light>().shadows = LightShadows.Soft;
        GetComponent<FlareLayer>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // are we using juice? if so, make sure postprocessing, fog, FOV, etc. are enabled.
        if (includeJuice)
        {
            UpdateJuice();
        }
        else {
            UpdateNoJuice();
        }
    }

    void UpdateJuice()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            isLockedOnToBig = !isLockedOnToBig;
        }

        // are we currently shaking? if so, deduct from the screenshake time we have left
        if (isShaking)
        {
            curScreenShakeTime -= Time.deltaTime;
        }

        // the basic numbers we're working with
        // (car min speed is assumed to be 0
        float carCurSpeed = carControls.curSpeed;
        float carMaxSpeed = carControls.maxSpeed;

        // FOV
        myCam.fieldOfView = Mathf.Clamp(Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, minFOV, maxFOV), minFOV, maxFOV);

        // new camera position and y-height
        float newHeight = Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, slowY, fastY);

        // handle basic lock-on
        // this lock-on code is definitely not production-ready!
        // making good game cameras is actually pretty hard.
        // there's a gdc talk on all the pitfalls here: https://www.youtube.com/watch?v=C7307qRmlMI
        if (isLockedOnToBig)
        {
            Vector3 newPos = car.transform.position + (Vector3.Normalize(big.transform.position-car.transform.position) * -5);
            newPos.y = car.transform.position.y + newHeight;
            transform.position = newPos;
            transform.LookAt(big.transform);
        } else
        {
            Vector3 newPos = newPos = car.transform.position + (car.transform.forward * -5);
            newPos.y = car.transform.position.y + newHeight;
            transform.position = newPos;
            transform.LookAt(car.transform);
            
        }

        // screenshake
        if (isShaking)
        {
            if (curScreenShakeTime > 0)
            {
                float curShakePrc = curScreenShakeTime / totalScreenShakeTime;
                float curShake = maxScreenShake * curShakePrc;

                Vector3 randomPoint = Random.onUnitSphere * curShake;

                transform.position += new Vector3(randomPoint.x, randomPoint.y, 0);

            }
            else
            {
                isShaking = false;
            }
        }

        // postprocessing: distortion, abberation, vignette
        float distortion = Mathf.Clamp(Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, minDistortion, maxDistortion), maxDistortion, minDistortion);
        lensDistortion.intensity.value = distortion;
        abberation.intensity.value = Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, 0, 1);
        vignette.intensity.value = Mathf.Clamp(Remap(Mathf.Abs(carCurSpeed), 0, carMaxSpeed, 0, .45f), 0, .45f);

    }

    void UpdateNoJuice()
    {
        Vector3 newPos = car.transform.position - (car.transform.forward * 5) + (car.transform.up);
        transform.position = newPos;
        transform.LookAt(car.transform.position);
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

    void OnGUI()
    {
        if (includeJuice)
        {
            lockOnText.text = (isLockedOnToBig) ? "Locked on!" : "Not locked on.";
        }
        
    }
}
