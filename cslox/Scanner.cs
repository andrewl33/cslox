using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            {"and",    TokenType.AND},
            {"class",  TokenType.CLASS},
            {"else",   TokenType.ELSE},
            {"false",  TokenType.FALSE},
            {"for",    TokenType.FOR},
            {"fun",    TokenType.FUN},
            {"if",     TokenType.IF},
            {"nil",    TokenType.NIL},
            {"or",     TokenType.OR},
            {"print",  TokenType.PRINT},
            {"return", TokenType.RETURN},
            {"super",  TokenType.SUPER},
            {"this",   TokenType.THIS},
            {"true",   TokenType.TRUE},
            {"var",    TokenType.VAR},
            {"while",  TokenType.WHILE}
        };
                 
          

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while(!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        public void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.                      
                    break;

                case '\n':
                    line++;
                    break;
                case '"': HandleString(); break;
                default:

                    if (IsDigit(c))
                    {
                        HandleNumber();
                    } 
                    else if(IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected Character.");
                    }


                    break;
            }
        }

        private void HandleString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            // unterminated string.
            if (IsAtEnd())
            {
                Lox.Error(line, "Unterminated string.");
                return;
            }

            // closing ".
            Advance();

            // Trim surrounding quotes.
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }

        private void HandleNumber()
        {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, Double.Parse(source.Substring(start, current - start)));
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = source.Substring(start, current - start);
            TokenType type;
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }
            else
            {
                type = TokenType.IDENTIFIER;
            }
            AddToken(type);
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsAlpha(char c)
        {
            return  (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                     c == '_';
        }

        private bool IsDigit(char c)
        {
            // Char.IsDigit accepts too many characters
            return c >= '0' && c <= '9';
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
