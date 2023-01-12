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

using MetroFramework;
using MetroFramework.Controls;

namespace CustomClientGUI
{
  public partial class frmMain : MetroFramework.Forms.MetroForm
  {
    private static frmMain _instance;

    public static frmMain Instance
    {
      get
      {
        if (_instance == null)
          _instance = new frmMain();
        return _instance;
      }
    }

    public MetroFramework.Controls.MetroPanel MetroContainer
    {
      get => mPanel;
      set => mPanel = value;
    }

    public frmMain()
    {
      InitializeComponent();
    }

    public MetroTabControl Mtc { get; set; }

    public ConnectionData SessionConfiguration { get; set; }

    private void Form1_Load(object    sender,
                            EventArgs e)
    {
      _instance = this;

      frmMain.Instance.SessionConfiguration = new ConnectionData();
      foreach (TabPage page in mtc.TabPages)
      {
        var ucDashBoard = new ucDashBoard("ucDashBoard_" + page.Text);
        ucDashBoard.Dock = DockStyle.Fill;
        mPanel.Controls.Add(ucDashBoard);

        var ucSettings = new ucSettings("ucSettings_" + page.Text);
        ucSettings.savedSettings();
        ucSettings.Dock = DockStyle.Fill;
        mPanel.Controls.Add(ucSettings);

        var ucLogs = new ucLogs("ucLogs_" + page.Text);
        ucLogs.Dock = DockStyle.Fill;
        mPanel.Controls.Add(ucLogs);
      }

      ucActivities ucActivities = new ucActivities();
      ucActivities.Dock = DockStyle.Fill;
      mPanel.Controls.Add(ucActivities);

      ucLogin ucLogin = new ucLogin();
      ucLogin.Dock = DockStyle.Fill;

      mPanel.Controls.Add(ucLogin);
      mPanel.Controls["ucLogin"]
            .BringToFront();
      SelectedTile = "ucDashBoard_";

      Mtc = mtc;
    }

    private void frmMain_Resize(object    sender,
                                EventArgs e)
    {
      if (mPanel.Controls.ContainsKey("ucLogin"))
      {
        mPanel.Controls["ucLogin"]
              .Dock = DockStyle.Fill;
      }
    }

    private void mTileExit_Click(object    sender,
                                 EventArgs e)
    {
      frmMain.Instance.Close();
    }

    private void tableLayoutPanel1_Paint(object         sender,
                                         PaintEventArgs e)
    {
    }

    private void mTileDashBoard_Click(object    sender,
                                      EventArgs e)
    {
      foreach (Control control in mPanel.Controls)
      {
        control.SendToBack();
      }

      mPanel.Controls["ucDashBoard_" + mtc.TabPages[mtc.SelectedIndex]
                                          .Text]
            .BringToFront();

      SelectedTile = "ucDashBoard_";
    }

    private void mTileActivities_Click(object    sender,
                                       EventArgs e)
    {
      foreach (Control control in mPanel.Controls)
      {
        control.SendToBack();
      }

      mPanel.Controls["ucActivities"]
            .BringToFront();


      SelectedTile = "ucActivities_";
    }

    private void mTileSettings_Click(object    sender,
                                     EventArgs e)
    {
      foreach (Control control in mPanel.Controls)
      {
        control.SendToBack();
      }

      mPanel.Controls["ucSettings_" + mtc.TabPages[mtc.SelectedIndex]
                                         .Text]
            .BringToFront();

      SelectedTile = "ucSettings_";
    }

    public string SelectedTile { get; set; }

    private void mtc_SelectedIndexChanged(object    sender,
                                          EventArgs e)
    {
      foreach (Control control in mPanel.Controls)
      {
        control.SendToBack();
      }

      if (SelectedTile == "ucActivities_")
      {
        mPanel.Controls["ucActivities"]
              .BringToFront();
      }
      else
      {
        mPanel.Controls[SelectedTile + mtc.TabPages[mtc.SelectedIndex]
                                          .Text]
              .BringToFront();
      }
    }

    private void mTileLogs_Click(object    sender,
                                 EventArgs e)
    {
      foreach (Control control in mPanel.Controls)
      {
        control.SendToBack();
      }

      mPanel.Controls["ucLogs_" + mtc.TabPages[mtc.SelectedIndex]
                                     .Text]
            .BringToFront();
      //mTileActivities.Style = MetroColorStyle.Blue;
      //mTileDashBoard.Style  = MetroColorStyle.Blue;
      //mTileSettings.Style   = MetroColorStyle.Blue;
      //mTileLogs.Style       = MetroColorStyle.Green;
      SelectedTile = "ucLogs_";
    }
  }
}
