﻿using System.Collections.Generic;
using System.Linq;
using CloneDeploy_DataModel;
using CloneDeploy_Entities;
using CloneDeploy_Entities.DTOs;

namespace CloneDeploy_Services
{
    public class ClusterGroupServices
    {
        private readonly UnitOfWork _uow;

        public ClusterGroupServices()
        {
            _uow = new UnitOfWork();
        }

        public ActionResultDTO AddClusterGroup(ClusterGroupEntity clusterGroup)
        {
            var actionResult = new ActionResultDTO();
            var validationResult = ValidateClusterGroup(clusterGroup, true);
            if (validationResult.Success)
            {
                _uow.ClusterGroupRepository.Insert(clusterGroup);
                _uow.Save();
                actionResult.Success = true;
                actionResult.Id = clusterGroup.Id;
            }
            else
            {
                actionResult.ErrorMessage = validationResult.ErrorMessage;
            }

            return actionResult;
        }

        public ActionResultDTO DeleteClusterGroup(int clusterGroupId)
        {
            var clusterGroup = GetClusterGroup(clusterGroupId);
            if (clusterGroup == null) return new ActionResultDTO {ErrorMessage = "Cluster Group Not Found", Id = 0};
            _uow.ClusterGroupRepository.Delete(clusterGroupId);
            _uow.Save();
            
            var sites = _uow.SiteRepository.Get(x => x.ClusterGroupId == clusterGroupId);
            var siteService = new SiteServices();
            foreach (var site in sites)
            {
                site.ClusterGroupId = -1;
                siteService.UpdateSite(site);
            }

            var buildings = _uow.BuildingRepository.Get(x => x.ClusterGroupId == clusterGroupId);
            var buildingService = new BuildingServices();
            foreach (var building in buildings)
            {
                building.ClusterGroupId = -1;
                buildingService.UpdateBuilding(building);
            }

            var rooms = _uow.RoomRepository.Get(x => x.ClusterGroupId == clusterGroupId);
            var roomService = new RoomServices();
            foreach (var room in rooms)
            {
                room.ClusterGroupId = -1;
                roomService.UpdateRoom(room);
            }

            var computers = _uow.ComputerRepository.Get(x => x.ClusterGroupId == clusterGroupId);
            var computerService = new ComputerServices();
            foreach (var computer in computers)
            {
                computer.ClusterGroupId = -1;
                computerService.UpdateComputer(computer);
            }

            var groups = _uow.GroupRepository.Get(x => x.ClusterGroupId == clusterGroupId);
            var groupService = new GroupServices();
            foreach (var group in groups)
            {
                group.ClusterGroupId = -1;
                groupService.UpdateGroup(group);
            }

            var groupProperties = _uow.GroupPropertyRepository.Get(x => x.ClusterGroupId == clusterGroupId);
            var groupPropertyService = new GroupPropertyServices();
            foreach (var groupProperty in groupProperties)
            {
                groupProperty.ClusterGroupId = -1;
                groupPropertyService.UpdateGroupProperty(groupProperty);
            }

            var actionResult = new ActionResultDTO();
            actionResult.Success = true;
            actionResult.Id = clusterGroup.Id;
            return actionResult;
        }

        public List<ClusterGroupDistributionPointEntity> GetClusterDps(int clusterId)
        {
            return _uow.ClusterGroupDistributionPointRepository.Get(x => x.ClusterGroupId == clusterId);
        }

        public ClusterGroupEntity GetClusterGroup(int clusterGroupId)
        {
            return _uow.ClusterGroupRepository.GetById(clusterGroupId);
        }

        public List<ClusterGroupServerEntity> GetClusterMulticastServers(int clusterId)
        {
            var multicastServers = _uow.ClusterGroupServersRepository.Get(x => x.ClusterGroupId == clusterId && x.MulticastRole == 1);
            return (from multicastServer in multicastServers let secServer = new SecondaryServerServices().GetSecondaryServer(multicastServer.ServerId) where secServer.IsActive == 1 select multicastServer).ToList();
        }

        public List<ClusterGroupServerEntity> GetClusterServers(int clusterId)
        {
           return _uow.ClusterGroupServersRepository.Get(x => x.ClusterGroupId == clusterId);
           
        }

        public List<ClusterGroupServerEntity> GetActiveClusterServers(int clusterId)
        {
            var clusterServers = _uow.ClusterGroupServersRepository.Get(x => x.ClusterGroupId == clusterId);
            return (from clusterServer in clusterServers let secServer = new SecondaryServerServices().GetSecondaryServer(clusterServer.ServerId) where secServer.IsActive == 1 select clusterServer).ToList();
        }

        public List<ClusterGroupServerEntity> GetClusterTftpServers(int clusterId)
        {
            var tftpServers = _uow.ClusterGroupServersRepository.Get(x => x.ClusterGroupId == clusterId && x.TftpRole == 1);
            return (from tftpServer in tftpServers let secServer = new SecondaryServerServices().GetSecondaryServer(tftpServer.ServerId) where secServer.IsActive == 1 select tftpServer).ToList();
        }

        public ClusterGroupEntity GetDefaultClusterGroup()
        {
            return _uow.ClusterGroupRepository.GetFirstOrDefault(x => x.Default == 1);
        }

        public List<ClusterGroupEntity> SearchClusterGroups(string searchString = "")
        {
            return _uow.ClusterGroupRepository.Get(s => s.Name.Contains(searchString));
        }

        public string TotalCount()
        {
            return _uow.ClusterGroupRepository.Count();
        }

        public ActionResultDTO UpdateClusterGroup(ClusterGroupEntity clusterGroup)
        {
            var s = GetClusterGroup(clusterGroup.Id);
            if (s == null) return new ActionResultDTO {ErrorMessage = "Cluster Group Not Found", Id = 0};
            var validationResult = ValidateClusterGroup(clusterGroup, false);
            var actionResult = new ActionResultDTO();
            if (validationResult.Success)
            {
                _uow.ClusterGroupRepository.Update(clusterGroup, clusterGroup.Id);
                _uow.Save();
                actionResult.Success = true;
                actionResult.Id = clusterGroup.Id;
            }
            else
            {
                actionResult.ErrorMessage = validationResult.ErrorMessage;
            }

            return actionResult;
        }

        private ValidationResultDTO ValidateClusterGroup(ClusterGroupEntity clusterGroup, bool isNewClusterGroup)
        {
            var validationResult = new ValidationResultDTO {Success = true};

            if (string.IsNullOrEmpty(clusterGroup.Name))
            {
                validationResult.Success = false;
                validationResult.ErrorMessage = "Cluster Group Name Is Not Valid";
                return validationResult;
            }

            if (clusterGroup.Name.ToLower() == "default")
            {
                validationResult.Success = false;
                validationResult.ErrorMessage = "Cluster Group Name Is Not Valid.  Default Is Reserved";
                return validationResult;
            }

            if (clusterGroup.Default == 1)
            {
                var existingDefaultCluster = GetDefaultClusterGroup();
                if (existingDefaultCluster != null && existingDefaultCluster.Id != clusterGroup.Id)
                {
                    validationResult.Success = false;
                    validationResult.ErrorMessage = "Only 1 Default Cluster Group Can Exist";
                    return validationResult;
                }
            }

            if (isNewClusterGroup)
            {
                if (_uow.ClusterGroupRepository.Exists(h => h.Name == clusterGroup.Name))
                {
                    validationResult.Success = false;
                    validationResult.ErrorMessage = "This Cluster Group Already Exists";
                    return validationResult;
                }
            }
            else
            {
                var originalClusterGroup = _uow.ClusterGroupRepository.GetById(clusterGroup.Id);
                if (originalClusterGroup.Name != clusterGroup.Name)
                {
                    if (_uow.ClusterGroupRepository.Exists(h => h.Name == clusterGroup.Name))
                    {
                        validationResult.Success = false;
                        validationResult.ErrorMessage = "This Cluster Group Already Exists";
                        return validationResult;
                    }
                }
            }

            return validationResult;
        }
    }
}