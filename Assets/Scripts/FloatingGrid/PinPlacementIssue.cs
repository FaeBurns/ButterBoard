namespace ButterBoard.FloatingGrid
{
    public class PinPlacementIssue
    {
        public GridPin PinWithIssue { get; }
        public GridPoint? PointProvidingIssue { get; }
        public PinPlacementIssueType IssueType { get; }

        public PinPlacementIssue(GridPin pinWithIssue, GridPoint? pointProvidingIssue, PinPlacementIssueType issueType)
        {
            PinWithIssue = pinWithIssue;
            PointProvidingIssue = pointProvidingIssue;
            IssueType = issueType;
        }

        public override string ToString()
        {
            return IssueType.ToString();
        }
    }

    public enum PinPlacementIssueType
    {
        EXISTING_INVALID_CONNECTION,
        PORT_NOT_FOUND,
        PORT_OCCUPIED,
        PORT_BLOCKED,
    }
}