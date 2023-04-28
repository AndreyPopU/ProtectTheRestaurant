using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int money;
    public int moneyToSpend;
    public int currentScene;
    public Text moneyText;
    public GameObject moneyAddText;
    public GameObject nextButton;
    public GameObject retryButton;
    public bool dialogueStarted;
    public bool dialogueEnded;
    public bool endDialogueStarted;
    public bool endDialogueEnded;
    public float totalFurniture;
    public CanvasGroup blackScreen;
    public Slider furnitureSlider;
    public Text furniturePercentage;
    public GameObject colliders;
    public GameObject sliderParent;
    public GameObject endPanel;
    public Text endPanelText;
    public bool gameEnded;
    [HideInInspector]
    public float furnitureLeft;
    public bool moneyLeft = true;

    private bool shouldFadeIn;
    private CanvasManager canvasManager;
    private bool writingText;
    private List<string> dialogue;
    private int phraseCount;
    private AngryCustomer angryCustomer;
    private Player player;
    public bool called;
    public float continueTime;
    public static GameManager instance;
    private float moneyEffectTime = 1;
    private float y;
    private float baseY;
    private List<Furniture> furniture = new List<Furniture>();
    public CameraManager cameraManager;
    private bool next;
    private bool calledLoad;
    void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this; DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) { return; }

        blackScreen.gameObject.SetActive(true);
        y = moneyAddText.transform.localPosition.y;
        baseY = y;
        player = FindObjectOfType<Player>();
        canvasManager = FindObjectOfType<CanvasManager>();
        angryCustomer = FindObjectOfType<AngryCustomer>();
        furniture.AddRange(FindObjectsOfType<Furniture>());
        totalFurniture = furniture.Count;
        furnitureSlider.maxValue = totalFurniture;
        furnitureSlider.value = totalFurniture;
        furnitureLeft = totalFurniture;
        moneyText.text = money.ToString() + "$";
        cameraManager = FindObjectOfType<CameraManager>();
    }

    void Update()
    {
        if (furniture.Count <= 0) EndGame();

        if (SceneManager.GetActiveScene().buildIndex != currentScene)
        {
            shouldFadeIn = false;
            if (SceneManager.GetActiveScene().buildIndex == 1) { money = 100; moneyText.text = money.ToString() + "$"; moneyLeft = true; }
            gameEnded = false; calledLoad = false; called = false; dialogueStarted = false; dialogueEnded = false; endDialogueStarted = false; endDialogueEnded = false; colliders.SetActive(false);

            UpdateFurnitureSlider();

            for (int i = 0; i < furniture.Count; i++)
            {
                if (furniture[i].broken) furniture[i].Repair();
                if (!furniture[i].broken) furniture[i].GoToStart();
            }
            currentScene = SceneManager.GetActiveScene().buildIndex;
            cameraManager.transform.position = new Vector3(0, -2, -10);
            player = FindObjectOfType<Player>();
            angryCustomer = FindObjectOfType<AngryCustomer>();
            if (cameraManager != null)
            {
                cameraManager.target = player.transform;
                cameraManager.transform.position = new Vector3(-3, -2, -10);
            }
        }

        if (shouldFadeIn)
        {
            if (blackScreen.alpha < 1)
            {
                blackScreen.alpha += Time.deltaTime;
            }
            else
            {
                if (!calledLoad)
                {
                    System.GC.Collect();
                    currentScene = 10;
                    if (next) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    else SceneManager.LoadScene(1);
                    calledLoad = true;
                }
            }
        }
        else
        {
            if (blackScreen.alpha > 0)
            {
                blackScreen.alpha -= Time.deltaTime;
            }
        }

        if (SceneManager.GetActiveScene().buildIndex == 0)
            if (continueTime > 0) continueTime -= Time.deltaTime;
            else shouldFadeIn = true;

        if (moneyEffectTime > 0)
        {
            y -= .5f;
            moneyAddText.transform.localPosition = new Vector2(moneyAddText.transform.localPosition.x, y);
            moneyEffectTime -= Time.deltaTime;
        }
        else moneyAddText.SetActive(false);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void NextLevel()
    {
        next = true;
        furnitureSlider.gameObject.SetActive(false);
        shouldFadeIn = true;
        endPanel.SetActive(false);

        if (moneyLeft)
        {
            if (money >= moneyToSpend) UpdateMoney(moneyToSpend);
            else { UpdateMoney(money); moneyLeft = false; }
        }
    }

    public void ResetGame()
    {
        next = false;
        furnitureSlider.gameObject.SetActive(false);
        shouldFadeIn = true;
        endPanel.SetActive(false);
    }

    public void EndGame()
    {
        gameEnded = true;
        called = true;
        if (furniture.Count <= 0) { retryButton.SetActive(true); nextButton.SetActive(false); }
        else { retryButton.SetActive(false); nextButton.SetActive(true); }
        endPanel.SetActive(true);
        float result = ((furnitureLeft / totalFurniture) * 100);

        if (moneyLeft)
        {
            string payString;
            if (money >= moneyToSpend) payString = " You will pay " + moneyToSpend + "$ to buy new furniture";
            else payString = " You will pay " + money + "$ to buy new furniture";
            endPanelText.text = "Congratulations! You kept " + ((int)result).ToString() + "% of your furniture!" + payString;
        }
        else endPanelText.text = "Congratulations! You kept " + ((int)result).ToString() + "% of your furniture!";
        
    }

    public void UpdateFurnitureSlider()
    {
        furnitureSlider.value = furnitureLeft;
        float result = ((furnitureLeft / totalFurniture) * 100);
        furniturePercentage.text = ((int)result).ToString() + "%";
    }

    public void UpdateMoney(int value)
    { 
        money -= value;
        y = baseY;
        moneyAddText.transform.localPosition = new Vector2(moneyAddText.transform.localPosition.x, y);
        moneyAddText.SetActive(true);
        moneyEffectTime = 1;
        moneyText.text = money.ToString() + "$";
    }

    public void StartDialogue(bool enter)
    {
        writingText = false;
        phraseCount = 0;
        if (enter)
        {
            colliders.SetActive(true);
            dialogueStarted = true;
            dialogue = angryCustomer.enterDialogue;
        }
        else
        {
            dialogue = angryCustomer.exitDialogue;
        }
        canvasManager.chatBox.SetActive(true);
        ChooseNextPhrase();
    }

    public IEnumerator ChatboxTextType(string phrase)
    {
        canvasManager.chatText.text = "";
        char[] phraseChars = phrase.ToCharArray();
        writingText = true;

        for (int i = 0; i < phraseChars.Length; i++)
        {
            if (!writingText)
            {
                canvasManager.chatText.text = phrase;
                break;
            }
            canvasManager.chatText.text += phraseChars[i];
            yield return new WaitForSeconds(.065f);
        }

        writingText = false;
    }

    public void ChooseNextPhrase()
    {
        if (!writingText)
        {
            if (phraseCount == dialogue.Count) 
            {
                if (player == null) player = FindObjectOfType<Player>();
                canvasManager.chatBox.SetActive(false); 
                player.canMove = true;
                furnitureSlider.gameObject.SetActive(true);
                dialogueEnded = true;
                if (endDialogueStarted) endDialogueEnded = true;
                sliderParent.SetActive(true);
            }
            else StartCoroutine(ChatboxTextType(dialogue[phraseCount]));
            phraseCount++;
        }
        else
        {
            StopCoroutine("ChatboxTextType");
            canvasManager.chatText.text = dialogue[phraseCount - 1];
            writingText = false;
        }
        
    }
}
