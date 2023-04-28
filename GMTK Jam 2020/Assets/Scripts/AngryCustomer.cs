using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngryCustomer : MonoBehaviour
{
    public float speed;
    public float stamina;
    public float restTime;
    public float waitTime;
    public bool reachedFurniture;
    public bool shouldSwing;
    public BoxCollider2D breakCollider;
    public List<string> enterDialogue;
    public List<string> exitDialogue;
    public Slider staminaBar;
    public GameObject chatBubble;
    public Transform pointPoint;
    public Transform leavePoint;
    public List<Furniture> furniture;

    private Animator animator;
    private float baseRestTime;
    private float baseWaitTime;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private Player player;
    private bool targetLocked;
    public Transform target;
    public Furniture targetFurniture;
    private int targetIndex;
    private Furniture selectedFurniture;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = FindObjectOfType<Player>();
        gameManager = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseRestTime = restTime;
        staminaBar.maxValue = stamina;
        staminaBar.value = stamina;
        baseWaitTime = waitTime;
        pointPoint = GameObject.Find("PointPoint").transform;
        leavePoint = GameObject.Find("LeavePoint").transform;

        furniture = new List<Furniture>();
        furniture.AddRange(FindObjectsOfType<Furniture>());
        for (int i = 0; i < furniture.Count; i++) { if (furniture[i].broken) { furniture.Remove(furniture[i]); i--; }}
    }

    void Update()
    {
        if (gameManager.gameEnded) return;

        if (gameManager.dialogueStarted && chatBubble.activeInHierarchy) chatBubble.SetActive(false); 

        if (shouldSwing)
        {
            if (waitTime > 0) waitTime -= Time.deltaTime;
            else Swing();
        }

        BreakStuff();
    }

    //void FixedUpdate () { if (targetLocked && !reachedFurniture) Move(); }

    void BreakStuff()
    {
        if (!gameManager.dialogueStarted) GoTo(pointPoint);
        if (!gameManager.dialogueEnded) return;

        if (restTime > 0) restTime -= Time.deltaTime;
        else
        {
            if (stamina <= 0) { if (!gameManager.endDialogueStarted) gameManager.StartDialogue(false); gameManager.endDialogueStarted = true;}


            if (gameManager.endDialogueEnded) GoTo(leavePoint);
            if (stamina <= 0) return;

            if (!targetLocked)
            {
                int randomFurniture = Random.Range(0, furniture.Count);
                selectedFurniture = furniture[randomFurniture];
                float closerDistance = Mathf.Infinity;

                for (int i = 0; i < 2; i++)
                {
                    if (Vector2.Distance(transform.position, selectedFurniture.goToPoints[i].position) < closerDistance)
                    {
                        target = selectedFurniture.goToPoints[i];
                        targetFurniture = target.GetComponentInParent<Furniture>();
                        closerDistance = Vector2.Distance(transform.position, selectedFurniture.goToPoints[i].position);
                        targetIndex = i;
                    }
                }

                if (!target.GetComponent<GoToPoint>().reachable)
                {
                    if (targetIndex == 0) { target = selectedFurniture.goToPoints[1]; }
                    else if (targetIndex == 1) { target = selectedFurniture.goToPoints[0]; }
                    targetFurniture = target.GetComponentInParent<Furniture>();
                }

                if (!target.GetComponent<GoToPoint>().reachable)
                {
                    target = null;
                    targetFurniture = null;
                    targetLocked = false;
                    return;
                }

                selectedFurniture.outline.SetActive(true);
                shouldSwing = false;
                reachedFurniture = false;
                targetLocked = true;
            }
            else
            {
                GoTo(target);
            }
        }
    }

    void Move()
    {
        if (!animator.GetBool("run")) animator.SetBool("run", true);

        if (target.position.x > transform.position.x) if (spriteRenderer.flipX) spriteRenderer.flipX = false;
        if (target.position.x < transform.position.x) if (!spriteRenderer.flipX) spriteRenderer.flipX = true;

        if (reachedFurniture)
        {
            shouldSwing = true;
            animator.SetBool("run", false);
        }

        if (targetFurniture.transform.position.x > transform.position.x) spriteRenderer.flipX = false;
        else spriteRenderer.flipX = true;
    }

    void GoTo(Transform target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (!animator.GetBool("run")) animator.SetBool("run", true);

        if (target.position.x > transform.position.x) if (spriteRenderer.flipX) spriteRenderer.flipX = false;
        if (target.position.x < transform.position.x) if (!spriteRenderer.flipX) spriteRenderer.flipX = true;

        if (Vector2.Distance(transform.position, target.position) < .15f)
        {
            if (target.GetComponentInParent<Furniture>())
            {
                if (target.GetComponentInParent<Furniture>().transform.position.x > transform.position.x) spriteRenderer.flipX = false;
                else if (target.GetComponentInParent<Furniture>().transform.position.x < transform.position.x) spriteRenderer.flipX = true;
            }

            if (gameManager.dialogueEnded)
            {
                reachedFurniture = true;
                shouldSwing = true;
            }

            animator.SetBool("run", false);

            if (gameManager.endDialogueEnded && !gameManager.called) gameManager.EndGame();
            
            if (!player.shouldCheckOnCustomer && !gameManager.dialogueEnded) player.shouldCheckOnCustomer = true;
        }
    }

    void Swing()
    {
        animator.SetTrigger("swing");
        stamina--;
        staminaBar.value = stamina;
        waitTime = baseWaitTime;
        restTime = baseRestTime;
        shouldSwing = false;
    }

    public void CheckForFurniture()
    {
        if (targetFurniture != null && breakCollider.IsTouching(targetFurniture.breakCollider))
        {
            if (player.carriedFurniture == targetFurniture) { player.DropFurniture(); }
            furniture.Remove(targetFurniture);
            targetFurniture.Break();
        }

        if (selectedFurniture != null) { selectedFurniture.outline.SetActive(false); }

        selectedFurniture = null;
        reachedFurniture = false;
        targetLocked = false;
        target = null;
    }

    Transform FindNearestFurniture()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;

        foreach (Furniture _furniture in furniture)
        {
            Transform t = _furniture.transform;
            float dist = Vector3.Distance(t.position, transform.position);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }
}