using UnityEngine;
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 fromDirection);
    //bool IsAlive { get; }
}