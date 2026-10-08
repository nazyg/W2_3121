using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float moveInputX;
    private float moveInputZ;
    public float speed = 5.0f;
    private int count;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject loseTextObject;

    [Header("Audio")]
    public float wallSoundCooldown = 0.25f;
    private float lastWallSoundTime = -10f;
    private bool hasWon = false;
    private bool isDead = false;

    void Start()
    {
        count = 0;
        rb = GetComponent<Rigidbody>();
        SetCountText();
        winTextObject.SetActive(false);
        if (loseTextObject != null) loseTextObject.SetActive(false);
    }

    void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        moveInputX = moveInput.x;
        moveInputZ = moveInput.y;
    }

    private void FixedUpdate()
    {
        Vector3 movementVector = new Vector3(moveInputX, 0.0f, moveInputZ);
        rb.AddForce(movementVector * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false);
            AudioManager.Instance.PlayPickup();
            count++;
            SetCountText();
        }
        else if (IsEnemy(other.gameObject))
        {
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (Time.time - lastWallSoundTime > wallSoundCooldown)
            {
                lastWallSoundTime = Time.time;
                AudioManager.Instance.PlayWallHit();
            }
        }
        else if (IsEnemy(collision.gameObject))
        {
            Die();
        }
    }

    private bool IsEnemy(GameObject obj)
    {
        if (obj.CompareTag("Enemy")) return true;
        Transform parent = obj.transform.parent;
        return parent != null && parent.CompareTag("Enemy");
    }

    private void Die()
    {
        if (isDead || hasWon) return;
        isDead = true;
        AudioManager.Instance.PlayPlayerDeath();
        if (loseTextObject != null) loseTextObject.SetActive(true);
        Destroy(gameObject);
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 8 && !hasWon)
        {
            hasWon = true;
            winTextObject.SetActive(true);
            AudioManager.Instance.PlayWin();
        }
    }
}
