namespace Team1_GraduationGame.Enemies
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/New Enemy")]
    public class BaseEnemy : ScriptableObject
    {
        public float speed;
        public float turnSpeed;
        public float accelerationTime;
        public float deAccelerationTime;

        public float viewAngleRange;
        public float viewDistance;
        [Tooltip("In seconds")] public float aggroTime;

        [Tooltip("0 means they cannot be pushed down")]
        public float pushedDownDuration; // If 0, they cannot be pushed down

    }

}
