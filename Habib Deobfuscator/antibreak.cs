using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class antibreak
    {
        static public bool execute(ModuleDefMD md)
        {
            try
            {
                for (int i = 0; i < md.EntryPoint.Body.Instructions.Count; i++)
                {
                    if (md.EntryPoint.Body.Instructions[i].OpCode == OpCodes.Call && md.EntryPoint.Body.Instructions[i].Operand.ToString().Contains("Exit"))
                    {
                        md.EntryPoint.Body.Instructions[i - 5].OpCode = OpCodes.Nop;
                        md.EntryPoint.Body.Instructions[i - 4].OpCode = OpCodes.Nop;
                        md.EntryPoint.Body.Instructions[i - 3].OpCode = OpCodes.Nop;
                        md.EntryPoint.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                        md.EntryPoint.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        md.EntryPoint.Body.Instructions[i].OpCode = OpCodes.Nop;
                    }
                }
            }
            catch { }
            return true;
        }
    }
}
