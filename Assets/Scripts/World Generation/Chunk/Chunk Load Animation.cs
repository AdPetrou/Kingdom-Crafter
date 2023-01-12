using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoadAnimation : MonoBehaviour
{
    private float speed = 2f;
    private Vector3 targetPos;

    private float waitTimer;
    private float timer;

    // Start is called before the first frame update
    private void Start()
    {
        waitTimer = Random.Range(0f, 2f);

        targetPos = transform.position;
        transform.position = new Vector3(transform.position.x, targetPos.y - VoxelData.chunkHeight, transform.position.z);
    }

    // Update is called once per frame
    private void Update()
    {
        if (timer < waitTimer)
        {
            timer += Time.deltaTime;
        }

        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);

            if ((targetPos.y - transform.position.y) < 0.05f)
            {
                transform.position = targetPos;
                Destroy(this);
            }
        }
    }
}
