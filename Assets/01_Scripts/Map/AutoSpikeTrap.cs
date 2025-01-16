using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class AutoSpikeTrap : MonoBehaviour
{
    public float spikeDamage = 5f;
    public float trapCool = 4f;
    public float trapStartTime;
    public Spike spike;
    public float additionalHeight = 2f;
    public LayerMask layerMask;
    public Collider[] colliders;
    
    private MeshRenderer meshRenderer;

    public void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        layerMask = LayerMask.GetMask("Player");
        StartCoroutine(trap());
    }

    private void Update()
    {
        Bounds bounds = meshRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.extents;

        size.y += additionalHeight / 2f;

        Collider[] hitColliders = Physics.OverlapBox(center, size, gameObject.transform.rotation, layerMask);
        colliders = hitColliders;
        
    }

    IEnumerator trap()
    {
        yield return new WaitForSeconds(trapStartTime);
        while (true)
        {
            spike.PopUp();
            foreach (var hitCollider in colliders)
            {
                hitCollider.gameObject.GetComponent<Player>().TakeDamage(spikeDamage);
            }
            yield return new WaitForSeconds(trapCool);
        }
    }
}
