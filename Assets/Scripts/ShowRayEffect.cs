using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowRayEffect : MonoBehaviour
{
    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    public void ShowRay(Vector3 start, Vector3 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        StartCoroutine(RayEffect());
    }

    private IEnumerator RayEffect()
    {
        line.enabled = true;

        yield return new WaitForSeconds(10f);

        line.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
