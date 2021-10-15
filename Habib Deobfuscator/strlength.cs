using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class strlength
    {
        public static void execute(ModuleDefMD md)
        {
            int counter = 0;
            foreach (TypeDef type in md.GetTypes())
            {
                if (!type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    try
                    {
                        if (!method.HasBody) continue;
                        if (!method.Body.HasInstructions) continue;

                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            IList<Instruction> instr = method.Body.Instructions;
                            if(instr[i].OpCode == OpCodes.Ldstr && instr[i+1].OpCode == OpCodes.Ldlen )
                            {
                                instr[i] = new Instruction(OpCodes.Ldc_I4, instr[i].Operand.ToString().Length);
                                instr.RemoveAt(i + 1);
                                counter++;
                            }
                        }
                    }
                    catch { }
                }
            }
            Console.WriteLine($"solved {counter} ldlen methods");
        }
    }
}
