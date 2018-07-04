using Postulate.Lite.Core.Merge;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MergeUI
{
	public partial class frmMain : Form
	{
		private ScriptManager _scriptManager = null;

		public string AssemblyFile { get; set; } = null;

		public frmMain()
		{
			InitializeComponent();
		}

		private void btnExecute_Click(object sender, EventArgs e)
		{

		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{

		}

		private void btnSelectFile_Click(object sender, EventArgs e)
		{

		}

		private async void btnSelectFile_Click_1(object sender, EventArgs e)
		{
			try
			{
				if (UserSelectAssembly(out string fileName))
				{
					await LoadAssemblyAsync(fileName);
				}
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message);
			}
		}

		private async Task LoadAssemblyAsync(string fileName)
		{
		}

		private bool UserSelectAssembly(out string fileName)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Assemblies|*.exe;*.dll|All Files|*.*";
			if (dlg.ShowDialog() == DialogResult.OK)
			{				
				fileName = dlg.FileName;
				return true;
			}

			fileName = null;
			return false;
		}
	}
}
