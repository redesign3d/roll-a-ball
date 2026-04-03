using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [FormerlySerializedAs("player")]
    [SerializeField] private Transform target;
    private Vector3 offset;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraController target is not assigned.");
            enabled = false;
            return;
        }

        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
    }
}
