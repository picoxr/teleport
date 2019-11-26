using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("12时间" + Time.timeSinceLevelLoad);
        StartCoroutine("UpdataBattery");
    }

    IEnumerator UpdataBattery()
    {
        while (true)
        {
           
            Debug.Log("时间" + Time.timeSinceLevelLoad);
            //一直在执行上边的代码 执行完  yield才生效
            yield return new WaitForSeconds(2f);
            Debug.Log("时间11" + Time.timeSinceLevelLoad);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
