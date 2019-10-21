using UnityEngine;

namespace Team1_GraduationGame.Events
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T item);
    }
}