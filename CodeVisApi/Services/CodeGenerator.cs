using System.Collections.Generic;
using CodeVisApi.Models;

namespace CodeVisApi.Services
{
    public class CodeGenerator
    {
        private readonly AstNode _ast;
        private readonly List<TacInstruction> _instructions;
        private int _tempCounter;

        public CodeGenerator(AstNode ast)
        {
            _ast = ast;
            _instructions = new List<TacInstruction>();
            _tempCounter = 1;
        }

        public List<string> Generate()
        {
            if (_ast == null) return new List<string>();
            Traverse(_ast);
            var result = new List<string>();
            foreach (var inst in _instructions)
            {
                result.Add(inst.ToString());
            }
            return result;
        }

        public List<TacInstruction> GetInstructions()
        {
            if (_instructions.Count == 0 && _ast != null)
            {
                Traverse(_ast);
            }
            return _instructions;
        }

        private string Traverse(AstNode node)
        {
            if (node == null) return "";

            if (node.Name == "Program")
            {
                foreach (var child in node.Children)
                {
                    Traverse(child);
                }
                return "";
            }

            if (node.Name == "VarDeclaration")
            {
                return Traverse(node.Children[0]); // Traverse the Assignment node
            }

            if (node.Name == "Assignment")
            {
                string targetVar = node.Children[0].Attributes["value"];
                string exprResult = Traverse(node.Children[1]);
                _instructions.Add(new TacInstruction("=", exprResult, "", targetVar));
                return "";
            }

            if (node.Name == "PrintStatement")
            {
                string exprResult = Traverse(node.Children[0]);
                _instructions.Add(new TacInstruction("printf", exprResult, "", ""));
                return "";
            }

            if (node.Name == "BinaryOp")
            {
                string left = Traverse(node.Children[0]);
                string right = Traverse(node.Children[1]);
                string op = node.Attributes["value"];
                string temp = NewTemp();
                _instructions.Add(new TacInstruction(op, left, right, temp));
                return temp;
            }

            if (node.Name == "Literal" || node.Name == "Identifier")
            {
                return node.Attributes["value"];
            }

            return "";
        }

        private string NewTemp()
        {
            return $"t{_tempCounter++}";
        }
    }
}
