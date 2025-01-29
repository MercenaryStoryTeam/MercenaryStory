using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreUI : MonoBehaviour
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
	}

	void Update()
	{
		float dist = Vector3.Distance(Cam.transform.position, transform.position);
		Vector3 newScale = startScale * dist / distance;
		transform.localScale = newScale;

		transform.rotation = Cam.transform.rotation;
	}
}