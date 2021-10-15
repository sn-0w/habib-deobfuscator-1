using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class mathfixer
    {
        static public bool  execute(ModuleDefMD md)
        {
            int counter = 0;
            foreach (TypeDef type in md.GetTypes())
            {
                if (!type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {

                    if (!method.HasBody) continue;
                    if (!method.Body.HasInstructions) continue;
                    try
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("DB926B2F0"))
                            {
                                int minus = 0;
                                string math = "";
                                while (true)

                                {
                                    minus++;
                                    if (method.Body.Instructions[i - minus].OpCode == OpCodes.Ldstr)
                                    {
                                        math = math + method.Body.Instructions[i - minus].Operand.ToString();
                                    }

                                    if (method.Body.Instructions[i - minus].OpCode == OpCodes.Newarr)
                                    {
                                        break;
                                    }
                                }
                                minus++;
                                minus++;
                                for (int reni = 1; reni < minus; reni++)
                                {
                                    method.Body.Instructions[i - reni].OpCode = OpCodes.Nop;
                                }


                                string reverseString = string.Empty;
                                for (int k = math.Length - 1; k >= 0; k--)
                                {
                                    reverseString += math[k];
                                }
                                method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                                int count = 0;
                                foreach (char chr in reverseString)
                                {
                                    count++;   
                                }
                                if (count > 1)
                                {
                                    //Console.WriteLine("math : " + reverseString);
                                    method.Body.Instructions[i].Operand = Int32.Parse(reverseString);
                                }
                                counter++;
                                //Console.WriteLine(reverseString);
                            }
                        }



                    }
                    catch (Exception e) { Console.WriteLine(e); }
                }
            }
            Console.WriteLine($"fixed {counter} math protections ");
            return true;
        }
    }
}
