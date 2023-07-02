using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class PlayerShipBuild : MonoBehaviour, IUnityAdsListener, IUnityAdsInitializationListener
{
    private GameObject target;
    private GameObject tmpSelection;
    private GameObject textBoxPanel;

    [SerializeField] private GameObject[] visualWeapons;
    [SerializeField] private SOActorModel defaultPlayerShip;
    private GameObject playerShip;
    private GameObject buyButton;
    private GameObject bankObj;
    private int bank = 600;
    private bool purchaseMade;

    [SerializeField] private string androidGameId;
    [SerializeField] private string iOSGameId;
    //[SerializeField] private bool testMode = true;
    private string adId;

    private void Awake()
    {
        CheckPlatform();
    }

    private void CheckPlatform()
    {
        string gameId = null;
#if UNITY_IOS
    {
        gameId = iOSGameId;
        adId = "Rewarded_iOS";
    }
#elif UNITY_ANDROID
        {
            gameId = androidGameId;
            adId = "Rewarded_Android";
        }
#endif
        Advertisement.Initialize(gameId, false, this);
    }

    private void Start()
    {
        StartCoroutine(WaitForAd());
        textBoxPanel = GameObject.Find("textBoxPanel");
        TurnOffSelectionHighlights();

        purchaseMade = false;
        bankObj = GameObject.Find("bank");
        bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
        buyButton = GameObject.Find("BUY?").gameObject;
        buyButton.SetActive(false);
        TurnOffPlayerShipVisuals();
        PreparePlayerShipForUpgrade();
    }

    private IEnumerator WaitForAd()
    {
        while (!Advertisement.isInitialized) yield return null;
        LoadAd();
    }

    private void LoadAd()
    {
        //Advertisement.AddListener(this);
        Advertisement.Load(adId);
    }

    private void TurnOffSelectionHighlights()
    {
        var selections = GameObject.FindGameObjectsWithTag("Selection");
        for (var i = 0; i < selections.Length; i++)
            if (selections[i].GetComponentInParent<ShopPiece>())
                if (selections[i].GetComponentInParent<ShopPiece>().ShopSelection.iconName == "sold Out")
                    selections[i].SetActive(false);
    }

    public void AttemptSelection(GameObject buttonName)
    {
        if (buttonName)
        {
            TurnOffSelectionHighlights();
            tmpSelection = buttonName;

            tmpSelection.transform.GetChild(1).gameObject.SetActive(true);

            UpdateDescriptionBox();
            //not sold
            if (buttonName.GetComponentInChildren<Text>().text !=
                "SOLD")
            {
                //can afford
                Affordable();
                //can not afford
                LackOfCredits();
            }
            else if (buttonName.GetComponentInChildren<Text>().text
                     == "SOLD")
            {
                SoldOut();
            }
        }
    }

    public void WatchAdvert()
    {
        Advertisement.Show(adId);
    }

    public void StartGame()
    {
        if (purchaseMade)
        {
            playerShip.name = "UpgradedShip";

            if (playerShip.transform.Find("energy +1(Clone)")) playerShip.GetComponent<Player>().Health = 2;

            DontDestroyOnLoad(playerShip);
        }

        GameManager.Instance.GetComponent<ScenesManager>().BeginGame(GameManager.gameLevelScene);
    }

    public void BuyItem()
    {
        Debug.Log("PURCHASED");
        purchaseMade = true;
        buyButton.SetActive(false);
        textBoxPanel.transform.Find("desc").gameObject.GetComponent<TextMesh>().text = "";
        textBoxPanel.transform.Find("name").gameObject.GetComponent<TextMesh>().text = "";
        //tmpSelection.SetActive(false);

        for (var i = 0; i < visualWeapons.Length; i++)
            if (visualWeapons[i].name == tmpSelection.GetComponent<ShopPiece>().ShopSelection.iconName)
                visualWeapons[i].SetActive(true);

        UpgradeToShip(tmpSelection.GetComponent<ShopPiece>().ShopSelection.iconName);

        bank = bank - short.Parse(tmpSelection.GetComponent<ShopPiece>().ShopSelection.cost);
        bankObj.transform.Find("bankText").GetComponent<TextMesh>().text = bank.ToString();
        tmpSelection.transform.Find("itemText").GetComponentInChildren<Text>().text = "SOLD";
    }

    private void UpgradeToShip(string upgrade)
    {
        var shipItem = Instantiate(Resources.Load(upgrade)) as GameObject;
        shipItem.transform.SetParent(playerShip.transform);
        shipItem.transform.localPosition = Vector3.zero;
    }

    private void Affordable()
    {
        if (bank >= int.Parse(tmpSelection.GetComponentInChildren<Text>().text))
        {
            Debug.Log("CAN BUY");
            buyButton.SetActive(true);
        }
    }

    private void SoldOut()
    {
        Debug.Log("SOLD OUT");
    }

    private void TurnOffPlayerShipVisuals()
    {
        for (var i = 0; i < visualWeapons.Length; i++) visualWeapons[i].gameObject.SetActive(false);
    }

    private void PreparePlayerShipForUpgrade()
    {
        playerShip = Instantiate(defaultPlayerShip.actor);
        playerShip.GetComponent<Player>().enabled = false;
        playerShip.transform.position = new Vector3(0, 10000, 0);
        playerShip.GetComponent<IActorTemplate>().ActorStats(defaultPlayerShip);
    }

    private void LackOfCredits()
    {
        if (bank < int.Parse(tmpSelection.GetComponentInChildren<Text>().text)) Debug.Log("CAN'T BUY");
    }

    private void UpdateDescriptionBox()
    {
        textBoxPanel.transform.Find("name").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponent<ShopPiece>().ShopSelection.iconName;
        textBoxPanel.transform.Find("desc").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponent<ShopPiece>().ShopSelection.description;
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            // REWARD PLAYER
            Debug.Log("Unity Ads Rewarded Ad Completed");
            bank += 300;
            bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
        }
        else if (showCompletionState == UnityAdsShowCompletionState.SKIPPED)
        {
            Debug.LogWarning("The ad was skipped.");
        }

        //Advertisement.Load(placementId, this);
        TurnOffSelectionHighlights();
    }

    public void OnUnityAdsReady(string placementId)
    {
    }

    public void OnUnityAdsDidError(string message)
    {
    }

    public void OnUnityAdsDidStart(string placementId)
    {
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }
}

internal interface IUnityAdsListener
{
}