using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Model {
    public class MpCopyItemCategory {
        public static List<Color> CopyItemCategoryColorList = new List<Color>() { Color.Red,Color.Blue,Color.Green,Color.Orange,Color.Pink };

        public static int CopyItemCategoryCount = 0;
        public int copyItemCategoryId = 0;

        public string copyItemCategoryName;
        public Color copyItemCategoryColor = Color.White;

        public List<int> copyItemIdList = new List<int>();

        public MpCopyItemCategory(string name,Color color) {
            copyItemCategoryId = ++CopyItemCategoryCount;
            copyItemCategoryName = name;
            copyItemCategoryColor = color;

        }
        public MpCopyItemCategory(string name) : this(name,CopyItemCategoryColorList[new Random().Next(0,MpCopyItemCategory.CopyItemCategoryColorList.Count)]) { }

    }
}
