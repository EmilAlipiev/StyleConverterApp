using StyleConverterApp.Models;

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

        Style xamarinFormsStyle;
        private void BtnConvert_Click(object sender, EventArgs e)
        {
            bool isAndroidStyle = false;
            var sourceStyle = Regex.Replace(rtbFrom.Text, @"\t|\n|\r", "").Trim();
            string key = "";
            string targetType = "";


            if (isAndroidStyle = sourceStyle.ToLower().StartsWith("<style"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(style));
                style androidStyle;

                using (TextReader reader = new StringReader(sourceStyle))
                {
                    androidStyle = (style)serializer.Deserialize(reader);
                }
                key = androidStyle.name;
                switch (androidStyle.parent)
                {
                    case "@style/Text":
                        targetType = "Label";
                        break;
                    default:
                        break;
                }

                xamarinFormsStyle = new Style()
                {
                    Key = txtName.Text.Length > 0 ? txtName.Text : key,
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

            }
            else
            {
                xamarinFormsStyle = new Style()
                {
                    Key = txtName.Text.Length > 0 ? txtName.Text : key,
                    TargetType = targetType
                };

                var keys = new string[] { "x:name", "text =", "text=", "command", "grid", "absolutelayout" };


                var items = sourceStyle.Split(new[] { "\"  " }, StringSplitOptions.RemoveEmptyEntries);

                if (items?.Length > 0)
                {
                    var setters = new List<StyleSetter>();
                    foreach (var itemRaw in items)
                    {
                        var item = itemRaw.Contains("\"") ? itemRaw.Trim() + "\"" : itemRaw.Trim();
                        if (item.StartsWith("<") && !item.Contains(".") && !item.Contains("/") &&
                            !item.Contains("Gesture"))
                        {
                            targetType = GetTargetType(xamarinFormsStyle, item);
                        }
                        else
                        {
                            if (!item.Contains("=") || keys.Any(k => item.ToLower().Contains(k)))
                                continue;

                           // var values = new string[] { "DynamicResource", "StaticResource", "OnIdiom", "OnPlatform" };

                            //if (values.Any(k => item.Contains(k)))
                            //{
                            //    item = item + " " + items[items.ToList().IndexOf(item) + 1];
                            //}

                            var valueItems = new string[2];
                            var styleItems = item.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (styleItems.Length > 2)
                            {
                                valueItems[0] = styleItems[0];
                                for (int i = 1; i < styleItems.Length; i++)
                                {
                                    valueItems[1] += styleItems[i];
                                }
                            }
                            else
                            {
                                valueItems = styleItems;
                            }
                            string property = valueItems[0].Trim();

                            string value = valueItems[1].Replace("\"", string.Empty).Replace(">", string.Empty)
                                .Replace("/", string.Empty).Trim();

                            setters.Add(new StyleSetter()
                            {
                                Property = Regex.Replace(property, @"\t|\n|\r", "").Trim(),
                                Value = Regex.Replace(value, @"\t|\n|\r", "").Trim(),
                            });
                            item = item.Replace("/>", string.Empty).Replace(">", string.Empty).Replace("\" \"","\"");
                            sourceStyle = sourceStyle.Replace(item, string.Empty);
                        }
                    }
                    xamarinFormsStyle.Setter = setters.ToArray();
                }
            }

            sourceStyle = sourceStyle.Insert(1 + targetType.Length, $" Style=\"{{StaticResource {xamarinFormsStyle.Key}}}\"");
            const string reduceMultiSpace = @"[ ]{2,}";
            sourceStyle = Regex.Replace(sourceStyle.Replace("\t", " "), reduceMultiSpace, " ");

            string result = GetStyleString<Style>(xamarinFormsStyle);
            result += Environment.NewLine + Environment.NewLine;
            result += isAndroidStyle ? $"<{targetType} Style=\"{{StaticResource {xamarinFormsStyle.Key}}}\"  />" : Regex.Replace(sourceStyle, @"\\t|\\n|\\r", "").Trim();
            rtbTo.Text = result;
        }

        private static string GetTargetType(Style xamarinFormsStyle, string item)
        {
            string targetType;
            var targeTypes = item.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            targetType = targeTypes[0].Remove(0, 1);
            xamarinFormsStyle.TargetType = targetType;
            if (targeTypes.Length > 1)
            {
                var nameTypes = targeTypes[1].Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (nameTypes[0] == "x:Name" && nameTypes.Length > 1)
                {
                    xamarinFormsStyle.Key = nameTypes[1].Replace("\"", string.Empty) + "Style";
                }
            }
            if (string.IsNullOrWhiteSpace(xamarinFormsStyle.Key))
            {
                xamarinFormsStyle.Key = targetType + "Style";
            }

            return targetType;
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
