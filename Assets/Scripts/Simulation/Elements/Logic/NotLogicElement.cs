namespace ButterBoard.Simulation.Elements.Logic
{
    public class NotLogicElement : BaseLogicElement
    {
        protected override bool GetLogicValue(bool[] inputValues)
        {
            return !inputValues[0];
        }
    }
}