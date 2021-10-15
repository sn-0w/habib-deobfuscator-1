using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class cctor
    {
        public static bool execute(ModuleDefMD md)
        {
            foreach (TypeDef type in md.GetTypes())
            {
                if (!type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    try
                    {
                        if (!method.HasBody && !method.Body.HasInstructions) continue;


                        if (method.Name.Contains(".cctor"))
                        {

                            Console.WriteLine("cctor method found");
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                //Console.WriteLine(i.ToString()+" "+method.Body.Instructions[i].OpCode.ToString());

                                if (method.Body.Instructions[i].OpCode.ToString() == "ldstr" && !(method.Body.Instructions[i].Operand.ToString().Contains("HABIB_EXTREME_PROTECTOR")))
                                {
                                    Console.WriteLine("real cctor found");
                                    Console.WriteLine(method.Body.Instructions[i].Operand.ToString());
                                    MethodDef realcctor = getmethodbyname(md, method.Body.Instructions[i].Operand.ToString());
                                    method.Body.Instructions.Clear();
                                    method.Body.Instructions.Add(OpCodes.Nop.ToInstruction());

                                    method.Body.Instructions.Add(OpCodes.Call.ToInstruction(realcctor));

                                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            return true;
        }

        static MethodDef getmethodbyname(ModuleDefMD md, string name)
        {
            string nohabib = name.Replace("<HABIB>", "");
            nohabib = nohabib.Replace("</HABIB>", "");
            foreach (TypeDef type in md.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Name.Contains(name))
                    {
                        return method;
                    }
                }
            }

            return md.EntryPoint;
        }
    }
}
