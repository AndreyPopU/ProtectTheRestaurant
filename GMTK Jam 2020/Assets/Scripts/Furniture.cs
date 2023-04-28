using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    public enum Type { Stool, Table};

    public Type type;
    public Vector2 holdOffset;
    public Sprite brokenSprite;
    public bool broken;
    public BoxCollider2D coreCollider;
    public GameObject outline;
    public Vector2 startPosition;
    public bool shouldBeRepaired;
    public AngryCustomer angryCustomer;

    private Player player;
    [HideInInspector]
    public Transform holder;
    public BoxCollider2D breakCollider;
    public Transform[] goToPoints;
    public Vector2 startPos;
    private CameraManager cameraManager;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    private Sprite normalSprite;

    void Start()
    {
        startPosition = transform.position;
        player = FindObjectOfType<Player>();
        gameManager = FindObjectOfType<GameManager>();
        cameraManager = FindObjectOfType<CameraManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer.sprite;
        holder = GameObject.FindGameObjectWithTag("FurnitureHolder").transform;
    }

    public void GoToStart()
    {
        transform.position = startPosition;
    }

    public void Repair()
    {
        if (!shouldBeRepaired) return;

        gameManager.furnitureLeft++;
        broken = false;

        for (int i = 0; i < 2; i++)
        {
            goToPoints[i].gameObject.SetActive(true);
        }

        spriteRenderer.sprite = normalSprite;
        spriteRenderer.sortingOrder = 1;
        if (coreCollider != null) coreCollider.enabled = true;
        shouldBeRepaired = false;
    }

    public void Break()
    {
        gameManager.furnitureLeft--;
        gameManager.UpdateFurnitureSlider();
        cameraManager.StartCoroutine(cameraManager.Shake(.1f, .1f));
        if (gameManager.money > gameManager.moneyToSpend) shouldBeRepaired = true; gameManager.moneyToSpend += 10;


        for (int i = 0; i < 2; i++)
        {
            goToPoints[i].gameObject.SetActive(false);
        }
        spriteRenderer.sprite = brokenSprite;
        spriteRenderer.sortingOrder = -1;
        broken = true;
        if (type == Type.Stool) coreCollider.enabled = false;
    }

    void Update()
    {
        if (outline.activeInHierarchy) if (player.furnitureInRange != this && angryCustomer.targetFurniture != this) outline.SetActive(false);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (player.furnitureInRange == this)
        {
            player.furnitureInRange = null;
            if (angryCustomer.targetFurniture != this) outline.SetActive(false);
        }
    }
}
