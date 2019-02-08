﻿using System;
using System.IO;
using System.Collections.Generic;

namespace cslox
{
    public class Lox
    {
        static Boolean hadError = false;
        static void Main(string[] args)
        {
            
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            if (hadError) Environment.Exit(65);

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found");
                Environment.Exit(72);
            }
            else
            {
                Run(File.ReadAllText(path));
            }
        }

        private static void RunPrompt()
        {
            while(true)
            {
                Console.Write(">");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach(Token token in tokens)
            {
                Console.WriteLine(token);
            }

        }

        public static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, String where, String message)
        {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}