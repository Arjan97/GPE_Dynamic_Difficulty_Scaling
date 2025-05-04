using JSG.FortuneSpinWheel;
using UnityEngine;
using System.Collections;
public class FortuneWheelOverlay : MonoBehaviour
{
    [SerializeField] GameObject overlayObject;
    [SerializeField] float closeDelay = 1f;
    FortuneSpinWheel wheel;
    Coroutine _fallback;

    void Awake()
    {
        if (overlayObject == null) overlayObject = gameObject;
        wheel = overlayObject.GetComponent<FortuneSpinWheel>();
        overlayObject.SetActive(false);
    }

    void OnEnable()
    {
        if (wheel != null)
        {
            wheel.OnSpinStart += Show;
            wheel.OnSpinComplete += HideWithDelay;
        }
    }

    void OnDisable()
    {
        if (wheel != null)
        {
            wheel.OnSpinStart -= Show;
            wheel.OnSpinComplete -= HideWithDelay;
        }
    }

    public void Show()
    {
        if (_fallback != null) StopCoroutine(_fallback);
        if (wheel != null)
            wheel.Reset();
        overlayObject.SetActive(true);
        _fallback = StartCoroutine(FallbackHide());
    }

    void HideWithDelay(int rewardIndex)
    {
        if (_fallback != null) StopCoroutine(_fallback);
        StartCoroutine(_HideAfter(closeDelay));
    }

    IEnumerator _HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        overlayObject.SetActive(false);

        if (wheel != null)
            wheel.Reset();

        _fallback = null;
    }


    IEnumerator FallbackHide()
    {
        // hide no matter what after e.g. 10s
        yield return new WaitForSeconds(10f);
        overlayObject.SetActive(false);
        _fallback = null;
    }
}
