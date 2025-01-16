using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private Vector3 originalPosition;
    public float popUpTime = 0.2f;
    public float replaceTime = 1f;
    public float spikeHeight = 2f;

    private Vector3 targetPosition;

    public void Awake()
    {
        originalPosition = transform.position;
        targetPosition = originalPosition + new Vector3(0, spikeHeight, 0);
    }

    public void PopUp()
    {
        StartCoroutine(PopUpRoutine());
    }

    private IEnumerator PopUpRoutine()
    {
        yield return StartCoroutine(MoveSpike(originalPosition, targetPosition, popUpTime));

        yield return StartCoroutine(MoveSpike(targetPosition, originalPosition, replaceTime));
    }

    private IEnumerator MoveSpike(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }
}