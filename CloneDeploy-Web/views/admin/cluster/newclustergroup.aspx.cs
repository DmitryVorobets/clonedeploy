﻿using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using CloneDeploy_Common;
using CloneDeploy_Entities;
using CloneDeploy_Web.BasePages;

namespace CloneDeploy_Web.views.admin.cluster
{
    public partial class newclustergroup : Admin
    {
        protected void btnAddCluster_OnClick(object sender, EventArgs e)
        {
            RequiresAuthorization(AuthorizationStrings.UpdateAdmin);
            var clusterGroup = new ClusterGroupEntity
            {
                Name = txtClusterName.Text,
                Default = chkDefault.Checked ? 1 : 0
            };

            var result = Call.ClusterGroupApi.Post(clusterGroup);
            if (result.Success)
            {
                var listOfServers = new List<ClusterGroupServerEntity>();
                foreach (GridViewRow row in gvServers.Rows)
                {
                    var cb = (CheckBox) row.FindControl("chkSelector");
                    if (!cb.Checked) continue;
                    var cbTftp = (CheckBox) row.FindControl("chkTftp");
                    var cbMulticast = (CheckBox) row.FindControl("chkMulticast");
                    var dataKey = gvServers.DataKeys[row.RowIndex];
                    if (dataKey == null) continue;

                    var clusterGroupServer = new ClusterGroupServerEntity();
                    clusterGroupServer.ClusterGroupId = result.Id;
                    clusterGroupServer.ServerId = Convert.ToInt32(dataKey.Value);

                    if (cbTftp.Checked)
                        clusterGroupServer.TftpRole = 1;
                    if (cbMulticast.Checked)
                        clusterGroupServer.MulticastRole = 1;

                    listOfServers.Add(clusterGroupServer);
                }

                Call.ClusterGroupServerApi.Post(listOfServers);

                var listOfDps = new List<ClusterGroupDistributionPointEntity>();
                foreach (GridViewRow row in gvDps.Rows)
                {
                    var cb = (CheckBox) row.FindControl("chkSelector");
                    if (!cb.Checked) continue;

                    var dataKey = gvDps.DataKeys[row.RowIndex];
                    if (dataKey == null) continue;

                    var clusterGroupDistributionPoint = new ClusterGroupDistributionPointEntity();
                    clusterGroupDistributionPoint.ClusterGroupId = result.Id;
                    clusterGroupDistributionPoint.DistributionPointId = Convert.ToInt32(dataKey.Value);

                    listOfDps.Add(clusterGroupDistributionPoint);
                }

                Call.ClusterGroupDistributionPointApi.Post(listOfDps);

                EndUserMessage = "Successfully Created Cluster Group";
                Response.Redirect("~/views/admin/cluster/editcluster.aspx?cat=sub1&clusterid=" + result.Id);
            }
            else
            {
                EndUserMessage = result.ErrorMessage;
            }
        }

        protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            ChkAll(gvServers);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            PopulateGrid();
        }

        protected void PopulateGrid()
        {
            var secondaryServers = Call.SecondaryServerApi.Get(int.MaxValue, "");
            if (GetSetting(SettingStrings.OperationMode) == "Cluster Primary")
            {
                var primary = new SecondaryServerEntity();
                primary.Id = -1;
                primary.Name = GetSetting(SettingStrings.ServerIdentifier);

                primary.TftpRole = Convert.ToBoolean(Convert.ToInt16(GetSetting(SettingStrings.TftpServerRole))) ? 1 : 0;
                primary.MulticastRole =
                    Convert.ToBoolean(Convert.ToInt16(GetSetting(SettingStrings.MulticastServerRole))) ? 1 : 0;
                secondaryServers.Insert(0, primary);
            }
            gvServers.DataSource = secondaryServers;
            gvServers.DataBind();

            foreach (GridViewRow row in gvServers.Rows)
            {
                var cbTftp = (CheckBox) row.FindControl("chkTftp");
                var cbMulticast = (CheckBox) row.FindControl("chkMulticast");
                var dataKey = gvServers.DataKeys[row.RowIndex];
                if (dataKey == null) continue;
                if (Convert.ToInt32(dataKey.Value) == -1)
                {
                    cbTftp.Visible = Convert.ToBoolean(Convert.ToInt16(GetSetting(SettingStrings.TftpServerRole)));
                    cbMulticast.Visible =
                        Convert.ToBoolean(Convert.ToInt16(GetSetting(SettingStrings.MulticastServerRole)));
                }
                else
                {
                    var secondaryServer = Call.SecondaryServerApi.Get(Convert.ToInt32(dataKey.Value));

                    if (secondaryServer.TftpRole != 1)
                        cbTftp.Visible = false;
                    if (secondaryServer.MulticastRole != 1)
                        cbMulticast.Visible = false;
                }
            }

            gvDps.DataSource = Call.DistributionPointApi.Get(int.MaxValue, "");
            gvDps.DataBind();
        }
    }
}