using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMain : MonoBehaviour
{
    public GameObject humanPrefab;

    public BaseHuman myHuman;

    public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.AddListener("List", OnList);
        NetManager.AddListener("Attack", OnAttack);
        NetManager.AddListener("Hurt", OnHurt);
        NetManager.AddListener("Die", OnDie);

        NetManager.Connect("127.0.0.1", 8888);

        GameObject obj = Instantiate(humanPrefab);
        var x = UnityEngine.Random.Range(-5, 5);
        var z = UnityEngine.Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);

        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();

        //obj.GetComponent<MeshRenderer>().materials[0].color = new Color(
        //    UnityEngine.Random.Range(0, 255),
        //    UnityEngine.Random.Range(0, 255),
        //    UnityEngine.Random.Range(0, 255)
        //    );

        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        string sendStr = "";

        sendStr = ActionProtocols.GetProtocolScript(ActionProtocols.Actions.Enter, new ClientState { 
            x = pos.x,
            y = pos.y,
            z = pos.z,
            eulY = eul.y
        });

        DebugUI.Instance.Log(sendStr);

        NetManager.Send(sendStr);

        sendStr = ActionProtocols.GetProtocolScript(ActionProtocols.Actions.List, null);
        //StartCoroutine(NetManager.DelaySend(1f, sendStr));


        NetManager.Send(sendStr);
    }

    private void OnDie(string str)
    {
        Debug.Log($"OnDie {str}");

        var desc = str;

        if (desc == myHuman.desc)
        {
            Debug.Log("Game Over");
            return;
        }

        if (!otherHumans.ContainsKey(desc))
        {
            return;
        }

        BaseHuman bh = otherHumans[desc];
        bh.gameObject.SetActive(false);
    }

    private void OnHurt(string str)
    {
        Debug.Log($"OnHurt {str}");

        var split = str.Split(',');

        var attackDesc = split[0];

        var hurtDesc = split[1];

        int damage = int.Parse(split[2]);

        if (hurtDesc == myHuman.desc)
        {
            myHuman.Hurt(damage);
            return;
        }

        if (!otherHumans.ContainsKey(hurtDesc))
        {
            return;
        }

        BaseHuman bh = otherHumans[hurtDesc];
        bh.Hurt(damage);
    }

    private void OnAttack(string str)
    {
        Debug.Log($"OnAttack {str}");

        var split = str.Split(',');

        var desc = split[0];

        float eulY = float.Parse(split[1]);

        if (!otherHumans.ContainsKey(desc))
        {
            return;
        }

        SyncHuman sh = otherHumans[desc] as SyncHuman;
        sh.SyncAttack(eulY);
    }

    private void OnList(string str)
    {
        Debug.Log($"OnList {str}");

        var splits = str.Split('/');
        var count = splits.Length;

        foreach(var sp in splits)
        {
            string[] split = sp.Split(',');
            string desc = split[0];

            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            float euly = float.Parse(split[4]);
            int hp = int.Parse(split[5]);

            if (desc == myHuman.desc || string.IsNullOrEmpty(desc))
            {
                continue;
            }

            if (!otherHumans.ContainsKey(desc))
            {
                GameObject go = Instantiate(humanPrefab);
                go.transform.position = new Vector3(x, y, z);
                go.transform.eulerAngles = new Vector3(0, euly, 0);

                BaseHuman h = go.AddComponent<SyncHuman>();
                h.desc = desc;
                otherHumans.Add(desc, h);
            }
        }
    }

    private void OnDestroy()
    {
        NetManager.Close();
    }

    private void OnLeave(string str)
    {
        Debug.Log($"OnLeave {str}");

        //var split = str.Split(',');

        var desc = str;

        if (!otherHumans.ContainsKey(desc))
        {
            return;
        }

        BaseHuman bh = otherHumans[desc];
        otherHumans.Remove(desc);
        Destroy(bh.gameObject);
    }

    private void OnMove(string str)
    {
        Debug.Log($"OnMove {str}");

        var split = str.Split(',');

        var desc = split[0];

        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);

        if (!otherHumans.ContainsKey(desc))
        {
            return;
        }

        BaseHuman bh = otherHumans[desc];
        Vector3 targetPos = new Vector3(x, y, z);
        bh.MoveTo(targetPos);
    }

    private void OnEnter(string str)
    {
        Debug.Log($"OnEnter {str}");

        string[] split = str.Split(',');
        string desc = split[0];

        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        float euly = float.Parse(split[4]);

        if (desc == myHuman.desc || string.IsNullOrEmpty(desc))
        {
            return;
        }

        GameObject go = Instantiate(humanPrefab);
        go.transform.position = new Vector3(x, y, z);
        go.transform.eulerAngles = new Vector3(0, euly, 0);

        BaseHuman h = go.AddComponent<SyncHuman>();
        h.desc = desc;
        otherHumans.Add(desc, h);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }

    private IEnumerator DelaySend(int seconds, string msg)
    {
        yield return new WaitForSeconds(seconds);

        NetManager.Send(msg);
    }
}
