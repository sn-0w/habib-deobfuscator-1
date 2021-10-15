using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class stringdecryptarray
    {
        static public int stringdecryptarraycount = 0;
        static public bool execute(ModuleDefMD md)
        {
            foreach (TypeDef type in md.GetTypes())
            {
                if (!type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {

                    if (!method.HasBody) continue;
                    if (!method.Body.HasInstructions) continue;

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Call)
                        {
                            if (method.Body.Instructions[i].Operand.ToString().Contains("ToChar"))
                            {
                                if (method.Body.Instructions[i + 1].OpCode == OpCodes.Call)
                                {
                                    if (method.Body.Instructions[i + 1].Operand.ToString().Contains("ToString"))
                                    {
                                        if (method.Body.Instructions[i - 1].OpCode == OpCodes.Ldc_I4_S)
                                        {
                                            string chaar = Convert.ToString(Convert.ToChar(int.Parse(method.Body.Instructions[i - 1].Operand.ToString())));
                                            method.Body.Instructions[i].OpCode = OpCodes.Ldstr;
                                            method.Body.Instructions[i].Operand = chaar;
                                            method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                            method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;

                                        }

                                    }
                                }
                            }
                        }
                    }
                }

            



        }
            return true;
        }
}
}
