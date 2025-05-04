using UnityEngine;

public class MobileUI : MonoBehaviour
{
    void Start()
    {
        // Hide this UI element if not running on a mobile platform.
        gameObject.SetActive(Application.isMobilePlatform);
    }
}
