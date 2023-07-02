using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    static int playerScore;
    public int PlayersScore
    {
        get
        {
            return playerScore;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScore(int incomingScore)
    {
        playerScore += incomingScore;
        UpdateScore();
    }
    

    public void ResetScore()
    {
        playerScore = 00000000;
        UpdateScore();
    }

    void UpdateScore()
    {
        if (GameObject.Find("score"))
        {
            GameObject.Find("score").GetComponent<Text>().text = playerScore.ToString();
        }
    }

}
