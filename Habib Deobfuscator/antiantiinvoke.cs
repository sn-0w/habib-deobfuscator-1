using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class antiantiinvoke
    {
        static public void execute(ModuleDefMD md)
        {
            int count = 0;

            foreach (TypeDef type in md.GetTypes())
            {
                if (!type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    try
                    {
                        if (!method.HasBody && !method.Body.HasInstructions) continue;
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("CallingAssembly"))
                            {
                                method.Body.Instructions[i].Operand = (method.Body.Instructions[i].Operand = md.Import(typeof(Assembly).GetMethod("GetExecutingAssembly")));
                                count++;
                            }
                        }
                    }
                    catch { }
                }
            }
            Console.WriteLine($"fixed {count} invoke detections");
        }
    }
}
