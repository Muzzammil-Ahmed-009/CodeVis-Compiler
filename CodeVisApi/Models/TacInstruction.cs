namespace CodeVisApi.Models
{
    public class TacInstruction
    {
        public string Operator { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string Result { get; set; }

        public TacInstruction(string op, string arg1, string arg2, string result)
        {
            Operator = op;
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }

        public override string ToString()
        {
            if (Operator == "=")
            {
                return $"{Result} := {Arg1}";
            }
            if (string.IsNullOrEmpty(Arg2))
            {
                // Unary or special (e.g. print)
                return $"{Operator} {Arg1} {(string.IsNullOrEmpty(Result) ? "" : $"-> {Result}")}".Trim();
            }
            return $"{Result} := {Arg1} {Operator} {Arg2}";
        }
    }
}
