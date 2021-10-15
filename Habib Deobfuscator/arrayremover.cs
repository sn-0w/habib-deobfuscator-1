using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class arrayremover
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

                            //field array
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Ldsfld && instr[i + 2].OpCode == OpCodes.Ldstr && instr[i + 3].OpCode == OpCodes.Stelem_Ref)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                    instr[i + 2].OpCode = OpCodes.Nop;
                                    instr[i + 3].OpCode = OpCodes.Nop;
                                    counter++;
                                }
                            }
                            catch { }
                            try
                            {
                                //field array constructor
                                if (instr[i].OpCode == OpCodes.Stsfld && instr[i - 1].OpCode == OpCodes.Newarr)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i - 1].OpCode = OpCodes.Nop;
                                    instr[i - 2].OpCode = OpCodes.Nop;
                                    instr[i - 3].OpCode = OpCodes.Nop;
                                    instr[i - 4].OpCode = OpCodes.Nop;
                                    counter++;
                                }
                            }
                            catch { }
                            // non field array
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Ldloc && instr[i + 2].OpCode == OpCodes.Ldstr && instr[i + 3].OpCode == OpCodes.Stelem_Ref)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                    instr[i + 2].OpCode = OpCodes.Nop;
                                    instr[i + 3].OpCode = OpCodes.Nop;
                                    counter++;
                                }
                            }
                            catch { }
                            //non field array constructor
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Stloc && instr[i - 1].OpCode == OpCodes.Newarr)
                                {
                                    //instr[i].OpCode = OpCodes.Nop;
                                    //instr[i - 1].OpCode = OpCodes.Nop;
                                    //instr[i - 2].OpCode = OpCodes.Nop;
                                    counter++;
                                }
                            }
                            catch { }
                            //field junk
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Stsfld && instr[i - 1].IsLdcI4())
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i - 1].OpCode = OpCodes.Nop;
                                }
                            }
                            catch { }
                            //ClearProjectError junk
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("ClearProjectError"))
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    // instr[i-1].OpCode = OpCodes.Nop;
                                }
                            }
                            catch { }
                            //anti kill
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("GetCurrentProcess") && instr[i + 1].OpCode == OpCodes.Callvirt && instr[i].Operand.ToString().Contains("Kill"))
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                }
                            }
                            catch { }
                            //anti exit
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("Enviroment::Exit") && instr[i - 1].IsLdcI4())
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i - 1].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                }
                            }
                            catch { }
                            //clean reference proxys
                            try
                            {
                                if (instr[i].Operand.ToString().Contains("String::Empty") && instr[i].OpCode == OpCodes.Ldsfld && instr[i - 3].OpCode == OpCodes.Ldnull)
                                {
                                    int max = 2;
                                    max += method.Parameters.Count();
                                    int min = instr.Count - max;
                                    for (int k = 0; k < min; k++)
                                    {
                                        instr[k].OpCode = OpCodes.Nop;
                                    }
                                }
                            }
                            catch { }

                        }
                    }
                    catch { }
                }
            }
            Console.WriteLine($"Removed {counter} arrays");
        }

        public static void execute2(ModuleDefMD md)
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

                            //clean -1 arrays
                            try
                            {
                                if (instr[i].Operand.ToString().Contains("Int32") && instr[i].OpCode == OpCodes.Newarr && instr[i + 4].OpCode == OpCodes.Ldc_I4_M1)
                                {
                                    instr[i - 1].OpCode = OpCodes.Nop;
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                }

                            }
                            catch { }
                            // clean -1 array bodies
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Ldloc_2 && instr[i + 1].IsLdcI4() && instr[i + 2].OpCode == OpCodes.Ldc_I4_M1 && instr[i + 3].OpCode == OpCodes.Stelem)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                    instr[i + 2].OpCode = OpCodes.Nop;
                                    instr[i + 3].OpCode = OpCodes.Nop;
                                }
                            }
                            catch { }

                            // clean array heads
                            try
                            {
                                if (instr[i].IsLdcI4() && instr[i + 1].OpCode == OpCodes.Newarr && instr[i + 1].Operand.ToString().Contains("System.String") && instr[i + 2].OpCode == OpCodes.Stloc)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                    instr[i + 2].OpCode = OpCodes.Nop;
                                }
                            }
                            catch { }

                        }
                    }
                    catch { }
                }
            }
            Console.WriteLine($"Removed {counter} arrays");
        }

        public static void onlyarray(ModuleDefMD md)
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

                            
                            // non field array
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Ldloc && instr[i + 2].OpCode == OpCodes.Ldstr && instr[i + 3].OpCode == OpCodes.Stelem_Ref)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i + 1].OpCode = OpCodes.Nop;
                                    instr[i + 2].OpCode = OpCodes.Nop;
                                    instr[i + 3].OpCode = OpCodes.Nop;
                                    counter++;
                                }
                            }
                            catch { }
                            //non field array constructor
                            try
                            {
                                if (instr[i].OpCode == OpCodes.Stloc && instr[i - 1].OpCode == OpCodes.Newarr)
                                {
                                    instr[i].OpCode = OpCodes.Nop;
                                    instr[i - 1].OpCode = OpCodes.Nop;
                                    instr[i - 2].OpCode = OpCodes.Nop;
                                    counter++;
                                }
                            }
                            catch { }
                            
                        }
                    }
                    catch { }
                }
            }
            Console.WriteLine($"Removed {counter} arrays");
        }
    }
}
