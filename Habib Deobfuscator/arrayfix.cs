using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class arrayfix
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
                        IDictionary<string, string> arrays = new Dictionary<string, string>();
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldloc || method.Body.Instructions[i].OpCode == OpCodes.Ldsfld)
                            {

                                if (method.Body.Instructions[i + 2].OpCode == OpCodes.Ldstr)
                                {
                                    try
                                    {
                                        arrays.Add(method.Body.Instructions[i].Operand.ToString(), method.Body.Instructions[i + 2].Operand.ToString());
                                    }
                                    catch (ArgumentException)
                                    {
                                        string key = method.Body.Instructions[i].Operand.ToString();
                                        string newval = arrays[key] + method.Body.Instructions[i + 2].Operand.ToString();
                                        arrays[key] = newval;

                                    }

                                }
                            }
                            if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("String::Concat"))
                            {
                                try
                                {
                                    if (method.Body.Instructions[i - 1].OpCode == OpCodes.Ldloc || method.Body.Instructions[i - 1].OpCode == OpCodes.Ldsfld)
                                    {
                                        
                                        string stringval = arrays[method.Body.Instructions[i - 1].Operand.ToString()];
                                        method.Body.Instructions[i].OpCode = OpCodes.Ldstr;
                                        method.Body.Instructions[i].Operand = stringval;
                                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                        counter++;
                                        
                                    }
                                }catch(Exception e)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }

            }
            Console.WriteLine($"fixed {counter} arrays");

        }
    }
}
