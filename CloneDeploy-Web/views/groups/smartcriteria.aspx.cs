﻿using System;
using System.Web.UI.WebControls;
using CloneDeploy_Common;
using CloneDeploy_Web.BasePages;

namespace CloneDeploy_Web.views.groups
{
    public partial class views_groups_smartcriteria : Groups
    {
        protected void btnTestQuery_OnClick(object sender, EventArgs e)
        {
            gvComputers.DataSource = Call.ComputerApi.TestSmartQuery(ddlSmartType.Text, int.MaxValue, txtContains.Text);
            gvComputers.DataBind();
            lblTotal.Text = gvComputers.Rows.Count + " Result(s)";
        }

        protected void btnUpdate_OnClick(object sender, EventArgs e)
        {
            RequiresAuthorizationOrManagedGroup(AuthorizationStrings.UpdateGroup, Group.Id);
            RequiresAuthorization(AuthorizationStrings.UpdateSmart);
            var group = Group;
            group.SmartCriteria = txtContains.Text;
            group.SmartType = ddlSmartType.Text;
            var result = Call.GroupApi.Put(group.Id, group);
            EndUserMessage = result.Success ? "Successfully Updated Smart Criteria" : result.ErrorMessage;
            Call.GroupApi.UpdateSmartMembership(group.Id);
        }

        protected void gvComputers_OnSorting(object sender, GridViewSortEventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) PopulateForm();
        }

        protected void PopulateForm()
        {
            if (Group.Type == "smart")
            {
                txtContains.Text = Group.SmartCriteria;
                ddlSmartType.Text = Group.SmartType;
            }
        }
    }
}