using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assembler
{
    public class AssemblerParser
    {
        private string _program { get; init; }
        private string[] ProgramArray { get; }

        private uint ProgramPtr;
        private Processor Processor { get; init; }
        private Dictionary<uint, string> Labels { get; init; }

        private Tuple<Register, object> LastCompareValue;

        private Dictionary<int, (DataType, string, object)> AddressDictionary;

        private Stack<int> Stack;
        private Stack<uint> ReturnStack;
        public AssemblerParser(string program, Processor processor)
        {
            _program = program.ToUpper();
            ProgramArray = _program.Split("\n");

            Processor = processor;
            Labels = new ();
            AddressDictionary = new ();
            Stack = new Stack<int>();
            ReturnStack = new Stack<uint>();
            LastCompareValue = new (0, 0);
            ProgramPtr = 0;
        }
        public void Run()
        {
            InitLabels();
            InitCustomData();

            for (; ProgramPtr < ProgramArray.Length; ProgramPtr++)
            {
                var splitStr = GetCleanCode(ProgramPtr);

                if (!splitStr[0].Contains(':'))
                {
                    RunCommand(splitStr);
                }
            }
        }
        private void InitCustomData()
        {
            uint i;
            bool isData = false;
            for (i = 0; i < ProgramArray.Length|| !GetCleanCode(i).Contains(".CODE"); i++)
            {
                var splitStr = GetCleanCode(i);

                if (splitStr[0].Contains(".DATA"))
                {
                    isData = true;
                    continue;
                }
                if (splitStr.Length < 2) break;
                else if (isData)
                {
                    string name = splitStr[0];
                    string type = splitStr[1];
                    string value = splitStr[2];

                    if (IsValidName(name) && IsValidType(type) && IsValidValue(value))
                    {
                        var datatype = Enum.Parse<DataType>(type);
                        AddressDictionary.Add((int)i, (datatype, name, value));
                    }
                }
            }
            ProgramPtr = ++i;
        }
        private bool IsValidValue(string value) => (value != null) ? ((value.Trim('\"').Contains("\"")) ? false : true) : false;
        private bool IsValidType(string type) => Enum.TryParse<DataType>(type, true, out var res);
        private bool IsValidName(string name) => Regex.IsMatch(name, @"[a-zA-Z_$0-9]");
        private void RunCommand(string[] splitStr)
        {
            var operation = splitStr[0];

            if (splitStr.Length == 3)
            {
                object? operand1 = splitStr[1].Replace(',', '\0');
                object? operand2 = splitStr[2];
                operand1 = IsRegister(operand1) ? GetRegister(operand1.ToString()) : Convert.ToInt32(operand1);
                if (IsRegister(operand2))
                {
                    operand2 = GetRegister(operand2.ToString());
                }
                else if(IsCustomData(operand2))
                {
                    operand2 = int.Parse((string)AddressDictionary.First(x => operand2.ToString().Contains(x.Value.Item2)).Value.Item3);
                }
                else operand2 = Convert.ToInt32(operand2);

                TwoOperandCommand((Register)operand1, operand2, Enum.Parse<Operation>(operation));
            }
            else if (splitStr.Length == 2)
            {
                object operand1 = splitStr[1];
                SingleOperationCommand(GetRegister(operand1.ToString()) ?? operand1, Enum.Parse<Operation>(operation));
            }
            else if(splitStr.Length == 1 && !string.IsNullOrEmpty(splitStr[0].Trim()))
            {
                ZeroOperationCommand(Enum.Parse<Operation>(operation));
            }
        }
        private void TwoOperandCommand(Register operand1, object operand2, Operation operation)
        {
            if(IsCustomData(operand2))
            {
                operand2 = AddressDictionary.First(x => x.Value.Item2 == operand2.ToString()).Value.Item3;
            }
            switch (operation)
            {
                case Operation.ADD:
                    Processor.Operaion(operand1, operand2, Operation.ADD);
                    break;
                case Operation.SUB:
                    Processor.Operaion(operand1, operand2, Operation.SUB);
                    break;
                case Operation.MOV:
                    Processor.Move(operand1, operand2);
                    break;
                case Operation.CMP:
                    LastCompareValue = new(operand1, operand2);
                    break;
                default:
                    break;
            }

        }
        private void SingleOperationCommand(object operand, Operation operation)
        {
            switch (operation)
            {
                case Operation.MUL:
                case Operation.DIV:
                    Processor.Operaion(Register.AX, operand, operation);
                    break;
                case Operation.INC:
                case Operation.DEC:
                    Processor.Operaion(Enum.Parse<Register>(operand.ToString()), 0, operation);

                    break;
                case Operation.JE:
                case Operation.JNE:
                case Operation.JL:
                case Operation.JLE:
                case Operation.JG:
                case Operation.JGE:
                    var result = Processor.Compare(LastCompareValue.Item1, LastCompareValue.Item2, operation);
                    if (result)
                    {
                        ProgramPtr = Labels.First(x => operand.ToString().Contains(x.Value)).Key;
                    }
                    break;
                case Operation.JMP:
                    ReturnStack.Push(ProgramPtr);
                    ProgramPtr = Labels.First(x => operand.ToString().Contains(x.Value)).Key;
                    break;
                case Operation.INT:
                    Processor.Interaction(operand);
                    break;
                case Operation.PUSH:
                    Stack.Push(GetRegisters()[GetRegisterInt(operand)]);
                    break;               
                case Operation.POP:
                    Processor.GetPop((Register)GetRegister(operand.ToString()), Stack.Pop());
                    break;

                default:
                    break;
            }
        }
        private void ZeroOperationCommand(Operation operation)
        {
            switch (operation)
            {
                case Operation.RET:
                    ProgramPtr = ReturnStack.Pop();
                    break;
                default:
                    break;
            }
        }
        private bool IsRegister(object operand) =>
            Enum.GetValues(typeof(Register)).Cast<Register?>().ToList().Find(x => operand.ToString().Contains(x.ToString())) != null;
        private Register? GetRegister(string register)
        {
            foreach (Register reg in Enum.GetValues(typeof(Register)))
            {
                if (register.Contains(reg.ToString()))
                {
                    return reg;
                }
            }
            return null;

        }
        private void InitLabels()
        {
            for (uint i = 0; i < ProgramArray.Length; i++)
            {
                var splitStr = GetCleanCode(i);

                if (splitStr[0].Contains(":"))
                {
                    Labels.Add(i, splitStr[0].Substring(0, splitStr.Length - 1));
                }
            }
        }
        private string[] GetCleanCode(uint i)
        {
            return ProgramArray[i].Remove
                    (ProgramArray[i].IndexOf(';') >= 0 ? ProgramArray[i].IndexOf(';') : ProgramArray[i].Length).Split(" ");
        }
        private int GetRegisterInt(object register)
        {
            return (int)Enum.Parse<Register>(register.ToString());
        }
        private bool IsCustomData(object operand)
        {
            if(operand == null) return false;
            return AddressDictionary.Any(x =>operand.ToString().Contains(x.Value.Item2));
        }
        public int[] GetRegisters()
        {
            return Processor.Registers;
        }        
        public int[] GetStack()
        {
            return Stack.ToArray();
        }
        
    }
}
