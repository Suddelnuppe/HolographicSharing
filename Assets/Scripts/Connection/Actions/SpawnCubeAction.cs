using UnityEngine;

namespace HolographicSharing
{
    public class SpawnCubeAction : HolographicAction
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Scale { get; }

        public SpawnCubeAction(string objectID, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ObjectID = objectID;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}