using UnityEngine;

public class bugMovement : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float moveSpeed = 3f;
    public float rotateSpeed = 8f;

    private Transform currentTarget;

    void Start()
    {
        // Start moving toward B first (optional)
        currentTarget = pointB;
    }

    void Update()
    {
        Move();
        RotateTowardsTarget();
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            moveSpeed * Time.deltaTime
        );

        // Check if reached target
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.05f)
        {
            SwitchTarget();
        }
    }

    void SwitchTarget()
    {
        currentTarget = (currentTarget == pointA) ? pointB : pointA;
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}
