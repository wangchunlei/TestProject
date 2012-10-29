namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;

    public class Disassembler
    {
        private byte[] _il = new byte[0];
        private Module _module;
        private static Dictionary<short, OpCode> _opcodes = new Dictionary<short, OpCode>();
        private StringBuilder _output;
        private int _pos;
        private ILStyler _styler;

        static Disassembler()
        {
            foreach (FieldInfo info in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (typeof(OpCode).IsAssignableFrom(info.FieldType))
                {
                    OpCode code = (OpCode) info.GetValue(null);
                    if (code.OpCodeType != OpCodeType.Nternal)
                    {
                        _opcodes.Add(code.Value, code);
                    }
                }
            }
        }

        private Disassembler(MethodBase method, ILStyler styler)
        {
            this._module = method.DeclaringType.Module;
            this._styler = styler;
            if (method.GetMethodBody() != null)
            {
                this._il = method.GetMethodBody().GetILAsByteArray();
            }
        }

        private StringBuilder Dis()
        {
            return this.Dis(0, this._il.Length);
        }

        private StringBuilder Dis(int offsetFrom, int offsetTo)
        {
            // This item is obfuscated and can not be translated.
            this._output = new StringBuilder();
            while (this._pos >= this._il.Length)
            {
            Label_000E:
                if (0 == 0)
                {
                    return this._output;
                }
                this.DisassembleNextInstruction(this._pos >= offsetFrom);
            }
            goto Label_000E;
        }

        public static string Disassemble(MethodBase method)
        {
            return Disassemble(method, new ILStyler());
        }

        private static string Disassemble(MethodBase method, ILStyler styler)
        {
            return Disassemble(method, styler, 0, 0x7fffffff);
        }

        public static string Disassemble(MethodBase method, int offsetFrom, int offsetTo)
        {
            return Disassemble(method, new ILStyler(), offsetFrom, offsetTo);
        }

        private static string Disassemble(MethodBase method, ILStyler styler, int offsetFrom, int offsetTo)
        {
            return new Disassembler(method, styler).Dis(offsetFrom, offsetTo).ToString();
        }

        private void DisassembleNextInstruction(bool write)
        {
            int num = this._pos;
            OpCode c = this.ReadOpCode();
            string str = this.ReadOperand(c);
            if (write)
            {
                this._output.AppendFormat("IL_{0:X4}:  {1} {2}", num, this._styler.StyleOpCode(c.Name.PadRight(11)), str);
                this._output.AppendLine();
            }
        }

        private static void DisassembleQuery()
        {
            Assembly assembly;
            string data = (string) AppDomain.CurrentDomain.GetData("path");
            string[] refs = (string[]) AppDomain.CurrentDomain.GetData("refs");
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args) {
                string simpleName = new AssemblyName(args.Name).Name.ToLowerInvariant();
                string assemblyFile = refs.FirstOrDefault<string>(r => Path.GetFileNameWithoutExtension(r).ToLowerInvariant() == simpleName);
                if (assemblyFile == null)
                {
                    return null;
                }
                return Assembly.LoadFrom(assemblyFile);
            };
            try
            {
                assembly = Assembly.LoadFrom(data);
            }
            catch (Exception exception)
            {
                Log.Write("File: " + data + " could not be loaded: " + exception.Message);
                return;
            }
            string directoryName = Path.GetDirectoryName(data);
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (name.Name.StartsWith("TypedDataContext", StringComparison.Ordinal))
                {
                    Assembly.LoadFrom(Path.Combine(directoryName, name.Name + ".dll"));
                }
            }
            string[] strArray = refs;
            int index = 0;
            while (true)
            {
                if (index >= strArray.Length)
                {
                    break;
                }
                string path = strArray[index];
                if (Path.IsPathRooted(path))
                {
                    try
                    {
                        Assembly.LoadFrom(path);
                    }
                    catch
                    {
                    }
                }
                index++;
            }
            Type type = assembly.GetType("UserQuery");
            MethodInfo method = null;
            if (assembly.EntryPoint != null)
            {
                method = assembly.EntryPoint;
            }
            else
            {
                try
                {
                    method = type.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
                catch
                {
                }
            }
            if (method == null)
            {
                method = type.GetMethod("RunUserAuthoredQuery", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            byte[] iLAsByteArray = method.GetMethodBody().GetILAsByteArray();
            int offsetFrom = 0;
            int num3 = iLAsByteArray.Length - 1;
            if (iLAsByteArray[0] == OpCodes.Nop.Value)
            {
                offsetFrom++;
            }
            if ((num3 > 0) && (iLAsByteArray[num3] == OpCodes.Ret.Value))
            {
                num3--;
            }
            if ((num3 > 0) && (iLAsByteArray[num3] == OpCodes.Nop.Value))
            {
                num3--;
            }
            if ((num3 > 0) && (iLAsByteArray[num3] == OpCodes.Pop.Value))
            {
                num3--;
            }
            StringBuilder builder = new Disassembler(method, new HtmlILStyler(false)).Dis(offsetFrom, num3);
            Type type2 = (assembly.EntryPoint == null) ? type : assembly.GetType(assembly.GetName().Name, false, true);
            if (type2 != null)
            {
                foreach (MethodInfo info2 in type2.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (((info2 != method) && (info2.Name != "RunUserAuthoredQuery")) && (info2 != method))
                    {
                        builder.AppendLine();
                        builder.AppendLine("<b>" + new HtmlILStyler(true).StyleIdentifier(info2.Name) + ":</b>");
                        builder.Append(new Disassembler(info2, new HtmlILStyler(false)).Dis());
                    }
                }
                foreach (Type type3 in type2.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
                {
                    foreach (MethodBase base2 in type3.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Concat<MethodBase>(type3.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)))
                    {
                        builder.AppendLine();
                        builder.AppendLine("<b>" + new HtmlILStyler(true).StyleIdentifier(type3.Name + "." + base2.Name) + ":</b>");
                        builder.Append(new Disassembler(base2, new HtmlILStyler(false)).Dis());
                    }
                }
            }
            string str4 = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html>\r\n  <head>\r\n    <style type=\"text/css\">body\r\n{\r\n\tmargin: 0.3em 0.3em 0.4em 0.5em;\r\n}\r\npre\r\n{\r\n\tmargin:0;\r\n\tpadding:0;\r\n\tfont-family: Consolas;\r\n}\r\nspan.opcode\r\n{\r\n\tcolor: #0000FF;\r\n}\r\nspan.ident\r\n{\r\n\tcolor: #008080;\r\n}\r\nspan.string\r\n{\r\n\tcolor: #DC1414;\r\n}\r\na:link, a:visited\r\n{\r\n\ttext-decoration:none;\r\n\tfont-weight:bold;\r\n}\r\na:hover\r\n{\r\n\ttext-decoration:underline;\r\n\tfont-weight:bold;\r\n}\r\n</style>    \r\n  </head>\r\n  <body>\r\n    <pre>" + builder.ToString() + "</pre>\r\n  </body>\r\n</html>";
            AppDomain.CurrentDomain.SetData("result", str4);
        }

        internal static string DisassembleQuery(string assemblyPath, string[] additionalReferences)
        {
            if (string.IsNullOrEmpty(assemblyPath) || (additionalReferences == null))
            {
                return "";
            }
            using (DomainIsolator isolator = new DomainIsolator("Disassembler"))
            {
                string data;
                isolator.Domain.SetData("path", assemblyPath);
                isolator.Domain.SetData("refs", additionalReferences);
                try
                {
                    isolator.Domain.DoCallBack(new CrossAppDomainDelegate(Disassembler.DisassembleQuery));
                    data = (string) isolator.Domain.GetData("result");
                }
                catch (FileNotFoundException exception)
                {
                    data = exception.Message;
                }
                return (data ?? "");
            }
        }

        private string FormatOperand(OpCode c, int operandLength)
        {
            if (operandLength == 0)
            {
                return "";
            }
            if (operandLength == 4)
            {
                return this.Get4ByteOperand(c);
            }
            if (c.OperandType == OperandType.ShortInlineBrTarget)
            {
                return this.GetShortRelativeTarget();
            }
            if (c.OperandType == OperandType.InlineSwitch)
            {
                return this.GetSwitchTarget(operandLength);
            }
            return null;
        }

        private string Get4ByteOperand(OpCode c)
        {
            int metadataToken = BitConverter.ToInt32(this._il, this._pos);
            switch (c.OperandType)
            {
                case OperandType.InlineBrTarget:
                {
                    int num2 = (this._pos + metadataToken) + 4;
                    return ("IL_" + num2.ToString("X4"));
                }
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                {
                    MemberInfo info;
                    try
                    {
                        info = this._module.ResolveMember(metadataToken);
                    }
                    catch
                    {
                        return null;
                    }
                    if (info == null)
                    {
                        return null;
                    }
                    string s = (info.ReflectedType != null) ? (GetFullName(info.ReflectedType) + "." + info.Name) : ((info is Type) ? GetFullName((Type) info) : info.Name);
                    return this._styler.StyleIdentifier(s);
                }
                case OperandType.InlineString:
                {
                    string str3 = this._module.ResolveString(metadataToken);
                    if (str3 != null)
                    {
                        str3 = this._styler.StyleString("\"" + str3 + "\"");
                    }
                    return str3;
                }
            }
            return null;
        }

        private static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.FullName;
            }
            string fullName = t.GetGenericTypeDefinition().FullName;
            string str3 = "";
            if (fullName.Contains<char>('+'))
            {
                str3 = fullName.Substring(fullName.IndexOf('+'));
                fullName = fullName.Substring(0, fullName.IndexOf('+'));
            }
            string[] strArray = new string[] { fullName.Split(new char[] { '`' }).First<string>(), "<", string.Join(",", (from ta in t.GetGenericArguments() select GetFullName(ta)).ToArray<string>()), ">", str3 };
            return string.Concat(strArray);
        }

        private string GetShortRelativeTarget()
        {
            int num = (this._pos + ((sbyte) this._il[this._pos])) + 1;
            return ("IL_" + num.ToString("X4"));
        }

        private string GetSwitchTarget(int operandLength)
        {
            int num = BitConverter.ToInt32(this._il, this._pos);
            string[] strArray = new string[num];
            for (int i = 0; i < num; i++)
            {
                int num3 = BitConverter.ToInt32(this._il, this._pos + ((i + 1) * 4));
                strArray[i] = "IL_" + (((this._pos + num3) + operandLength)).ToString("X4");
            }
            return ("(" + string.Join(", ", strArray) + ")");
        }

        private OpCode ReadOpCode()
        {
            byte key = this._il[this._pos++];
            if (_opcodes.ContainsKey(key))
            {
                return _opcodes[key];
            }
            if (this._pos == this._il.Length)
            {
                throw new Exception("Cannot find opcode " + key);
            }
            short num3 = (short) ((key * 0x100) + this._il[this._pos++]);
            if (!_opcodes.ContainsKey(num3))
            {
                throw new Exception("Cannot find opcode " + num3);
            }
            return _opcodes[num3];
        }

        private string ReadOperand(OpCode c)
        {
            int operandLength = (c.OperandType == OperandType.InlineNone) ? 0 : ((((c.OperandType == OperandType.ShortInlineBrTarget) || (c.OperandType == OperandType.ShortInlineI)) || (c.OperandType == OperandType.ShortInlineVar)) ? 1 : ((c.OperandType == OperandType.InlineVar) ? 2 : (((c.OperandType == OperandType.InlineI8) || (c.OperandType == OperandType.InlineR)) ? 8 : ((c.OperandType == OperandType.InlineSwitch) ? (4 * (BitConverter.ToInt32(this._il, this._pos) + 1)) : 4))));
            if ((this._pos + operandLength) > this._il.Length)
            {
                throw new Exception("Unexpected end of IL");
            }
            string str = this.FormatOperand(c, operandLength);
            if (str == null)
            {
                str = "";
                for (int i = 0; i < operandLength; i++)
                {
                    str = str + this._il[this._pos + i].ToString("X2") + " ";
                }
            }
            this._pos += operandLength;
            return str;
        }
    }
}

