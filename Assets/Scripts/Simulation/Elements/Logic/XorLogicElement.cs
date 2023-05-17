namespace ButterBoard.Simulation.Elements.Logic
{
    public class XorLogicElement : BaseLogicElement
    {
        protected override bool GetLogicValue(bool[] inputValues)
        {
            return inputValues[0] ^ inputValues[1];
        }
    }
}