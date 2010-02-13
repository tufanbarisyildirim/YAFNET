﻿namespace YAF.Controls
{
   
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using YAF.Classes.Core;


   
    public class RecaptchaControl : WebControl, IValidator
    {
        private bool allowMultipleInstances = YafContext.Current.BoardSettings.RecaptureMultipleInstances;
        private string customThemeWidget;
        private string errorMessage;
        private bool overrideSecureMode;
        private string privateKey = YafContext.Current.BoardSettings.RecaptchaPrivateKey; 
            // ConfigurationManager.AppSettings["RecaptchaPrivateKey"];
        private string publicKey = YafContext.Current.BoardSettings.RecaptchaPublicKey;
            // ConfigurationManager.AppSettings["RecaptchaPublicKey"];
        private const string RECAPTCHA_CHALLENGE_FIELD = "recaptcha_challenge_field";
        private const string RECAPTCHA_HOST = "http://api.recaptcha.net";
        private const string RECAPTCHA_RESPONSE_FIELD = "recaptcha_response_field";
        private const string RECAPTCHA_SECURE_HOST = "https://api-secure.recaptcha.net";
        private RecaptchaResponse recaptchaResponse;
        private bool skipRecaptcha;
        private string theme;

        public RecaptchaControl()
        {
           /* if (!bool.TryParse(ConfigurationManager.AppSettings["RecaptchaSkipValidation"], out this.skipRecaptcha))
            {
                this.skipRecaptcha = false;
            }
            */
            this.skipRecaptcha = false;
        }

        private bool CheckIfRecaptchaExists()
        {
            foreach (object obj2 in this.Page.Validators)
            {
                if (obj2 is RecaptchaControl)
                {
                    return true;
                }
            }
            return false;
        }

        private string GenerateChallengeUrl(bool noScript)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((this.Context.Request.IsSecureConnection || this.overrideSecureMode) ? "https://api-secure.recaptcha.net" : "http://api.recaptcha.net");
            builder.Append(noScript ? "/noscript?" : "/challenge?");
            builder.AppendFormat("k={0}", this.PublicKey);
            if ((this.recaptchaResponse != null) && (this.recaptchaResponse.ErrorCode != string.Empty))
            {
                builder.AppendFormat("&error={0}", this.recaptchaResponse.ErrorCode);
            }
            return builder.ToString();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (string.IsNullOrEmpty(this.PublicKey) || string.IsNullOrEmpty(this.PrivateKey))
            {
                throw new ApplicationException("reCAPTCHA needs to be configured with a public & private key.");
            }
            if (this.allowMultipleInstances || !this.CheckIfRecaptchaExists())
            {
                this.Page.Validators.Add(this);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.skipRecaptcha)
            {
                writer.WriteLine("reCAPTCHA validation is skipped. Set SkipRecaptcha property to false to enable validation.");
            }
            else
            {
                this.RenderContents(writer);
            }
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            output.RenderBeginTag(HtmlTextWriterTag.Script);
            output.Indent++;
            output.WriteLine("var RecaptchaOptions = {");
            output.Indent++;
            output.WriteLine("theme : '{0}',", this.theme ?? string.Empty);
            if (this.customThemeWidget != null)
            {
                output.WriteLine("custom_theme_widget : '{0}',", this.customThemeWidget);
            }
            output.WriteLine("tabindex : {0}", this.TabIndex);
            output.Indent--;
            output.WriteLine("};");
            output.Indent--;
            output.RenderEndTag();
            output.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            output.AddAttribute(HtmlTextWriterAttribute.Src, this.GenerateChallengeUrl(false), false);
            output.RenderBeginTag(HtmlTextWriterTag.Script);
            output.RenderEndTag();
            output.RenderBeginTag(HtmlTextWriterTag.Noscript);
            output.Indent++;
            output.AddAttribute(HtmlTextWriterAttribute.Src, this.GenerateChallengeUrl(true), false);
            output.AddAttribute(HtmlTextWriterAttribute.Width, "500");
            output.AddAttribute(HtmlTextWriterAttribute.Height, "300");
            output.AddAttribute("frameborder", "0");
            output.RenderBeginTag(HtmlTextWriterTag.Iframe);
            output.RenderEndTag();
            output.WriteBreak();
            output.AddAttribute(HtmlTextWriterAttribute.Name, "recaptcha_challenge_field");
            output.AddAttribute(HtmlTextWriterAttribute.Rows, "3");
            output.AddAttribute(HtmlTextWriterAttribute.Cols, "40");
            output.RenderBeginTag(HtmlTextWriterTag.Textarea);
            output.RenderEndTag();
            output.AddAttribute(HtmlTextWriterAttribute.Name, "recaptcha_response_field");
            output.AddAttribute(HtmlTextWriterAttribute.Value, "manual_challenge");
            output.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();
            output.Indent--;
            output.RenderEndTag();
        }

        public void Validate()
        {
            if (this.skipRecaptcha)
            {
                this.recaptchaResponse = RecaptchaResponse.Valid;
            }
            if (((this.recaptchaResponse == null) && this.Visible) && this.Enabled)
            {
                RecaptchaValidator validator = new RecaptchaValidator();
                validator.PrivateKey = this.PrivateKey;
                validator.RemoteIP = this.Page.Request.UserHostAddress;
                validator.Challenge = this.Context.Request.Form["recaptcha_challenge_field"];
                validator.Response = this.Context.Request.Form["recaptcha_response_field"];
                try
                {
                    this.recaptchaResponse = validator.Validate();
                }
                catch (ArgumentNullException exception)
                {
                    this.recaptchaResponse = null;
                    this.errorMessage = exception.Message;
                }
            }
        }

        [Category("Settings"), DefaultValue(false), Description("Set this to true to enable multiple reCAPTCHA on a single page. There may be complication between controls when this is enabled.")]
        public bool AllowMultipleInstances
        {
            get
            {
                return this.allowMultipleInstances;
            }
            set
            {
                this.allowMultipleInstances = value;
            }
        }

        [Category("Appearence"), Description("When using custom theming, this is a div element which contains the widget. "), DefaultValue((string) null)]
        public string CustomThemeWidget
        {
            get
            {
                return this.customThemeWidget;
            }
            set
            {
                this.customThemeWidget = value;
            }
        }

        [DefaultValue("The verification words are incorrect."), Localizable(true)]
        public string ErrorMessage
        {
            get
            {
                if (this.errorMessage != null)
                {
                    return this.errorMessage;
                }
                return "The verification words are incorrect.";
            }
            set
            {
                this.errorMessage = value;
            }
        }

        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                if ((!this.Page.IsPostBack || !this.Visible) || (!this.Enabled || this.skipRecaptcha))
                {
                    return true;
                }
                if (this.recaptchaResponse == null)
                {
                    this.Validate();
                }
                return ((this.recaptchaResponse != null) && this.recaptchaResponse.IsValid);
            }
            set
            {
                throw new NotImplementedException("This setter is not implemented.");
            }
        }

        [Category("Settings"), Description("Set this to true to override reCAPTCHA usage of Secure API."), DefaultValue(false)]
        public bool OverrideSecureMode
        {
            get
            {
                return this.overrideSecureMode;
            }
            set
            {
                this.overrideSecureMode = value;
            }
        }

        [Description("The private key from admin.recaptcha.net. Can also be set using RecaptchaPrivateKey in AppSettings."), Category("Settings")]
        public string PrivateKey
        {
            get
            {
                return this.privateKey;
            }
            set
            {
                this.privateKey = value;
            }
        }

        [Category("Settings"), Description("The public key from admin.recaptcha.net. Can also be set using RecaptchaPublicKey in AppSettings.")]
        public string PublicKey
        {
            get
            {
                return this.publicKey;
            }
            set
            {
                this.publicKey = value;
            }
        }

        [Description("Set this to true to stop reCAPTCHA validation. Useful for testing platform. Can also be set using RecaptchaSkipValidation in AppSettings."), DefaultValue(false), Category("Settings")]
        public bool SkipRecaptcha
        {
            get
            {
                return this.skipRecaptcha;
            }
            set
            {
                this.skipRecaptcha = value;
            }
        }

        [DefaultValue("red"), Description("The theme for the reCAPTCHA control. Currently supported values are red, blackglass, white, and clean"), Category("Appearence")]
        public string Theme
        {
            get
            {
                return this.theme;
            }
            set
            {
                this.theme = value;
            }
        }
    }
}
