using UnityEngine;

public class NickName : MonoBehaviour
{
    public GameObject Cam;
    
    Vector3 startScale;
    public float distance = 350;

    void Start()
    {
        Cam = GameObject.FindGameObjectWithTag("VirtualCamera");
        startScale = transform.localScale; 
    }

    void Update()
    {
        float dist = Vector3.Distance(Cam.transform.position, transform.position);
        Vector3 newScale = startScale * dist / distance;
        transform.localScale = newScale;

        transform.rotation = Cam.transform.rotation;
    }
}