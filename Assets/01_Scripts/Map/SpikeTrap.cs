using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SpikeTrap : MonoBehaviour
{
    public float spikeDamage = 5f;
    public float trapCool = 5f;
    public bool trapActive = false;
    public Spike spike;

    public LayerMask layerMask;
    private MeshRenderer meshRenderer;

    public float additionalHeight = 2f;

    private void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        layerMask = LayerMask.GetMask("Player");
    }

    void Update()
    {
        Bounds bounds = meshRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.extents;

        size.y += additionalHeight / 2f;

        Collider[] hitColliders = Physics.OverlapBox(center, size, gameObject.transform.rotation, layerMask);

        foreach (var hitCollider in hitColliders)
        {
            print("Detected: " + hitCollider.name);
        }

        if (hitColliders.Length > 0 && !trapActive)
        {
            StartCoroutine(StartCool());
            foreach (Collider hitCollider in hitColliders)
            {
                hitCollider.gameObject.GetComponent<Player>().TakeDamage(spikeDamage);
            }
            spike.PopUp();
        }
    }

    void OnDrawGizmos()
    {
        if (meshRenderer == null)
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        Bounds bounds = meshRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.extents;

        // 박스의 높이 조정
        size.y += additionalHeight / 2f;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(center, gameObject.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size * 2);
    }

    IEnumerator StartCool()
    {
        trapActive = true;
        yield return new WaitForSeconds(trapCool);
        trapActive = false;
    }
}