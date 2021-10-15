
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ihatemath
{
    class Program
    {
        public static int FixedSizeOf = 0;
        public static int EmptyTypesFixed = 0;
        public static int ParseFixed = 0;
        public static int MathsFixed = 0;
        public static int OperationFixed = 0;
        public static int StringsLengths = 0;
        public static int DecimalCompareFixed = 0;
        public static void execute(ModuleDefMD md)
        {

            ModuleDefMD module = md;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (TypeDef type in module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        try
                        {
                            SizeOfFixer(method);
                            EmptyTypesFixer(method);
                           // Cleaner(method);
                            UnParse(method);
                            //Cleaner(method);
                           StringsLengthFixer(method);
                          //  Cleaner(method);
                            MathsFixer(method);
                          //  Cleaner(method);

                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.ToString());
                        }

                    }
                }
            }
            stopWatch.Stop();
            //Console.WriteLine("Done ! Elapsed time : " + stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine($"fixed {ParseFixed.ToString()} parse things");
            Console.WriteLine($"fixed {EmptyTypesFixed.ToString()} empty types");
            Console.WriteLine($"fixed {FixedSizeOf.ToString()} sizeof things");
            Console.WriteLine($"fixed {MathsFixed.ToString()} maths things");
            Console.WriteLine($"fixed {OperationFixed.ToString()} operations things");
            Console.WriteLine($"fixed {StringsLengths.ToString()} stringlength things");
            Console.WriteLine($"fixed {DecimalCompareFixed.ToString()} decimalcompares things");

        }


        //This Method solve Decimal.Compare
        public static void DecimalCompareFixer(MethodDef method)
        {
            for (int i = 4; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("Compare") && method.Body.Instructions[i - 1].OpCode == OpCodes.Newobj && method.Body.Instructions[i - 2].IsLdcI4() && method.Body.Instructions[i - 3].OpCode == OpCodes.Newobj && method.Body.Instructions[i - 4].IsLdcI4())
                {
                    decimal first = method.Body.Instructions[i - 4].GetLdcI4Value();
                    decimal second = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int result = decimal.Compare(first, second);
                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 3].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 4].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                    method.Body.Instructions[i].Operand = result;
                    DecimalCompareFixed++;
                }
            }
        }

        //This Method Replace Math.X and solve c# native operations
        public static void MathsFixer(MethodDef method)
        {
            for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R8 && method.Body.Instructions[i + 1].OpCode == OpCodes.Call && method.Body.Instructions[i + 1].Operand.ToString().Contains("Math"))
                {
                    MemberRef MathMethod = (MemberRef)method.Body.Instructions[i + 1].Operand;
                    double argument = (double)method.Body.Instructions[i].Operand;
                    MethodInfo methodInfo = typeof(Math).GetMethod(MathMethod.Name, new System.Type[] { typeof(double) });
                    double result = (double)methodInfo.Invoke(null, new object[] { argument });
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = result;
                    MathsFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R4 && method.Body.Instructions[i + 1].OpCode == OpCodes.Call && method.Body.Instructions[i + 1].Operand.ToString().Contains("Math"))
                {
                    MemberRef MathMethod = (MemberRef)method.Body.Instructions[i + 1].Operand;
                    float argument = (float)method.Body.Instructions[i].Operand;
                    MethodInfo methodInfo = typeof(Math).GetMethod(MathMethod.Name, new System.Type[] { typeof(float) });
                    float result = (float)methodInfo.Invoke(null, new object[] { argument });
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = result;
                    MathsFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4 && method.Body.Instructions[i + 1].OpCode == OpCodes.Call && method.Body.Instructions[i + 1].Operand.ToString().Contains("Math"))
                {
                    MemberRef MathMethod = (MemberRef)method.Body.Instructions[i + 1].Operand;
                    int argument = (int)method.Body.Instructions[i].Operand;
                    MethodInfo methodInfo = typeof(Math).GetMethod(MathMethod.Name, new System.Type[] { typeof(int) });
                    int result = (int)methodInfo.Invoke(null, new object[] { argument });
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = result;
                    MathsFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R8 && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldc_R8 && method.Body.Instructions[i + 2].OpCode == OpCodes.Call && method.Body.Instructions[i + 2].Operand.ToString().Contains("Math"))
                {
                    MemberRef MathMethod = (MemberRef)method.Body.Instructions[i + 2].Operand;
                    double argument = (double)method.Body.Instructions[i].Operand;
                    MethodInfo methodInfo = typeof(Math).GetMethod(MathMethod.Name, new System.Type[] { typeof(double) });
                    double result = (double)methodInfo.Invoke(null, new object[] { argument });
                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = result;
                    MathsFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R4 && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldc_R4 && method.Body.Instructions[i + 2].OpCode == OpCodes.Call && method.Body.Instructions[i + 2].Operand.ToString().Contains("Math"))
                {
                    MemberRef MathMethod = (MemberRef)method.Body.Instructions[i + 2].Operand;
                    float argument = (float)method.Body.Instructions[i].Operand;
                    MethodInfo methodInfo = typeof(Math).GetMethod(MathMethod.Name, new System.Type[] { typeof(float) });
                    float result = (float)methodInfo.Invoke(null, new object[] { argument });
                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = result;
                    MathsFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4 && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldc_I4 && method.Body.Instructions[i + 2].OpCode == OpCodes.Call && method.Body.Instructions[i + 2].Operand.ToString().Contains("Math"))
                {
                    MemberRef MathMethod = (MemberRef)method.Body.Instructions[i + 2].Operand;
                    int argument = (int)method.Body.Instructions[i].Operand;
                    MethodInfo methodInfo = typeof(Math).GetMethod(MathMethod.Name, new System.Type[] { typeof(int) });
                    int result = (int)methodInfo.Invoke(null, new object[] { argument });
                    method.Body.Instructions[i + 2].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = result;
                    MathsFixed++;
                }
                else if (method.Body.Instructions[i].IsSub() && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg - secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].IsMul() && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg * secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].IsDiv() && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg / secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Xor && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg ^ secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].IsSub() && method.Body.Instructions[i - 1].IsRem() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg ^ secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Shl && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg << secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].IsShr() && method.Body.Instructions[i - 1].IsRem() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg >> secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.And && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg & secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
                else if (method.Body.Instructions[i].OpCode == OpCodes.Or && method.Body.Instructions[i - 1].IsLdcI4() && method.Body.Instructions[i - 2].IsLdcI4())
                {
                    int firstarg = method.Body.Instructions[i - 2].GetLdcI4Value();
                    int secondarg = method.Body.Instructions[i - 1].GetLdcI4Value();
                    int result = firstarg | secondarg;
                    method.Body.Instructions[i - 1].Operand = result;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
                    OperationFixed++;
                }
            }
        }
        public static void Cleaner(MethodDef method)
        {

            BlocksCflowDeobfuscator blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
            Blocks blocks = new Blocks(method);
            blocksCflowDeobfuscator.Initialize(blocks);
            blocksCflowDeobfuscator.Deobfuscate();
            blocks.RepartitionBlocks();
            IList<Instruction> list;
            IList<ExceptionHandler> exceptionHandlers;
            blocks.GetCode(out list, out exceptionHandlers);
            DotNetUtils.RestoreBody(method, list, exceptionHandlers);
        }
        //This Method Replace X.Parse
        public static void UnParse(MethodDef method)
        {
            for (int i = 1; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("Parse") && method.Body.Instructions[i - 1].OpCode == OpCodes.Ldstr)
                {
                    MemberRef Parse = (MemberRef)method.Body.Instructions[i].Operand;
                    if (Parse.DeclaringType.Name.Contains("Int32"))
                    {
                        int result = int.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                        method.Body.Instructions[i].Operand = result;
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("Single"))
                    {
                        float result = float.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_R4;
                        method.Body.Instructions[i].Operand = result;
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("Int64"))
                    {
                        long result = long.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_I8;
                        method.Body.Instructions[i].Operand = result;
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("Double"))
                    {
                        double result = double.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_R8;
                        method.Body.Instructions[i].Operand = result;
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("Decimal"))
                    {
                        Decimal result = Decimal.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_R4;
                        method.Body.Instructions[i].Operand = (float)result;
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("UInt32"))
                    {
                        uint result = uint.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                        method.Body.Instructions[i].Operand = (int)result;
                        method.Body.Instructions.Add(OpCodes.Conv_U4.ToInstruction());
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("UInt64"))
                    {
                        ulong result = ulong.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_I8;
                        method.Body.Instructions[i].Operand = (long)result;
                        method.Body.Instructions.Add(OpCodes.Conv_U8.ToInstruction());
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("Int16"))
                    {
                        short result = short.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                        method.Body.Instructions[i].Operand = (int)result;
                        method.Body.Instructions.Add(OpCodes.Conv_I2.ToInstruction());
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                    else if (Parse.DeclaringType.Name.Contains("UInt16"))
                    {
                        ushort result = ushort.Parse(method.Body.Instructions[i - 1].Operand.ToString());
                        method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                        method.Body.Instructions[i].Operand = (int)result;
                        method.Body.Instructions.Add(OpCodes.Conv_U2.ToInstruction());
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        ParseFixed++;
                    }
                }
            }
        }
        //This Method Remove size.EmptyType
        public static void EmptyTypesFixer(MethodDef method)
        {
            for (int i = 1; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Ldsfld && method.Body.Instructions[i].Operand.ToString().Contains("EmptyTypes") && method.Body.Instructions[i + 1].OpCode == OpCodes.Ldlen)
                {
                    method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4_0;
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    EmptyTypesFixed++;
                }
            }
        }
        //This Method solve sizeof(X)
        public static int GetManagedSize(Type type)
        {
            var method = new System.Reflection.Emit.DynamicMethod("GetManagedSizeImpl", typeof(uint), null);

            System.Reflection.Emit.ILGenerator gen = method.GetILGenerator();

            gen.Emit(System.Reflection.Emit.OpCodes.Sizeof, type);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            var func = (Func<uint>)method.CreateDelegate(typeof(Func<uint>));
            return checked((int)func());
        }

        public static void SizeOfFixer(MethodDef method)
        {
            for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
            {
                Instruction instr = method.Body.Instructions[i];
                if (instr.OpCode == OpCodes.Sizeof)
                {
                    Type SizeOfType = Type.GetType(instr.Operand.ToString());
                    if (SizeOfType != null)
                    {
                        instr.OpCode = OpCodes.Ldc_I4;
                        //See Here : https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/sizeof
                        instr.Operand = GetManagedSize(SizeOfType);
                        //instr.Operand = Marshal.SizeOf(SizeOfType);
                        FixedSizeOf++;
                    }

                }
            }
        }
        //This Method solve string.length
        public static void StringsLengthFixer(MethodDef method)
        {
            for (int i = 1; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr && method.Body.Instructions[i + 1].OpCode == OpCodes.Call && method.Body.Instructions[i + 1].Operand.ToString().Contains("get_Length"))
                {
                    string stringarg = (string)method.Body.Instructions[i].Operand;
                    int result = stringarg.Length;
                    method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                    method.Body.Instructions[i].Operand = result;
                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                    StringsLengths++;
                }
            }
        }
    }
    public static class Extensions
    {
        public static bool IsAdd(this Instruction op)
        {
            return op.OpCode == OpCodes.Add || op.OpCode == OpCodes.Add_Ovf || op.OpCode == OpCodes.Add_Ovf_Un;
        }
        public static bool IsSub(this Instruction op)
        {
            return op.OpCode == OpCodes.Sub || op.OpCode == OpCodes.Sub_Ovf || op.OpCode == OpCodes.Sub_Ovf_Un;
        }
        public static bool IsMul(this Instruction op)
        {
            return op.OpCode == OpCodes.Mul || op.OpCode == OpCodes.Mul_Ovf || op.OpCode == OpCodes.Mul_Ovf_Un;
        }
        public static bool IsDiv(this Instruction op)
        {
            return op.OpCode == OpCodes.Div || op.OpCode == OpCodes.Div_Un;
        }
        public static bool IsRem(this Instruction op)
        {
            return op.OpCode == OpCodes.Rem || op.OpCode == OpCodes.Rem_Un;
        }
        public static bool IsShr(this Instruction op)
        {
            return op.OpCode == OpCodes.Shr || op.OpCode == OpCodes.Shr_Un;
        }
    }
}