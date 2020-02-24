﻿using System;
using BlockChain.Core;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BlockChain.Transactions.Scripting;
using BlockChain.Transactions.Scripting.Enums;
using BlockChain.Transactions.Scripting.Scripts;

namespace BlockChain.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var script = new Script();
            script.Push(OPCODE.OP_1);
            script.Push(OPCODE.OP_2);
            script.Push(OPCODE.OP_3);
            script.Push(OPCODE.OP_4);
            script.Push(OPCODE.OP_5);
            Interpreter.Initialize();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++) script.Run();
            Console.WriteLine(sw.ElapsedMilliseconds);
            /*
            var blockChain = new BlockChain<ExampleBlockData>("blockchain.db");
            //for(int i = 0;i<5000000;i++) 
            //    blockChain.Add(new Block<ExampleBlockData>(Convert.FromBase64String("AQAAAAmKCD7tIp5wWCHSYMEE9XHhXHA4AQsH0gMQiiqQkIjwHuOynzH++oBbPhLPtQl1PNwX9KnuriDiafYwR6WMlRz5B3jtcrfXCB4AAP/yBwEAMTIzNDY1NDIzNTE0M2UAAAA=")));

            var sw = Stopwatch.StartNew();
            int x = 0;
            //foreach (var b in blockChain) x++;

            //var y = 
            //    blockChain.ToList();
            //Console.WriteLine(y.Count);
            byte[] file = File.ReadAllBytes("blockchain.db");

            Console.WriteLine($"Read {x} blocks in {sw.ElapsedMilliseconds} milliseconds");*/
        }
    }
}