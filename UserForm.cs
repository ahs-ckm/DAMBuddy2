using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAMBuddy2
{
    public partial class UserForm : Form
    {

        public string mUser;
        public string mPassword;

        public UserForm()
        {
            InitializeComponent();
            tbUser.Text = mUser;
            tbPassword.Text = mPassword;

        }

        private void tbUser_TextChanged(object sender, EventArgs e)
        {
            mUser = tbUser.Text;
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            mPassword = tbPassword.Text;
        }

        private void UserForm_Shown(object sender, EventArgs e)
        {
            tbUser.Text = mUser;
            tbPassword.Text = mPassword;
        }
    }
}
