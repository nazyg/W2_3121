using UnityEngine;

// Oyuncunun arkasında toz/duman izi bırakır.
// Sadece top hareket ederken parçacık üretir ve iz, hareket yönünün tersine doğru yayılır.
[RequireComponent(typeof(ParticleSystem))]
public class PlayerTrail : MonoBehaviour
{
    public Rigidbody target;

    [Header("Motion")]
    public float minSpeed = 0.5f;
    public float speedForMaxEmission = 8f;
    public float maxEmission = 60f;

    [Header("Placement")]
    public float behindOffset = 0.45f;
    public float heightOffset = -0.3f;

    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Start()
    {
        if (target == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) target = pc.GetComponent<Rigidbody>();
        }
    }

    void LateUpdate()
    {
        var emission = ps.emission;

        // Oyuncu yok edildiyse (öldüyse) yeni parçacık üretme, mevcutlar sönsün.
        if (target == null)
        {
            emission.rateOverTime = 0f;
            return;
        }

        Vector3 velocity = target.linearVelocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;

        if (speed < minSpeed)
        {
            emission.rateOverTime = 0f;
            transform.position = target.position + Vector3.up * heightOffset;
            return;
        }

        Vector3 dir = velocity / speed;
        transform.position = target.position - dir * behindOffset + Vector3.up * heightOffset;
        transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);

        emission.rateOverTime = Mathf.Lerp(0f, maxEmission, Mathf.Clamp01(speed / speedForMaxEmission));
    }
}
