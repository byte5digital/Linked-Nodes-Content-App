namespace byte5.LinkedNodesContentApp.Models
{
    public class LinkedNodesConfigModel
    {
        public bool OverviewShowId { get; set; }
        public bool OverviewShowPath { get; set; }
        public bool OverviewShowPropertyAlias { get; set; }
        public bool EventsPreventDeletionOfLinkedContentNodes { get; set; }
        public bool EventsPreventDeletionOfLinkedMediaNodes { get; set; }
    }
}