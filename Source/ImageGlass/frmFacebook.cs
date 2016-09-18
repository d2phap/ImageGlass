/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2015 DUONG DIEU PHAP
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

using Facebook;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using ImageGlass.Services.Configuration;

namespace ImageGlass
{
    public partial class frmFacebook : Form
    {
        public frmFacebook()
        {
            InitializeComponent();
        }

        
        private const string AppId = "435065586551768";
        private const string ExtendedPermissions = "user_about_me,publish_actions";
        private FacebookClient _fb;
        private string _filename = "";

        /// <summary>
        /// Get, set filename to upload
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            set
            {
                if (File.Exists(value))
                {
                    _filename = value;
                    Text = _filename;
                }
            }
        }

        private void frmFacebook_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalSetting.IsForcedActive = true;
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            //Do uploading
            if (btnUpload.Tag.ToString() == "0")
            {
                txtMessage.Enabled = false;

                btnUpload.Text = GlobalSetting.LangPack.Items["frmFacebook.btnUpload._Cancel"];
                btnUpload.Tag = 1;
                UploadPhoto();
            }
            else if (btnUpload.Tag.ToString().Length > 1)
            {
                string photoLink = "https://www.facebook.com/photo.php?fbid=" + btnUpload.Tag.ToString();
                System.Diagnostics.Process.Start(photoLink);
            }
            else //Do cancellation
            {
                txtMessage.Enabled = true;
                btnUpload.Text = GlobalSetting.LangPack.Items["frmFacebook.btnUpload._Upload"];
                btnUpload.Tag = 0;
                lblStatus.Text = string.Format(GlobalSetting.LangPack.Items["frmFacebook._StatusBegin"], btnUpload.Text);

                if (_fb != null)
                {
                    _fb.CancelAsync();
                }
            }
        }

        private void frmFacebook_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GlobalSetting.FacebookAccessToken.Trim()))
            {
                // open the Facebook Login Dialog and ask for user permissions.
                var fbLoginDlg = new frmFaceBookLogin(AppId, ExtendedPermissions);
                fbLoginDlg.ShowDialog();

                // The user has taken action, either allowed/denied or cancelled the authorization,
                // which can be known by looking at the dialogs FacebookOAuthResult property.
                // Depending on the result take appropriate actions.
                TakeLoggedInAction(fbLoginDlg.FacebookOAuthResult);
            }

            //Load language
            lblMessage.Text = GlobalSetting.LangPack.Items["frmFacebook.lblMessage"];
            btnClose.Text = GlobalSetting.LangPack.Items["frmFacebook.btnClose"];
            btnUpload.Text = GlobalSetting.LangPack.Items["frmFacebook.btnUpload._Upload"];
            lblStatus.Text = string.Format(GlobalSetting.LangPack.Items["frmFacebook._StatusBegin"],
                                            btnUpload.Text);
            lblPercent.Text = "";
        }

        private void TakeLoggedInAction(Facebook.FacebookOAuthResult facebookOAuthResult)
        {
            if (facebookOAuthResult == null)
            {
                // the user closed the FacebookLoginDialog, so do nothing.
                // MessageBox.Show("Cancelled!");
                Close();
                return;
            }

            // Even though facebookOAuthResult is not null, it could had been an 
            // OAuth 2.0 error, so make sure to check IsSuccess property always.
            if (facebookOAuthResult.IsSuccess)
            {
                // since our respone_type in FacebookLoginDialog was token,
                // we got the access_token
                // The user now has successfully granted permission to our app.
                GlobalSetting.FacebookAccessToken = facebookOAuthResult.AccessToken;
            }
            else
            {
                // for some reason we failed to get the access token.
                // most likely the user clicked don't allow.
                MessageBox.Show(facebookOAuthResult.ErrorDescription);
                Close();
            }
        }

       
        private void UploadPhoto()
        {
            if (!File.Exists(_filename))
            {
                lblStatus.Text = GlobalSetting.LangPack.Items["frmFacebook._StatusInvalid"];
                return;
            }

            var mediaObject = new FacebookMediaObject
            {
                ContentType = "image/jpeg",
                FileName = Path.GetFileName(_filename)
            }.SetValue(File.ReadAllBytes(_filename));
            
            lblPercent.Text = "0 %";
            picStatus.Visible = true;
            lblStatus.Text = GlobalSetting.LangPack.Items["frmFacebook._StatusUploading"];

            var fb = new FacebookClient(GlobalSetting.FacebookAccessToken);
            fb.UploadProgressChanged += fb_UploadProgressChanged;
            fb.PostCompleted += fb_PostCompleted;

            // for cancellation
            _fb = fb;
            
            fb.PostTaskAsync("/me/photos", new Dictionary<string, object> 
            { 
                { "source", mediaObject }, 
                { "message", txtMessage.Text.Trim() }
            });
        }

        public void fb_UploadProgressChanged(object sender, FacebookUploadProgressChangedEventArgs e)
        {
            lblPercent.BeginInvoke(new MethodInvoker(() =>
            {
                lblPercent.Text = e.ProgressPercentage.ToString() + " %";
            }));
        }

        public void fb_PostCompleted(object sender, FacebookApiEventArgs e)
        {
            btnUpload.Tag = 0;
            picStatus.Visible = false;
            btnUpload.Text = GlobalSetting.LangPack.Items["frmFacebook.btnUpload._Upload"];

            if (e.Cancelled)
            {
                lblStatus.Text = GlobalSetting.LangPack.Items["frmFacebook._StatusCancel"];
            }
            else if (e.Error == null)
            {
                // upload successful.
                lblStatus.Text = GlobalSetting.LangPack.Items["frmFacebook._StatusSuccessful"];
                btnUpload.Tag = ((IDictionary<string, object>)e.GetResultData())["id"].ToString(); //Get Post ID
                btnUpload.Text = GlobalSetting.LangPack.Items["frmFacebook.btnUpload._ViewImage"];
            }
            else
            {
                // upload failed
                lblStatus.Text = e.Error.Message;
                
            }            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
