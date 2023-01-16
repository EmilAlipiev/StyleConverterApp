using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleConverterApp.Models
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Label
    {
        private string marginField;

        private string fontFamilyField;

        private byte fontSizeField;

        private string horizontalOptionsField;

        private string horizontalTextAlignmentField;

        private string textColorField;

        private string verticalOptionsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Margin
        {
            get
            {
                return this.marginField;
            }
            set
            {
                this.marginField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FontFamily
        {
            get
            {
                return this.fontFamilyField;
            }
            set
            {
                this.fontFamilyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte FontSize
        {
            get
            {
                return this.fontSizeField;
            }
            set
            {
                this.fontSizeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string HorizontalOptions
        {
            get
            {
                return this.horizontalOptionsField;
            }
            set
            {
                this.horizontalOptionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string HorizontalTextAlignment
        {
            get
            {
                return this.horizontalTextAlignmentField;
            }
            set
            {
                this.horizontalTextAlignmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TextColor
        {
            get
            {
                return this.textColorField;
            }
            set
            {
                this.textColorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string VerticalOptions
        {
            get
            {
                return this.verticalOptionsField;
            }
            set
            {
                this.verticalOptionsField = value;
            }
        }
    }


}
