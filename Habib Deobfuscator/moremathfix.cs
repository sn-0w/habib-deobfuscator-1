using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntptrPoint
{
    class PointRemover
    {
        public static int counter = 0;
        public static int amount = 0;
        public static int mathFixed = 0;
        public static int timeSpanCleaned = 0;

        public static void execute(ModuleDefMD md)
        {
            foreach (TypeDef types in md.GetTypes())
            {
                foreach (MethodDef method in types.Methods)
                {
                    if (!method.HasBody) continue;
                    for (int x = 0; x < method.Body.Instructions.Count; x++)
                    {
                        Instruction inst = method.Body.Instructions[x];
                        //int baseIndex = method.Body.Instructions.IndexOf(inst);
                        if (inst.OpCode.Equals(OpCodes.Xor) || inst.OpCode.Equals(OpCodes.Mul) || inst.OpCode.Equals(OpCodes.Add) || inst.OpCode.Equals(OpCodes.Sub))
                        {
                            if (method.Body.Instructions[x - 1].OpCode.Equals(OpCodes.Ldc_I4) && method.Body.Instructions[x - 2].OpCode.Equals(OpCodes.Ldc_I4))
                            {
                                int endCalc = -1;
                                int typeCalc = -1;
                                switch (inst.OpCode.ToString())
                                {
                                    case "xor":
                                        typeCalc = 0;
                                        endCalc = int.Parse(method.Body.Instructions[x - 2].Operand.ToString()) ^ int.Parse(method.Body.Instructions[x - 1].Operand.ToString());
                                        break;
                                    case "mul":
                                        typeCalc = 1;
                                        endCalc = int.Parse(method.Body.Instructions[x - 2].Operand.ToString()) * int.Parse(method.Body.Instructions[x - 1].Operand.ToString());
                                        break;
                                    case "add":
                                        typeCalc = 2;
                                        endCalc = int.Parse(method.Body.Instructions[x - 2].Operand.ToString()) + int.Parse(method.Body.Instructions[x - 1].Operand.ToString());
                                        break;
                                    case "sub":
                                        typeCalc = 3;
                                        endCalc = int.Parse(method.Body.Instructions[x - 2].Operand.ToString()) - int.Parse(method.Body.Instructions[x - 1].Operand.ToString());
                                        break;
                                }
                                
                                switch (typeCalc)
                                {
                                    case 0:
                                        //Console.WriteLine(" Calculation fixed '" + method.Body.Instructions[x - 2].Operand.ToString() + " ^ " + method.Body.Instructions[x - 1].Operand.ToString() + "' -> '" + endCalc + "'!");
                                        counter++;
                                        break;
                                    case 1:
                                        Console.WriteLine(" Calculation fixed '" + method.Body.Instructions[x - 2].Operand.ToString() + " * " + method.Body.Instructions[x - 1].Operand.ToString() + "' -> '" + endCalc + "'!");
                                        counter++;

                                        break;
                                    case 2:
                                        // Console.WriteLine(" Calculation fixed '" + method.Body.Instructions[x - 2].Operand.ToString() + " + " + method.Body.Instructions[x - 1].Operand.ToString() + "' -> '" + endCalc + "'!");
                                        counter++;

                                        break;
                                    case 3:
                                        //Console.WriteLine(" Calculation fixed '" + method.Body.Instructions[x - 2].Operand.ToString() + " - " + method.Body.Instructions[x - 1].Operand.ToString() + "' -> '" + endCalc + "'!");
                                        counter++;

                                        break;
                                }
                                Instruction calculated = new Instruction(OpCodes.Ldc_I4, endCalc);
                                method.Body.Instructions.RemoveAt(x - 2);
                                method.Body.Instructions.RemoveAt(x - 2);
                                method.Body.Instructions.RemoveAt(x - 2);
                                method.Body.Instructions.Insert(x - 2, OpCodes.Ldc_I4.ToInstruction(endCalc));
                                mathFixed++;
                            }
                        }
                        if (inst.Operand == null) { continue; }
                        if (inst.OpCode.Equals(OpCodes.Newobj))
                        {
                            if (inst.Operand.ToString().Contains("TimeSpan"))
                            {
                                int days, hours, minutes, seconds;
                                days = method.Body.Instructions[x - 4].GetLdcI4Value();
                                hours = method.Body.Instructions[x - 3].GetLdcI4Value();
                                minutes = method.Body.Instructions[x - 2].GetLdcI4Value();
                                seconds = method.Body.Instructions[x - 1].GetLdcI4Value();
                                TimeSpan ts = new TimeSpan(days, hours, minutes, seconds);
                                string getTsValFunc = method.Body.Instructions[x + 3].Operand.ToString();
                                int getTsIndex = x + 3;
                                int endVar = -1;
                                switch (getTsValFunc.Split(':')[2].Replace("()", null).ToLower().Replace("get_total", null))
                                {
                                    case "days":
                                        endVar = Convert.ToInt32(ts.TotalDays);
                                        break;
                                    case "hours":
                                        endVar = Convert.ToInt32(ts.TotalHours);
                                        break;
                                    case "minutes":
                                        endVar = Convert.ToInt32(ts.TotalMinutes);
                                        break;
                                    case "milliseconds":
                                        endVar = Convert.ToInt32(ts.TotalMilliseconds);
                                        break;
                                    case "seconds":
                                        endVar = Convert.ToInt32(ts.TotalSeconds);
                                        break;
                                }
                                for (int x_ = 0; x_ < method.Body.Instructions.Count; x_++)
                                {
                                    method.Body.Instructions.RemoveAt(x - 4);
                                    if (method.Body.Instructions[x - 4].OpCode.Equals(OpCodes.Call))
                                    {
                                        method.Body.Instructions.RemoveAt(x - 4);
                                        break;
                                    }
                                }
                                method.Body.Instructions.Insert(x - 4, new Instruction(OpCodes.Ldc_I4, endVar));
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(" TimeSpan Mutation Removed at IL_" + int.Parse(inst.Offset.ToString("X"), System.Globalization.NumberStyles.HexNumber) + "!");
                                timeSpanCleaned++;
                            }
                        }
                    }
                    IList<Instruction> instr = method.Body.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        // CODE: new Point(1676, 3352).Y 
                        // A POINT MUTATION IN DNSPY LOOKS LIKE:

                        //19  0047    ldc.i4    0x68C 
                        //20  004C    ldc.i4    0xD18
                        //21  0051    newobj    instance void [System.Drawing]System.Drawing.Point::.ctor(int32, int32)
                        //22  0056    stloc.s   V_69(69)
                        //23  0058    ldloca.s  V_69(69)
                        //24  005A    call    instance int32[System.Drawing]System.Drawing.Point::get_Y()


                        // 2 instructions before Point::.ctor (constructor) is X value, 1 instruction before is Y value

                        // 3 instructions ahead is .Y /.X  get_X obviously means .X   get_Y obviously means .Y

                        // Here, we will try getting point via instruction 21 (the newobj opcode). If this exists, it is possible to check if there is point mutation

                        try
                        {
                            if (instr[i + 3].OpCode != OpCodes.Call || instr[i].OpCode != OpCodes.Newobj || !instr[i].Operand.ToString().Contains("Point::.ctor") || !instr[i + 3].Operand.ToString().Contains("::get_") || !instr[i + 1].OpCode.Name.StartsWith("stloc") || !instr[i + 2].OpCode.Name.StartsWith("ldloca")) continue;
                        }
                        catch
                        {
                            continue; // If instruction is 0, 1, or 2, this will stop the OutOfRangeExeption. (skip)
                        }
                        if (instr[i + 3].Operand.ToString().Contains("::get_X")) // IF .X
                        {
                            if (!instr[i - 1].OpCode.ToString().Contains("ldc.i4"))
                                continue; //Opcode has to be an LDCi4 instruction
                            instr[i].OpCode = OpCodes.Nop; //removes newobj
                            instr[i + 1].OpCode = OpCodes.Nop; // removes stloc
                            instr[i + 2].OpCode = OpCodes.Nop; // removes ldloca
                            instr[i + 3].OpCode = OpCodes.Nop; // removes .X
                            instr[i - 1].OpCode = OpCodes.Nop; // Because looks for .X, we can remove the Y Value shit so only X value remains

                        }
                        if (instr[i + 3].Operand.ToString().Contains("::get_Y"))
                        {
                            if (!instr[i - 2].OpCode.ToString().Contains("ldc.i4"))
                                continue;
                            instr[i].OpCode = OpCodes.Nop;
                            instr[i + 1].OpCode = OpCodes.Nop;
                            instr[i + 2].OpCode = OpCodes.Nop;
                            instr[i + 3].OpCode = OpCodes.Nop;
                            instr[i - 2].OpCode = OpCodes.Nop;
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(" Removed Point at IL_" + int.Parse(instr[i + 3].Offset.ToString("X"), System.Globalization.NumberStyles.HexNumber) + "!");
                        amount++;
                    }
                }
            }
            Console.WriteLine($"fixed {counter} math things");
        }
    }
}