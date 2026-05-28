using System.Collections.Generic;
using CodeVisApi.Models;

namespace CodeVisApi.Services
{
    public class Optimizer
    {
        public (List<string> Formatted, List<TacInstruction> Instructions) Optimize(List<TacInstruction> instructions)
        {
            var optimizedList = new List<TacInstruction>();
            
            for (int i = 0; i < instructions.Count; i++)
            {
                var current = instructions[i];
                
                // Copy Propagation / Peephole Optimization
                if (current.Operator == "=" && current.Arg1.StartsWith("t") && optimizedList.Count > 0)
                {
                    var prev = optimizedList[optimizedList.Count - 1];
                    if (prev.Result == current.Arg1)
                    {
                        prev.Result = current.Result;
                        continue; 
                    }
                }
                
                optimizedList.Add(new TacInstruction(current.Operator, current.Arg1, current.Arg2, current.Result));
            }

            var resultStrings = new List<string>();
            foreach (var inst in optimizedList)
            {
                if (inst.Operator == "=")
                {
                    resultStrings.Add($"{inst.Result} = {inst.Arg1}");
                }
                else if (string.IsNullOrEmpty(inst.Arg2))
                {
                    resultStrings.Add($"{inst.Operator} {inst.Arg1} {(string.IsNullOrEmpty(inst.Result) ? "" : $"-> {inst.Result}")}".Trim());
                }
                else
                {
                    resultStrings.Add($"{inst.Result} = {inst.Arg1} {inst.Operator} {inst.Arg2}");
                }
            }

            return (resultStrings, optimizedList);
        }
    }
}
