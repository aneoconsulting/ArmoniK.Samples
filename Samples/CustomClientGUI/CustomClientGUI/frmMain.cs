using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CustomClientGUI.Data;

namespace CustomClientGUI
{
  public partial class frmMain : MetroFramework.Forms.MetroForm
  {
    private static frmMain _instance;

    public static frmMain Instance
      => _instance ?? (_instance = new frmMain());

    public MetroFramework.Controls.MetroPanel MetroContainer
    {
      get => mPanel;
      set => mPanel = value;
    }
    public frmMain()
    {
      InitializeComponent();
      DataConnection = new ConnectionData();
    }

    public ConnectionData DataConnection { get; set; }

    private void Form1_Load(object sender, EventArgs e)
    {
      ucLogin uc = new ucLogin();
      uc.Dock = DockStyle.Fill;
      mPanel.Controls.Add(uc);
      _instance = this;
    }

    private void frmMain_Resize(object sender, EventArgs e)
    {
      if (mPanel.Controls.ContainsKey("ucLogin"))
      {
        mPanel.Controls["ucLogin"]
              .Dock = DockStyle.Fill;
      }
    }

    private void metroTile3_Click(object sender, EventArgs e)
    {

    }

    private void mTileExit_Click(object sender, EventArgs e)
    {
      frmMain.Instance.Close();
    }

    private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }
  }
}
