using UnityEngine;

public class Patrol : MonoBehaviour
{
    public enum PathMode { Loop, PingPong }
    public enum PauseMode { NoPause, FreezePose, PlayIdle }

    [Tooltip("The points to walk to, in order.")]
    public Transform[] waypoints;

    [Tooltip("How fast to walk, in units per second.")]
    public float speed = 2.0f;

    [Tooltip("Loop: 1-2-3-1. Ping Pong: 1-2-3-2-1.")]
    public PathMode pathMode = PathMode.Loop;

    [Tooltip("What to do at each point. Play Idle needs a Speed parameter.")]
    public PauseMode pauseMode = PauseMode.FreezePose;

    [Tooltip("Seconds to pause at each point. 0 = no pause.")]
    public float waitTime = 1.0f;

    Rigidbody2D rigidbody2d;
    Animator animator;
    int targetIndex = 0;
    int step = 1;
    float waitTimer = 0.0f;
    bool hasSpeedParameter;

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        hasSpeedParameter = HasParameter("Speed");
        StartWalking();
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                StartWalking();
            }
            return;
        }

        Vector2 position = rigidbody2d.position;
        Vector2 target = waypoints[targetIndex].position;
        Vector2 direction = target - position;

        if (direction.magnitude < 0.01f)
        {
            NextWaypoint();
            if (pauseMode != PauseMode.NoPause && waitTime > 0)
            {
                StartWaiting();
            }
            return;
        }

        direction.Normalize();
        animator.SetFloat("Look X", direction.x);
        animator.SetFloat("Look Y", direction.y);
        float stepSize = speed * Time.deltaTime;
        Vector2 newPosition =
            Vector2.MoveTowards(position, target, stepSize);
        rigidbody2d.MovePosition(newPosition);
    }

    void NextWaypoint()
    {
        if (waypoints.Length < 2)
        {
            return;
        }

        if (pathMode == PathMode.Loop)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
        }
        else
        {
            int next = targetIndex + step;
            if (next < 0 || next >= waypoints.Length)
            {
                step = -step;
            }
            targetIndex += step;
        }
    }

    void StartWaiting()
    {
        waitTimer = waitTime;
        if (pauseMode == PauseMode.PlayIdle && hasSpeedParameter)
        {
            animator.SetFloat("Speed", 0);
        }
        else
        {
            animator.speed = 0;
        }
    }

    void StartWalking()
    {
        animator.speed = 1;
        if (hasSpeedParameter)
        {
            animator.SetFloat("Speed", 1);
        }
    }

    bool HasParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            DrawPathLine(waypoints[i], waypoints[i + 1]);
        }
        if (pathMode == PathMode.Loop)
        {
            int last = waypoints.Length - 1;
            DrawPathLine(waypoints[last], waypoints[0]);
        }
    }

    void DrawPathLine(Transform from, Transform to)
    {
        if (from != null && to != null)
        {
            Gizmos.DrawLine(from.position, to.position);
        }
    }
}
