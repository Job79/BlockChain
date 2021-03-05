using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CryptoChain.Core.Abstractions;
using CryptoChain.Core.Helpers;

namespace CryptoChain.Core.Transactions.Scripting.Interpreter
{
    public class ScriptInterpreter : IScriptInterpreter
    {
        /// <summary>
        /// Dictionary with all the operations
        /// </summary>
        private readonly Dictionary<Opcode, Operation> _operations;
        
        /// <summary>
        /// Current block height. Passed to the executionStack to be used in the script engine.
        /// Needs to be manually passed to the interpreter
        /// </summary>
        public uint CurrentBlockHeight { get; set; }

        /// <summary>
        /// Load all Operations into the Operations dictionary.
        /// </summary>
        public ScriptInterpreter()
        {
            _operations = Assembly.GetExecutingAssembly()
                .GetTypes().Where(x=>(x.Namespace??"").Contains("Interpreter.Operations"))
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes().OfType<OpCode>().Any() && m.IsStatic)
                .ToDictionary(k => (
                        (OpCode) k.GetCustomAttributes().First()).Opcode,
                    v => new Operation(v, (OpCode)v.GetCustomAttributes().First()));
        }

        /// <summary>
        /// Execute one or more scripts
        /// </summary>
        /// <param name="t">The transaction containing/referenced by the executed script</param>
        /// <param name="scripts">The script(s) you want to execute. Always order unlocking before locking script!</param>
        /// <returns>ExecutionResult</returns>
        public ExecutionResult Execute(ref Transaction t, params IScript[] scripts) =>
            Execute(ref t, out _, scripts);

        /// <summary>
        /// Execute scripts outside transaction context
        /// </summary>
        /// <param name="scripts">The scripts to be executed</param>
        /// <returns>ExecutionResult</returns>
        public ExecutionResult Execute(params IScript[] scripts)
        {
            var tx = new Transaction();
            return Execute(ref tx, scripts);
        }

        /// <summary>
        /// Execute scripts from an transaction upon a transaction
        /// </summary>
        /// <param name="t">The transaction containing/referenced by the scripts</param>
        /// <param name="output">The execution stack first item after executing. Useful when using OP_RETURN</param>
        /// <param name="scripts">The scripts to be evaluated</param>
        /// <returns>ExecutionResult</returns>
        public ExecutionResult Execute(ref Transaction t, out byte[]? output, params IScript[] scripts)
        {
            var stack = new ExecutionStack(t, CurrentBlockHeight);

            for (int i = 0; i < scripts.Length; i++)
            {
                var s = scripts[i].Clone(); //Clone the script, because the interpreter empties the script
                stack.SetScript(ref s); //reference is needed to grab data from script
                
                while (s.Length > 0)
                {
                    try
                    {
                        Opcode op = s.Next();
                        DebugUtils.Info("Executing: " + op);
                        var res = _operations[op].Execute(ref stack);
                        DebugUtils.Write(stack.ToString());

                        if (res != null)
                        {
                            output = stack.FirstOrDefault();
                            return res.Value;
                        }
                    }
                    catch(Exception e)
                    {
                        output = stack.FirstOrDefault();
                        if (e is KeyNotFoundException) return ExecutionResult.UNKNOWN_CODE;
                        DebugUtils.WriteLine(e.ToString(), DebugUtils.MessageState.ERROR);
                        return ExecutionResult.UNKNOWN_ERROR;
                    }
                }
            }
            
            output = stack.FirstOrDefault();

            int count = stack.Count;
            if (count == 0) return ExecutionResult.INVALID_STACK;
            if (count > 1) return ExecutionResult.MORE_ITEMS_ON_BOTTOM;
            return stack.PeekBool() ? ExecutionResult.SUCCESS : ExecutionResult.FAILURE;
        }
    }
}