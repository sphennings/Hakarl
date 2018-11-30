using System.Collections.Generic;
using static Hakarl.TokenType;

namespace Hakarl
{
    internal class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int current;
        private int line = 1;
        private int start;

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(LEFT_PAREN);
                    break;
                case ')':
                    AddToken(RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(LEFT_BRACE);
                    break;
                case '}':
                    AddToken(RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(COMMA);
                    break;
                case '.':
                    AddToken(DOT);
                    break;
                case '-':
                    AddToken(MINUS);
                    break;
                case '+':
                    AddToken(PLUS);
                    break;
                case ';':
                    AddToken(SEMICOLON);
                    break;
                case '*':
                    AddToken(STAR);
                    break;

                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                    break;
                case '/':
                    if (Match('/'))
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    else
                        AddToken(SLASH);
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                case '"':
                    ScanString(); break;

                default:
                    Hakarl.Error(line, "Unexpected character.");
                    break;
            }
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type, object literal = null)
        {
            var text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
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
            return IsAtEnd() ? '\0' : source[current];
        }

        private void ScanString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            // Unterminated string.
            if (IsAtEnd())
            {
                Hakarl.Error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            var value = source.Substring(start + 1, current - start - 2);
            AddToken(STRING, value);
        }
    }
}