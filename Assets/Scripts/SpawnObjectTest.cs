using HolographicSharing;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class SpawnObjectTest : MonoBehaviour
{

    private GameObject _button;

    private SceneManager _sceneManager;
    void Start()
    {
        _sceneManager = gameObject.AddComponent<SceneManager>();
        
        _button = GameObject.Find("SpawnButton");
        _button.GetComponent<Interactable>().OnClick.AddListener(delegate
        {
            _sceneManager.SpawnSkeletonOutgoing();
        });
    }

}
