using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Assembler;
class Program
{
    public static void Main()
    {
        string program = File.ReadAllText("asm.asm");

        var asm = new AssemblerParser(program, new Processor());
        asm.Run();

        Console.WriteLine("-------REGISTERS-------");
        int i = 0;
        foreach (var item in asm.GetRegisters())
            Console.WriteLine($"{(Register)i++}: {item}");

        Console.WriteLine("\n-------STACK-------");
        foreach (var item in asm.GetStack())
        {
            Console.WriteLine(item);
        }
    }
}
