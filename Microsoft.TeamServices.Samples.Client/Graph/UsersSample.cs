﻿using Microsoft.VisualStudio.Services.Graph;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;

namespace Microsoft.TeamServices.Samples.Client.Graph
{
    [ClientSample(GraphResourceIds.AreaName, GraphResourceIds.Users.UsersResourceName)]
    public class UsersSample : ClientSample
    {
        /// <summary>
        /// Returns all users in account.
        /// </summary>
        /// <returns></returns>
        [ClientSampleMethod]
        public List<GraphUser> GetAllUsers()
        {
            VssConnection connection = Context.Connection;
            GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();
            List<GraphUser> users = graphClient.GetUsersAsync().Result;

            foreach (var user in users)
            {
                Context.Log("{0} {1} {2}",
                    user.Descriptor.ToString().PadRight(8),
                    user.DisplayName.PadRight(20),
                    user.PrincipalName.PadRight(20)
                    );
            }

            return users;
        }

        /// <summary>
        /// Add an existing MSA guest user (by UPN), and then remove it
        /// </summary>
        [ClientSampleMethod]
        public void AddRemoveMSAUserByUPN()
        {
            // Get the client
            VssConnection connection = Context.Connection;
            GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();

            //
            // Part 1: add the MSA user
            // 

            GraphUserCreationContext addMSAUserContext = new GraphUserPrincipalNameCreationContext
            { 
                PrincipalName = "fabrikamfiber1@hotmail.com"
            };

            GraphUser newUser = graphClient.CreateUserAsync(addMSAUserContext).Result; //Bug 967656: REST API: Adding MSA guest user to AAD backed account fails via REST API and Client library
            string userDescriptor = newUser.Descriptor;

            Context.Log("New user added! ID: {0}", userDescriptor);

            //
            // Part 2: get the user
            //
            newUser = graphClient.GetUserAsync(userDescriptor).Result;

            //
            // Part 3: remove the user
            // 

            graphClient.DeleteUserAsync(userDescriptor).SyncResult();

            // Try to get the deleted user
            try
            {
                newUser = graphClient.GetUserAsync(userDescriptor).Result;
                if (!newUser.Disabled) throw new Exception();
            }
            catch (Exception e)
            {
                Context.Log("The deleted user is not disabled!");
            }
        }

        /// <summary>
        /// Add an existing Azure Active Directory user (by UPN), and then remove it
        /// </summary>
        [ClientSampleMethod]
        public void AddRemoveAADUserByUPN()
        {
            // Get the client
            VssConnection connection = Context.Connection;
            GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();

            //
            // Part 1: add the AAD user
            // 

            GraphUserCreationContext addAADUserContext = new GraphUserPrincipalNameCreationContext
            {
                PrincipalName = "jmcleod@vscsi.us"
            };

            GraphUser newUser = graphClient.CreateUserAsync(addAADUserContext).Result;
            string userDescriptor = newUser.Descriptor;

            Context.Log("New user added! ID: {0}", userDescriptor);

            //
            // Part 2: get the user
            //
            newUser = graphClient.GetUserAsync(userDescriptor).Result;

            //
            // Part 3: remove the user
            // 

            graphClient.DeleteUserAsync(userDescriptor).SyncResult();

            // Try to get the deleted user
            try
            {
                newUser = graphClient.GetUserAsync(userDescriptor).Result;
                if (!newUser.Disabled) throw new Exception();
            }
            catch (Exception e)
            {
                Context.Log("The deleted user is not disabled!");
            }
        }

        /// <summary>
        /// Add an existing Azure Active Directory user (by UPN), added to a group, and then remove it
        /// </summary>
        [ClientSampleMethod]
        public void AddRemoveAADUserByUPNToGroup()
        {
            // Get the client
            VssConnection connection = Context.Connection;
            GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();

            //
            // Part 1: create a group at the account level
            // 

            GraphGroupCreationContext createGroupContext = new GraphGroupVstsCreationContext
            {
                DisplayName = "Developers",
                Description = "Group created via client library"
            };

            GraphGroup newVSTSGroup = graphClient.CreateGroupAsync(createGroupContext).Result; //Bug 963554: Graph REST API client is failing to parse base64 encoded GroupDescriptor
            IEnumerable<VisualStudio.Services.Common.SubjectDescriptor> parentGroup = new List<VisualStudio.Services.Common.SubjectDescriptor>() { newVSTSGroup.Descriptor };
            string groupDescriptor = newVSTSGroup.Descriptor;

            Context.Log("New group created! ID: {0}", groupDescriptor);

            //
            // Part 2: add the AAD user
            // 

            GraphUserCreationContext addAADUserContext = new GraphUserPrincipalNameCreationContext
            {
                PrincipalName = "jtseng@vscsi.us"
            };

            GraphUser newUser = graphClient.CreateUserAsync(addAADUserContext, parentGroup).Result;
            string userDescriptor = newUser.Descriptor;

            Context.Log("New user added! ID: {0}", userDescriptor);

            //
            // Part 3: get the user
            //
            newUser = graphClient.GetUserAsync(userDescriptor).Result;

            //
            // Part 4: remove the user
            // 

            graphClient.DeleteUserAsync(userDescriptor).SyncResult();

            // Try to get the deleted user
            try
            {
                newUser = graphClient.GetUserAsync(userDescriptor).Result;
                if (!newUser.Disabled) throw new Exception();
            }
            catch (Exception e)
            {
                Context.Log("The deleted user is not disabled!");
            }

            // Part 5: remove the group
            graphClient.DeleteGroupAsync(groupDescriptor).SyncResult();
        }

        /// <summary>
        /// Add an existing Azure Active Directory user (by OID), and then remove it
        /// </summary>
        [ClientSampleMethod]
        public void AddRemoveAADUserByOID()
        {
            // Get the client
            VssConnection connection = Context.Connection;
            GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();

            //
            // Part 1: add the AAD user
            // 

            GraphUserCreationContext addAADUserContext = new GraphUserOriginIdCreationContext
            {
                OriginId = "ddddb7d1-2de3-4bab-98b6-ddcc994e964d"
            };

            GraphUser newUser = graphClient.CreateUserAsync(addAADUserContext).Result;
            string userDescriptor = newUser.Descriptor;

            Context.Log("New user added! ID: {0}", userDescriptor);

            //
            // Part 2: get the user
            //
            newUser = graphClient.GetUserAsync(userDescriptor).Result;

            //
            // Part 3: remove the user
            // 

            graphClient.DeleteUserAsync(userDescriptor).SyncResult();

            // Try to get the deleted user
            try
            {
                newUser = graphClient.GetUserAsync(userDescriptor).Result;
                if (!newUser.Disabled) throw new Exception();
            }
            catch (Exception e)
            {
                Context.Log("The deleted user is not disabled!");
            }
        }

        /// <summary>
        /// Add an existing Azure Active Directory user (by OID), with a specific VSID, and then remove it
        /// </summary>
        [ClientSampleMethod]
        public void AddRemoveAADUserByOIDWithVSID()
        {
            // Get the client
            VssConnection connection = Context.Connection;
            GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();

            //
            // Part 1: add the AAD user
            // 

            GraphUserCreationContext addAADUserContext = new GraphUserOriginIdCreationContext
            {
                OriginId = "ddddb7d1-2de3-4bab-98b6-ddcc994e964d",
                Id = Guid.NewGuid()
            };

            GraphUser newUser = graphClient.CreateUserAsync(addAADUserContext).Result;
            string userDescriptor = newUser.Descriptor;

            Context.Log("New user added! ID: {0}", userDescriptor);

            //
            // Part 2: get the user
            //
            newUser = graphClient.GetUserAsync(userDescriptor).Result;

            //
            // Part 3: remove the user
            // 

            graphClient.DeleteUserAsync(userDescriptor).SyncResult();

            // Try to get the deleted user
            try
            {
                newUser = graphClient.GetUserAsync(userDescriptor).Result;
                if (!newUser.Disabled) throw new Exception();
            }
            catch (Exception e)
            {
                Context.Log("The deleted user is not disabled!");
            }
        }
    }
}