namespace UpgradeSystem.Interfaces
{
    public abstract class AreaEffect : AbstractEffect
    {
        public override object Activate(AbstractStatsClass target)
        {
            throw new System.NotImplementedException();
        }

        public abstract SingleActivationEffect GetEffectToDeploy(PlayerEffectDispatcher pl);

    }
}