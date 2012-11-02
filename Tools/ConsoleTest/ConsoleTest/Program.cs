using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
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
                        foreach(var mn in menus)
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
    public class XElementCopmare:IEqualityComparer<XElement>
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
