using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (isDead)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Physics.Raycast(ray, out hit);

            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);

                var sendStr = ActionProtocols.GetProtocolScript(ActionProtocols.Actions.Move, new ClientState
                {
                    x = hit.point.x,
                    y = hit.point.y,
                    z = hit.point.z,
                });

                NetManager.Send(sendStr);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking)
            {
                return;
            }

            if (isMoving)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Physics.Raycast(ray, out hit);

            transform.LookAt(hit.point);
            Attack();

            var sendStr = ActionProtocols.GetProtocolScript(ActionProtocols.Actions.Attack, new ClientState
            {
                eulY = transform.eulerAngles.y
            });

            NetManager.Send(sendStr);

            // attack judge

            Vector3 lineEnd = transform.position + 2f * Vector3.up;
            Vector3 lineStart = lineEnd + 10 * transform.forward;

            this.GetComponent<ShowRayEffect>()?.ShowRay(lineStart, lineEnd);

            if (Physics.Linecast(lineStart, lineEnd, out hit))
            {
                GameObject go = hit.collider.gameObject;
                
                if (go == this.gameObject)
                {
                    return;
                }

                SyncHuman h = go.GetComponentInParent<SyncHuman>();
                
                if (h != null)
                {
                    sendStr = ActionProtocols.GetProtocolScript(ActionProtocols.Actions.Hurt, new ClientState { desc = h.desc });

                    StartCoroutine(NetManager.DelaySend(1f, sendStr));
                }
            }
        }
    }
}
