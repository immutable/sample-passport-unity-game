using UnityEngine;

public class RotateAnimation : MonoBehaviour
{
    public RectTransform rt;

    void Start()
    {
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * -10);
    }
}
