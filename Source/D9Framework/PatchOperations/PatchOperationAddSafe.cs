﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Verse;

namespace D9Framework
{
    /// <summary>
    /// Works exactly like PatchOperationAdd, except that if the parent node doesn't exist this operation creates it.
    /// </summary>
    class PatchOperationAddSafe : PatchOperationPathed
    {
        private enum Order { Append, Prepend }
        private XmlContainer value;
        private Order parentOrder = Order.Append, childOrder = Order.Append;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            bool result = false;
            XmlNode node = value.node;
            // select nodes with xpath
            foreach(object item in xml.SelectNodes(base.xpath))
            {
                result = true;
                XmlNode cur = item as XmlNode;
                // for each parent:
                if (parentOrder == Order.Append)
                {
                    foreach (XmlNode parent in node.ChildNodes) ConditionallyApplyParent(cur, parent);
                }
                else if(parentOrder == Order.Prepend)
                {
                    for (int i = cur.ChildNodes.Count - 1; i >= 0; i--) ConditionallyApplyParent(cur, node.ChildNodes[i]);
                }                
            }
            return result;
        }

        private void ConditionallyApplyParent(XmlNode target, XmlNode parent)
        {
            bool identicalNodeExists = false;
            foreach(XmlNode targetChild in target.ChildNodes)
            {
                // check for identical node
                if (XmlNodesEqual(targetChild, parent))
                {
                    identicalNodeExists = true;
                    // append children to existing node
                }        
            }
            if (!identicalNodeExists)
            {
                // create node
                // append children to parent
                foreach (XmlNode child in parent.ChildNodes) AppendOrPrependNode(target, child, childOrder);
            }
        }

        private static bool XmlNodesEqual(XmlNode a, XmlNode b)
        {
            return true;
        }

        private void AppendOrPrependNode(XmlNode target, XmlNode node, Order order)
        {
            if(order == Order.Append)
            {
                foreach (XmlNode childNode in node.ChildNodes) target.AppendChild(target.OwnerDocument.ImportNode(childNode, true));
            }
            else if(order == Order.Prepend)
            {
                for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
                {
                    target.PrependChild(target.OwnerDocument.ImportNode(node.ChildNodes[num], true));
                }
            }
        }
    }
}