using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLMeger
{
    class Program
    {
        static void Main(string[] args)
        {
            test();
        }
        private static void test()
        {
            var sqlString = "select *      from PrintTask as p".ToLower();
            var sqlArray = sqlString.Split(' ').Where(c => c != "" && c != "as").ToArray();
            for (int i = 0; i < sqlArray.Length; i++)
            {
                var syntax = sqlArray[i];

                switch (syntax)
                {
                    case "from":
                    case "join":
                    case "innerjoin":
                    case "leftjoin":
                        if (sqlArray[i + 2] != null && sqlArray[i + 2].ToLower() == "as")
                        {
                            sqlArray[i + 3] = sqlArray[i + 3] + " with(nolock)";
                        }
                        else
                        {
                            sqlArray[i + 1] = sqlArray[i + 1] + " with(nolock)";
                        }
                        break;
                }
            }
            sqlString = string.Join(" ", sqlArray);
            Console.WriteLine(sqlString);
        }

        public static void MakeAssembly(AssemblyName myAssemblyName, string fileName)
        {
            // Get the assembly builder from the application domain associated with the current thread.
            AssemblyBuilder myAssemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.RunAndSave);
            // Create a dynamic module in the assembly.
            ModuleBuilder myModuleBuilder = myAssemblyBuilder.DefineDynamicModule("MyModule", fileName);
            // Create a type in the module.
            TypeBuilder myTypeBuilder = myModuleBuilder.DefineType("MyType");
            // Create a method called 'Main'.
            MethodBuilder myMethodBuilder = myTypeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.HideBySig |
               MethodAttributes.Static, typeof(void), null);
            // Get the Intermediate Language generator for the method.
            ILGenerator myILGenerator = myMethodBuilder.GetILGenerator();
            // Use the utility method to generate the IL instructions that print a string to the console.
            myILGenerator.EmitWriteLine("Hello World!");
            // Generate the 'ret' IL instruction.
            myILGenerator.Emit(OpCodes.Ret);
            // End the creation of the type.
            myTypeBuilder.CreateType();
            // Set the method with name 'Main' as the entry point in the assembly.
            myAssemblyBuilder.SetEntryPoint(myMethodBuilder);
            myAssemblyBuilder.Save(fileName);
        }
        private void XmlMeger()
        {
            XDocument aDoc = XDocument.Load("1.xml");
            XDocument bDoc = XDocument.Load("2.xml");

            var menus = aDoc.Descendants("Menu").Concat(bDoc.Descendants("Menu")).Distinct(new XElementCopmare());
            var menuCategorys = aDoc.Descendants("MenuCategory").Concat(bDoc.Descendants("MenuCategory")).Distinct(new XElementCopmare());
            var components = aDoc.Descendants("Component").Concat(bDoc.Descendants("Component")).Distinct(new XElementCopmare());

            XDocument nDoc = XDocument.Load(new StringReader(@"
                                <DomasMenu xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
                                    <Components ID='bd8a2e03-c2ec-4af9-ad99-fc8b05509ce6' >
                                    </Components>
                                </DomasMenu>
                            "));
            foreach (var comp in components)
            {
                var xName = comp.Attribute("Name").Value;
                var nE = new XElement("Component", new XAttribute("ID", comp.Attribute("ID").Value), new XAttribute("Name", xName));
                nE.Add(new XElement("MenuCategorys"));
                nDoc.Root.Element("Components").Add(nE);
                var nem = nE.Element("MenuCategorys");
                foreach (var mc in menuCategorys)
                {
                    if (mc.Parent.Parent.Attribute("Name").Value == xName)
                    {
                        var xmName = mc.Attribute("Name").Value;
                        var nMc = new XElement("MenuCategory", new XAttribute("ID", mc.Attribute("ID").Value), new XAttribute("Name", xmName));
                        nMc.Add(new XElement("Menus"));
                        nem.Add(nMc);
                        var nMn = nMc.Element("Menus");
                        foreach (var mn in menus)
                        {
                            if (mn.Parent.Parent.Attribute("Name").Value == xmName)
                            {
                                var xmnName = mn.Attribute("Name").Value;
                                var nMenu = new XElement("Menu", new XAttribute("ID", mn.Attribute("ID").Value), new XAttribute("Name", xmnName));
                                nMn.Add(nMenu);
                            }
                        }
                    }
                }
            }
        }
    }

    public class XElementCopmare : IEqualityComparer<XElement>
    {
        public bool Equals(XElement x, XElement y)
        {
            return x.Attribute("Name").Value == y.Attribute("Name").Value;
        }

        public int GetHashCode(XElement obj)
        {
            return obj.Attribute("Name").Value.GetHashCode();
        }
    }
}
