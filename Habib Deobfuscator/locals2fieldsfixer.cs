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
	class locals2fieldsfixer
	{
		public static void Fix(ModuleDefMD md, Module SysModule)
		{
			int counter = 0;
			bool flag = md != null && SysModule != null;
			if (flag)
			{
				//foreach (TypeDef typeDef in from x in md.Types where x.HasMethods && !x.IsGlobalModuleType select x)
				foreach (TypeDef typeDef in from x in md.Types where x.HasMethods select x)

				{
					foreach (MethodDef methodDef in typeDef.Methods.Where((MethodDef x) => x.HasBody))
					{
						IList<Instruction> instructions = methodDef.Body.Instructions;
						for (int i = 0; i < instructions.Count; i++)
						{
							bool flag2 = instructions[i].OpCode == OpCodes.Ldsfld && ((IField)instructions[i].Operand).DeclaringType == md.GlobalType && ((IField)instructions[i].Operand).ResolveFieldDef().FieldType == md.CorLibTypes.Int32;
							if (flag2)
							{
								try
								{
									object value = SysModule.ResolveField(((IField)instructions[i].Operand).MDToken.ToInt32()).GetValue(null);
									instructions[i] = new Instruction(OpCodes.Ldc_I4, value);
									counter++;
								}
								catch (Exception ex)
								{
									Console.WriteLine(ex);
								}
							}
						}
					}
				}
			}
			Console.WriteLine($"Restored {counter} Fields ");

		}
	}
}
