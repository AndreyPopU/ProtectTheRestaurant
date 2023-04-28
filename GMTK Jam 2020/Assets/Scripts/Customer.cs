using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public float speed;
    public GameObject bubble;

    private Transform safePoint;
    private GameManager gameManager;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        safePoint = GameObject.FindGameObjectWithTag("SafePoint").transform;

        speed = Random.Range(6, 9);
    }

    void Update()
    {
        if (gameManager.dialogueStarted)
        {
            if (transform.position.x > 0) GetComponent<SpriteRenderer>().flipX = true; else GetComponent<SpriteRenderer>().flipX = false;
            if (bubble.activeInHierarchy) bubble.SetActive(false);
        }

        if (gameManager.dialogueEnded) GoTo(safePoint);
    }

    void GoTo(Transform target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (!animator.GetBool("run")) animator.SetBool("run", true); GetComponent<SpriteRenderer>().flipX = true;

        if (Vector2.Distance(transform.position, target.position) < .25f) gameObject.SetActive(false);
    }
}
