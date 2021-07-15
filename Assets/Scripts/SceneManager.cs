using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace HolographicSharing
{
    public class SceneManager : MonoBehaviour
    {
        private Dictionary<string, GameObject> _sceneGraph;

        private MqttConnection _connection;

        private GameObject pointerSphere;

        void Start()
        {
            gameObject.AddComponent<MqttConnection>();
            _connection = gameObject.GetComponent<MqttConnection>();
            _sceneGraph = new Dictionary<string, GameObject>();
            pointerSphere = GameObject.Find("PointerSphere");
            pointerSphere.SetActive(false);
        }

        private void AddObject(string objectID, GameObject newObject)
        {
            _sceneGraph.Add(objectID, newObject);
        }

        private void DeleteObjectIncoming(DeleteObjectAction deleteObjectAction)
        {
            if(_sceneGraph.TryGetValue(deleteObjectAction.ObjectID, out var currentObject))
            {
                Destroy(currentObject);
                _sceneGraph.Remove(deleteObjectAction.ObjectID);
                _connection.DeleteObjectMessage(deleteObjectAction.ObjectID);
            }
        }

        private void ManipulateObjectIncoming(ManipulateObjectAction manipulateObjectAction)
        {
            if(_sceneGraph.TryGetValue(manipulateObjectAction.ObjectID, out var currentObject))
            {
                currentObject.transform.position = manipulateObjectAction.Position;
                currentObject.transform.rotation = manipulateObjectAction.Rotation;
                currentObject.transform.localScale = manipulateObjectAction.Scale;

                currentObject.transform.hasChanged = false;
            }
        }

        private void SpawnCubeIncoming(SpawnCubeAction cubeAction)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = cubeAction.Position;
            cube.transform.rotation = cubeAction.Rotation;
            cube.transform.localScale = cubeAction.Scale;
            
            cube.AddComponent<ObjectManipulator>();
        
            AddObject(cubeAction.ObjectID, cube);
        }

        private void SpawnSphereIncoming(SpawnSphereAction sphereAction)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = sphereAction.Position;
            sphere.transform.rotation = sphereAction.Rotation;
            sphere.transform.localScale = sphereAction.Scale;
            
            sphere.AddComponent<ObjectManipulator>();
            
            AddObject(sphereAction.ObjectID, sphere);
        }
        
        private void SpawnSkeletonIncoming(SpawnSkeletonAction skeletonAction)
        {
            GameObject skeletonPrefab = Resources.Load("HumanOrgans/SkeletonPrefab") as GameObject;
            GameObject skeleton = Instantiate(skeletonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            skeleton.transform.position = skeletonAction.Position;
            skeleton.transform.rotation = skeletonAction.Rotation;
            skeleton.transform.localScale = skeletonAction.Scale;
            
            skeleton.AddComponent<ObjectManipulator>();
            
            AddObject(skeletonAction.ObjectID, skeleton);
        }

        public void SpawnSkeletonOutgoing()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject skeletonPrefab = Resources.Load("HumanOrgans/SkeletonPrefab") as GameObject;
                GameObject skeleton = Instantiate(skeletonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                skeleton.transform.position = Camera.main.transform.position + Camera.main.transform.forward + new Vector3(0.0f, -0.5f, 0.0f);
                skeleton.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f));
                skeleton.transform.localScale = new Vector3(.4f, .4f, .4f);

                skeleton.AddComponent<ObjectManipulator>();
                skeleton.AddComponent<PointerFocus>();

                string objectID = Guid.NewGuid().ToString();
            
                AddObject(objectID, skeleton);
                _connection.SpawnObjectMessage(ObjectType.Skeleton, Camera.main.transform.position + Camera.main.transform.forward, objectID);
            });
        }

        public void SpawnCubeOutgoing()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                cube.transform.localScale = new Vector3(.2f, .2f, .2f);

                cube.AddComponent<ObjectManipulator>();
                cube.AddComponent<PointerFocus>();

                string objectID = Guid.NewGuid().ToString();
            
                AddObject(objectID, cube);
                _connection.SpawnObjectMessage(ObjectType.Square, Camera.main.transform.position + Camera.main.transform.forward, objectID);
            });
        }

        private void Update()
        {
            Queue<HolographicAction> actionQueue = _connection.GetCurrentActions();
            while (actionQueue?.Count > 0)
            {
                HolographicAction action = actionQueue.Dequeue();
                switch (action)
                {
                    case SpawnCubeAction cubeAction:
                        SpawnCubeIncoming(cubeAction);
                        break;
                    case SpawnSphereAction sphereAction:
                        SpawnSphereIncoming(sphereAction);
                        break;
                    case SpawnSkeletonAction skeletonAction:
                        SpawnSkeletonIncoming(skeletonAction);
                        break;
                    case DeleteObjectAction deleteObjectAction:
                        DeleteObjectIncoming(deleteObjectAction);
                        break;
                    case ManipulateObjectAction manipulateObjectAction:
                        ManipulateObjectIncoming(manipulateObjectAction);
                        break;
                }
            }
            
            
            foreach (var sceneObject in _sceneGraph)
            {
                if (sceneObject.Value.transform.hasChanged)
                {
                    _connection.ManipulateObjectMessage(sceneObject.Key, sceneObject.Value.transform);
                    sceneObject.Value.transform.hasChanged = false;
                }
            }
            
            
            foreach(var source in CoreServices.InputSystem.DetectedInputSources)
            {
                if (source.SourceType == InputSourceType.Hand)
                {
                    foreach (var p in source.Pointers)
                    {
                        if (p is IMixedRealityNearPointer)
                        {
                            // Ignore near pointers, we only want the rays
                            continue;
                        }
                        if (p.Result != null)
                        {
                            var startPoint = p.Position;
                            var endPoint = p.Result.Details.Point;
                            var hitObject = p.Result.Details.Object;
                            if (hitObject)
                            {
                                pointerSphere.SetActive(true);
                                pointerSphere.transform.position = endPoint;
                            }
                            else
                            {
                                pointerSphere.SetActive(false);
                            }
                        }

                    }
                }
            }
        }
    }
}