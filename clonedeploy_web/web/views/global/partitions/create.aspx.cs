﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Global;
using Models;

public partial class views_global_partitions_create : BasePages.Global
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnSubmit_OnClick(object sender, EventArgs e)
    {
        var layout = new PartitionLayout
        {
           Name = txtLayoutName.Text,
           Table = ddlLayoutType.Text,
           ImageEnvironment = ddlEnvironment.Text,
           Priority = Convert.ToInt32(txtPriority.Text)
           
        };

        new BLL.PartitionLayout().AddPartitionLayout(layout);
     
    }
}