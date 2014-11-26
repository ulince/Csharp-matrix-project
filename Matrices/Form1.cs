using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Matrices
{
    public partial class Form1 : Form
    {
        public int rows;
        public int columns;
        bool start;
        int counter;
        /* Possible directions of movement */
        Point[] rules = new Point[8];
        /* Queue of future locations to visit */
        Queue<Point> locations;
        Point currentPosition;
        /* Background thread to perform simulation */
        BackgroundWorker bw;
        
        public Form1()
        {
            InitializeComponent();
            initializeRules();

            /* Get number of rows from textBox */
            rows = Int32.Parse(textBox1.Text);
            /* Get number of columns from textBox */
            columns = Int32.Parse(textBox2.Text);
            start = false;
            locations = new Queue<Point>();
            currentPosition = new Point();
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            /* DowWork event handler */
            bw.DoWork += bw_DoWork;
            /* ProgressChanged event handler */
            bw.ProgressChanged += bw_ProgressChanged;
            /* RunWorkerCompleted event handler */
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
        }

        /* Shows a Message when the simulation ends */
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            start = false;
            if ((e.Cancelled == true))
            {
                MessageBox.Show("Finished.");
            }
            else if (!(e.Error == null))
            {
                MessageBox.Show("Error.");
            }
            else
            {
                MessageBox.Show("Finished.");
            }
        }

        /* Writes the next number in the correspnding cell
         * The position is given by e.UserState, which is a Point object*/
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Point p = (Point)e.UserState;
            /* Gets the label inside the cell at p.X, p.Y */
            Label lb = (Label)tableLayoutPanel1.GetControlFromPosition(p.X, p.Y);
            
            if (string.IsNullOrEmpty(lb.Text))
            {
                counter++;
                /* Writes the counter on the label's Text */
                lb.Text = counter.ToString();
            }
        }

        /* Performs the computations on a Background thread. */
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            locations.Enqueue(currentPosition);

            /* While the tread has not been cancelled and the next number to write is less 
             * than the number of cells */
            while(worker.CancellationPending == false && counter < (rows * columns) - 1)
            {
                for (int i = 0; i < 8; i++)
                {
                    /* Get the new position by adding each movement rule to the
                     * current position*/
                    var newPosition = new Point(currentPosition.X + rules[i].X,
                        currentPosition.Y + rules[i].Y);

                    /* If the new position os outside the table, continue */
                    if (newPosition.X > columns - 1 || newPosition.X < 0 ||
                        newPosition.Y > rows - 1 || newPosition.Y < 0)
                    {
                        continue;
                    }

                    /* Make the thread sleep for 50 milliseconds */
                    System.Threading.Thread.Sleep(30);
                    /* Once the new position is calculated, update the table in the Report
                     * Progress method*/
                    worker.ReportProgress(0, newPosition);

                    /* Enqueue the new location to use it in the future */
                    if(!locations.Contains(newPosition))
                        locations.Enqueue(newPosition);
                }

                /* Once all 8 possible movements of the current location have been
                 * visited and enqueued, dequeue a position and make it the current position */
                if(locations.Count > 0)
                    currentPosition = locations.Dequeue();
            }
            e.Cancel = true;
        }

        /* Create the table with the specified columns and rows */
        private void crear_Click(object sender, EventArgs e)
        {
            start = false;
            counter = 0;
            currentPosition = new Point();
            locations.Clear();
            
            /* Get the number of rows from the textBox */
            int rws;
            bool parse = Int32.TryParse(textBox1.Text, out rws);
            if(!parse)
            {
                MessageBox.Show("You should write a number between 4 and 30.");
                return;
            }
            rows = rws;

            /* Get the number of columns from the textBox */
            int clmns;
            parse = Int32.TryParse(textBox2.Text, out clmns);
            if (!parse)
            {
                MessageBox.Show("You should write a number between 4 and 30.");
                return;
            }
            columns = clmns;

            if (rows < 4 || rows > 30 || columns < 4 || columns > 30)
            {
                MessageBox.Show("Number of columns and rows must be between 4 and 30");
                return;
            }

            /* Create the table */
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();

            tableLayoutPanel1.RowCount = rows;
            tableLayoutPanel1.ColumnCount = columns;

            tableLayoutPanel1.SuspendLayout();

            for (int i = 0; i < rows; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 1));

            for (int j = 0; j < columns; j++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));

            /* Add a label with empty text to each cell on the table */
            for (int x = 0; x < columns; x++)
            {
                for(int y = 0; y < rows; y++)
                {
                    Label l = new Label();
                    l.Text = "";
                    l.Font = new Font(l.Font.FontFamily, 60/columns);
                    l.AutoSize = true;
                    l.Dock = DockStyle.Fill;
                    l.TextAlign = ContentAlignment.MiddleCenter;
                    l.Click += label_Click;
                    tableLayoutPanel1.Controls.Add(l,x,y);
                }
            }

                tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();

        }

        /* Label Click event handler */
        void label_Click(object sender, EventArgs e)
        {
            if (!start)
            {
                (sender as Label).Text = "Start";
                start = true;
                currentPosition = new Point();
                /* When the user clicks a cell, make the current position the row and
                 * column index of that cell*/
                currentPosition.X = tableLayoutPanel1.GetRow((Label)sender);
                currentPosition.Y = tableLayoutPanel1.GetColumn((Label)sender);
            }
        }

        /* Initialize the array of 8 rules with the possible movements,
         * where each value is the offset to sum to the current position*/
        void initializeRules()
        {
            rules[0] = new Point(1,2);
            rules[1] = new Point(2,1);
            rules[2] = new Point(2,-1);
            rules[3] = new Point(1,-2);
            rules[4] = new Point(-1,-2);
            rules[5] = new Point(-2,-1);
            rules[6] = new Point(-2,1);
            rules[7] = new Point(-1,2);
        }

        /* Execute the BackgroundWorker if it is not busy */
        private void simulation_Click(object sender, EventArgs e)
        {           
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }        
        }

        /* Stops the simulation when the user clicks */
        private void stop_Click(object sender, EventArgs e)
        {
            if (bw.WorkerSupportsCancellation == true)
            {
                bw.CancelAsync();
            }
            currentPosition = new Point();
            start = false;
        }

        /* Clear the table */
        private void button3_Click(object sender, EventArgs e)
        {
            crear_Click(button1, null);
        }

    }
}
