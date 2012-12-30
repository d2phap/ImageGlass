/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2012 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass
{
    public partial class frmFaceBookLogin : Form
    {
        private readonly Uri _loginUrl;

        public Facebook.FacebookOAuthResult FacebookOAuthResult { get; private set; }

        public frmFaceBookLogin(string appId, string extendedPermissions)
        {
            if (string.IsNullOrEmpty(appId))
                throw new ArgumentNullException("appId");

            // Make sure to set the app id.
            var oauthClient = new Facebook.FacebookOAuthClient { AppId = appId };

            IDictionary<string, object> loginParameters = new Dictionary<string, object>();
            
            // The requested response: an access token (token), an authorization code (code), or both (code token).
            loginParameters["response_type"] = "token";

            // list of additional display modes can be found at http://developers.facebook.com/docs/reference/dialogs/#display
            loginParameters["display"] = "popup";

            // add the 'scope' parameter only if we have extendedPermissions.
            if (!string.IsNullOrEmpty(extendedPermissions))
            {
                // A comma-delimited list of permissions
                loginParameters["scope"] = extendedPermissions;
            }

            // when the Form is loaded navigate to the login url.
            _loginUrl = oauthClient.GetLoginUrl(loginParameters);

            InitializeComponent();
        }

       
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // whenever the browser navigates to a new url, try parsing the url
            // the url may be the result of OAuth 2.0 authentication.

            Facebook.FacebookOAuthResult oauthResult;
            if (Facebook.FacebookOAuthResult.TryParse(e.Url, out oauthResult))
            {
                // The url is the result of OAuth 2.0 authentication.
                this.FacebookOAuthResult = oauthResult;
                this.DialogResult = FacebookOAuthResult.IsSuccess ? DialogResult.OK : DialogResult.No;
            }
            else
            {
                // The url is NOT the result of OAuth 2.0 authentication.
                this.FacebookOAuthResult = null;
            }
        }

        private void frmFaceBookLogin_Load(object sender, EventArgs e)
        {
            // make sure to use the AbsoluteUri.
            webBrowser1.Navigate(_loginUrl.AbsoluteUri);
        }
    }
}
