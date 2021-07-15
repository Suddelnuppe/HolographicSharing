using UnityEngine;

namespace HolographicSharing
{
    public class SpawnSkeletonAction : HolographicAction
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Scale { get; }

        public SpawnSkeletonAction(string objectID, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.ObjectID = objectID;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }
    }
}