using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public enum Operation
    {
        ADD,
        SUB,
        MUL,
        DIV,
        INC,
        DEC,

        MOV,

        CMP,
        JMP,
        RET,

        JE,
        JNE,
        JG,
        JGE,
        JL,
        JLE,
        INT,
        PUSH,
        POP,

    }
}
