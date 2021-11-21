using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    public float speed = 1.2f;

    public string desc = "";

    protected bool isMoving = false;

    private Vector3 targetPosition;

    private Animator animator;

    public void MoveTo(Vector3 pos)
    {
        //Debug.Log(pos.ToString());
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);

    }

    public void MoveUpdate()
    {
        if (!isMoving)
        {
            return;
        }

        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
        transform.LookAt(targetPosition);

        var distance = Vector3.Distance(pos, targetPosition);

        if (distance < 1.0f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    // Start is called before the first frame update
    protected void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected void Update()
    {
        MoveUpdate();
    }
}
