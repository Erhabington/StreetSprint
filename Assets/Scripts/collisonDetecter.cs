using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class collisonDetecter : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private AudioSource collsionFX;
    [SerializeField] RawImage fadeOut;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CollisionEnd());
    }

    private IEnumerator CollisionEnd()
    {
        collsionFX.Play();
        player.GetComponent<PlayerMovement>().enabled = false;
        Animation anim = player.GetComponent<Animation>();
        if (anim != null)
        {
            anim.Play("Dizzy");
        }
        yield return new WaitForSeconds(1);
        fadeOut.GetComponent<Animator>().enabled = true;
        yield return new WaitForSeconds(3);
        MasterUI.score = 0;
        SceneManager.LoadScene(0);
    }
}
