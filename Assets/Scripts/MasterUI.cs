using UnityEngine;

public class MasterUI : MonoBehaviour
{
    public static int score=0;
    [SerializeField] GameObject scoreDisplay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scoreDisplay.GetComponent<TMPro.TMP_Text>().text = "Score: " + score;
    }

    public void updateScore()
    {
        
        score += 5;
    }
}
