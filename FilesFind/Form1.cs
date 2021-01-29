﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FilesFind
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            lstFiles.View = View.Details;//只有设置为这个HeaderStyle才有用
            lstFiles.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lstFiles.GridLines = true;//显示网格线
            lstFiles.FullRowSelect = true;//选择时选择是行，而不是某列 
            lstFiles.Columns.Add("文件名称", 500, HorizontalAlignment.Left);//文本左 
        }

        private void btnSelPathForWatch_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fld = new FolderBrowserDialog();//c#实现的类，代码关键
            fld.ShowDialog();
            txtFilesPath.Text = fld.SelectedPath.Trim();

        }

        private void btnToSetting_Click(object sender, EventArgs e)
        {

            //string ddd = "E:\\PanGu_Release_V2.3.1.0\\新加词.txt";
            //string ddd2 = "E:\\PanGu_Release_V2.3.1.0\\新加词2.txt";

            //string s =  FObject.ReadFile(ddd);

            //string[] lst = GetArrByWrap(s);

            //foreach (string s1 in lst)
            //{
            //    string newtitle = s1;

            //    if (s1.IndexOf('（') > -1)
            //    {
            //        string[] as1 = s1.Split('（');
            //        newtitle = as1[0];
                      
            //    }

            //    if (newtitle.IndexOf('《') > -1)
            //    {
            //        string[] as1 = newtitle.Split('《');
            //        newtitle = as1[0];

            //    }
            //    if (newtitle.IndexOf('(') > -1)
            //    {
            //        string[] as1 = newtitle.Split('(');
            //        newtitle = as1[0];
                      
            //    }
            //    if (newtitle.IndexOf('】') > -1)
            //    {
            //        string[] as1 = newtitle.Split('】');
            //        newtitle = as1[0];
                      
            //    }
            //    if (newtitle.IndexOf('［') > -1)
            //    {
            //        string[] as1 = newtitle.Split('［');
            //        newtitle = as1[0];

            //    }
            //    if (newtitle.IndexOf('[') > -1)
            //    {
            //        string[] as1 = newtitle.Split('[');
            //        newtitle = as1[0];

            //    }

            //    if (newtitle.IndexOf('﹝') > -1)
            //    {
            //        string[] as1 = newtitle.Split('﹝');
            //        newtitle = as1[0];

            //    }
            //    if (newtitle.IndexOf('-') > -1)
            //    {
            //        string[] as1 = newtitle.Split('-');
            //        newtitle = as1[0];

            //    }
            //    if (newtitle.IndexOf('【') > -1)
            //    {
            //        string[] as1 = newtitle.Split('【');
            //        newtitle = as1[0];

            //    }
            //    if (newtitle.IndexOf('》') > -1)
            //    {
            //        string[] as1 = newtitle.Split('》');
            //        newtitle = as1[0];

            //    }
            //    if (newtitle.IndexOf('《') > -1)
            //    {
            //        string[] as1 = newtitle.Split('《');
            //        newtitle = as1[0];

            //    }

            //    if (newtitle.IndexOf('（') > -1)
            //    {
            //        string[] as1 = newtitle.Split('（');
            //        newtitle = as1[0];

            //    }

            //    //else
            //    //{
            //    //    newtitle = s1;
                    
            //    //}

            //    FObject.WriteFile(ddd2, string.Concat(newtitle, "\r\n"), true);
            //}

            //MessageBox.Show(lst.Length.ToString());

            ToSearch();
        }

        public static string[] GetArrByWrap(string sContent)
        {
            Regex re = new Regex("\r\n");
            string[] aItems = re.Split(sContent);

            return aItems;
        }

        private string[] aScanFileType;
        Thread search_thread = null;
        private delegate void DelegateShowInfo(string info);//用来在扫描过程中回调
        private delegate void DelegateBtnEnable(bool isenable, int itype);//用来在扫描过程中回调
        private delegate void DelegateListViewItemUpdate(ListViewItem lvi, Color cl, string Text);//查找木马后回调更新



        private DelegateShowInfo dlgShowScanInfo;
        private DelegateShowInfo dlgAddScanFileToList;
        private DelegateBtnEnable dlgIsEnableBtn;
        private DelegateListViewItemUpdate dlgListViewItemUpdate;
        public void ToSearch()
        {
           
            string sScanFileType = txtFileType.Text.Trim();
            if (!string.IsNullOrEmpty(sScanFileType))
            {
                aScanFileType = sScanFileType.Split(',');
            }
            else
            {
                MessageBox.Show("请设置扫描的文件类型！如 .aspx,.html");
            }

            lstFiles.Items.Clear();
            dlgShowScanInfo = ShowScanInfo;
            dlgAddScanFileToList = AddScanFileToList;
            //dlgIsEnableBtn = IsEnableBtn;
            dlgListViewItemUpdate = ListViewItemUpdate;


            if (search_thread == null)
                search_thread = new Thread(new ThreadStart(startsearch));

            if (search_thread.ThreadState == ThreadState.Stopped)
            {
                search_thread = null;
                search_thread = new Thread(new ThreadStart(startsearch));
            }


            if (!search_thread.IsAlive)
            {
                search_thread.Start();
                 
            }
            
            //if (!search_thread.IsAlive)
            //{
            //    search_thread.Start();

            //    btnPause.Enabled = true;
            //    btnStop.Enabled = true;
            //}
        }

        private void AddScanFileToList(string filepath)
        {
            string sContent = FObject.ReadFile(filepath);
            FObject.WriteFile(filepath, string.Concat("---start---\r\n\r\n", sContent));
            //FObject.WriteFile(filepath, "\r\n\r\n---end---", true);

            ListViewItem lvi = new ListViewItem();
            lvi.SubItems[0].Text = filepath;
            lvi.SubItems.Add("未替换");
            lstFiles.Items.Add(lvi);

            //string sContent = FObject.ReadFile(filepath);
             

        }
        private void startsearch()
        {
            string FilePath = txtFilesPath.Text.Trim();
            ScanFiles(FilePath);
            lbFindingInfo.Invoke(dlgShowScanInfo, string.Format("扫描完毕！共扫描文件{0}个", lstFiles.Items.Count));
            //btnPause.Invoke(dlgIsEnableBtn, false, 0);
            //btnStop.Invoke(dlgIsEnableBtn, false, 1);

        }
        private void ScanFiles(string filepath)
        {
            if (filepath.Trim().Length > 0)
            {

                string[] filecollect = null;
                try
                {
                    lbFindingInfo.Invoke(dlgShowScanInfo, string.Concat("正在计算列表:", filepath));
                    filecollect = Directory.GetFileSystemEntries(filepath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("出错了！" + ex.Message);
                    ex.ToString();
                }

                if (!Equals(filecollect, null))
                {
                    foreach (string file in filecollect)
                    {

                        lbFindingInfo.Invoke(dlgShowScanInfo, file);

                        if (Directory.Exists(file))
                        {
                            ScanFiles(file);
                        }
                        else
                        {
                            foreach (string file_extend in aScanFileType)
                            {
                                if (file.EndsWith(file_extend))
                                {
                                    lstFiles.Invoke(dlgAddScanFileToList, file);
                                }
                            }

                        }
                    }
                }


            }
        }
        private void ShowScanInfo(string filepath)
        {
            lbFindingInfo.Text = filepath;
        }
         
        private void ListViewItemUpdate(ListViewItem lvi, Color cl, string Text)
        {
            lvi.SubItems[1].Text = Text;
            lvi.ForeColor = cl;
        }

         
 
    }
}
