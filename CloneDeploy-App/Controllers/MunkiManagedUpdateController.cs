﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CloneDeploy_App.Controllers.Authorization;
using CloneDeploy_App.DTOs;
using CloneDeploy_Entities;


namespace CloneDeploy_App.Controllers
{
    public class MunkiManagedUpdateController: ApiController
    {
        [GlobalAuth(Permission = "GlobalRead")]
        public MunkiManifestManagedUpdateEntity Get(int id)
        {

            return BLL.MunkiManagedUpdate.GetManagedUpdate(id);

        }

        [GlobalAuth(Permission = "GlobalCreate")]
        public ApiBoolDTO Post(MunkiManifestManagedUpdateEntity managedInstall)
        {
            var apiBoolDto = new ApiBoolDTO();
            apiBoolDto.Value = BLL.MunkiManagedUpdate.AddManagedUpdateToTemplate(managedInstall);
          
            return apiBoolDto;
        }

       

        [GlobalAuth(Permission = "GlobalDelete")]
        public ApiBoolDTO Delete(int id)
        {
            var apiBoolDto = new ApiBoolDTO();
            apiBoolDto.Value = BLL.MunkiManagedUpdate.DeleteManagedUpdateFromTemplate(id);
           return apiBoolDto;
        }
    }
}