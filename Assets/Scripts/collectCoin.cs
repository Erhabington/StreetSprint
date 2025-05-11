using UnityEngine;

public class collectCoin : MonoBehaviour
{
    [SerializeField] private GameObject masterUI;
    [SerializeField] AudioSource fxCoin;
    private MasterUI script;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        script = masterUI.GetComponent<MasterUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        fxCoin.Play();
        script.updateScore();
        this.gameObject.SetActive(false);
    }
}
