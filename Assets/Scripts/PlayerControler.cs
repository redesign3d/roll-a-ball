using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    private Rigidbody rb;
    public float moveSpeed;
    [SerializeField]
    private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
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
    }
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            var crashSoundObject = GameObject.Find("Crash Sound");
            if (crashSoundObject != null)
            {
                audioSource = crashSoundObject.GetComponent<AudioSource>();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float moveX = 0f; //Left/rigth movement
        float moveZ = 0f; //Forwar/backward movement

        // Horizontal input (X axis)
        if (Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;

        // "Vertical" input (Z axis): up/down here representing forward/backward
        if (Input.GetKey(KeyCode.UpArrow))
            moveZ = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            moveZ = -1f;


        // Create a movement vector
        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        rb.AddForce(movement * moveSpeed, ForceMode.Force);
    }
}
