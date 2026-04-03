using UnityEngine;

public class ConfettiWhenEmpty : MonoBehaviour
{
    [Header("Assign your confetti Particle System prefab here")]
    [SerializeField] private ParticleSystem confettiPrefab;

    [Header("Optional")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distanceFromCamera = 5f;
    [SerializeField] private float topOffset = 1f;
    [SerializeField] private Vector2 fallSpeed = new Vector2(3.3f, 6.6f);

    private bool hasPlayed;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        CountedPrefab.OnCountChanged += HandleCountChanged;
    }

    private void OnDisable()
    {
        CountedPrefab.OnCountChanged -= HandleCountChanged;
    }

    private void Start()
    {
        if (CountedPrefab.ActiveCount == 0)
            PlayConfetti();
    }

    private void HandleCountChanged(int count)
    {
        if (!hasPlayed && count == 0)
            PlayConfetti();
    }

    private void PlayConfetti()
    {
        hasPlayed = true;

        if (confettiPrefab == null || targetCamera == null)
        {
            Debug.LogWarning("Confetti prefab or camera is missing.");
            return;
        }

        float depthOffset = Mathf.Max(0.01f, distanceFromCamera);
        float depth = targetCamera.orthographic
            ? targetCamera.nearClipPlane + depthOffset
            : depthOffset;

        Vector3 left = targetCamera.ViewportToWorldPoint(new Vector3(0f, 1f, depth));
        Vector3 right = targetCamera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        Vector3 center = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));

        Vector3 spawnPosition = center + targetCamera.transform.up * topOffset;

        // Align particle system with camera so particles fall parallel to screen
        ParticleSystem confetti = Instantiate(confettiPrefab, spawnPosition, targetCamera.transform.rotation);
        confetti.transform.SetParent(targetCamera.transform, true);

        var main = confetti.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = 0f;

        var shape = confetti.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        float width = Vector3.Distance(left, right);
        shape.scale = new Vector3(width * 1.1f, 0.5f, 0.1f);

        ConfigureVelocity(confetti);

        confetti.Play();

        Destroy(confetti.gameObject, 12f);
    }

    private void ConfigureVelocity(ParticleSystem confetti)
    {
        var velocity = confetti.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;

        float minFall = Mathf.Min(Mathf.Abs(fallSpeed.x), Mathf.Abs(fallSpeed.y));
        float maxFall = Mathf.Max(Mathf.Abs(fallSpeed.x), Mathf.Abs(fallSpeed.y));
        float horizontalMax = maxFall * 0.35f;

        velocity.x = new ParticleSystem.MinMaxCurve(-horizontalMax, horizontalMax);
        velocity.y = new ParticleSystem.MinMaxCurve(-maxFall, -minFall);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    }
}
