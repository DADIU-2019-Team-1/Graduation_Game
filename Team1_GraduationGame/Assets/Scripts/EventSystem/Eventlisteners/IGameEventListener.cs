using UnityEngine;

public interface IGameEventListener<T>
{
    void OnEventRaised(T item);
}
