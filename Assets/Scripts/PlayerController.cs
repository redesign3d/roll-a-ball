using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private const string BasketVisualRootName = "BasketVisual";
    private const string CollectedEggsRootName = "CollectedEggs";
    private const int EggColumns = 3;
    private const int EggRows = 2;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float collectedEggScale = 0.22f;
    [SerializeField] private AudioSource audioSource;

    private Rigidbody rb;
    private BoxCollider basketCollider;
    private Transform collectedEggsRoot;
    private int collectedEggCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        basketCollider = GetComponent<BoxCollider>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        EnsureCollectedEggsRoot();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.angularDamping = Mathf.Max(rb.angularDamping, 10f);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PickUp"))
        {
            return;
        }

        AddCollectedEgg(other);
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
        Vector3 moveInput = GetMoveInput();
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 targetPlanarVelocity = moveInput * moveSpeed;
        float speedChange = moveInput.sqrMagnitude > 0.001f ? acceleration : deceleration;
        Vector3 nextPlanarVelocity = Vector3.MoveTowards(
            currentPlanarVelocity,
            targetPlanarVelocity,
            speedChange * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(nextPlanarVelocity.x, currentVelocity.y, nextPlanarVelocity.z);
        rb.angularVelocity = Vector3.zero;

        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput, Vector3.up);
            Quaternion nextRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(nextRotation);
        }
    }

    private static Vector3 GetMoveInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        return Vector3.ClampMagnitude(new Vector3(moveX, 0f, moveZ), 1f);
    }

    private void EnsureCollectedEggsRoot()
    {
        Transform existingRoot = transform.Find(CollectedEggsRootName);
        if (existingRoot != null)
        {
            collectedEggsRoot = existingRoot;
            return;
        }

        collectedEggsRoot = new GameObject(CollectedEggsRootName).transform;
        collectedEggsRoot.SetParent(transform, false);

        Transform basketVisual = transform.Find(BasketVisualRootName);
        if (basketVisual != null)
        {
            collectedEggsRoot.SetSiblingIndex(basketVisual.GetSiblingIndex() + 1);
        }
    }

    private void AddCollectedEgg(Collider source)
    {
        MeshFilter sourceMeshFilter = source.GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
        if (sourceMeshFilter == null || sourceRenderer == null)
        {
            return;
        }

        if (collectedEggsRoot == null)
        {
            EnsureCollectedEggsRoot();
        }

        Vector3 eggSize = GetCollectedEggSize(sourceMeshFilter, source.transform.localScale);
        GameObject egg = new GameObject($"CollectedEgg {collectedEggCount + 1}");
        egg.transform.SetParent(collectedEggsRoot, false);
        egg.transform.localPosition = GetCollectedEggPosition(collectedEggCount, eggSize);
        egg.transform.localRotation = source.transform.localRotation;
        egg.transform.localScale = source.transform.localScale * collectedEggScale;

        MeshFilter eggMeshFilter = egg.AddComponent<MeshFilter>();
        eggMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

        MeshRenderer eggRenderer = egg.AddComponent<MeshRenderer>();
        eggRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
        eggRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        eggRenderer.receiveShadows = true;

        collectedEggCount++;
    }

    private Vector3 GetCollectedEggPosition(int index, Vector3 eggSize)
    {
        int eggsPerLayer = EggColumns * EggRows;
        int layer = index / eggsPerLayer;
        int indexInLayer = index % eggsPerLayer;
        int column = indexInLayer % EggColumns;
        int row = indexInLayer / EggColumns;

        float basketWidth = basketCollider != null ? basketCollider.size.x : 0.9f;
        float basketDepth = basketCollider != null ? basketCollider.size.z : 0.5f;
        float basketFloor = basketCollider != null ? basketCollider.center.y * 0.25f : 0.12f;
        float layerHeight = basketCollider != null ? Mathf.Max(0.08f, basketCollider.size.y * 0.11f) : 0.1f;

        float minXSpacing = Mathf.Max(eggSize.x * 1.05f, 0.1f);
        float minZSpacing = Mathf.Max(eggSize.z * 1.05f, 0.1f);
        float minLayerHeight = Mathf.Max(eggSize.y * 1.1f, 0.08f);

        float xSpacing = Mathf.Max(minXSpacing, Mathf.Min(0.2f, basketWidth * 0.28f));
        float zSpacing = Mathf.Max(minZSpacing, Mathf.Min(0.16f, basketDepth * 0.38f));
        layerHeight = Mathf.Max(layerHeight, minLayerHeight);

        float x = (column - (EggColumns - 1) * 0.5f) * xSpacing;
        float z = (row - (EggRows - 1) * 0.5f) * zSpacing;
        float y = basketFloor + layer * layerHeight;

        return new Vector3(x, y, z);
    }

    private Vector3 GetCollectedEggSize(MeshFilter sourceMeshFilter, Vector3 sourceScale)
    {
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            return Vector3.one * collectedEggScale;
        }

        Vector3 meshSize = sourceMeshFilter.sharedMesh.bounds.size;
        Vector3 scaledSize = Vector3.Scale(meshSize, sourceScale) * collectedEggScale;

        return new Vector3(
            Mathf.Max(0.001f, scaledSize.x),
            Mathf.Max(0.001f, scaledSize.y),
            Mathf.Max(0.001f, scaledSize.z));
    }
}
