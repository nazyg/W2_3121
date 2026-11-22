using UnityEngine;
using UnityEngine.AI;

public class EnenmyController : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    private Animator anim;
    void Start()
    {
        if (agent == null)
        {
            agent = GetComponentInParent<NavMeshAgent>();
        }
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if(player != null)
        {
            agent.SetDestination(player.position);
        }

        if (anim != null)
        {
            anim.SetBool("Walk", agent.velocity.magnitude > 0.1f);
        }
    }
}
