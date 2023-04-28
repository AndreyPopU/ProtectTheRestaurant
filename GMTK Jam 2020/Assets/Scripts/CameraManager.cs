using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform target;
    public Vector2 roomEnd;

    //public float smoothness;
    //public Vector3 offset;
    public bool shaking;

    private Vector3 clampedPosition;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void FixedUpdate()
    {
        if (!gameManager.dialogueEnded) return;

        if (!shaking && target != null)
        {
            clampedPosition.x = Mathf.Clamp(target.position.x, -roomEnd.x, roomEnd.x);
            clampedPosition.y = Mathf.Clamp(target.position.y, -roomEnd.y, roomEnd.y);
            clampedPosition.z = -10;
            //transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothness);
            transform.position = clampedPosition;
        }
    }

    public IEnumerator Shake(float duration, float force)
    {
        while (duration > 0)
        {
            float x = Random.Range(transform.position.x + force, transform.position.x - force);
            float y = Random.Range(transform.position.y + force, transform.position.y - force);

            transform.position = new Vector3(x, y, transform.position.z);

            duration -= Time.deltaTime;

            yield return null;
        }
    }
}
