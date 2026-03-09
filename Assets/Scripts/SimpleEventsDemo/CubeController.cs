using UnityEngine;

public class CubeController : MonoBehaviour
{
    [SerializeField] DemoEventManagerSO eventManager;
    Vector3 originalScale;
    bool isLarge = false;

    private void OnEnable()
    {
        eventManager.onResize += ResizeCube;
    }

    private void OnDisable()
    {
        eventManager.onResize -= ResizeCube;
    }

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void ResizeCube()
    {
        if (!isLarge)
        {
            transform.localScale *= 3;
            isLarge = true;
        }
        else
        {
            transform.localScale = originalScale;
            isLarge = false;
        }
    }
}
