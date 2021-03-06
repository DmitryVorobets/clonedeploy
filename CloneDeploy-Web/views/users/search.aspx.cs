﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CloneDeploy_Common;
using CloneDeploy_Entities;
using CloneDeploy_Web.BasePages;

namespace CloneDeploy_Web.views.users
{
    public partial class UserSearch : Users
    {
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            var deletedCount = 0;
            var adminMessage = string.Empty;
            foreach (GridViewRow row in gvUsers.Rows)
            {
                var cb = (CheckBox) row.FindControl("chkSelector");
                if (cb == null || !cb.Checked) continue;
                var dataKey = gvUsers.DataKeys[row.RowIndex];
                if (dataKey != null)
                {
                    var user = Call.CloneDeployUserApi.Get(Convert.ToInt32(dataKey.Value));

                    if (user.Membership == "Administrator")
                    {
                        adminMessage =
                            "<br/>Administrators Must Be Changed To A User Before They Can Be Deleted";
                        break;
                    }
                    if (Call.CloneDeployUserApi.Delete(user.Id).Success)
                        deletedCount++;
                }
            }
            EndUserMessage = "Successfully Deleted " + deletedCount + " Users" + adminMessage;
            PopulateGrid();
        }

        protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            ChkAll(gvUsers);
        }

        protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            PopulateGrid();
            var listUsers = (List<CloneDeployUserEntity>) gvUsers.DataSource;
            switch (e.SortExpression)
            {
                case "Name":
                    listUsers = GetSortDirection(e.SortExpression) == "Asc"
                        ? listUsers.OrderBy(g => g.Name).ToList()
                        : listUsers.OrderByDescending(g => g.Name).ToList();
                    break;
                case "Membership":
                    listUsers = GetSortDirection(e.SortExpression) == "Asc"
                        ? listUsers.OrderBy(g => g.Membership).ToList()
                        : listUsers.OrderByDescending(g => g.Membership).ToList();
                    break;
            }

            gvUsers.DataSource = listUsers;
            gvUsers.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CloneDeployCurrentUser.Membership != "Administrator")
            {
                Session["UserId"] = CloneDeployCurrentUser.Id.ToString();
                Response.Redirect("~/views/users/resetpass.aspx", true);
            }

            //Just In Case
            RequiresAuthorization(AuthorizationStrings.Administrator);

            if (IsPostBack) return;
            PopulateGrid();
        }

        protected void PopulateGrid()
        {
            gvUsers.DataSource = Call.CloneDeployUserApi.Get(int.MaxValue, txtSearch.Text);
            gvUsers.DataBind();
            lblTotal.Text = gvUsers.Rows.Count + " Result(s) / " + Call.CloneDeployUserApi.GetCount() + " Total User(s)";
        }

        protected void search_Changed(object sender, EventArgs e)
        {
            PopulateGrid();
        }
    }
}