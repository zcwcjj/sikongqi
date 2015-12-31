﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OPCAutomation;
using System.Threading;

namespace sikongqi
{
    enum ResultOfExp { Sucess,Failure};
    
    public partial class Form1 : Form
    {
        const string opcname = "KEPware.KEPServerEx.V4";
        const string VB30ITEM = "sikongqi1.plc200.vb30";
        const string VB80ITEM = "sikongqi1.plc200.vb80";
        const string I04_I15 = "sikongqi1.plc200.input";
        const string START = "sikongqi1.plc200.start";
        const string RESET = "sikongqi1.plc200.reset";
        const string PAUSE = "sikongqi1.plc200.pause";
        const int ENDCIRCLE = 19;
        const uint[] CHECKSTATUS = new uint[ENDCIRCLE] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
        int vb30;
        int vb80;
        int start;
        int reset;
        int pause;
        int status = 0;
        


        OPCServer server;
        /// <summary>
        /// VB30或VB80改变
        /// </summary>
        OPCGroup statusGroup;
        /// <summary>
        /// 查看结果
        /// </summary>
        OPCGroup resultGroup;
        /// <summary>
        /// 控制命令
        /// </summary>
        OPCGroup CommandGroup;

        tttEntities dbcontex;
        sikongqi exp;

        BackgroundWorker backgroundworker1 = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();
            init();
           // ParameterizedThreadStart x =new ParameterizedThreadStart(f);
            
            
            
        }
        void f(object a){}
       

        void init()
        {
            server = new OPCServer();
            server.Connect(opcname);
            statusGroup= server.OPCGroups.Add();
            statusGroup.OPCItems.AddItem(VB30ITEM, 1);
            statusGroup.OPCItems.AddItem(VB80ITEM, 2);


            resultGroup = server.OPCGroups.Add();
            resultGroup.OPCItems.AddItem(I04_I15,1);

            CommandGroup = server.OPCGroups.Add();
            CommandGroup.OPCItems.AddItem(START,1);
            CommandGroup.OPCItems.AddItem(PAUSE, 2);
            CommandGroup.OPCItems.AddItem(RESET, 3);

            statusGroup.DataChange += statusGroup_DataChange;
            //resultGroup.DataChange += resultGroup_DataChange;
            CommandGroup.DataChange += CommandGroup_DataChange;
            
            #region 
         
            statusGroup.IsSubscribed = true;
            //resultGroup.IsSubscribed = true;
            CommandGroup.IsSubscribed = true;
            resultGroup.IsActive = true;
            statusGroup.IsActive = true;
            CommandGroup.IsActive = true;
            


            resultGroup.UpdateRate = 10;
            statusGroup.UpdateRate = 10;
            CommandGroup.UpdateRate = 10;
            #endregion

            toolStripProgressBar1.Step = 1;
            toolStripProgressBar1.Maximum = 19;
            


        }


        /// <summary>
        /// 处理开始，暂停，即停等命令
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        void CommandGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            //关闭检测
          //  CommandGroup.IsSubscribed = false;
           // if (ClientHandles.Length > 1) return;

            for (int i = 1; i < ClientHandles.Length + 1; i++)
            {
                object index1 = ClientHandles.GetValue(i);
                object index2 = ItemValues.GetValue(i);
                
                
                
                if (((bool) index2))
                {
                    switch (Int32.Parse(index1.ToString()))
                    {
                        ///start pushed.
                        case 1:
                            {
                                
                                setCommandStates(1, 0, 0);
                                toolStripLabelCommand.Text = "实验进行中";
                                
                                exp = new sikongqi();
                                exp.startTime = DateTime.Now;
                                textBoxStartTime.Text = exp.startTime.ToString();
                                exp.planNumber = 100;




                                break;
                            }
                            // 暂停处理逻辑
                        case 2:
                            {
                                setCommandStates(0, 0, 1);
                                toolStripLabelCommand.Text = "实验暂停";
                                break;
                            }

                            //复位处理逻辑
                        case 3:
                            {
                                setCommandStates(0, 1, 0);
                                toolStripLabelCommand.Text = "实验重置";
                                break;
                            }
                    }

                }
            }

            //打开订阅
          //  CommandGroup.IsSubscribed = true;
            
        }

        //设置开始暂停重置状态
        void setCommandStates(int Pstart, int Preset, int Ppause)
        {
            int flag = 0;
            if (Pstart == 1)
                flag++;
            if (Preset == 1)
                flag++;
            if (Ppause == 1)
                flag++;
            if (flag > 1)
            {
                throw(new Exception("命令冲突！"));
                //MessageBox.Show("setting wriong");
               // return;
            }

            this.start = Pstart;
            this.reset = Preset;
            this.pause = Ppause;
            
 
        }
        /// <summary>
        /// 查看结果
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        void resultGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            for (int i = 1; i < ClientHandles.Length+1; i++)
            {
                object index1 = ClientHandles.GetValue(i);
                object index2 = ItemValues.GetValue(i);

                if (Int32.Parse(index1.ToString()) == 1) 
                {
                    toolStripProgressBar1.Value = Int32.Parse(index2.ToString());
                }
                    
               
            }
            
        }

        /// <summary>
        /// 处理VB80和VB30 信号的变化
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        void statusGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            for (int i = 1; i < ClientHandles.Length + 1; i++)
            {
                object index1 = ClientHandles.GetValue(i);
                object index2 = ItemValues.GetValue(i);
                //如果是1号位置,VB30
                if (Int32.Parse(index1.ToString()) == 1)
                {
                    
                    int statusnow =Int32.Parse(index2.ToString());
                    uint result = 0xfffff ;
                    
                    

                    //确保状态周期循环
                    if (statusnow == (status + 1)%ENDCIRCLE  )
                    {
                        

                        //判断相等 ,注意-1错误
                        if ( (result = readResult()) == CHECKSTATUS[statusnow - 1])
                        {
                            //更新状态
                            status = statusnow;

                        }
                        else
                        {
                            logExp();
                            
 
                        }

                        

                       
 
                    }
                    
                    //更新UI
                    toolStripProgressBar1.Value = statusnow;

                }
                    
                   
            }
            
        }


        void startExp()
        {
            if (start == 0 && reset == 0 && pause == 0)
            {
                dbcontex = new tttEntities();
                exp = new sikongqi();
                exp.startTime = DateTime.Now;
                exp.result = ( Int32.Parse( ResultOfExp.Sucess.ToString()));
                
            }


        }

        void resetExp()
        { }

        void pauserExp()
        { }
        void logExp()
        { }

       uint readResult()
        {
            
            object   value;
           
           
            
            object quality,timestamaps;
            resultGroup.OPCItems.Item(1).Read(1, out value, out quality, out timestamaps);
           return (uint) value & 0xFFFF;
            //resultGroup.SyncRead(1, 1,ref serverhandle, out value, out error, out quality, out timestamaps);

        }
    

    }
}
