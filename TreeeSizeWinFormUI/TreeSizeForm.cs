using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TreeSize;

namespace TreeeSizeWinFormUI
{
    public partial class TreeSizeForm : Form
    {
        List<string> Drives = new List<string>();
        string selectedDrive;

        public TreeSizeForm()
        {
            InitializeComponent();
            FillDrivesList();
        }

        private void FillDrivesList()
        {
            foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
            {
                if (driveInfo.IsReady)
                {
                    Drives.Add(driveInfo.Name);
                }
            }
            this.DiskListDropDown.SelectedIndexChanged -= new EventHandler(DiskListDropDown_SelectedIndexChanged);
            DiskListDropDown.DataSource = Drives;
            this.DiskListDropDown.SelectedIndexChanged += new EventHandler(DiskListDropDown_SelectedIndexChanged);
            DiskListDropDown.SelectedItem = null;
        }

        private void PopulateTreeView2(string drive)
        {
            TreeNode rootNode;
            Folder folder = new Folder(drive);
            if (folder != null)
            {
                rootNode = new TreeNode(folder.Path);
                rootNode.Tag = folder;
                GetDirectories(folder.SubFolders.ToList(), rootNode);
                treeView1.Nodes.Add(rootNode);
                AddFilesNode(rootNode, folder);
            }
        }

        private void AddFilesNode(TreeNode rootNode, Folder folder)
        {
            if (folder.FilesVirtualSubFolder != null)
            {
                TreeNode filesNode = rootNode.Nodes.Add("[" + folder?.FilesVirtualSubFolder?.Files.Count + " files in " + folder.Path + "]");
                filesNode.Tag = folder.FilesVirtualSubFolder;
                filesNode.ImageIndex = 100;
                filesNode.SelectedImageIndex = 100;

                LoadFiles(folder?.FilesVirtualSubFolder, filesNode);
            }
            else
            {
                LoadFiles(folder, rootNode);
            }
        }

        private void LoadFiles(Folder folder, TreeNode tds)
        {
            List<FileItem> Files = null;
            if (folder?.Files != null)
            {
                Files = folder.Files.ToList();
            }

            if (Files != null)
                foreach (var file in Files)
                {
                    TreeNode tdss = tds.Nodes.Add(file.Name);
                    tdss.Tag = file;
                    tdss.ImageKey = "Document-icon.png";
                    tdss.SelectedImageKey = "Document-icon.png";
                }
        }

        private void GetDirectories(List<Folder> subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            List<Folder> subSubDirs;
            foreach (Folder subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.SubFolders.ToList();
                if (subSubDirs.Count != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                AddFilesNode(aNode, subDir);
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Folder nodeDirInfo = null;
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            if (newSelected.Tag.GetType() == typeof(Folder))
            {
                nodeDirInfo = (Folder)newSelected.Tag;
            }
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;
            if (nodeDirInfo != null)
            {
                item = new ListViewItem("[Total folder length]", 200);
                subItems = new ListViewItem.ListViewSubItem[]
                    {
                            new ListViewItem.ListViewSubItem(item, ""),
                            new ListViewItem.ListViewSubItem(item, nodeDirInfo.Size)
                    };
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
                foreach (Folder dir in nodeDirInfo.SubFolders)
                {
                    item = new ListViewItem(dir.Name, 0);
                    subItems = new ListViewItem.ListViewSubItem[]
                        {
                            new ListViewItem.ListViewSubItem(item, "Directory"),
                            new ListViewItem.ListViewSubItem(item, dir.Size)
                        };
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
                if (nodeDirInfo.FilesVirtualSubFolder == null)
                    foreach (FileItem file in nodeDirInfo.Files)
                    {
                        item = new ListViewItem(file.Name, 1);
                        subItems = new ListViewItem.ListViewSubItem[]
                            {
                                new ListViewItem.ListViewSubItem(item, "File"),
                                new ListViewItem.ListViewSubItem(item, file.Size)
                            };
                        item.SubItems.AddRange(subItems);
                        listView1.Items.Add(item);
                    }
                if (nodeDirInfo.FilesVirtualSubFolder != null)
                {
                    Folder fileSubFoler = nodeDirInfo.FilesVirtualSubFolder;
                    item = new ListViewItem("[" + nodeDirInfo.FilesVirtualSubFolder?.Files.Count + " files in " + nodeDirInfo.Path + "]", 200);
                    subItems = new ListViewItem.ListViewSubItem[]
                        {
                            new ListViewItem.ListViewSubItem(item, "Directory"),
                            new ListViewItem.ListViewSubItem(item, fileSubFoler.Size)
                        };
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
            }
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        public static void SetTreeViewTheme(IntPtr treeHandle)
        {
            SetWindowTheme(treeHandle, "explorer", null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetTreeViewTheme(treeView1.Handle);
        }

        private void DiskListDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DiskListDropDown.SelectedItem != null)
            {
                selectedDrive = DiskListDropDown.SelectedItem.ToString();
                PopulateTreeView2(selectedDrive);
            }
        }
    }
}

