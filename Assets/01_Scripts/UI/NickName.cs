using UnityEngine;
using Photon.Pun;

public class NickName : MonoBehaviour
{
    public TextMesh textMesh;
    public GameObject Cam;
    public Player player;
    private PhotonView photonView;
    
    Vector3 startScale;
    public float distance = 350;

    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();
        textMesh = gameObject.GetComponent<TextMesh>();
        Cam = GameObject.FindGameObjectWithTag("VirtualCamera");
        startScale = transform.localScale;
        
        photonView = gameObject.GetComponentInParent<PhotonView>();

        if (photonView.IsMine)
        {
            photonView.Owner.NickName = PhotonNetwork.NickName;
        }
        textMesh.text = photonView.Owner.NickName;
    }

    void Update()
    {
        float dist = Vector3.Distance(Cam.transform.position, transform.position);
        Vector3 newScale = startScale * dist / distance;
        transform.localScale = newScale;

        transform.rotation = Cam.transform.rotation;
    }
}