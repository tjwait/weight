using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace weight
{
    public partial class Form1 : Form
    {

        private delegate void show_message(string str);//显示返回信息
        private delegate void show_data(string str);//显示单次读取重量
        string devid = "";
        string locker_id = "";

        public Form1()
        {
            InitializeComponent();


            string[] portsArray = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string portnumber in portsArray)
            {
                this.comboBox1.Items.Add(portnumber);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!this.serialPort1.IsOpen)
            {
                this.serialPort1.PortName = this.comboBox1.SelectedItem.ToString();//串口名
                this.serialPort1.BaudRate = (int)9600;
                this.serialPort1.DataBits = (int)8;
                this.serialPort1.Parity = System.IO.Ports.Parity.None;
                this.serialPort1.StopBits = System.IO.Ports.StopBits.One;
                this.serialPort1.DataReceived += SerialPort1_DataReceived;
                this.serialPort1.Open();
                this.button1.Text = "关闭串口";//按钮显示关闭

            }
            else
            {
                this.serialPort1.Close();
                //MessageBox.Show("Err:Open Comm Port");
                this.button1.Text = "打开串口";
            }
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
            System.Threading.Thread.Sleep(200);
            int len = this.serialPort1.BytesToRead;
            byte[] buf1 = new byte[len];
            byte[] buf = new byte[20];
            this.serialPort1.Read(buf1, 0, len);

            if (len > 3)
            {
                //去掉反斜杠
                int j = 0;
                int mark = 0;
                for (int i = 0; i < len; i++)
                {
                    if ((buf1[i] == 0x5c) && mark == 0)
                    {

                        mark = 1;
                    }
                    else 
                    {
                        mark = 0;
                        buf[j++] = buf1[i];
                    }

                }

                len = j;
                /*
                if (len == 6 && (buf[2] == 0x01 || buf[2] == 0x02))
                {
                    show_message sw = new show_message(show__message);

                    string str = "";
                    //0-5字节为表地址
                    str += buf[0].ToString("X2");
                    str += buf[1].ToString("X2");
                    str += buf[2].ToString("X2");
                    str += buf[3].ToString("X2");
                    str += buf[4].ToString("X2");
                    str += buf[5].ToString("X2");

                    this.BeginInvoke(sw, str);
                }
                else if (len == 6 && buf[2] == 0x03)
                {
                    Array.Reverse(buf);
                    UInt16 value = BitConverter.ToUInt16(buf, 1);
                    show_data sd = new show_data(show__value);
                    if (value == 16232)
                    {
                        int a = 10;
                    }
                    this.BeginInvoke(sd, value.ToString());
                }
                */

                if (buf[0] == 0x24)//称重板命令
                {
                    if (len == 10 && (buf[3] == 0x01 || buf[3] == 0x11))//如果接收到的为新的去皮命令结果
                    {
                        show_message sw = new show_message(show__message);

                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += "  ";
                        str += buf[4].ToString("X2");
                        str += buf[5].ToString("X2");
                        str += buf[6].ToString("X2");
                        str += buf[7].ToString("X2");
                        this.BeginInvoke(sw, str);
                    }
                    else if (len == 10 && (buf[3] == 0x02 || buf[3] == 0x12))//如果接收到的为新的校准命令结果
                    {
                        show_message sw = new show_message(show__message);


                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += "   ";
                        //以下为byte转float型显示
                        str += Convert.ToString(BitConverter.ToSingle(buf.Skip(4).Take(4).ToArray(), 0));
                        //str += buf[4].ToString("X2");
                        //str += buf[5].ToString("X2");
                        //str += buf[6].ToString("X2");
                        //str += buf[7].ToString("X2");
                        this.BeginInvoke(sw, str);
                    }
                    else if (len == 8 && buf[3] == 0x03)//获取重量
                    {
                        //系统同嵌入式大小端不同，在此执行数据顺序翻转
                        Array.Reverse(buf);
                        //反转后从第14号下表开始取数值，14号小标是根据buf长度为20来确定的，如果未来buf固定长度有变化，则该位置可能需要变化
                        UInt16 value = BitConverter.ToUInt16(buf, 14);
                        show_data sd = new show_data(show__value);
                        this.BeginInvoke(sd, value.ToString());
                    }
                    else if (len == 6 && buf[3] == 0x04)//获取称重板地址
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += buf[4].ToString("X2");
                        devid = buf[1].ToString("X2") + " " + buf[2].ToString("X2");//将地址赋予全局变量
                        this.BeginInvoke(sw, str);
                    }
                    else if (len == 6 && buf[3] == 0x05)//设定称重板地址
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += buf[4].ToString("X2");
                        devid = buf[1].ToString("X2") + " " + buf[2].ToString("X2");//将地址赋予全局变量
                        this.BeginInvoke(sw, str);
                    }
                    else if (len == 6 && buf[3] == 0x15)//设定称重板地址，保存命令
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += buf[4].ToString("X2");
                        devid = buf[1].ToString("X2") + " " + buf[2].ToString("X2");//将地址赋予全局变量
                        this.BeginInvoke(sw, str);
                    }
                }
                else if (buf[0] == 0x40)//锁命令
                {
                    if (len == 6 && buf[3] == 0x04)//获取锁地址
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += buf[4].ToString("X2");
                        locker_id = buf[1].ToString("X2") + " " + buf[2].ToString("X2");//将地址赋予全局变量
                        this.BeginInvoke(sw, str);
                    }

                    else if (len == 6 && buf[3] == 0x05)//设定锁地址返回
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += buf[4].ToString("X2");
                        locker_id = buf[1].ToString("X2") + " " + buf[2].ToString("X2");//将地址赋予全局变量
                        this.BeginInvoke(sw, str);
                    }

                    else if (len == 6 && buf[3] == 0x15)//设定锁地址保存返回
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        str += buf[4].ToString("X2");
                        locker_id = buf[1].ToString("X2") + " " + buf[2].ToString("X2");//将地址赋予全局变量
                        this.BeginInvoke(sw, str);
                    }

                    else if (len == 7 && buf[3] == 0x03)//获取锁状态
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        if (buf[4] == 0xFF) { str += "  门状态异常，需维修！  "; }
                        else if (buf[4] == 0x02) { str += "  门关闭  "; }
                        else if (buf[4] == 0x01) { str += "  门打开  "; }
                        else if (buf[4] == 0x02) { str += "  锁芯已经吸回，可以打开  "; }
                        //str += buf[4].ToString("X2");
                        this.BeginInvoke(sw, str);
                    }

                    else if (len == 7 && buf[3] == 0x01)//获取锁状态
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        if (buf[4] == 0xFF) { str += "  门状态异常，需维修！  "; }
                        else if (buf[4] == 0x00) { str += "  门打开正常  "; }
                        else if (buf[4] == 0x01) { str += "  门已经打开  "; }
                        else if (buf[4] == 0x03) { str += "  门没有被打开，已经关闭  "; }
                        //str += buf[4].ToString("X2");
                        this.BeginInvoke(sw, str);
                    }

                    else if (len == 7 && buf[3] == 0x02)//主动向上位机推送的门的状态
                    {
                        show_message sw = new show_message(show__message);
                        string str = "";
                        str += buf[1].ToString("X2");
                        str += buf[2].ToString("X2");
                        str += buf[3].ToString("X2");
                        if (buf[4] == 0x21) { str += "  门关闭正常  "; }
                        else if (buf[4] == 0x22) { str += "  门关闭异常  "; }
                        else if (buf[4] == 0x23) { str += "  门在不停的被开关  "; }
                        //str += buf[4].ToString("X2");
                        this.BeginInvoke(sw, str);
                    }

                }

            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        //串口刷新按钮点击事件
        private void button2_Click(object sender, EventArgs e)
        {
            string[] portsArray = System.IO.Ports.SerialPort.GetPortNames();

            this.comboBox1.Items.Clear();

            foreach (string portnumber in portsArray)
            {
                this.comboBox1.Items.Add(portnumber);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        //去皮
        private void button3_Click(object sender, EventArgs e)
        {
            string read_cmd = "00 01 01 02";
            byte[] cmd = HexStringToByteArray(read_cmd);
            try
            {
                this.serialPort1.Write(cmd, 0, cmd.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //校准
        private void button4_Click(object sender, EventArgs e)
        {
            int value = Convert.ToInt32(this.textBox3.Text);
            byte[] buf = BitConverter.GetBytes(value);//由于int为4个字节，因此转换后buf长度为4，但实际校准数据只需要两个字节即可，因此只截取低两位字节
            //string read_cmd = "00 01 02";

            byte[] send_buf = new byte[6];
            send_buf[0] = 0x00;
            send_buf[1] = 0x01;
            send_buf[2] = 0x02;
            send_buf[3] = buf[1];
            send_buf[4] = buf[0];

            for (int i = 0; i < 5; i++)
            {
                send_buf[5] += send_buf[i];
            }

            try
            {
                this.serialPort1.Write(send_buf, 0, send_buf.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //单次读取
        private void button5_Click(object sender, EventArgs e)
        {
            string read_cmd = "00 01 03 04";
            byte[] cmd = HexStringToByteArray(read_cmd);
            try
            {
                this.serialPort1.Write(cmd, 0, cmd.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        //显示返回数据
        void show__message(string str)
        {
            try
            {
                this.textBox1.Text = str+"\r\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        //显示返回数据
        void show__value(string str)
        {
            try
            {
                this.label2.Text = str;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Enabled = false;

            /*
            string read_cmd = "00 01 03 04";
            byte[] cmd = HexStringToByteArray(read_cmd);
            try
            {
                this.serialPort1.Write(cmd, 0, cmd.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            */


                //在连续读取的代码中没有添加判断devid是否为空的内容，以防如果为空，则屏幕一直输出messagebox，此处若为空则程序崩溃，暂不做优化处理
                byte[] devid_buf = HexStringToByteArray(devid);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x03;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            this.Enabled = true;
        }
        //连续读取
        private void button6_Click(object sender, EventArgs e)
        {
            if (this.button6.Text == "连续读取")
            {
                this.button6.Text = "停止读取";
                this.timer1.Enabled = true;
            }
            else
            {
                this.button6.Text = "连续读取";
                this.timer1.Enabled = false;
            }
        }

        //去皮新
        private void button7_Click(object sender, EventArgs e)
        {
            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            { 
                byte[] devid_buf = HexStringToByteArray(devid);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x01;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        //去皮保存新
        private void button8_Click(object sender, EventArgs e)
        {

            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            {
                byte[] devid_buf = HexStringToByteArray(devid);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x11;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        //校准新
        private void button9_Click(object sender, EventArgs e)
        {
            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            {
                byte[] devid_buf = HexStringToByteArray(devid);
                int value = Convert.ToInt32(this.textBox3.Text);
                byte[] buf = BitConverter.GetBytes(value);//由于int为4个字节，因此转换后buf长度为4，但实际校准数据只需要两个字节即可，因此只截取低两位字节
                                                          //string read_cmd = "00 01 02";
                byte[] send_buf = new byte[8];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x02;
                send_buf[4] = buf[1];
                send_buf[5] = buf[0];

                for (int i = 1; i < 6; i++)
                {
                    send_buf[6] += send_buf[i];
                }
                send_buf[7] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        //校准保存新,无需再提交砝码重量
        private void button10_Click(object sender, EventArgs e)
        {
            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            {
                byte[] devid_buf = HexStringToByteArray(devid);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x12;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        //单次读取新
        private void button11_Click(object sender, EventArgs e)
        {
            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            {
                this.button11.Enabled = false;
                byte[] devid_buf = HexStringToByteArray(devid);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x03;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                this.timer2.Enabled = true;
            }

        }

        //连续读取新
        private void button12_Click(object sender, EventArgs e)
        {
            if (this.button12.Text == "连续读取新")
            {
                this.button12.Text = "停止读取新";
                if(this.textBox2.Text != "")
                {
                    this.timer1.Interval = Convert.ToInt32(this.textBox2.Text);
                }
                this.timer1.Enabled = true;
            }
            else
            {
                this.button12.Text = "连续读取新";
                this.timer1.Enabled = false;
            }
        }

        //地址获取
        private void button13_Click(object sender, EventArgs e)
        {
            string read_cmd = "24 FF FF 04 02 23";
            byte[] cmd = HexStringToByteArray(read_cmd);
            try
            {
                this.serialPort1.Write(cmd, 0, cmd.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //地址设定
        private void button14_Click(object sender, EventArgs e)
        {
            //int value = Convert.ToInt32(this.textBox3.Text);
            byte[] buf = BitConverter.GetBytes(Int32.Parse(this.numericUpDown1.Value.ToString()));//由于int为4个字节，因此转换后buf长度为4，但实际校准数据只需要两个字节即可，因此只截取低两位字节
            //string read_cmd = "00 01 02";

            byte[] send_buf = new byte[8];
            send_buf[0] = 0x24;
            send_buf[1] = 0xff;
            send_buf[2] = 0xff;
            send_buf[3] = 0x05;
            send_buf[4] = buf[1];
            send_buf[5] = buf[0];

            for (int i = 1; i < 6; i++)
            {
                send_buf[6] += send_buf[i];
            }
            send_buf[7] = 0x23;
            try
            {
                this.serialPort1.Write(send_buf, 0, send_buf.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //MessageBox.Show(this.numericUpDown1.Value.ToString()); 
        }

        //地址设定保存
        private void button15_Click(object sender, EventArgs e)
        {
            byte[] devid_buf = HexStringToByteArray(devid);
            byte[] send_buf = new byte[6];
            send_buf[0] = 0x24;
            send_buf[1] = devid_buf[0];
            send_buf[2] = devid_buf[1];
            send_buf[3] = 0x15;

            for (int i = 1; i < 4; i++)
            {
                send_buf[4] += send_buf[i];
            }
            send_buf[5] = 0x23;
            try
            {
                this.serialPort1.Write(send_buf, 0, send_buf.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.timer2.Enabled = false;
            this.button11.Enabled = true;
        }


        //获取锁地址
        private void button16_Click(object sender, EventArgs e)
        {
            string read_cmd = "40 FF FF 04 02 23";
            byte[] cmd = HexStringToByteArray(read_cmd);
            try
            {
                this.serialPort1.Write(cmd, 0, cmd.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //设定锁地址
        private void button19_Click(object sender, EventArgs e)
        {
            //int value = Convert.ToInt32(this.textBox3.Text);
            byte[] buf = BitConverter.GetBytes(Int32.Parse(this.numericUpDown2.Value.ToString()));//由于int为4个字节，因此转换后buf长度为4，但实际校准数据只需要两个字节即可，因此只截取低两位字节
            //string read_cmd = "00 01 02";

            byte[] send_buf = new byte[8];
            send_buf[0] = 0x40;
            send_buf[1] = 0xff;
            send_buf[2] = 0xff;
            send_buf[3] = 0x05;
            send_buf[4] = buf[1];
            send_buf[5] = buf[0];

            for (int i = 1; i < 6; i++)
            {
                send_buf[6] += send_buf[i];
            }
            send_buf[7] = 0x23;
            try
            {
                this.serialPort1.Write(send_buf, 0, send_buf.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //锁地址保存
        private void button20_Click(object sender, EventArgs e)
        {
            if (locker_id == "")
            {
                MessageBox.Show("锁地址为空");
            }
            else
            {


                byte[] devid_buf = HexStringToByteArray(locker_id);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x40;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x15;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        //获取锁状态
        private void button18_Click(object sender, EventArgs e)
        {
            if (locker_id == "")
            {
                MessageBox.Show("锁地址为空");
            }
            else
            {
                this.button11.Enabled = false;
                byte[] devid_buf = HexStringToByteArray(locker_id);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x40;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x03;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                this.timer2.Enabled = true;
            }
        }


        //开锁
        private void button17_Click(object sender, EventArgs e)
        {
            if (locker_id == "")
            {
                MessageBox.Show("锁地址为空");
            }
            else
            {
                this.button11.Enabled = false;
                byte[] devid_buf = HexStringToByteArray(locker_id);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x40;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x01;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                this.timer2.Enabled = true;
            }
        }


        //业务展示——校准
        private void button21_Click(object sender, EventArgs e)
        {
            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            {
                byte[] devid_buf = HexStringToByteArray(devid);
                int value = Convert.ToInt32(this.textBox4.Text);
                byte[] buf = BitConverter.GetBytes(value);//由于int为4个字节，因此转换后buf长度为4，但实际校准数据只需要两个字节即可，因此只截取低两位字节
                                                          //string read_cmd = "00 01 02";
                byte[] send_buf = new byte[8];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x02;
                send_buf[4] = buf[1];
                send_buf[5] = buf[0];

                for (int i = 1; i < 6; i++)
                {
                    send_buf[6] += send_buf[i];
                }
                send_buf[7] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        //业务展示——开门
        private void button22_Click(object sender, EventArgs e)
        {

            if (locker_id == "")
            {
                MessageBox.Show("锁地址为空");
            }
            else
            {
                this.button11.Enabled = false;
                byte[] devid_buf = HexStringToByteArray(locker_id);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x40;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x01;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                this.timer2.Enabled = true;
            }
        }
        

        //业务展示——称重
        private void button23_Click(object sender, EventArgs e)
        {
            if (devid == "")
            {
                MessageBox.Show("地址为空");
            }
            else
            {
                this.button11.Enabled = false;
                byte[] devid_buf = HexStringToByteArray(devid);
                byte[] send_buf = new byte[6];
                send_buf[0] = 0x24;
                send_buf[1] = devid_buf[0];
                send_buf[2] = devid_buf[1];
                send_buf[3] = 0x03;

                for (int i = 1; i < 4; i++)
                {
                    send_buf[4] += send_buf[i];
                }
                send_buf[5] = 0x23;
                try
                {
                    this.serialPort1.Write(send_buf, 0, send_buf.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                this.timer2.Enabled = true;
            }
        }
    }
}
