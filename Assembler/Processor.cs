using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public class Processor
    {
        public int[] Registers { get; init; }
        public Processor()
        {
            Registers = new int[6];
            foreach (var i in Registers)
                Registers[i] = 0;
        }
        public void Move(Register destination, object source)
        {
            if (source is int)
            {
                Registers[(int)destination] = (int)source;
            }
            else if (source is Register)
            {
                Registers[(int)destination] = Registers[(int)(source)];
            }
            else throw new Exception("MOV: Type of sourse is undefined");
        }
        public bool Compare(Register register, object comparer, Operation condition)
        {

            if (!(comparer is int || comparer is Register))
            {
                throw new Exception("CMP: Type of comparer is undefined");
            }

            if (comparer is Register)
                comparer = Registers[(int)comparer];

            bool result = false;
            switch (condition)
            {
                case Operation.JE:
                    result = Registers[(int)register] == (int)comparer;
                    break;
                case Operation.JNE:
                    result = Registers[(int)register] != (int)comparer;
                    break;
                case Operation.JG:
                    result = Registers[(int)register] > (int)comparer;
                    break;
                case Operation.JGE:
                    result = Registers[(int)register] >= (int)comparer;
                    break;
                case Operation.JL:
                    result = Registers[(int)register] < (int)comparer;
                    break;
                case Operation.JLE:
                    result = Registers[(int)register] <= (int)comparer;
                    break;
            }
            return result;
        }
        public void Operaion(Register register, object number, Operation operation)
        {
            //if (number == null) throw new Exception($"{operation.ToString().ToUpper()}: Number is null");

            if (!(number is int || number is Register))
            {
                throw new Exception($"{operation.ToString().ToUpper()}: Number type is undefined");
            }
            if (number is Register) number = Registers[(int)number];

            switch (operation)
            {
                case Operation.ADD:
                    Registers[(int)register] += (int)number;
                    break;
                case Operation.SUB:
                    Registers[(int)register] -= (int)number;
                    break;
                case Operation.MUL:
                    Registers[(int)register] *= (int)number;
                    break;
                case Operation.DIV:
                    Registers[(int)Register.DX] = Registers[(int)register] % (int)number;
                    Registers[(int)register] /= (int)number;
                    break;
                case Operation.INC:
                    Registers[(int)register]++;
                    break;
                case Operation.DEC:
                    Registers[(int)register]--;
                    break;
                default:
                    break;
            }

        }
        public void Interaction(object number)
        {
            if(number.ToString().Contains("21H"))
                switch (Registers[(int)Register.AX])
                {
                    case 9:
                        Console.WriteLine(Registers[(int)Register.DX]);
                        break;
                    case 10:
                        var res = Console.ReadLine();
                        int.TryParse(res, System.Globalization.NumberStyles.None, null, out Registers[(int)Register.AX]);
                        break;
                    case 76:
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
        }
        public void GetPop(Register register, int popNumber)
        {
            Registers[(int)register] = popNumber;
        }
    }

}
