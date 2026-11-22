using UnityEngine;

// Animation event receiver for the enemy's Walk clip.
// The clip's "PlayStep" event is called on the GameObject that has the Animator (Armature).
public class EnemyFootstep : MonoBehaviour
{
    public void PlayStep()
    {
        // Footstep sound or effect can go here later.
    }
}
