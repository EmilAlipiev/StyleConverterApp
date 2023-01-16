using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using StyleConverterApp.Models;
using System.Text.RegularExpressions;

namespace StyleConverterShared
{

    public static class ConverterHelper
    {
        private static Style xamarinFormsStyle;
        public static string ConvertText(string textToConvert, string styleName)
        {
            bool isAndroidStyle = false;
            string sourceStyle = RemoveSpacesTabsNewLines(textToConvert);
            string key = "";
            string targetType = "";


            if (isAndroidStyle = sourceStyle.ToLower().Contains("android:"))
            {
                if (isAndroidStyle = sourceStyle.ToLower().StartsWith("style"))
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

                    DefineXamarinFormsStyle(styleName);

                    if (androidStyle.item?.Length > 0)
                    {
                        var setters = new List<StyleSetter>();
                        foreach (var item in androidStyle.item)
                        {
                            GetAndroidStyle(setters, item.name, item.Value);
                        }
                        xamarinFormsStyle.Setter = setters.ToArray();
                    }
                }
                else
                {
                    var items = sourceStyle.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (items?.Count > 2)
                    {
                        var setters = new List<StyleSetter>();

                        targetType = GetTargetTypeFromAndroid(items[0]);
                        DefineXamarinFormsStyle(styleName);

                        items.RemoveAt(0);
                        items.RemoveAt(items.Count - 1);
                        foreach (var item in items)
                        {
                            var nameValue = item.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (nameValue.Length > 1)
                            {
                                string value = nameValue[1].Replace("\"", "");

                                GetAndroidStyle(setters, nameValue[0], value);
                            }
                        }

                        xamarinFormsStyle.Setter = setters.ToArray();
                    }
                }
            }
            else
            {
                xamarinFormsStyle = new Style();

                var keys = new string[] { "x:name", "text =", "text=", "binding", "command", "grid.", "absolutelayout.", "itemsource", "toggled", "istoggled", "focused", "definitions" };


                var items = sourceStyle.Split(new[] { "\"  " }, StringSplitOptions.RemoveEmptyEntries);

                if (items?.Length > 0)
                {
                    var setters = new List<StyleSetter>();
                    foreach (var itemRaw in items) //Splited list by " space
                    {
                        var item = RemoveSpacesTabsNewLines(itemRaw);
                        item = item.Contains("\"") ? item.Trim() + "\"" : item.Trim();

                        int indexEmpty = item.IndexOf(' ');
                        int indexParantheses = item.IndexOf('{');
                        if (indexEmpty > 0) // has space
                        {
                            if (indexParantheses > 0)
                            {
                                if (indexParantheses < indexEmpty)
                                {
                                    targetType = GetTargetTypeAndSetters(item, keys, ref setters, styleName);
                                }
                                else
                                {
                                    GetSubItems(keys, ref setters, item);
                                }
                            }
                            else
                            {
                                GetSubItems(keys, ref setters, item);
                            }
                        }
                        else
                        {
                            targetType = GetTargetTypeAndSetters(item, keys, ref setters, styleName);
                        }

                    }
                    xamarinFormsStyle.Setter = setters.ToArray();
                }
            }

            sourceStyle = sourceStyle.Insert(1 + targetType.Length, $" Style=\"{{StaticResource {xamarinFormsStyle.Key}}}\"");
            sourceStyle = RemoveMultiSpaces(sourceStyle);

            string result = GetStyleString<Style>(xamarinFormsStyle);
            result += Environment.NewLine + Environment.NewLine;
            result += isAndroidStyle ? $"<{targetType} Style=\"{{StaticResource {xamarinFormsStyle.Key}}}\"  />" : Regex.Replace(sourceStyle, @"\\t|\\n|\\r", "").Trim();
            return result;

            string GetTargetTypeAndSetters(string item, string[] keys, ref List<StyleSetter> setters, string styleName)
            {
                if (item.StartsWith("<") && !item.Contains(".") && !item.Contains("/") && !item.Contains("Gesture"))
                {
                    targetType = GetTargetType(xamarinFormsStyle, item, out string firstStyle, styleName);
                    if (firstStyle.Length > 0)
                    {
                        GetStyleSource(ref sourceStyle, ref setters, firstStyle);
                    }
                }
                else if (item.Contains("Style"))
                {
                    var nameTypes = item.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (nameTypes.Length > 1)
                    {
                        xamarinFormsStyle.BasedOn = nameTypes[1].Replace("\"", string.Empty);

                        item = CleanItemFromTags(item);
                        sourceStyle = sourceStyle.Replace(item, string.Empty);
                    }
                }
                else
                {
                    if (!item.Contains("=") || keys.Any(k => item.ToLower().Contains(k)))
                        return targetType;

                    // var values = new string[] { "DynamicResource", "StaticResource", "OnIdiom", "OnPlatform" };

                    //if (values.Any(k => item.Contains(k)))
                    //{
                    //    item = item + " " + items[items.ToList().IndexOf(item) + 1];
                    //}

                    GetStyleSource(ref sourceStyle, ref setters, item);
                }

                return targetType;
            }

            void GetSubItems(string[] keys, ref List<StyleSetter> setters, string item)
            {
                var subItems = item.Split(new[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subItem in subItems)
                {
                    targetType = GetTargetTypeAndSetters(subItem, keys, ref setters, styleName);
                }
            }

            void DefineXamarinFormsStyle(string styleName) => xamarinFormsStyle = new Style()
            {
                Key = styleName.Length > 0 ? styleName : key,
                TargetType = targetType
            };
        }

        private static string GetTargetTypeFromAndroid(string androidType)
        {
            androidType = androidType.Replace("<", "");
            switch (androidType)
            {
                case "TextView":
                    return "Label";
                default:
                    return "Label";
            }
        }

        private static void GetAndroidStyle(List<StyleSetter> setters, string name, string value)
        {
            setters.Add(new StyleSetter()
            {
                Property = GetProperty(name),
                Value = GetValue(value, name)
            });
        }

        private static void GetStyleSource(ref string sourceStyle, ref List<StyleSetter> setters, string item)
        {

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

            string value = CleanItemFromTags(valueItems[1]);

            setters.Add(GetSetter(property, value));

            item = CleanItemFromTags(item);
            sourceStyle = sourceStyle.Replace(item, string.Empty);
        }

        private static StyleSetter GetSetter(string property, string value)
        {
            return new StyleSetter()
            {
                Property = RemoveMultiSpaces(RemoveSpacesTabsNewLines(property)).Replace("\"", ""),
                Value = RemoveMultiSpaces(RemoveSpacesTabsNewLines(value)).Replace("\"", "")
            };
        }

        private static string RemoveMultiSpaces(string text)
        {
            const string reduceMultiSpace = @"[ ]{2,}";
            text = Regex.Replace(text.Replace("\t", " "), reduceMultiSpace, " ");
            return text;
        }

        private static string RemoveSpacesTabsNewLines(string text)
        {
            return Regex.Replace(text, @"\t|\n|\r", "").Trim();
        }

        private static string CleanItemFromTags(string item)
        {
            item = item.Replace("/>", string.Empty).Replace(">\"", string.Empty).Replace(">", string.Empty).Replace("\" \"", "\"");
            return item;
        }


        public static string GetValue(string value, string name = "")
        {
            if (value.EndsWith("px"))
            {
                value = value.Replace("px", "");
            }

            if (name.ToLower().Contains("fontfamily") && value.Contains("*"))
            {
                value = value.Replace("*", "");
            }


            if (value == "wrap_content")
                value = "FillAndExpand";
            else if (value == "fill_parent")
                value = "Fill";
            else if (value == "center")
                value = "Center";


            return value;
        }

        public static string GetProperty(string name)
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
                case "android:gravity":
                    return "HorizontalTextOptions";
                case "android:layout_width":
                    return "HorizontalOptions";
                case "android:layout_height":
                    return "VerticalOptions";
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

        public static string GetTargetType(Style xamarinFormsStyle, string item, out string firstStyle, string styleName)
        {
            string targetType;
            var targeTypes = item.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            targetType = targeTypes[0].Remove(0, 1);

            xamarinFormsStyle.TargetType = targetType;
            xamarinFormsStyle.Key = targetType.Replace(":", "") + "Style";
            firstStyle = "";
            if (styleName.Length > 0)
            {
                xamarinFormsStyle.Key = styleName;
            }
            else
            {
                if (targeTypes.Length > 1)
                {
                    var nameTypes = targeTypes[1].Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (nameTypes[0] == "x:Name" && nameTypes.Length > 1)
                    {
                        xamarinFormsStyle.Key = nameTypes[1].Replace("\"", string.Empty) + "Style";
                    }
                    else
                    {
                        firstStyle = targeTypes[1];
                    }
                }
            }

            return targetType;
        }

        public static string GetStyleString<T>(T xamarinFormsStyle)
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
    }
}
