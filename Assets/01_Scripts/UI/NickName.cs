using UnityEngine;

public class NickName : MonoBehaviour
{
    public TextMesh textMesh;
    public GameObject Cam;
    
    Vector3 startScale;
    public float distance = 350;

    void Start()
    {
        textMesh = gameObject.GetComponent<TextMesh>();
        Cam = GameObject.FindGameObjectWithTag("VirtualCamera");
        startScale = transform.localScale; 
        textMesh.text = FirebaseManager.Instance.CurrentUserData.user_Name;
    }

    void Update()
    {
        float dist = Vector3.Distance(Cam.transform.position, transform.position);
        Vector3 newScale = startScale * dist / distance;
        transform.localScale = newScale;

        transform.rotation = Cam.transform.rotation;
    }
}