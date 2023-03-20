namespace ButterBoard.Simulation
{
    public interface ITickableObject
    {
        public void DoTick();
        public void PushValues();

        public void Cleanse();
    }
}