using System.Collections.Generic;
using CodeVisApi.Models;

namespace CodeVisApi.Services
{
    public class SemanticAnalyzer
    {
        private readonly AstNode _ast;
        private readonly List<string> _errors;
        private readonly List<SymbolTableEntry> _symbolTable;

        public SemanticAnalyzer(AstNode ast, List<string> errors)
        {
            _ast = ast;
            _errors = errors;
            _symbolTable = new List<SymbolTableEntry>();
        }

        public List<SymbolTableEntry> Analyze()
        {
            if (_ast == null) return _symbolTable;
            Traverse(_ast);
            return _symbolTable;
        }

        private void Traverse(AstNode node)
        {
            if (node.Name == "Program")
            {
                foreach (var child in node.Children) Traverse(child);
            }
            else if (node.Name == "VarDeclaration")
            {
                var type = node.Attributes.ContainsKey("type") ? node.Attributes["type"] : "int";
                
                var assignNode = node.Children[0]; // The Assignment node
                var idNode = assignNode.Children[0];
                var exprNode = assignNode.Children[1];

                string varName = idNode.Attributes.ContainsKey("value") ? idNode.Attributes["value"] : "unknown";

                if (IsDeclared(varName))
                {
                    _errors.Add($"Semantic Error: Variable '{varName}' is already declared.");
                }
                else
                {
                    string exprType = GetExpressionType(exprNode);
                    if (type == "int" && exprType == "string")
                    {
                        _errors.Add($"Semantic Error: Type mismatch. Cannot assign string to int variable '{varName}'.");
                    }
                    else if ((type == "string" || type == "String") && exprType == "int")
                    {
                        _errors.Add($"Semantic Error: Type mismatch. Cannot assign int to {type} variable '{varName}'.");
                    }
                    else
                    {
                        _symbolTable.Add(new SymbolTableEntry
                        {
                            Name = varName,
                            DataType = type,
                            ScopeLevel = 0,
                            LineDeclared = 0 // Line number info lost in basic AST, can be added later
                        });
                    }
                }
                Traverse(exprNode); // Traverse the expression only
            }
            else if (node.Name == "Assignment")
            {
                var idNode = node.Children[0];
                var exprNode = node.Children[1];

                string varName = idNode.Attributes.ContainsKey("value") ? idNode.Attributes["value"] : "unknown";

                if (!IsDeclared(varName))
                {
                    _errors.Add($"Semantic Error: Variable '{varName}' is not declared.");
                }
                else
                {
                    string exprType = GetExpressionType(exprNode);
                    var symbol = GetSymbol(varName);
                    if (symbol != null)
                    {
                        if (symbol.DataType == "int" && exprType == "string")
                        {
                            _errors.Add($"Semantic Error: Type mismatch. Cannot assign string to int variable '{varName}'.");
                        }
                        else if ((symbol.DataType == "string" || symbol.DataType == "String") && exprType == "int")
                        {
                            _errors.Add($"Semantic Error: Type mismatch. Cannot assign int to {symbol.DataType} variable '{varName}'.");
                        }
                    }
                }
                Traverse(exprNode); // Traverse the expression only
            }
            else if (node.Name == "PrintStatement")
            {
                if (node.Children.Count > 0)
                {
                    Traverse(node.Children[0]); // Traverse the expression being printed
                }
            }
            else if (node.Name == "Identifier")
            {
                string varName = node.Attributes.ContainsKey("value") ? node.Attributes["value"] : "unknown";
                if (!IsDeclared(varName))
                {
                    _errors.Add($"Semantic Error: Variable '{varName}' is not declared.");
                }
            }
            else if (node.Name == "BinaryOp")
            {
                string leftType = GetExpressionType(node.Children[0]);
                string rightType = GetExpressionType(node.Children[1]);
                
                if (leftType != rightType || leftType == "string")
                {
                    _errors.Add($"Semantic Error: Invalid operands for operator '{node.Attributes["value"]}'. Both must be of type int.");
                }
                Traverse(node.Children[0]);
                Traverse(node.Children[1]);
            }
        }

        private bool IsDeclared(string name)
        {
            return _symbolTable.Exists(s => s.Name == name);
        }

        private SymbolTableEntry GetSymbol(string name)
        {
            return _symbolTable.Find(s => s.Name == name);
        }

        private string GetExpressionType(AstNode node)
        {
            if (node.Name == "Literal")
            {
                string val = node.Attributes.ContainsKey("value") ? node.Attributes["value"] : "";
                if (val.StartsWith("\"")) return "string";
                return "int";
            }
            if (node.Name == "Identifier")
            {
                var symbol = GetSymbol(node.Attributes.ContainsKey("value") ? node.Attributes["value"] : "");
                return symbol != null ? symbol.DataType : "unknown";
            }
            if (node.Name == "BinaryOp")
            {
                string left = GetExpressionType(node.Children[0]);
                string right = GetExpressionType(node.Children[1]);
                if (left == "int" && right == "int") return "int";
                return "unknown"; // indicates error handled elsewhere
            }
            return "unknown";
        }
    }
}
