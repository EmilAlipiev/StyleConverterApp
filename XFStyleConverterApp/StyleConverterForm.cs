using StyleConverterApp.Models;

using StyleConverterShared;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace XFStyleConverterApp
{
    public partial class StyleConverterForm : Form
    {
        public StyleConverterForm()
        {
            InitializeComponent();

            cbSetter.Items.Add("android:textSize");
            cbSetter.Items.Add("android:textStyle");
            cbSetter.Items.Add("android:letterSpacing");
            cbSetter.Items.Add("android:lineSpacingMultiplier");
            cbSetter.Items.Add("android:height");
            cbSetter.Items.Add("android:fontFamily");
            cbSetter.Items.Add("android:textColor");
            cbSetter.Items.Add("android:width");
        }

     
        private void BtnConvert_Click(object sender, EventArgs e)
        {
            rtbTo.Text = ConverterHelper.ConvertText(rtbFrom.Text, txtName.Text);
        }
 

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbTo.Text);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            string prp = cbSetter.SelectedItem.ToString();
            string property = ConverterHelper.GetProperty(prp);
            string value = ConverterHelper.GetValue(txtValue.Text, prp);

            txtStyle.Text = ConverterHelper.GetStyleString(new StyleSetter() { Property = property, Value = value });
            Clipboard.SetText(txtStyle.Text);
        }
    }
}