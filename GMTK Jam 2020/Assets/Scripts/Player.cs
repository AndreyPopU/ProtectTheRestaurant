using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float carrySpeed;
    public Furniture carriedFurniture;
    public Transform holdPosition;
    public bool canMove;
    public GameObject bubble;

    private Transform pointToGoTo;
    [HideInInspector]
    public bool shouldCheckOnCustomer;
    private Vector2 movement;
    private Rigidbody2D rb;
    [HideInInspector]
    public Animator animator;
    private GameManager gameManager;
    [HideInInspector]
    public Furniture furnitureInRange;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        pointToGoTo = GameObject.Find("PlayerPointPoint").transform;
    }

    void Update()
    {
        if (gameManager.gameEnded) return;

        if (gameManager.dialogueStarted && bubble.activeInHierarchy) bubble.SetActive(false);

        if (shouldCheckOnCustomer && !gameManager.dialogueStarted) GoTo(pointToGoTo);

        if (!canMove) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedFurniture == null)
            {
                if (furnitureInRange != null && !furnitureInRange.broken)
                {
                    animator.SetBool("carry", true);
                    furnitureInRange.transform.SetParent(holdPosition.transform);
                    carriedFurniture = furnitureInRange;
                    furnitureInRange.transform.localPosition = Vector2.zero + furnitureInRange.holdOffset;
                }
            }
            else
            if (carriedFurniture != null) DropFurniture();
        }

        if (Input.GetButton("Horizontal") && Input.GetAxisRaw("Horizontal") > 0) if (transform.localScale.x < 0) transform.localScale = new Vector2(1, 1);
        if (Input.GetButton("Horizontal") && Input.GetAxisRaw("Horizontal") < 0) if (transform.localScale.x > 0) transform.localScale = new Vector2(-1, 1);
    }

    void FixedUpdate()
    {
        Move();
    }

    public void Move()
    {
        if (!canMove) return;

        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        animator.SetFloat("Speed", movement.magnitude);
        if (carriedFurniture != null) rb.velocity = movement * carrySpeed * Time.fixedDeltaTime; 
        else rb.velocity = movement * speed * Time.fixedDeltaTime;
    }

    public void DropFurniture()
    {
        carriedFurniture.transform.SetParent(carriedFurniture.holder);
        carriedFurniture.goToPoints[0].transform.localPosition = new Vector2(carriedFurniture.startPos.x, carriedFurniture.startPos.y);
        carriedFurniture.goToPoints[1].transform.localPosition = new Vector2(-carriedFurniture.startPos.x, carriedFurniture.startPos.y);
        carriedFurniture = null;
        animator.SetBool("carry", false);
    }

    void GoTo(Transform target)
    {
        if (Vector2.Distance(transform.position, target.position) < .25f) { gameManager.StartDialogue(true); animator.SetFloat("Speed", 0); return; }

        transform.position = Vector2.MoveTowards(transform.position, target.position, (speed / 50) * Time.deltaTime);
        animator.SetFloat("Speed", Mathf.Abs(transform.position.x));
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (carriedFurniture != null) return;

        if (other.GetComponent<Furniture>() && !other.GetComponent<Furniture>().broken)
        {
            furnitureInRange = other.GetComponent<Furniture>();
            furnitureInRange.outline.SetActive(true);
        }
    }
}
