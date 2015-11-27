using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OPCAutomation;

namespace sikongqi
{
    enum ResultOfExp { Sucess,Failure};
    
    public partial class Form1 : Form
    {
        const string opcname = "KEPware.KEPServerEx.V4";
        const string VB30ITEM = "sikongqi1.plc200.vb30";
        const string VB80ITEM = "sikongqi1.plc200.vb80";
        const string I04_I15 = "sikongqi1.plc200.input";
        const string START = "";
        const string RESET = "";
        const string PAUSE = "";
        int vb30;
        int vb80;
        int start;
        int reset;
        int pause;
        


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



        public Form1()
        {
            InitializeComponent();
            init();
            
        }

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
            resultGroup.DataChange += resultGroup_DataChange;
            CommandGroup.DataChange += CommandGroup_DataChange;
            
            #region 
         
            statusGroup.IsSubscribed = true;
            resultGroup.IsSubscribed = true;
            CommandGroup.IsSubscribed = true;
            resultGroup.IsActive = true;
            statusGroup.IsActive = true;
            CommandGroup.IsActive = true;
            


            resultGroup.UpdateRate = 10;
            statusGroup.UpdateRate = 10;
            CommandGroup.UpdateRate = 10;
            #endregion


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
            CommandGroup.IsSubscribed = false;

            for (int i = 1; i < ClientHandles.Length + 1; i++)
            {
                object index1 = ClientHandles.GetValue(i);
                object index2 = ItemValues.GetValue(i);
                int temp = Int32.Parse(index2.ToString());
                if (temp == 1)
                {
                    switch (Int32.Parse(index1.ToString()))
                    {
                        ///start pushed.
                        case 1:
                            {

                                setCommandStates(1, 0, 0);
                                exp = new sikongqi();
                                exp.startTime = DateTime.Now;
                                exp.planNumber = 100;




                                break;
                            }
                            // 暂停处理逻辑
                        case 2:
                            {
                                setCommandStates(0, 0, 1);
                                break;
                            }

                            //复位处理逻辑
                        case 3:
                            {
                                setCommandStates(0, 1, 0);
                                break;
                            }
                    }

                }
            }

            //打开订阅
            CommandGroup.IsSubscribed = true;
            
        }

        void setCommandStates(int Pstart, int Preset, int Ppause)
        {
            int flag = 0;
            if (Pstart == 1)
                flag++;
            if (Preset == 1)
                flag++;
            if (Ppause == 1)
                flag++;
            if (flag >= 1)
            {
                MessageBox.Show("setting wriong");
                return;
            }

            this.start = Pstart;
            this.reset = Preset;
            this.pause = Ppause;
 
        }
        /// <summary>
        /// 
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

                if (  Int32.Parse( index1.ToString()) == 1)
                    textBox3.Text = index2.ToString();
               
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

                if (Int32.Parse(index1.ToString()) == 1)
                    textBox1.Text = index2.ToString();
                else if (Int32.Parse(index1.ToString()) == 2)
                    textBox2.Text = index2.ToString();
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

    

    }
}
