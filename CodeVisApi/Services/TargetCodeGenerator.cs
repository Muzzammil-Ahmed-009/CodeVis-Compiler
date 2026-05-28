using System.Collections.Generic;
using CodeVisApi.Models;

namespace CodeVisApi.Services
{
    public class TargetCodeGenerator
    {
        private int _regCounter = 1;

        public List<string> Generate(List<TacInstruction> instructions)
        {
            var assembly = new List<string>();

            foreach (var inst in instructions)
            {
                if (inst.Operator == "=" && string.IsNullOrEmpty(inst.Arg2))
                {
                    // Simple assignment: X = Y
                    string reg = $"R{_regCounter++}";
                    assembly.Add($"MOV {reg}, {inst.Arg1}");
                    assembly.Add($"MOV {inst.Result}, {reg}");
                    assembly.Add(""); // Blank line for readability
                }
                else if (inst.Operator == "print" || inst.Operator == "printf")
                {
                    assembly.Add($"PRINT {inst.Arg1}");
                    assembly.Add("");
                }
                else if (!string.IsNullOrEmpty(inst.Arg2))
                {
                    // Binary operation: Target = Arg1 Op Arg2
                    string reg = $"R{_regCounter++}";
                    string opCode = GetAssemblyOp(inst.Operator);

                    assembly.Add($"MOV {reg}, {inst.Arg1}");
                    assembly.Add($"{opCode} {reg}, {inst.Arg2}");
                    assembly.Add($"MOV {inst.Result}, {reg}");
                    assembly.Add(""); // Blank line for readability
                }
            }

            return assembly;
        }

        private string GetAssemblyOp(string op)
        {
            switch (op)
            {
                case "+": return "ADD";
                case "-": return "SUB";
                case "*": return "MUL";
                case "/": return "DIV";
                default: return "OP";
            }
        }
    }
}
