using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using UnityEngine;
using UnityEngine.Serialization;

namespace HolographicSharing
{
    public class MqttConnection : MonoBehaviour
    {
        private IMqttClient _mqttClient;

        private string _ownGuid;

        private Queue<HolographicAction> _messageQueue;

        private void Start()
        {
            _messageQueue = new Queue<HolographicAction>();
            
            _ownGuid = Guid.NewGuid().ToString();
            
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            
            var options = new MqttClientOptionsBuilder()
#if UNITY_IOS
                .WithClientId("clientId-x4nSxDGOJq")
#else
                .WithClientId("clientId-EBGvBoqWR2")
#endif
                .WithWebSocketServer("broker.mqttdashboard.com:8000/mqtt")
                .WithCleanSession()
                .WithKeepAlivePeriod(new TimeSpan(60))
                .Build();
            
            _mqttClient.UseConnectedHandler(async e =>
            {
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("testtopic/objectSpawnTest123").Build());
            });
            
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                MqttMessage message = JsonUtility.FromJson<MqttMessage>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                
                if (!message.clientID.Equals(_ownGuid))
                {
                    switch (message.type)
                    {
                        case OperationType.Create:
                            CreateObject(message.objectType, message.objectID, message.position, message.rotation, message.scale);
                            break;
                        case OperationType.Delete:
                            DeleteObject((message.objectID));
                            break;
                        case OperationType.Manipulate:
                            ManipulateObject(message.objectID, message.position, message.rotation, message.scale);
                            break;
                    }
                }
            });

            _mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        private void OnApplicationQuit()
        {
            _mqttClient.DisconnectAsync();
        }

        public async void SpawnObjectMessage(ObjectType objectType, Vector3 spawnPosition, string objectID)
        {
            MqttMessage mqttMessage = new MqttMessage
            {
                timestamp = GetCurrentTimestamp(),
                clientID = _ownGuid,
                objectType = objectType,
                objectID = objectID,
                type = OperationType.Create,
                position = spawnPosition,
                scale = new Vector3(.2f, .2f, .2f)
            };

            string messageJson = JsonUtility.ToJson(mqttMessage);
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("testtopic/objectSpawnTest123")
                .WithPayload(messageJson)
                .WithExactlyOnceQoS()
                .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async void ManipulateObjectMessage(string objectID, Transform objectTransform)
        {
            MqttMessage mqttMessage = new MqttMessage
            {
                timestamp = GetCurrentTimestamp(),
                clientID = _ownGuid,
                objectID = objectID,
                type = OperationType.Manipulate,
                position = objectTransform.position,
                rotation = objectTransform.rotation,
                scale = objectTransform.localScale
            };

            string messageJson = JsonUtility.ToJson(mqttMessage);
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("testtopic/objectSpawnTest123")
                .WithPayload(messageJson)
                .WithExactlyOnceQoS()
                .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async void DeleteObjectMessage(string objectID)
        {
            MqttMessage mqttMessage = new MqttMessage
            {
                timestamp = GetCurrentTimestamp(),
                clientID = _ownGuid,
                objectID = objectID,
                type = OperationType.Delete
            };

            string messageJson = JsonUtility.ToJson(mqttMessage);
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("testtopic/objectSpawnTest123")
                .WithPayload(messageJson)
                .WithExactlyOnceQoS()
                .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
        }

        private int GetCurrentTimestamp()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        }

        private void CreateObject(ObjectType objectType, string objectID, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            switch (objectType)
            {
                case ObjectType.Square:
                    SpawnCubeAction cubeAction = new SpawnCubeAction(objectID, position, rotation, scale);
                    _messageQueue.Enqueue(cubeAction);
                    break;
                case ObjectType.Sphere:
                    SpawnSphereAction sphereAction = new SpawnSphereAction(objectID, position, rotation, scale);
                    _messageQueue.Enqueue(sphereAction);
                    break;
                case ObjectType.Skeleton:
                    SpawnSkeletonAction skeletonAction = new SpawnSkeletonAction(objectID, position, rotation, scale);
                    _messageQueue.Enqueue(skeletonAction);
                    break;
            }
        }

        private void DeleteObject(string objectID)
        {
            DeleteObjectAction deleteObjectAction = new DeleteObjectAction(objectID);
            _messageQueue.Enqueue(deleteObjectAction);
        }

        private void ManipulateObject(string objectID, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ManipulateObjectAction manipulateObjectAction = new ManipulateObjectAction(objectID, position, rotation, scale);
            _messageQueue.Enqueue(manipulateObjectAction);
        }

        public Queue<HolographicAction> GetCurrentActions()
        {
            return _messageQueue;
        }
    }

    [Serializable]
    public class MqttMessage
    {
        [FormerlySerializedAs("ClientID")] public string clientID;
        [FormerlySerializedAs("Timestamp")] public int timestamp;
        [FormerlySerializedAs("Type")] public OperationType type;
        [FormerlySerializedAs("Position")] public Vector3 position;
        [FormerlySerializedAs("Rotation")] public Quaternion rotation;
        [FormerlySerializedAs("Scale")] public Vector3 scale;
        [FormerlySerializedAs("ObjectType")] public ObjectType objectType;
        [FormerlySerializedAs("ObjectID")] public string objectID;
    }

    public enum OperationType : ushort
    {
        Create =  1,
        Delete = 2,
        Manipulate = 3
    }

    public enum ObjectType : ushort
    {
        Square = 1,
        Sphere = 2,
        Skeleton = 3,
    }
}
