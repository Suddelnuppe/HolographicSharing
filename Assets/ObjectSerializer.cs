using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSerializer : MonoBehaviour
{

    private GameObject _gameObject;
    // Start is called before the first frame update
    void Start()
    {
        _gameObject = GameObject.Find("Cube");
        string json = JsonUtility.ToJson(_gameObject.GetComponent<MeshFilter>().ToString());
        
        Debug.LogError(json.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
