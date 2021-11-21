using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMain : MonoBehaviour
{
    public GameObject humanPrefab;

    public BaseHuman myHuman;

    public Dictionary<string, BaseHuman> otherHumans;

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
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
        Vector3 eu1 = myHuman.transform.eulerAngles;
        string sendStr = "Enter|";

        // protocol type: <Command>|<RemoteIPEndPoint>,<Location.x>,<Location.y>,<Location.z>,<Rotation.y>
        sendStr = $"{sendStr}{myHuman.desc},{pos.x},{pos.y},{pos.z},{eu1.y}";

        DebugUI.Instance.Log(sendStr);

        NetManager.Send(sendStr);
    }

    private void OnDestroy()
    {
        NetManager.Disconnect();
    }

    private void OnLeave(string str)
    {
        Debug.Log($"OnLeave {str}");
    }

    private void OnMove(string str)
    {
        Debug.Log($"OnMove {str}");
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

        if (desc == myHuman.desc)
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
}
