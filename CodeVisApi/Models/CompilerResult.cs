using System.Collections.Generic;

namespace CodeVisApi.Models
{
    public class CompilerResult
    {
        public List<Token> Tokens { get; set; } = new List<Token>();
        public List<SymbolTableEntry> SymbolTable { get; set; } = new List<SymbolTableEntry>();
        public AstNode Ast { get; set; }
        public List<string> Tac { get; set; } = new List<string>();
        public List<string> OptimizedTac { get; set; } = new List<string>();
        public List<string> AssemblyCode { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
