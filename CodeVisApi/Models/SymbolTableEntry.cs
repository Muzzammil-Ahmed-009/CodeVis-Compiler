namespace CodeVisApi.Models
{
    public class SymbolTableEntry
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public object Value { get; set; }
        public int ScopeLevel { get; set; }
        public int LineDeclared { get; set; }
    }
}
