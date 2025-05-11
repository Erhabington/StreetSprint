using UnityEngine;

public class coinRotate : MonoBehaviour
{
    [SerializeField]float Speed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Speed, 0, Space.World);
    }
}
