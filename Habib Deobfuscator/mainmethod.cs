using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class mainmethod
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

                            try
                            {
                                if (instr[i].Operand.ToString().Contains("Int32") && instr[i].OpCode == OpCodes.Call && instr[i - 3].OpCode == OpCodes.Ldstr)
                                {
                                    int num = Int32.Parse(instr[i - 3].Operand.ToString());
                                    instr[i].OpCode = OpCodes.Ldc_I4;
                                    instr[i].Operand = num;
                                    instr[i - 1].OpCode = OpCodes.Nop;
                                    instr[i - 2].OpCode = OpCodes.Nop;
                                    instr[i - 3].OpCode = OpCodes.Nop;
                                    counter++;
                                }

                            }
                            catch { }


                        }
                    }
                    catch { }
                }
            }

            Console.WriteLine($"fixed {counter} array complications");
        }
    }
}
