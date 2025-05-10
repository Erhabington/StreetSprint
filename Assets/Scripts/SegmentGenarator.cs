using System.Collections;
using UnityEngine;

public class SegmentGenarator : MonoBehaviour
{
    public GameObject[] segment;
    [SerializeField] int zPos = 105;
    [SerializeField] bool creatingSegment = false;
    [SerializeField] int segmentNum;
    void Update()
    {
        if (creatingSegment==false)
        {
            creatingSegment = true;
            StartCoroutine(SegmentGen());
        }
        
    }
    IEnumerator SegmentGen()
    {
        segmentNum = Random.Range(0, 3);
        Instantiate(segment[segmentNum], new Vector3(0, 0, zPos), Quaternion.identity);
        zPos += 105;
        yield return new WaitForSeconds(3);
        creatingSegment = false;
    }
}
