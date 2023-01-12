using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycle : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private float secondsInFullDay = 120f;
    private Vector3 centre = new Vector3((VoxelData.worldWidth * VoxelData.chunkWidth) / 2, (VoxelData.worldHeight * VoxelData.chunkHeight) / 2, (VoxelData.worldWidth * VoxelData.chunkWidth) / 2);
    private World world;

    [Range(0, 1)] [SerializeField] private float currentTimeOfDay = 0;
    private float sunInitalIntensity;

    public Color[] colors;

    public float returnTimeOfDay() { return currentTimeOfDay; }

    private JobDistribution jobDist;

    public float speed = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        world = FindObjectOfType<World>();
        gameObject.transform.position = centre + new Vector3(0, (VoxelData.worldHeight * VoxelData.chunkHeight), 0);

        sun.color = colors[1];
        sunInitalIntensity = sun.intensity;
        jobDist = FindObjectOfType<JobDistribution>();
    }

    // Update is called once per frame
    void Update()
    {
        updateSun();

        currentTimeOfDay += (Time.deltaTime / secondsInFullDay) * VoxelData.timeMultiplier;
        world.grassMat.SetFloat("Time_Of_Day", currentTimeOfDay);

        if (Input.GetKeyDown(KeyCode.T))
        {
            currentTimeOfDay = 0.5f;
        }

        if(currentTimeOfDay >= 1)
        {
            currentTimeOfDay = 0;
        }

    }

    void updateSun()
    {
        sun.transform.localRotation = Quaternion.Euler((currentTimeOfDay * 360f) - 90, 170, 0);

        float intensityMultiplier = 1f;
        float minIntensity = 1f;


        if (currentTimeOfDay <= 0.18f || currentTimeOfDay >= 0.85f)
        {
            intensityMultiplier = minIntensity;         
        }

        else if (currentTimeOfDay <= 0.2f)
        {
            intensityMultiplier = Mathf.Clamp01((currentTimeOfDay - 0.23f) * (1 / 0.02f)) + minIntensity;
        }

        else if (currentTimeOfDay >= 0.83f)
        {
            intensityMultiplier = Mathf.Clamp01((1 + minIntensity) - ((currentTimeOfDay - 0.83f) * (1 / 0.02f)));
        }

        if (currentTimeOfDay > 0.05f && currentTimeOfDay < 0.45f)
            sun.color = Color.Lerp(sun.color, colors[0], speed);

        else if (currentTimeOfDay > 0.45f && currentTimeOfDay < 0.55f)
            sun.color = Color.Lerp(sun.color, colors[0], speed);

        else if (currentTimeOfDay > 0.55f && currentTimeOfDay < 0.95f)
            sun.color = Color.Lerp(sun.color, colors[1], speed);

        sun.intensity = sunInitalIntensity * intensityMultiplier;
    }
}
