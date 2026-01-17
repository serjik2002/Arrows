using UnityEngine;
using System.Collections;

public class BottomBarIndicator : MonoBehaviour
{
    [SerializeField] private RectTransform _indicator;
    [SerializeField] private float _moveDuration = 0.25f;

    private Coroutine _moveCoroutine;

    public void MoveTo(RectTransform target)
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _moveCoroutine = StartCoroutine(MoveIndicator(target));
    }

    IEnumerator MoveIndicator(RectTransform target)
    {
        Vector3 startPos = _indicator.position;
        Vector3 endPos = new Vector3(
            target.position.x,
            _indicator.position.y,
            _indicator.position.z
        );

        float elapsed = 0f;

        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _moveDuration;

            _indicator.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        _indicator.position = endPos;
    }
}
