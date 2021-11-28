using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    public float speed = 2.2f;

    public string desc = "";

    protected bool isMoving = false;

    protected bool isAttacking = false;

    private Vector3 targetPosition;

    private Animator animator;

    protected float attackTime = float.MinValue;

    protected int hp = 5;

    protected bool isDead = false;

    protected bool isHurt = false;

    protected TextMesh HealthPoint;

    public void MoveTo(Vector3 pos)
    {
        //Debug.Log(pos.ToString());
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);

    }

    public void MoveUpdate()
    {
        if (!isMoving || isDead)
        {
            return;
        }

        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
        transform.LookAt(targetPosition);

        var distance = Vector3.Distance(pos, targetPosition);

        //Debug.Log(distance);

        if (distance < 0.5f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    // Start is called before the first frame update
    protected void Start()
    {
        animator = GetComponent<Animator>();
        HealthPoint = this.gameObject.GetComponentInChildren<TextMesh>();
        HealthPoint.text = $"HP:{hp}";
    }

    // Update is called once per frame
    protected void Update()
    {
        MoveUpdate();
        AttackUpdate();
    }

    public void Attack()
    {
        isAttacking = true;
        attackTime = Time.time;
        animator.SetBool("isAttacking", true);
    }

    public void AttackUpdate()
    {
        if (!isAttacking || isDead)
        {
            return;
        }

        if (Time.time - attackTime < 1.2f)
        {
            return;
        }

        isAttacking = false;

        animator.SetBool("isAttacking", false);
    }

    public void Hurt(int damage)
    {
        if (isDead)
        {
            return;
        }

        this.hp -= damage;

        //isHurt = true;

        HealthPoint.text = $"HP:{hp}";

        if (this.hp <= 0)
        {
            isDead = true;
            return;
        }
    }
}
