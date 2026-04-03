using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private AudioSource audioSource;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PickUp"))
        {
            return;
        }

        other.gameObject.SetActive(false);

        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Crash Sound AudioSource is not assigned.");
        }
    }

    private void FixedUpdate()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }

        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        rb.AddForce(movement * moveSpeed, ForceMode.Force);
    }
}
