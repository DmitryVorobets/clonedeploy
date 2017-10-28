﻿using System.Collections.Generic;
using CloneDeploy_Common;
using CloneDeploy_Entities;
using CloneDeploy_Entities.DTOs;
using log4net;
using Newtonsoft.Json;

namespace CloneDeploy_Services
{
    /// <summary>
    ///     Summary description for Authenticate
    /// </summary>
    public class AuthenticationServices
    {
        private readonly UserGroupServices _userGroupServices;
        private readonly UserLockoutServices _userLockoutServices;
        private readonly ILog log = LogManager.GetLogger(typeof(AuthenticationServices));
        private readonly UserServices _userServices;

        public AuthenticationServices()
        {
            _userServices = new UserServices();
            _userGroupServices = new UserGroupServices();
            _userLockoutServices = new UserLockoutServices();
        }

        public string ConsoleLogin(string username, string password, string task, string ip)
        {
            log.Info("Console Login Request Received: " + username + " " + password + " " + task + " " + ip);
            var result = new Dictionary<string, string>();

            var validationResult = GlobalLogin(username, password, "Console");

            if (!validationResult.Success)
            {
                log.Info("Console Login Request Failed");
                result.Add("valid", "false");
                result.Add("user_id", "");
                result.Add("user_token", "");
            }
            else
            {
                log.Info("Console Login Request Succeeded");
                var cloneDeployUser = _userServices.GetUser(username);
                result.Add("valid", "true");
                result.Add("user_id", cloneDeployUser.Id.ToString());
                result.Add("user_token", cloneDeployUser.Token);
            }

            return JsonConvert.SerializeObject(result);
        }

        public ValidationResultDTO GlobalLogin(string userName, string password, string loginType)
        {
            var validationResult = new ValidationResultDTO
            {
                ErrorMessage = "Incorrect Username Or Password",
                Success = false
            };

            
            var auditLog = new AuditLogEntity();
            var auditLogService = new AuditLogServices();
            auditLog.ObjectId = -1;
            auditLog.ObjectName = userName;
            auditLog.Ip = IpServices.GetIPAddress();
            auditLog.UserId = -1;
            auditLog.ObjectType = "User";
            auditLog.AuditType = AuditEntry.Type.FailedLogin;

            //Check if user exists in Clone Deploy
            var user = _userServices.GetUser(userName);
            if (user == null)
            {
                //Check For a first time LDAP User Group Login
                if (SettingServices.GetSettingValue(SettingStrings.LdapEnabled) == "1")
                {
                    foreach (var ldapGroup in _userGroupServices.GetLdapGroups())
                    {
                        if (new LdapServices().Authenticate(userName, password, ldapGroup.GroupLdapName))
                        {
                            //user is a valid ldap user via ldap group that has not yet logged in.
                            //Add the user and allow login.                         
                            var cdUser = new CloneDeployUserEntity
                            {
                                Name = userName,
                                Salt = Utility.CreateSalt(64),
                                Token = Utility.GenerateKey(),
                                IsLdapUser = 1
                            };
                            //Create a local random db pass, should never actually be possible to use.
                            cdUser.Password = Utility.CreatePasswordHash(Utility.GenerateKey(), cdUser.Salt);
                            if (_userServices.AddUser(cdUser).Success)
                            {
                                //add user to group
                                var newUser = _userServices.GetUser(userName);
                                _userGroupServices.AddNewGroupMember(ldapGroup.Id, newUser.Id);
                                auditLog.UserId = newUser.Id;
                                auditLog.ObjectId = newUser.Id;
                                validationResult.Success = true;
                                auditLog.AuditType = AuditEntry.Type.SuccessfulLogin;

                                break;
                            }
                        }
                    }
                }
                auditLogService.AddAuditLog(auditLog);
                return validationResult;
            }

            if (_userLockoutServices.AccountIsLocked(user.Id))
            {
                _userLockoutServices.ProcessBadLogin(user.Id);
                validationResult.ErrorMessage = "Account Is Locked";
                auditLog.UserId = user.Id;
                auditLog.ObjectId = user.Id;
                auditLogService.AddAuditLog(auditLog);
                return validationResult;
            }

            //Check against AD
            if (user.IsLdapUser == 1 && SettingServices.GetSettingValue(SettingStrings.LdapEnabled) == "1")
            {
                //Check if user is authenticated against an ldap group
                if (user.UserGroupId != -1)
                {
                    //user is part of a group, is the group an ldap group?
                    var userGroup = _userGroupServices.GetUserGroup(user.UserGroupId);
                    if (userGroup != null)
                    {
                        if (userGroup.IsLdapGroup == 1)
                        {
                            //the group is an ldap group
                            //make sure user is still in that ldap group
                            if (new LdapServices().Authenticate(userName, password, userGroup.GroupLdapName))
                            {
                                validationResult.Success = true;
                            }
                            else
                            {
                                //user is either not in that group anymore, not in the directory, or bad password
                                validationResult.Success = false;

                                if (new LdapServices().Authenticate(userName, password))
                                {
                                    //password was good but user is no longer in the group
                                    //delete the user
                                    _userServices.DeleteUser(user.Id);
                                }
                            }
                        }
                        else
                        {
                            //the group is not an ldap group
                            //still need to check creds against directory
                            if (new LdapServices().Authenticate(userName, password)) validationResult.Success = true;
                        }
                    }
                    else
                    {
                        //group didn't exist for some reason
                        //still need to check creds against directory
                        if (new LdapServices().Authenticate(userName, password)) validationResult.Success = true;
                    }
                }
                else
                {
                    //user is not part of a group, check creds against directory
                    if (new LdapServices().Authenticate(userName, password)) validationResult.Success = true;
                }
            }
            else if (user.IsLdapUser == 1 && SettingServices.GetSettingValue(SettingStrings.LdapEnabled) != "1")
            {
                //prevent ldap user from logging in with local pass if ldap auth gets turned off
                validationResult.Success = false;
            }
            //Check against local DB
            else
            {
                var hash = Utility.CreatePasswordHash(password, user.Salt);
                if (user.Password == hash) validationResult.Success = true;
            }

            if (validationResult.Success)
            {
                auditLog.AuditType = AuditEntry.Type.SuccessfulLogin;
                auditLog.UserId = user.Id;
                auditLog.ObjectId = user.Id;
                auditLogService.AddAuditLog(auditLog);
                _userLockoutServices.DeleteUserLockouts(user.Id);
                return validationResult;
            }
            auditLog.AuditType = AuditEntry.Type.FailedLogin;
            auditLog.UserId = user.Id;
            auditLog.ObjectId = user.Id;
            auditLogService.AddAuditLog(auditLog);
            _userLockoutServices.ProcessBadLogin(user.Id);
            return validationResult;
        }

        public string IpxeLogin(string username, string password, string kernel, string bootImage, string task)
        {
            var webPath = SettingServices.GetSettingValue(SettingStrings.WebPath) + "api/ClientImaging/";
            var newLineChar = "\n";
            string userToken;
            if (SettingServices.GetSettingValue(SettingStrings.DebugRequiresLogin) == "No" ||
                SettingServices.GetSettingValue(SettingStrings.OnDemandRequiresLogin) == "No" ||
                SettingServices.GetSettingValue(SettingStrings.RegisterRequiresLogin) == "No" ||
                SettingServices.GetSettingValue(SettingStrings.WebTaskRequiresLogin) == "No")
                userToken = SettingServices.GetSettingValue(SettingStrings.UniversalToken);
            else
            {
                userToken = "";
            }
            var globalComputerArgs = SettingServices.GetSettingValue(SettingStrings.GlobalComputerArgs);
            var validationResult = GlobalLogin(username, password, "iPXE");
            if (!validationResult.Success) return "goto Menu";
            var lines = "#!ipxe" + newLineChar;
            lines += "kernel " + webPath + "IpxeBoot?filename=" + kernel +
                     "&type=kernel" +
                     " initrd=" + bootImage + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                     webPath + " USER_TOKEN=" + userToken + " consoleblank=0 " +
                     globalComputerArgs + newLineChar;
            lines += "imgfetch --name " + bootImage + " " + webPath +
                     "IpxeBoot?filename=" +
                     bootImage + "&type=bootimage" + newLineChar;
            lines += "boot";

            return lines;
        }
    }
}