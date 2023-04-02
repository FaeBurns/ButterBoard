namespace ButterBoard.Simulation.Elements.Logic
{
    public class OrLogicElement : BaseLogicElement
    {
        protected override bool GetLogicValue(bool[] inputValues)
        {
            // return result of or applied on all inputValues
            bool result = false;
            foreach (bool value in inputValues)
            {
                result = result || value;
            }
            return result;
        }
    }
}