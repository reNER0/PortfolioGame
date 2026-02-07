using System;

public static class GameBus
{
    public static Action<Player> OnPlayerDead;
    public static Action<Predictable, int> OnPredictableHit;
    public static Action OnBadEffect;
}
