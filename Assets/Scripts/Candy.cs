using System.Collections;
using UnityEngine;

public class Candy : MonoBehaviour
{
    public int type;

    
    public void MoveTo(Vector2 newPosition, float duration)
    {
        StartCoroutine(MoveAnimation(newPosition, duration));
    }

    IEnumerator MoveAnimation(Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = transform.position;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}
