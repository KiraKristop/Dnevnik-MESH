namespace MeshDiary;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1080, 720);
        MinimumSize = new Size(1020, 640);
        Text = "DnevnikMESH By KiraKris";
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(14,22,38);
        Font = new Font("Segoe UI", 9f);
    }
}
