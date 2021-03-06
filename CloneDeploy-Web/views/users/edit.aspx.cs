﻿using System;
using CloneDeploy_Common;
using CloneDeploy_Web.BasePages;

namespace CloneDeploy_Web.views.users
{
    public partial class EditUser : Users
    {
        protected void btnGenKey_OnClick(object sender, EventArgs e)
        {
            txtToken.Text = Utility.GenerateKey();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Call.CloneDeployUserApi.GetAdminCount() == 1 && ddluserMembership.Text != "Administrator" &&
                CloneDeployUser.Membership == "Administrator")
            {
                EndUserMessage = "There Must Be At Least One Administrator";
                return;
            }

            if (CloneDeployUser.UserGroupId != -1 && ddluserMembership.Text != CloneDeployUser.Membership)
            {
                EndUserMessage = "This User's Role Is Controlled By A Group";
                return;
            }

            var updatedUser = CloneDeployUser;
            if (!string.IsNullOrEmpty(txtUserPwd.Text))
            {
                if (txtUserPwd.Text == txtUserPwdConfirm.Text)
                {
                    updatedUser.Salt = Utility.CreateSalt(64);
                    updatedUser.Password = Utility.CreatePasswordHash(txtUserPwd.Text, updatedUser.Salt);
                }
                else
                {
                    EndUserMessage = "Passwords Did Not Match";
                    return;
                }
            }

            updatedUser.Name = txtUserName.Text;
            updatedUser.Membership = ddluserMembership.Text;
            updatedUser.Email = txtEmail.Text;
            updatedUser.Token = txtToken.Text;
            updatedUser.NotifyLockout = chkLockout.Checked ? 1 : 0;
            updatedUser.NotifyError = chkError.Checked ? 1 : 0;
            updatedUser.NotifyComplete = chkComplete.Checked ? 1 : 0;
            updatedUser.NotifyImageApproved = chkApproved.Checked ? 1 : 0;
            updatedUser.NotifyServerStatusChange = chkSecServer.Checked ? 1 : 0;
            var result = Call.CloneDeployUserApi.Put(updatedUser.Id, updatedUser);
            EndUserMessage = !result.Success ? result.ErrorMessage : "Successfully Updated User";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RequiresAuthorization(AuthorizationStrings.Administrator);
            if (!IsPostBack) PopulateForm();
        }

        protected void PopulateForm()
        {
            if (CloneDeployUser.IsLdapUser == 1)
            {
                chkldap.Checked = true;
                passwords.Visible = false;
            }
            txtUserName.Text = CloneDeployUser.Name;
            ddluserMembership.Text = CloneDeployUser.Membership;
            txtEmail.Text = CloneDeployUser.Email;
            txtToken.Text = CloneDeployUser.Token;
            chkLockout.Checked = CloneDeployUser.NotifyLockout == 1;
            chkError.Checked = CloneDeployUser.NotifyError == 1;
            chkComplete.Checked = CloneDeployUser.NotifyComplete == 1;
            chkApproved.Checked = CloneDeployUser.NotifyImageApproved == 1;
            chkSecServer.Checked = CloneDeployUser.NotifyServerStatusChange == 1;
        }
    }
}