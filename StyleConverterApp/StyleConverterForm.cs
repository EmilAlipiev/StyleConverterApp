using StyleConverterApp.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace StyleConverterApp
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

            XmlSerializer serializer = new XmlSerializer(typeof(style));
            style androidStyle;
            var sourceStyle = rtbFrom.Text;
            using (TextReader reader = new StringReader(sourceStyle))
            {
                androidStyle = (style)serializer.Deserialize(reader);
            }
            string key = androidStyle.name;
            string targetType = "";
            switch (androidStyle.parent)
            {
                case "@style/Text":
                    targetType = "Label";
                    break;
                default:
                    break;
            }
            Style xamarinFormsStyle = new Style()
            {
                Key = key,
                TargetType = targetType
            };

            if (androidStyle.item?.Length > 0)
            {
                var setters = new List<StyleSetter>();
                foreach (var item in androidStyle.item)
                {
                    setters.Add(new StyleSetter()
                    {
                        Property = GetProperty(item.name),
                        Value = GetValue(item.Value, item.name)
                    });
                }
                xamarinFormsStyle.Setter = setters.ToArray();
            }

            string result = GetStyleString<Style>(xamarinFormsStyle);

            rtbTo.Text = result;
        }

        private static string GetStyleString<T>(T xamarinFormsStyle)
        {
            XmlSerializer xFserializer = new XmlSerializer(typeof(T));
            var nameSpaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            nameSpaces.Add("x", "http://schemas.microsoft.com/winfx/2009/xaml");
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
            string result = "";
            using (StringWriter sw = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(sw, settings))
            {

                xFserializer.Serialize(sw, xamarinFormsStyle, nameSpaces);
                result = sw.ToString();
            }

            return result.Replace("xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"", "").Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "").Trim();
        }

        private string GetValue(string value, string name = "")
        {
            if (value.EndsWith("px"))
            {
                value = value.Replace("px", "");
            }

            if (name.ToLower().Contains("fontfamily") && value.Contains("*"))
            {
                value = value.Replace("*", "");
            }

            return value;
        }

        private string GetProperty(string name)
        {
            switch (name)
            {
                case "android:textSize":
                    return "FontSize";
                case "android:textStyle":
                    return "FontAttributes";
                case "android:letterSpacing":
                    return "CharacterSpacing";
                case "android:lineSpacingMultiplier":
                    return "LineHeight";
                case "android:width":
                    return "WidthRequest";
                case "android:height":
                    return "HeightRequest";
                case "android:fontFamily":
                    return "FontFamily";
                case "android:textColor":
                    return "TextColor";
                //case "":
                //    return "HorizontalOptions";
                //case "android:layout_width":
                //    return "VerticalOptions";
                //case "":
                //    return "HorizontalTextAlignment";
                //case "":
                //    return "Margin";
                //case "":
                //    return "Padding";
                //case "":
                //    return "BackgroundColor";
                //case "android:layout_height":
                //    return "LineBreakMode";

                default:
                    return "NOTFOUND";
            }
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbTo.Text);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            string prp = cbSetter.SelectedItem.ToString();
            string property = GetProperty(prp);
            string value = GetValue(txtValue.Text, prp);

            txtStyle.Text = GetStyleString(new StyleSetter() { Property = property, Value = value });
            Clipboard.SetText(txtStyle.Text);
        }
    }
}
