using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FshareAPI
{
    public class FshareFileOrFolderInfos
    {
        private List<FshareFileOrFolderInfo> ColFhareFileOrFolderInfos = new List<FshareFileOrFolderInfo>();
        public void Add(FshareFileOrFolderInfo Item)
        {
            ColFhareFileOrFolderInfos.Add(Item);
        }
        public void Remove(FshareFileOrFolderInfo Item)
        {
            ColFhareFileOrFolderInfos.Remove(Item);
        }
        public int Count()
        {
            return ColFhareFileOrFolderInfos.Count;
        }
        public FshareFileOrFolderInfo Item(int Index)
        {
            return ColFhareFileOrFolderInfos[Index];
        }
    }
}
