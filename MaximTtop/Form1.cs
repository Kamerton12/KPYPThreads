using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace MaximTtop
    //Viborom
    //Porazryadnayya
{
    public partial class Form1 : Form
    {
        SortOnThread chooseT, radixT;
        ManualResetEvent ev = new ManualResetEvent(false);

        List<int> choose = new List<int>();
        List<int> radix = new List<int>();

        public Form1()
        {
            InitializeComponent();
            //List<int> a = new List<int>();
            Random r = new Random();
            for (int i = 0; i < 50000; i++)
            {
                int p = r.Next();
                choose.Add(p);
                radix.Add(p);
            }
                //    a.Add(r.Next());
            //choose.Clear();
            //choose.InsertRange(0, a);
            //radix.Clear();
            //radix.InsertRange(0, a);
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            //start
            if(chooseT == null || (chooseT != null && chooseT.isDone()))
            {
                chooseT = new SortOnThread(ref listBox1, ref ev, false, choose);

                radixT = new SortOnThread(ref listBox2, ref ev, true, radix);

                listBox1.Items.Clear();
                listBox2.Items.Clear();
            }
            ev.Set();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
            //pause
            ev.Reset();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            button3_Click(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            //stop
            ev.Set();
            try
            {
                chooseT.kill();
                radixT.kill();
            } catch (Exception ex) { }
            radixT = null;
            chooseT = null;
            ev.Reset();
        }
    }

    class SortOnThread
    {
        List<int> arr;
        public Thread t;
        ListBox listBox1;
        ManualResetEvent ev;
        public SortOnThread(ref ListBox listBox1, ref ManualResetEvent ev, bool isRadix, List<int> arr)
        {
            this.listBox1 = listBox1;
            this.ev = ev;
            this.arr = arr;
            if (!isRadix)
                t = new Thread(this.sortChoose);
            else
                t = new Thread(this.sortRadix);
            t.Start();
        }

        public bool isDone()
        {
            return t.ThreadState == ThreadState.Stopped;
        }

        bool die = false;

        public void kill()
        {
            die = true;
        }

        public void sortChoose()
        {
            for (int i = 0; i < arr.Count; i++)
            {
                ev.WaitOne();
                if (die)
                    return;
                int min = arr[i];
                int minIndex = i;
                for(int j = i; j < arr.Count; j++)
                {
                    if (min > arr[j])
                    {
                        min = arr[j];
                        minIndex = j;
                    }
                }
                arr[minIndex] = arr[i];
                arr[i] = min;
                try
                {
                    if (i % 100 == 0)
                        listBox1.Invoke((MethodInvoker)(() =>
                        {
                            listBox1.Items.Add("Находим " + i + " минимальный элемент");
                            listBox1.SelectedIndex = i / 100;
                            //listBox1.Items.Clear();
                            //for (int j = 0; j < arr.Count; j++)
                            //    listBox1.Items.Add(arr[j]);
                        }));
                }
                catch (Exception e) { }
                //Thread.Sleep(100);
            }
            listBox1.Invoke((MethodInvoker)(() => {
                listBox1.Items.Add("Готово");
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }));
        }

        public int pow10(int x)
        {
            int ans = 1;
            for (int i = 0; i < x; i++)
                ans *= 10;
            return ans;
        }

        public void sortRadix()
        {
            int max = arr[0];
            for(int i = 1; i < arr.Count; i++)
                max = Math.Max(max, arr[i]);
            int iterCount = (int)Math.Ceiling(Math.Log10(max));
            for (int i = 0; i < iterCount; i++)
            {
                ev.WaitOne();
                if (die)
                    return;
                List<int>[] indexes = new List<int>[10];
                for (int j = 0; j < 10; j++)
                    indexes[j] = new List<int>();
                for (int j = 0; j < arr.Count; j++)
                {
                    int digit = (arr[j] / pow10(i)) % 10;
                    indexes[digit].Add(arr[j]);
                }
                int index = 0;
                for(int j = 0; j < 10; j++)
                    for(int k = 0; k < indexes[j].Count; k++)
                        arr[index++] = indexes[j][k];
                try
                { 
                    
                    listBox1.Invoke((MethodInvoker)(() => {
                        listBox1.Items.Add("Сортируем по " + (i + 1) + " разряду");
                        //listBox1.Items.Clear();
                        //for (int j = 0; j < arr.Count; j++)
                        //    listBox1.Items.Add(arr[j]);
                    }));
                } catch (Exception e) { }
            //Thread.Sleep(1000);
            }
            listBox1.Invoke((MethodInvoker)(() => {
                listBox1.Items.Add("Готово");
            }));
        }
    }
}
