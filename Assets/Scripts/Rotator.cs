using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(15f, 30f, 45f);

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
