using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TreeSize;

namespace TreeSizeWinFormUI
{
    public partial class TreeSizeForm : Form
    {
        private List<string> drives = new List<string>();
        private string selectedDrive;

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
                    drives.Add(driveInfo.Name);
                }
            }
            this.DiskListDropDown.SelectedIndexChanged -= new EventHandler(DiskListDropDown_SelectedIndexChanged);
            DiskListDropDown.DataSource = drives;
            this.DiskListDropDown.SelectedIndexChanged += new EventHandler(DiskListDropDown_SelectedIndexChanged);
            DiskListDropDown.SelectedItem = null;
        }

        private void PopulateTreeView(string drive)
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode;
            FolderItem folder = null;
            folder = new FolderItem(drive);
            WriteLogs(folder.log);
            if (folder != null)
            {
                rootNode = new TreeNode(folder.Path);
                rootNode.Tag = folder;
                GetDirectories(folder.SubFolders.ToList(), rootNode);
                treeView1.Nodes.Add(rootNode);
                AddFilesNode(rootNode, folder);
            }
        }

        private void WriteLogs(StringCollection log)
        {
            logMessagesTextBox.Text = "";
            foreach (string item in log)
            {
                logMessagesTextBox.Text += item.ToString() + "\r\n";
            }
        }

        private void AddFilesNode(TreeNode rootNode, FolderItem folder)
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

        private void LoadFiles(FolderItem folder, TreeNode tds)
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

        private void GetDirectories(List<FolderItem> subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            List<FolderItem> subSubDirs;
            foreach (FolderItem subDir in subDirs)
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
            FolderItem nodeDirInfo = null;
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            if (newSelected.Tag.GetType() == typeof(FolderItem))
            {
                nodeDirInfo = (FolderItem)newSelected.Tag;
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
                foreach (FolderItem dir in nodeDirInfo.SubFolders)
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
                    FolderItem fileSubFoler = nodeDirInfo.FilesVirtualSubFolder;
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

        private void TreeSizeForm_Load(object sender, EventArgs e)
        {
            SetTreeViewTheme(treeView1.Handle);
        }

        private void DiskListDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (DiskListDropDown.SelectedItem != null)
            {
                selectedDrive = DiskListDropDown.SelectedItem.ToString();
                PopulateTreeView(selectedDrive);
            }
        }
    }
}

