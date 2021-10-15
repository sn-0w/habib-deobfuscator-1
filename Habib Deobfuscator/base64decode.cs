using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class base64decode
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
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                            {
                                if (method.Body.Instructions[i + 1].OpCode == OpCodes.Call && method.Body.Instructions[i + 1].Operand.ToString().Contains("FromBase64String"))
                                {
                                    if (method.Body.Instructions[i + 2].OpCode == OpCodes.Callvirt && method.Body.Instructions[i + 2].Operand.ToString().Contains("GetString"))
                                    {
                                        string base64 = method.Body.Instructions[i].Operand.ToString();

                                        //method.Body.Instructions[i-1].OpCode = OpCodes.Nop;
                                        method.Body.Instructions[i].Operand = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                                        method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                        method.Body.Instructions[i+1].OpCode = OpCodes.Nop;
                                        method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;

                                        counter++;


                                    }
                                }
                            }
                        }

                    }
                    catch { }
                    }
            }
            Console.WriteLine($"decrypted {counter} base64 strings");
        }

        public static void solidsbase64(ModuleDef Module)
        {
            foreach (TypeDef type in Module.Types)
            {
                if (!type.IsGlobalModuleType) continue;
                Console.WriteLine(type.FullName);

                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    if (!method.Body.HasInstructions) continue;
                    //if (method.IsConstructor) continue;
                    Console.WriteLine(method.FullName);

                    var instructions = method.Body.Instructions;
                    for (int i = 2; i < instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("get_UTF8") && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldstr && method.Body.Instructions[i + 2].Operand.ToString().Contains("FromBase64String"))
                        {
                            var valuebase64 = System.Convert.FromBase64String(method.Body.Instructions[i + 1].Operand.ToString());

                            Console.WriteLine(method.Body.Instructions[i].ToString());

                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i + 1].OpCode = OpCodes.Ldstr;
                            method.Body.Instructions[i + 1].Operand = System.Text.Encoding.UTF8.GetString(valuebase64);
                            method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i + 3].OpCode = OpCodes.Nop;
                        }
                    }
                }
            }
        }
    }
}
