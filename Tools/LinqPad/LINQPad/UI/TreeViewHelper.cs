namespace LINQPad.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal static class TreeViewHelper
    {
        public static TreeNode GetNextLeafNode(TreeNode n)
        {
            if (n == null)
            {
                return null;
            }
            if (n.FirstNode == null)
            {
                if (n.NextNode == null)
                {
                    while (n.NextNode == null)
                    {
                        n = n.Parent;
                        if (n == null)
                        {
                            return null;
                        }
                    }
                    n = n.NextNode;
                }
                else
                {
                    return n.NextNode;
                }
            }
            while (n.FirstNode != null)
            {
                n.Expand();
                n = n.FirstNode;
            }
            return n;
        }

        public static TreeNode GetPreviousLeafNode(TreeNode n)
        {
            if (n != null)
            {
                while (n.PrevNode == null)
                {
                    n = n.Parent;
                    if (n == null)
                    {
                        return null;
                    }
                }
                n = n.PrevNode;
                while (n.LastNode != null)
                {
                    n.Expand();
                    n = n.LastNode;
                }
                return n;
            }
            return null;
        }

        public static void MoveToNextLeafNode(TreeView tv)
        {
            TreeNode nextLeafNode = GetNextLeafNode(tv.SelectedNode);
            if (nextLeafNode != null)
            {
                nextLeafNode.EnsureVisible();
                tv.SelectedNode = nextLeafNode;
            }
        }

        public static void MoveToPreviousLeafNode(TreeView tv)
        {
            TreeNode previousLeafNode = GetPreviousLeafNode(tv.SelectedNode);
            if (previousLeafNode != null)
            {
                previousLeafNode.EnsureVisible();
                tv.SelectedNode = previousLeafNode;
            }
        }

        public static ImageList UpscaleImages(Font f, ImageList il)
        {
            try
            {
                if (f.Height < 0x18)
                {
                    return il;
                }
                ImageList list2 = new ImageList();
                int height = f.Height;
                list2.ImageSize = new Size(height, height);
                int num2 = 0;
                foreach (Image image in il.Images)
                {
                    Image image2 = ControlUtil.ResizeImage(image, height, height, true);
                    list2.Images.Add(il.Images.Keys[num2++], image2);
                }
                return list2;
            }
            catch
            {
                return il;
            }
        }
    }
}

