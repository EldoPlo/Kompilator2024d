using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Antlr4.Runtime;

namespace Kompilator2024
{
    public class MyErrorListener : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Syntax error at line {line}:{charPositionInLine} - {msg}");
            Console.ResetColor();        
        }
    }
    public class Compilation
    {
        public bool isValid = false;
        public void Calculate(string input, string output)
        {
            var lexer = new l4Lexer(new AntlrInputStream(input));
            var tokens = new CommonTokenStream(lexer);
            var memory = new MemoryHandler();
            var codegen = new CodeGenerator();
            var parser = new l4Parser(tokens);
            var visitor = new LanguageVisitor(memory, codegen);
            
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new MyErrorListener());

           
            var tree = parser.program_all();
           
            if (parser.NumberOfSyntaxErrors != 0)
            {
                throw new Exception("PROBLEM W KOMPILACJI");
            }
            try
            {
                var result = visitor.Visit(tree);
                if (result == null)
                {
                    throw new Exception("Błąd: Wynik odwiedzenia drzewa jest pusty (null).");
                    return;
                }
                if (visitor.GetErrors().Count != 0 || memory.GetErrors().Count != 0)
                {
                    isValid = true;
                    List<string> allerrors = new List<string>();
                   
                    allerrors.AddRange(visitor.GetErrors());
                    allerrors.AddRange(memory.GetErrors());
                    allerrors.Add("\n");
                    allerrors.Add("\u001B[31mCOMPILATION ERROR\u001B[0m");
                    throw new ApplicationException(string.Join("\n", allerrors));
                }

                WriteCode(output, result);
            }
            catch (ApplicationException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                
            }
        }

        private void WriteCode(string path, VisitorDataTransmiter result)
        {
            if (result.CodeBuilder.Length == 0)
            {
                Console.WriteLine("Błąd: CodeBuilder jest pusty.");
            }
            else
            {
                File.WriteAllText(path, result.CodeBuilder.ToString());
            }
        }
    }
}
