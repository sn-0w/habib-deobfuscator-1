using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Habib_Deobfuscator
{
    class Program
    {
        static public int debugcount = 0;
        static public int decompilecount = 0;
        static void Main(string[] args)
        {
            //bin decrypter
            if (args[0].EndsWith(".bin"))
            {
                byte[] bytes = File.ReadAllBytes(args[0]);
                StackTrace stackTrace = new StackTrace();
                RijndaelManaged rijndaelManaged = new RijndaelManaged();

                int num = 0;
                int num2 = 0;
                byte[] array9 = new byte[32];
                MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider();
                byte[] array10 = md5CryptoServiceProvider.ComputeHash(Convert.FromBase64String("YjBlOGZiYjFkZThhNGQ2OTkwZTA0OTM5YTRmMzJiZGM="));
                checked
                {

                    Array.Copy(array10, num + num2, array9, num + num2, 16 + num + num2);
                    string inputStr = "15";
                    int num3 = (int)Math.Round(Conversion.Val(inputStr));
                    if (true)
                    {
                        Array.Copy(array10, num + num2, array9, num3 + num + num2, 16 + num + num2);
                        rijndaelManaged.Key = array9;
                        rijndaelManaged.Mode = CipherMode.ECB;
                        byte[] buffer = (byte[])bytes;
                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            using (MemoryStream memoryStream2 = new MemoryStream())
                            {
                                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                                {
                                    deflateStream.CopyTo(memoryStream2);
                                }
                                byte[] array11 = memoryStream2.ToArray();
                                byte[] array12 = rijndaelManaged.CreateDecryptor().TransformFinalBlock(array11, num + num2, array11.Length);
                                File.WriteAllBytes(args[0] + ".bin2", rijndaelManaged.CreateDecryptor().TransformFinalBlock(array12, num + num2, array12.Length));
                                Environment.Exit(0);
                            }
                        }
                    }

                }
            }


            ModuleDefMD md = ModuleDefMD.Load(args[0]);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Module SysModule = Assembly.UnsafeLoadFrom(args[0]).ManifestModule;


            //part 1
            for (int i = 0; i < md.Resources.Count(); i++)
            {
                if (md.Resources[i].ToString().Split(' ')[3] == "32")
                {
                    md.Resources.RemoveAt(i);
                    i--;
                }
            }

            cctor.execute(md);
            antidecompile.execute(md);
            antibreak.execute(md);
            stringdecryptarray.execute(md);
            nopnop.NopRemover(md);
            arrayfix.execute(md);
            ihatemath.Program.execute(md);
            IntptrPoint.PointRemover.execute(md);
            antiantiinvoke.execute(md);
            base64decode.execute(md);
            nopnop.NopRemover(md);
            locals2fieldsfixer.Fix(md, SysModule);
            strlength.execute(md);
            mathfixer.execute(md);
            arrayremover.execute(md);
            proxyremover.execute(md);
            nopnop.NopRemover(md);
            arrayremover.execute2(md);
            nopnop.NopRemover(md);
            mainmethod.execute(md);
            nopnop.NopRemover(md);
            mathfixer.execute(md);
            nopnop.NopRemover(md);

            //de4dot moment
            hell.fixcflow(md);

            //fix that method breaking but lazy lmfao
            md.EntryPoint.Body.Instructions.RemoveAt(2053);

            //part 2
            stringdecryptarray.execute(md);
            nopnop.NopRemover(md);
            arrayfix.execute(md);
            base64decode.execute(md);
            nopnop.NopRemover(md);
            arrayremover.onlyarray(md);
            nopnop.NopRemover(md);
            proxy_deobfuscator(md);


            foreach (var type in md.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body == null) continue;

                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        if (instr[i].OpCode == OpCodes.Ldstr && instr[i].Operand.ToString() == "dnSpy-x86" && instr[i+13].OpCode == OpCodes.Stloc && instr[i -43].OpCode == OpCodes.Ldtoken)
                        {
                            int start = i - 43;
                            int end = i + 12;
                            instr[i + 12].OpCode = OpCodes.Ldc_I4_0;
                            for(start = i - 43; start < end;start++)
                            {
                                instr[start].OpCode = OpCodes.Nop;
                            }
                        }
                        if(instr[i].OpCode == OpCodes.Call && (instr[i].Operand.ToString().Contains("get_IsAttached")  || instr[i].Operand.ToString().Contains("IsLogging") || instr[i].Operand.ToString().Contains("IsDebuggerPresent")))
                        {
                            instr[i].OpCode = OpCodes.Ldc_I4_0;
                        }
                    }
                }
            }

            nopnop.NopRemover(md);
            hell.fixcflow(md);

            //writing to disk
            ModuleWriterOptions writerOptions = new ModuleWriterOptions(md);
            writerOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            writerOptions.Logger = DummyLogger.NoThrowInstance;
            md.Write(args[0] + "-sus.exe", writerOptions);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("saved as " + md.Name + "-sus.exe");
            Console.ReadLine();
            Environment.Exit(0);
        }


        public static void proxy_deobfuscator(ModuleDefMD md)
        {
            foreach (var type in md.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.Body == null) continue;

                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        var instr = method.Body.Instructions;
                        if (instr[i].OpCode == OpCodes.Call)
                        {
                            try
                            {
                                IMethod possible_proxy = (IMethod)instr[i].Operand;
                                MethodDef method_proxy = (MethodDef)possible_proxy;
                                if (method_proxy == null) continue;

                                if (method_proxy.Body.Instructions.Count() == 2 + method_proxy.GetParamCount() && method_proxy.Body.Instructions.Last().OpCode == OpCodes.Ret)
                                {
                                    switch (method_proxy.Body.Instructions[method_proxy.GetParamCount()].OpCode.ToString())
                                    {
                                        case "newobj":
                                            {
                                                instr[i].OpCode = OpCodes.Newobj;
                                                instr[i].Operand = method_proxy.Body.Instructions[method_proxy.GetParamCount()].Operand;
                                                break;
                                            }
                                        case "call":
                                            {
                                                instr[i].OpCode = OpCodes.Call;
                                                instr[i].Operand = method_proxy.Body.Instructions[method_proxy.GetParamCount()].Operand;
                                                break;
                                            }
                                    }
                                }

                            }
                            catch { }
                        }
                    }
                }
            }
        }
    }
}

