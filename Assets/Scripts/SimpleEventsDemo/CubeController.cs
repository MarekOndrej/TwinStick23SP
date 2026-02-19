using UnityEngine;

public class CubeController : MonoBehaviour
{
    [SerializeField] InputListener inputListener;
    Vector3 originalScale;
    bool isLarge = false;

    private void OnEnable()
    {
        inputListener.onSpacePressed += ResizeCube;
    }

    private void OnDisable()
    {
        inputListener.onSpacePressed -= ResizeCube;
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
