﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CloneDeploy_Entities;
using CloneDeploy_Web.Helpers;

namespace CloneDeploy_Web.views.admin.cluster
{
    public partial class editsecondary : BasePages.Admin
    {
        public SecondaryServerEntity SecondaryServer { get { return Read(); } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) PopulateForm();
        }

      

        protected void PopulateForm()
        {
            lblServerId.Text = SecondaryServer.Name;
            txtApi.Text = SecondaryServer.ApiURL;
            txtAccountName.Text = SecondaryServer.ServiceAccountName;
            txtAccountPassword.Text = SecondaryServer.ServiceAccountPassword;
            lblImage.Text = SecondaryServer.ImageRole == 1 ? "Yes" : "No";
            lblTftp.Text = SecondaryServer.TftpRole == 1 ? "Yes" : "No";
            lblMulticast.Text = SecondaryServer.MulticastRole == 1 ? "Yes" : "No";

        }

        private SecondaryServerEntity Read()
        {
            return Call.SecondaryServerApi.Get(Convert.ToInt32(Request.QueryString["ssid"]));

        }

        protected void btnUpdateSettings_OnClick(object sender, EventArgs e)
        {
            RequiresAuthorization(Authorizations.UpdateAdmin);
            var secondaryServer = new SecondaryServerEntity()
            {
                ApiURL = txtApi.Text,
                ServiceAccountName = txtAccountName.Text,
                ServiceAccountPassword = txtAccountPassword.Text
            };

            var result = Call.SecondaryServerApi.Put(SecondaryServer.Id, secondaryServer);
            if (result.Success)
            {
                EndUserMessage = "Successfully Updated Secondary Server";
                PopulateForm();

            }
            else
            {
                EndUserMessage = result.ErrorMessage;
            }
        }

       
    }
}