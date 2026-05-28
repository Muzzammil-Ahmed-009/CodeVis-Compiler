using Microsoft.AspNetCore.Mvc;
using CodeVisApi.Models;
using CodeVisApi.Services;
using System.Collections.Generic;

namespace CodeVisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompilerController : ControllerBase
    {
        [HttpPost]
        public ActionResult<CompilerResult> Compile([FromBody] CompileRequest request)
        {
            var result = new CompilerResult();
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                result.Errors.Add("Code cannot be empty.");
                return Ok(result);
            }

            // Phase 1: Lexical Analysis
            var lexer = new Lexer(request.Code, result.Errors);
            result.Tokens = lexer.Tokenize();

            if (result.Errors.Count > 0)
            {
                return Ok(result); // Stop on lexical errors
            }

            // Phase 2: Syntax Analysis
            var parser = new Parser(result.Tokens, result.Errors);
            result.Ast = parser.ParseProgram();

            if (result.Errors.Count > 0)
            {
                return Ok(result); // Stop on syntax errors
            }

            // Phase 3: Semantic Analysis
            var semanticAnalyzer = new SemanticAnalyzer(result.Ast, result.Errors);
            result.SymbolTable = semanticAnalyzer.Analyze();

            // Proceed to TAC generation even if there are semantic errors (like undeclared variables)
            // to allow users to see the intermediate code for their assignments.

            // Phase 4: Intermediate Code Generation
            var codeGenerator = new CodeGenerator(result.Ast);
            result.Tac = codeGenerator.Generate();
            var rawInstructions = codeGenerator.GetInstructions();

            // Phase 5: Optimization
            var optimizer = new Optimizer();
            var (optimizedStrings, optimizedInstructions) = optimizer.Optimize(rawInstructions);
            result.OptimizedTac = optimizedStrings;

            // Phase 6: Target Code Generation
            var targetGenerator = new TargetCodeGenerator();
            result.AssemblyCode = targetGenerator.Generate(optimizedInstructions);

            return Ok(result);
        }
    }
}
