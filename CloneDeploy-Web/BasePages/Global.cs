﻿using System;
using CloneDeploy_Common;
using CloneDeploy_Entities;

namespace CloneDeploy_Web.BasePages
{
    public class Global : PageBaseMaster
    {
        public BootTemplateEntity BootTemplate { get; set; }
        public FileFolderEntity FileFolder { get; set; }
        public SysprepTagEntity SysprepTag { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SysprepTag = !string.IsNullOrEmpty(Request["syspreptagid"])
                ? Call.SysprepTagApi.Get(Convert.ToInt32(Request.QueryString["syspreptagid"]))
                : null;
            BootTemplate = !string.IsNullOrEmpty(Request["templateid"])
                ? Call.BootTemplateApi.Get(Convert.ToInt32(Request.QueryString["templateid"]))
                : null;

            FileFolder = !string.IsNullOrEmpty(Request["fileid"])
                ? Call.FileFolderApi.Get(Convert.ToInt32(Request.QueryString["fileid"]))
                : null;
       

            RequiresAuthorization(AuthorizationStrings.ReadGlobal);
        }
    }
}