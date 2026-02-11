using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        //  copy camera rotation
        transform.rotation = camera.transform.rotation;
    }


}
