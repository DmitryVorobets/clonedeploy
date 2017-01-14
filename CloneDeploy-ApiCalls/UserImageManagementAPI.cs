﻿using System.Collections.Generic;
using CloneDeploy_Entities;
using CloneDeploy_Entities.DTOs;
using RestSharp;

namespace CloneDeploy_ApiCalls
{
    public class UserImageManagementAPI : GenericAPI<UserImageManagementEntity>
    {
        public UserImageManagementAPI(string resource):base(resource)
        {
		
        }
      

        public ActionResultDTO Post(List<UserImageManagementEntity> listOfImages)
        {
            _request.Method = Method.POST;
            _request.Resource = string.Format("api/{0}/Post/", _resource);
            _request.AddJsonBody(listOfImages);
            return new ApiRequest().Execute<ActionResultDTO>(_request);
        }

        
    }
}