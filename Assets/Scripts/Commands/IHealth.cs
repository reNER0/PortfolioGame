using System;

namespace Assets.Scripts.Commands
{
    internal interface IHealth
    {
        public int GetHealth();
        public int GetMaxHealth();
        public event Action<int> HealthChanged;
    }
}
