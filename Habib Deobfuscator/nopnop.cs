using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class nopnop
    {
        static int counter = 0;
        public static void NopRemover(ModuleDefMD modulee)
        {
            foreach (TypeDef typeDef in modulee.Types)
            {
                foreach (MethodDef methodDef in typeDef.Methods)
                {
                    if (methodDef.HasBody)
                    {
                        RemoveUnusedNops(methodDef);
                    }
                }
            }
            Console.WriteLine($"Remove {counter} unused nop codes");
        }

        public static void RemoveUnusedNops(MethodDef MethodDef)
        {
            if (MethodDef.HasBody)
            {
                for (int i = 0; i < MethodDef.Body.Instructions.Count; i++)
                {
                    Instruction instruction = MethodDef.Body.Instructions[i];
                    if (instruction.OpCode == OpCodes.Nop)
                    {
                        if (!IsNopBranchTarget(MethodDef, instruction))
                        {
                            if (!IsNopSwitchTarget(MethodDef, instruction))
                            {
                                if (!IsNopExceptionHandlerTarget(MethodDef, instruction))
                                {
                                    counter++;
                                    MethodDef.Body.Instructions.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsNopBranchTarget(MethodDef MethodDef, Instruction NopInstr)
        {
            for (int i = 0; i < MethodDef.Body.Instructions.Count; i++)
            {
                Instruction instruction = MethodDef.Body.Instructions[i];
                if (instruction.OpCode.OperandType == OperandType.InlineBrTarget || instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                {
                    if (instruction.Operand != null)
                    {
                        Instruction instruction2 = (Instruction)instruction.Operand;
                        if (instruction2 == NopInstr)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsNopSwitchTarget(MethodDef MethodDef, Instruction NopInstr)
        {
            for (int i = 0; i < MethodDef.Body.Instructions.Count; i++)
            {
                Instruction instruction = MethodDef.Body.Instructions[i];
                if (instruction.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    if (instruction.Operand != null)
                    {
                        Instruction[] source = (Instruction[])instruction.Operand;
                        if (source.Contains(NopInstr))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsNopExceptionHandlerTarget(MethodDef MethodDef, Instruction NopInstr)
        {
            bool result;
            if (!MethodDef.Body.HasExceptionHandlers)
            {
                result = false;
            }
            else
            {
                IList<ExceptionHandler> exceptionHandlers = MethodDef.Body.ExceptionHandlers;
                foreach (ExceptionHandler exceptionHandler in exceptionHandlers)
                {
                    if (exceptionHandler.FilterStart == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.HandlerEnd == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.HandlerStart == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.TryEnd == NopInstr)
                    {
                        return true;
                    }
                    if (exceptionHandler.TryStart == NopInstr)
                    {
                        return true;
                    }
                }
                result = false;
            }
            return result;
        }
    }
}
