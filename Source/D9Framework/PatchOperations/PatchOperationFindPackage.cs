﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Xml;

namespace D9Framework
{
    class PatchOperationFindPackage : PatchOperation
    {
# pragma warning disable CS0649
        private List<string> packageIds;
        private PatchOperation match, nomatch;
#pragma warning restore CS0649

        protected override bool ApplyWorker(XmlDocument xml)
        {
            bool found = false;
            foreach(string id in packageIds)
            {
                if (ModLister.GetActiveModWithIdentifier(id) != null || ModLister.GetExpansionWithIdentifier(id) != null)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                if (match != null) return match.Apply(xml);
            }                
            else
            {
                if (nomatch != null) return nomatch.Apply(xml);
            }                
            return true;
        }

        public override string ToString()
        {
            return $"{base.ToString()}({packageIds.ToCommaList(false)})";
        }
    }
}
