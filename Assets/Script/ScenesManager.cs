using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScenesManager : MonoBehaviour
{
    Scenes scenes;
    public enum Scenes
    {
        bootUp,
        title,
        shop,
        level1,
        level2,
        level3,
        gameOver
    }
    
    float gameTimer = 0;
    float[] endLevelTimer = {30,30,45};
    int currentSceneNumber = 0;
    bool gameEnding = false;

    public MusicMode musicMode;
    public enum MusicMode
    {
        noSound, fadeDown, musicOn
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MusicVolume(MusicMode.musicOn));
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
    {
        StartCoroutine(MusicVolume(MusicMode.musicOn));
        
        GetComponent<GameManager>().SetLivesDisplay(GameManager.playerLives);
        
        if (GameObject.Find("score"))
        {
            GameObject.Find("score").GetComponent<Text>().text = GetComponent<ScoreManager>().PlayersScore.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSceneNumber != SceneManager.GetActiveScene().buildIndex)
        {
            currentSceneNumber = SceneManager.GetActiveScene().buildIndex;
            GetScene();
        }
        GameTimer();
    }

    public void ResetScene()
    {
        StartCoroutine(MusicVolume(MusicMode.noSound));
        gameTimer = 0;
        SceneManager.LoadScene(GameManager.currentScene);
    }

    public void GameOver()
    {
        Debug.Log("ENDSCORE: " + GameManager.Instance.GetComponent<ScoreManager>().PlayersScore);
        SceneManager.LoadScene("gameOver");
    }

    public void BeginGame(int gameLevel)
    {
        SceneManager.LoadScene(gameLevel);
    }
    
    void GetScene()
    {
        scenes = (Scenes)currentSceneNumber;
    }
    
    void GameTimer()
    {
        switch (scenes)
        {
            case Scenes.level1 : case Scenes.level2 : case Scenes.level3 :
            {
                if (GetComponentInChildren<AudioSource>().clip == null)
                {
                    AudioClip lvlMusic = Resources.Load<AudioClip>("Sound/lvlMusic") as AudioClip;
                    GetComponentInChildren<AudioSource>().clip = lvlMusic;
                    GetComponentInChildren<AudioSource>().Play();
                }
                
                if (gameTimer < endLevelTimer[currentSceneNumber-3])
                {
                    //if level has not completed
                    gameTimer += Time.deltaTime;
                }
                
                else
                {
                    StartCoroutine(MusicVolume(MusicMode.fadeDown));

                    //if level is completed
                    if (!gameEnding)
                    {
                        gameEnding = true;
                        if (SceneManager.GetActiveScene().name != "level3")
                        {
                            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTransition>().LevelEnds = true;
                        }
                        else
                        {
                            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTransition> ().GameCompleted = true;
                        }
                        
                        SendInJsonFormat(SceneManager.GetActiveScene().name);
                        
                        Invoke("NextLevel",4);
                    }
                    
                }
                
                break;
            }
            
            default :
            {
                GetComponentInChildren<AudioSource>().clip = null;
                break;
            }
        }
    }
    
    void NextLevel()
    {
        gameEnding = false;
        gameTimer = 0;
        SceneManager.LoadScene(GameManager.currentScene+1);
        StartCoroutine(MusicVolume(MusicMode.musicOn));
    }

    IEnumerator MusicVolume(MusicMode musicMode)
    {
        switch (musicMode)
        {
            case MusicMode.noSound :
            {
                GetComponentInChildren<AudioSource>().Stop();
                break;
            }
            case MusicMode.fadeDown :
            {
                GetComponentInChildren<AudioSource>().volume -= Time.deltaTime/3;
                break;
            }
            case MusicMode.musicOn:
            {
                if (GetComponentInChildren<AudioSource>().clip != null)
                {
                    GetComponentInChildren<AudioSource>().Play();
                    GetComponentInChildren<AudioSource>().volume = 1;
                }


            }
                yield return new WaitForSeconds(0.1f);
                
                break;
        }
    }

    void SendInJsonFormat(string lastLevel)
    {
        if (lastLevel == "level3")
        {
            GameStats gameStats = new GameStats();
            gameStats.livesLeft = GameManager.playerLives;
            gameStats.completed = System.DateTime.Now.ToString();
            gameStats.score = GetComponent<ScoreManager>().PlayersScore;

            string json = JsonUtility.ToJson(gameStats, true);
            Debug.Log(json);
            
            Debug.Log(Application.persistentDataPath + "/GameStatsSaved.json");
            System.IO.File.WriteAllText(Application.persistentDataPath + "/GameStatsSaved.json",json);
        }
    }
}
