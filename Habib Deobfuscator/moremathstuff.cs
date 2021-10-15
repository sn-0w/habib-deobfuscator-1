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
    class moremathstuff
    {
        public static void execute(ModuleDefMD md)
        {
            var moduleDefMd = md;
            var allMethods = LoadMethods();

            foreach (var typeDef in moduleDefMd.GetTypes()
                .Where(x => x.HasMethods))
            {
                foreach (var methodDef in typeDef.Methods
                    .Where(x => x.HasBody))
                {
                    var instructions = methodDef.Body.Instructions;
                    for (int i = 0; i < instructions.Count; i++)
                    {
                        if (instructions[i].OpCode == OpCodes.Call
                            && instructions[i - 1].OpCode == OpCodes.Ldc_R8
                            && instructions[i].Operand.ToString().Contains("Math::"))
                        {
                            var memberRef = instructions[i].Operand as MemberRef;
                            if (allMethods.Any(x =>
                                x.Item1 == memberRef.Name))
                            {
                                var resultMethod = allMethods.Find(x =>
                                    x.Item2.ToString() == memberRef.Signature.ToString()
                                        .Replace("System.", string.Empty)
                                        .Replace(" ", $" {memberRef.Name}")).Item2;
                                if (resultMethod != null)
                                {
                                    var invokedValue = resultMethod.Invoke(null,
                                        new object[] { (double)instructions[i - 1].Operand });

                                    instructions[i].OpCode = OpCodes.Ldc_R8;
                                    instructions[i].Operand = invokedValue;
                                    instructions[i - 1].OpCode = OpCodes.Nop;

                                    Console.WriteLine($"[+] Math::{resultMethod.Name} returned: {invokedValue}");
                                }
                            }
                        }
                    }
                }
            }
        }

        static List<Tuple<string, MethodInfo>> LoadMethods()
        {
            var sortedList = new List<Tuple<string, MethodInfo>>();
            var allMethods = typeof(Math).GetMethods()
                .Where(x =>
                    x.Name != "GetHashCode" && x.Name != "GetType"
                                            && x.Name != "ToString"
                                            && x.Name != "Equals")
                .ToList();
            foreach (var methodInfo in allMethods)
                Console.WriteLine(methodInfo.Name);
            foreach (var methodInfo in allMethods)
                sortedList.Add(new Tuple<string, MethodInfo>(methodInfo.Name, methodInfo));

            return sortedList;
        }

    }
}
