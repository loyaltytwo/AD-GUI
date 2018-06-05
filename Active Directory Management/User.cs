﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Active_Directory_Management
{
    class User
    {
        private XElement xmlNode;
        private DirectoryEntry entry;

        public string Username { get; set; } = "bazhr1";
        public string Password { get; set; } = "1234567Br";

        private XDocument xmlFile = new XDocument(Active_Directory_Management.Properties.Resources.usersXML);

        public Dictionary<string, string> Properties { get; set; }

        private string memberOf;
        public string Dn { get; }
        
        public User(XElement userXmlNode)
        {
            Dn = xmlNode.Attribute("dn").Value;

            xmlNode = userXmlNode;
            entry = new DirectoryEntry("LDAP://" + Dn, Username, Password);

            UpdateProperties();
        }

        public User(DirectoryEntry userEntry)
        {
            Dn = entry.Properties["distinguishedName"].Value.ToString();

            entry = userEntry;
            xmlNode = xmlFile.Root.Descendants("user")
                .Where(t => t.Attribute("dn").Value == Dn)
                .FirstOrDefault();

            UpdateProperties();
        }

        public User(XElement userXmlNode, DirectoryEntry userEntry)
        {
            Dn = entry.Properties["distinguishedName"].Value.ToString();

            entry = userEntry;
            xmlNode = userXmlNode;

            UpdateProperties();
        }

        private void UpdateProperties()
        {
            foreach (XElement elem in xmlNode.Elements())
            {
                if (elem.Name == "memberOf")
                    memberOf = elem.Value;
                else
                    Properties.Add(elem.Name.ToString(), elem.Value.ToString());
            }
        }

        public bool Enabled
        {
            get
            {
                return !Convert.ToBoolean(int.Parse(Properties["userAccountControl"]) & 0x2);
            }
            set
            {
                if(value == true)
                {
                    string newUserAccountControl = (int.Parse(Properties["userAccountControl"]) | 0x2).ToString();
                    
                    // Update local variable
                    Properties["userAccountControl"] = newUserAccountControl;
                }
                else
                {
                    string newUserAccountControl = (int.Parse(Properties["userAccountControl"]) & ~0x2).ToString();

                    // Update local variable
                    Properties["userAccountControl"] = newUserAccountControl;
                }
            }
        }

        public bool MemberOf(string groupDistinguishedName)
        {
            return Properties["memberof"].Contains(groupDistinguishedName);
        }
        
        public void AddGroup(string groupDistinguishedName)
        {
            if (!MemberOf(groupDistinguishedName))
            {
                DirectoryEntry groupEntry = new DirectoryEntry("LDAP://" + groupDistinguishedName);

                // Update property in object
                Properties["memberOf"] += groupDistinguishedName;

                // Update property in XML file
                xmlNode.Element("memberOf").Value += groupDistinguishedName;

                // Update Active Directory
                groupEntry.Properties["member"].Add(Dn);
                // TODO Commit Changes

                groupEntry.Close();
            }
        }

        public void RemoveGroup(string groupDistinguishedName)
        {
            if (MemberOf(groupDistinguishedName))
            {
                DirectoryEntry groupEntry = new DirectoryEntry("LDAP://" + groupDistinguishedName);

                string newMemberOf = Properties["memberOf"].Remove(
                    Properties["memberOf"].IndexOf(groupDistinguishedName),
                    groupDistinguishedName.Length);

                // Update property in object
                Properties["memberOf"] = newMemberOf;

                // Update property in XML file
                xmlNode.Element("memberOf").Value = newMemberOf;

                // Update Active Directory
                groupEntry.Properties["member"].Remove(Dn);
                // TODO Commit Changes

                groupEntry.Close();
            }
        }

        public void SetMembership(string groupDistinguishedName, bool state)
        {
            if (state == true)
                AddGroup(groupDistinguishedName);
            else
                RemoveGroup(groupDistinguishedName);
        }

        public void CommitChanges()
        {
            foreach(KeyValuePair<string, string> prop in Properties)
            {
                xmlNode.Element(prop.Key).Value = prop.Value;

                entry.Properties[prop.Key].Value = prop.Value;
                entry.CommitChanges();
            }
        }
    }
}