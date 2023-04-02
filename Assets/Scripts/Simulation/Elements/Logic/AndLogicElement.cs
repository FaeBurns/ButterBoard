namespace ButterBoard.Simulation.Elements.Logic
{
    public class AndLogicElement : BaseLogicElement
    {
        protected override bool GetLogicValue(bool[] inputValues)
        {
            // return result of and applied on all inputValues
            bool result = true;
            foreach (bool value in inputValues)
            {
                result = result && value;
            }
            return result;
        }
    }

}