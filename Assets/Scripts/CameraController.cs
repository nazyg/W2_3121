using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player; // Reference to the player GameObject
    private Vector3 offset; // Offset between the camera and the player
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - player.transform.position; // Calculate the initial offset
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player != null)
            transform.position = player.transform.position + offset; // Update the camera position based on the player's position and the offset
        
    }
}
