using System.Collections.Generic;
using static Hakarl.TokenType;

namespace Hakarl
{
    internal class Scanner
    {
        private readonly string source;
        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            {"and", AND},
            {"class", CLASS},
            {"else", ELSE},
            {"false", FALSE},
            {"for", FOR},
            {"fun", FUN},
            {"if", IF},
            {"nil", NIL},
            {"or", OR},
            {"print", PRINT},
            {"return", RETURN},
            {"super", SUPER},
            {"this", THIS},
            {"true", TRUE},
            {"var", VAR},
            {"while", WHILE}
        };
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
                    if (Peek() == '/')
                    {
                        // Consume the second "/".
                        Advance();
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    }
                    else if (Peek() == '*')
                        ScanComment();
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
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    } else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    } else
                    {
                        Hakarl.Error(line, "Unexpected character.");
                    }
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

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
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

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consumes the "."
                Advance();

                while (IsDigit(Peek())) Advance();
            }
            AddToken(NUMBER, double.Parse(source.Substring(start, current - start)));
        }

        private void ScanIdentifier()
        {
            while (IsAlphanumeric(Peek())) Advance();
            // See if the identifier is a reserved word.
            var text = source.Substring(start, current - start);
            TokenType type;
            if (!keywords.TryGetValue(text, out type))
            {
                type = IDENTIFIER;
            }

            AddToken(type);
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool IsAlphanumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void ScanComment()
        {
            // Consume the initial "*".
            Advance();
            while ((!Match('*') && PeekNext() != '/') && !IsAtEnd())
                Advance();

            if (IsAtEnd())
            {
                Hakarl.Error(line, "Unterminated multiline comment.");
                return;
            }
            // Consume the terminal "/".
            Advance();
        }
    }
}