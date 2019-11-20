using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("enter formula string");
            }
            else
            {
                Calculator calc = new Calculator();
                calc.Calc(args[0]);

                //correct
                //calc.Calc("123");
                //calc.Calc("12+3");
                //calc.Calc("1+3");
                //calc.Calc("+123");
                //calc.Calc("12*3+4");
                //calc.Calc("1/3+4");
                //calc.Calc("1/3-5");
                //calc.Calc("1*3-5+6");
                //calc.Calc("1*3-5+6*2");
                //calc.Calc("0.123+0.123");
                //calc.Calc(".124+.123");
                //calc.Calc("(1+2)");
                //calc.Calc("(1)*2");
                //calc.Calc("(1+2)*4");
                //calc.Calc("(10*3)+2+(15-144*4)-(1)");

                // wrong
                //calc.Calc("+");
                //calc.Calc("");
                //calc.Calc(null);            
                //calc.Calc("0.123.");
                //calc.Calc("0.123+0.123..32");
                //calc.Calc("0.123 + 0. 123..32");
                //calc.Calc("+++");
                //calc.Calc("123++5");
                //calc.Calc("123+5**8");

                //calc.Calc("()");
                //calc.Calc("()+1");
                //calc.Calc("(");
                //calc.Calc(")");
                //calc.Calc("())");
                //calc.Calc("1-))");
                //calc.Calc("1*)+5");

                // check tokens
                //Calculator.GetAllTokens("123");
                //Calculator.GetAllTokens("12+3");
                //Calculator.GetAllTokens("1+3");
                //Calculator.GetAllTokens("+123");
                //Calculator.GetAllTokens("+");
                //Calculator.GetAllTokens("");
                //Calculator.GetAllTokens("1*3-5+6*2");
            }
            Console.ReadKey();
        }
    }

    class Calculator
    {

        static string c_TestStringForNum = "01234567890.";
        static string c_TestStringForOperation = "+-/*()";
        static string c_TestStringForFormula = c_TestStringForNum + c_TestStringForOperation;

        private Stack<double> m_OperandStack;
        private Stack<string> m_OperatorStack;

        public Calculator()
        {
            m_OperandStack = new Stack<double>();
            m_OperatorStack = new Stack<string>();

        }

        void ClearStack()
        {
            m_OperandStack.Clear();
            m_OperatorStack.Clear();
        }


        static bool isFormulaCorrect(string input)
        {
            if (input == null || input.Length == 0)
                return false;

            bool found;

            for (int i = 0; i < input.Length; i++)
            {

                found = false;

                for (int j = 0; j < c_TestStringForFormula.Length; j++)
                {
                    if (input[i] == c_TestStringForFormula[j])
                        found = true;
                }
                if (!found)
                {
                    Console.WriteLine("symbol [" + input[i] + "] is not allowed");
                    return false;
                }

            }
            return true;
        }

        public void Calc(string input)
        {
            Console.WriteLine("---");
            Console.WriteLine();

            Console.WriteLine("Calc " + input + "   len= " + (input != null ? input.Length.ToString() : ""));
            //Console.WriteLine("Calc " + input);

            if (!isFormulaCorrect(input))
            {
                Console.WriteLine("formula is incorrect. Allowed symbols: " + c_TestStringForFormula);
                return;
            }

            ClearStack();

            int index = 0;
            string currToken;
            double result;
            bool convertResult;

            while (index < input.Length)
            {
                currToken = GetToken(ref index, input); // we get from here correct operators and operands

                Console.WriteLine(" > " + currToken + "  i= " + index);
                //Console.WriteLine(" >token " + currToken);

                if (isOperator(currToken))
                {
                    if (isParen(currToken))
                    {
                        if (isLeftParen(currToken))
                        {
                            m_OperatorStack.Push(currToken);
                        }
                        else
                        {   // right
                            if (!TryApplyOperationsInsideParenInStack())
                            {
                                Console.WriteLine("failed to apply operations in stack inside (). Incorrect formula");
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (TryApplyOperationInStack(currToken))
                        {
                            m_OperatorStack.Push(currToken);
                        }
                        else
                        {
                            Console.WriteLine("failed to apply operations in stack. Incorrect formula");
                            return;
                        }
                    }
                }
                else
                {
                    convertResult = double.TryParse(currToken, out result);
                    if (convertResult)
                    {
                        //Console.WriteLine("token parse to double " + currToken + " / " + result);
                        m_OperandStack.Push(result);
                    }
                    else
                    {
                        Console.WriteLine("failed to convert " + currToken + " to double");
                        return;
                    }
                }
            }
            CalcFinalResult();
        }

        // calc part of formula inside current ()
        bool TryApplyOperationsInsideParenInStack()
        {

            string currOperator;
            double result, operand1, operand2;

            Console.WriteLine("TryApplyOperationsInsideParenInStack");

            while (m_OperatorStack.Count > 0)
            {
                currOperator = m_OperatorStack.Pop();

                if (isLeftParen(currOperator))
                {
                    return true;
                }
                else
                {
                    if (m_OperandStack.Count > 1)
                    {
                        operand2 = m_OperandStack.Pop();
                        operand1 = m_OperandStack.Pop();

                        result = GetOperationResult(currOperator, operand1, operand2);

                        Console.WriteLine("tmp res " + operand1 + " " + currOperator + " " + operand2 + " = " + result);

                        m_OperandStack.Push(result);
                    }
                    else
                    {
                        Console.WriteLine("~not enough operands for " + currOperator + " operation. Formula is incorrect");
                        return false;
                    }
                }
            }
            Console.WriteLine("we didnt encounter corresponding left paren. Formula is incorrect");
            return false;

        }

        void CalcFinalResult()
        {

            Console.WriteLine("CalcFinalResult ");

            string currOperator;
            double result, operand1, operand2;

            // apply all remaining 
            while (m_OperatorStack.Count > 0)
            {
                currOperator = m_OperatorStack.Pop();

                if (m_OperandStack.Count > 1)
                {
                    operand2 = m_OperandStack.Pop();
                    operand1 = m_OperandStack.Pop();

                    result = GetOperationResult(currOperator, operand1, operand2);

                    Console.WriteLine("tmp res " + operand1 + " " + currOperator + " " + operand2 + " = " + result);

                    m_OperandStack.Push(result);
                }
                else
                {
                    Console.WriteLine("_not enough operands for " + currOperator + " operation. Formula is incorrect");
                    return;
                }
            }

            if (m_OperatorStack.Count == 0)
            {
                switch (m_OperandStack.Count)
                {
                    case 0:
                        Console.WriteLine("zero operand stack");
                        break;

                    case 1:
                        Console.WriteLine("result " + String.Format("{0:F3}", m_OperandStack.Pop()));
                        break;

                    default:
                        Console.WriteLine("more than 1 operands in stack. The formula is incorrect");
                        break;
                }
            }
            else
            {
                Console.WriteLine("operator stack isn't empty. The formula is incorrect");
            }
        }

        bool TryApplyOperationInStack(string currToken)
        {

            Console.WriteLine("TryApplyOperationInStack " + currToken);

            string currOperator;
            int priorityCurrToken;
            int priorityCurrOper;
            double result, operand1, operand2;

            while (m_OperatorStack.Count > 0)
            {
                currOperator = m_OperatorStack.Peek();
                priorityCurrToken = GetPriority(currToken);
                priorityCurrOper = GetPriority(currOperator);

                //Console.WriteLine("pri " + currToken + " " + priorityCurrToken);
                //Console.WriteLine("pri " + currOperator + " " + currOperator);

                if (priorityCurrOper >= priorityCurrToken)
                {
                    currOperator = m_OperatorStack.Pop();

                    if (m_OperandStack.Count > 1)
                    {
                        operand2 = m_OperandStack.Pop();
                        operand1 = m_OperandStack.Pop();

                        result = GetOperationResult(currOperator, operand1, operand2);

                        Console.WriteLine("tmp res " + operand1 + " " + currOperator + " " + operand2 + " = " + result);

                        m_OperandStack.Push(result);
                    }
                    else
                    {
                        Console.WriteLine("not enough operands for " + currOperator + " operation. Formula is incorrect");
                        return false;
                    }
                }
                else
                    break;
            }
            return true;
        }

        static double GetOperationResult(string operation, double operand1, double operand2)
        {

            switch (operation)
            {

                case "-":
                    return (operand1 - operand2);

                case "+":
                    return (operand1 + operand2);

                case "*":
                    return (operand1 * operand2);

                case "/":
                    return (operand1 / operand2);

                default:
                    return 0;
            }
        }

        static bool isParen(string oper)
        {
            return (oper.Length == 1 && (oper[0] == '(' || oper[0] == ')'));
        }

        static bool isLeftParen(string oper)
        {
            return (oper.Length == 1 && oper[0] == '(');
        }

        static int GetPriority(string oper)
        {

            switch (oper)
            {

                case "(":
                case ")":
                    return 0;

                case "-":
                case "+":
                    return 2;

                case "*":
                case "/":
                    return 3;

                default: return 0;
            }
        }

        static public void GetAllTokens(string input)
        {
            Console.WriteLine(" GetAllTokens input= " + input + " / len= " + input.Length);

            string token;
            int index = 0;

            while (index < input.Length)
            {
                token = GetToken(ref index, input);

                Console.WriteLine("token " + token + " / index " + index);
            }
        }

        // gets token which starts at index position
        // until we get to next token or eof input
        // get current token
        // sets index to the next position after token
        static string GetToken(ref int index, string input)
        {

            // TODO check params for correctness

            // we try to get a num until we encounter next operator or eof string 
            // if we encountered operator first, then return it

            int startIndex = index;

            for (int i = startIndex; i < input.Length; i++)
            {

                if (isOperator(input[i]))
                {

                    if (i == startIndex)
                    {
                        index = startIndex + 1;
                        return input.Substring(startIndex, 1); // cut operator
                    }
                    else
                    {
                        index = i;
                        return input.Substring(startIndex, i - startIndex); // cut substring before operator
                    }
                }
            }
            index = input.Length;
            return input.Substring(startIndex, input.Length - startIndex);
        }

        static bool isOperator(char token)
        {
            for (int i = 0; i < c_TestStringForOperation.Length; i++)
            {
                if (token == c_TestStringForOperation[i])
                    return true;
            }
            return false;
        }

        static bool isOperator(string token)
        {
            return (token.Length == 1 && isOperator(token[0]));
        }
    }
}
