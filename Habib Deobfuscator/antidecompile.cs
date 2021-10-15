using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class antidecompile
    {
        public static int debugcount = 0;
        public static int decompilecount = 0;
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
                        try
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Box)
                            {
                                if (method.Body.Instructions[i].Operand.ToString().Contains("<HABIB>"))
                                {
                                    decompilecount++;

                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Call)
                            {
                                if (method.Body.Instructions[i].Operand.ToString().Contains("Debugger::Log"))
                                {
                                    //Console.WriteLine(method.Body.Instructions[i].Operand.ToString());
                                    debugcount++;

                                    method.Body.Instructions[i - 3].Operand = "ZnVjaw==";

                                }
                            }
                        }
                        catch { }

                    }
                }
            }
            Console.WriteLine($"Removed {debugcount} anti debug things");
            Console.WriteLine($"Removed {decompilecount} anti decompile things");

            return true;
        }
    }
}
