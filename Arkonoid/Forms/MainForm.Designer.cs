using System.Drawing;
using System.Windows.Forms;

namespace Arkanoid   // <—— совпадает с MainForm.cs
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

       
    }
}
