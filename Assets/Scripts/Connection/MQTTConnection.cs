using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frontline.AR;
using Frontline.Services.Audio;
using Frontline.Services.Notification;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace HolographicSharing
{
    public class MQTTConnection : MonoBehaviour
    {
        private IMqttClient mqttClient;
        
        private INotificationService notificationService;

        private SceneManager _sceneManager;

        private Guid ownGUID;

        private void Update()
        {
        }

        private void OnDestroy()
        {
            Destroy(_sceneManager);
        }

        private void Start()
        {
            ownGUID = new Guid();
            
            gameObject.AddComponent<SceneManager>();
            _sceneManager = gameObject.GetComponent<SceneManager>();
            
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            
            var options = new MqttClientOptionsBuilder()
                .WithClientId("clientId-wVpslsS8U1")
                .WithWebSocketServer("broker.mqttdashboard.com:8000/mqtt")
                .WithCleanSession()
                .WithKeepAlivePeriod(new TimeSpan(60))
                .Build();
            
            
                
            mqttClient.UseConnectedHandler(async e =>
            {
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("testtopic/holgertest123").Build());
            });
            
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                MQTTMessage message = JsonConvert.DeserializeObject<MQTTMessage>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

                if (message.ClientID != ownGUID)
                {
                    switch (message.Type)
                    {
                        case OperationType.CREATE:
                            CreateObject(message.ObjectType, message.ObjectID.ToString(), message.Transform);
                            break;
                        case OperationType.DELETE:
                            DeleteObject((message.ObjectID.ToString()));
                            break;
                        case OperationType.MANIPULATE:
                            break;
                    }
                }
            });

            mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        private void OnApplicationQuit()
        {
            mqttClient.DisconnectAsync();
        }

        public async void SpawnObjectMessage(ObjectType objectType)
        {
            MQTTMessage mqttMessage = new MQTTMessage();
            mqttMessage.Timestamp = GetCurrentTimestamp();
            mqttMessage.ClientID = ownGUID;
            mqttMessage.ObjectType = objectType;
            mqttMessage.ObjectID = new Guid();
            mqttMessage.Type = OperationType.CREATE;
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("MyTopic")
                .WithPayload(mqttMessage)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async void DeleteObjectMessage(string objectID)
        {
            MQTTMessage mqttMessage = new MQTTMessage();
            mqttMessage.Timestamp = GetCurrentTimestamp();
            mqttMessage.ClientID = ownGUID;
            mqttMessage.ObjectID = new Guid(objectID);
            mqttMessage.Type = OperationType.DELETE;
            
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("MyTopic")
                .WithPayload(mqttMessage)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        private int GetCurrentTimestamp()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        }

        private void CreateObject(ObjectType objectType, string objectID, Transform objectTransform)
        {
            switch (objectType)
            {
                case ObjectType.SQUARE:
                    _sceneManager.SpawnCube(objectID, objectTransform);
                    break;
                case ObjectType.SPHERE:
                    _sceneManager.SpawnSphere(objectID, objectTransform);
                    break;
            }
        }

        private void DeleteObject(string objectID)
        {
            _sceneManager.DeleteObject(objectID);
        }

        private void ManipulateObject(string objectID, Transform newTransform)
        {
            _sceneManager.ManipulateObject(objectID, newTransform);
        }
    }

    public class MQTTMessage
    {
        [JsonProperty("clientID")]
        public Guid ClientID { get; set; }
    
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("type")]
        public OperationType Type { get; set; }
    
        [JsonProperty("transform")]
        public Transform Transform { get; set; }
    
        [JsonProperty("objectType")]
        public ObjectType ObjectType { get; set; }
    
        [JsonProperty("objectID")]
        public Guid ObjectID { get; set; }
    }

    public enum OperationType : ushort
    {
        CREATE =  1,
        DELETE = 2,
        MANIPULATE = 3
    }

    public enum ObjectType : ushort
    {
        SQUARE = 1,
        SPHERE = 2,
    }
}
