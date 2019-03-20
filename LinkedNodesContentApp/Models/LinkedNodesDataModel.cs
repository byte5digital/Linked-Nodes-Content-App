namespace byte5.LinkedNodesContentApp.Models
{
    public class LinkedNodesDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAlias { get; set; }
        public string Path { get; set; }
        public int RelLinkCountPerProp { get; set; }
    }
}

