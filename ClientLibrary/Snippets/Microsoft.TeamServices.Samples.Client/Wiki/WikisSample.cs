﻿using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Wiki.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.TeamServices.Samples.Client.Wiki
{
    [ClientSample(WikiConstants.AreaName, WikiConstants.WikisResourceName)]
    public class WikiV2Sample : ClientSample
    {
        [ClientSampleMethod]
        public WikiV2 CreateProjectWikiIfNotExisting()
        {
            VssConnection connection = this.Context.Connection;
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;

            List<WikiV2> wikis = wikiClient.GetAllWikisAsync(projectId).SyncResult();

            WikiV2 createdWiki = null;
            var isProjectWikiExisting = false;
            if (wikis != null && wikis.Count > 0)
            {
                isProjectWikiExisting = wikis.Where(wiki => wiki.Type.Equals(WikiType.ProjectWiki)).Any();
            }

            if (isProjectWikiExisting == false)
            {
                // No project wiki existing. Create one.
                var createParameters = new WikiCreateParametersV2()
                {
                    Name = "sampleProjectWiki",
                    ProjectId = projectId,
                    Type = WikiType.ProjectWiki
                };

                createdWiki = wikiClient.CreateWikiAsync(createParameters).SyncResult();

                Console.WriteLine("Created wiki with name '{0}' in project '{1}'", createdWiki.Name, createdWiki.ProjectId);
            }
            else
            {
                Console.WriteLine("Project wiki already exists for this project.");
            }

            return createdWiki;            
        }

        [ClientSampleMethod]
        public WikiV2 CreateCodeWiki()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;
            List<GitRepository> repositories = gitClient.GetRepositoriesAsync(projectId).Result;

            WikiV2 createdWiki = null;
            Guid repositoryId = repositories[0].Id;
            // No project wiki existing. Create one.
            var createParameters = new WikiCreateParametersV2()
            {
                Name = "sampleCodeWiki",
                ProjectId = projectId,
                RepositoryId = repositoryId,
                Type = WikiType.CodeWiki,
                MappedPath = "/docs",      // a folder path in the repository
                Version = new GitVersionDescriptor()
                {
                    Version = "master"
                }
            };

            createdWiki = wikiClient.CreateWikiAsync(createParameters).SyncResult();

            Console.WriteLine("Created wiki with name '{0}' in project '{1}'", createdWiki.Name, createdWiki.ProjectId);

            return createdWiki;
        }

        [ClientSampleMethod]
        public WikiV2 GetWikiByName()
        {
            VssConnection connection = this.Context.Connection;
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;

            WikiV2 wiki = wikiClient.GetWikiAsync(projectId, "sampleProjectWiki").SyncResult();

            Console.WriteLine("Retrieved wiki with name '{0}' in project '{1}'", wiki.Name, wiki.ProjectId);

            return wiki;
        }

        [ClientSampleMethod]
        public WikiV2 GetWikiById()
        {
            VssConnection connection = this.Context.Connection;
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;
            List<WikiV2> allWikis = wikiClient.GetAllWikisAsync().SyncResult();


            WikiV2 wiki = wikiClient.GetWikiAsync(allWikis[0].Id).SyncResult();

            Console.WriteLine("Retrieved wiki with name '{0}' in project '{1}'", wiki.Name, wiki.ProjectId);

            return wiki;
        }

        [ClientSampleMethod]
        public IEnumerable<WikiV2> GetAllWikisInAProject()
        {
            VssConnection connection = this.Context.Connection;
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;

            List<WikiV2> wikis = wikiClient.GetAllWikisAsync(projectId).SyncResult();

            foreach (WikiV2 wiki in wikis)
            {
                Console.WriteLine("Retrieved wiki with name '{0}' in project '{1}'", wiki.Name, wiki.ProjectId);
            }

            return wikis;
        }

        [ClientSampleMethod]
        public IEnumerable<WikiV2> GetAllWikisInACollection()
        {
            VssConnection connection = this.Context.Connection;
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            List<WikiV2> wikis = wikiClient.GetAllWikisAsync().SyncResult();

            foreach (WikiV2 wiki in wikis)
            {
                Console.WriteLine("Retrieved wiki with name '{0}' in project '{1}'", wiki.Name, wiki.ProjectId);
            }

            return wikis;
        }

        [ClientSampleMethod]
        public WikiV2 UpdateWiki()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();
            
            // Get all the existing wikis
            List<WikiV2> wikis = wikiClient.GetAllWikisAsync().SyncResult();

            // Get the code wiki for which we need to update the versions
            WikiV2 codeWiki = wikis.Where(wiki => wiki.Type == WikiType.CodeWiki).FirstOrDefault();
            
            if (codeWiki == null)
            {
                Console.WriteLine("No code wiki to continue the update operation.");
            
                return null;
            }

            // Get the versions in that wiki
            List<GitVersionDescriptor> versions = codeWiki.Versions.ToList();

            // Append the new version
            List<GitBranchStats> branches = gitClient.GetBranchesAsync(codeWiki.ProjectId, codeWiki.RepositoryId).SyncResult();
            foreach(var branch in branches)
            {
                versions.Add(new GitVersionDescriptor()
                {
                    Version = branch.Name
                });
            }

            WikiUpdateParameters updateParams = new WikiUpdateParameters()
            {
                Versions = versions
            };

            WikiV2 updatedCodeWiki = wikiClient.UpdateWikiAsync(updateParams, codeWiki.ProjectId, codeWiki.Name).SyncResult();

            Console.WriteLine("Updated wiki with name '{0}' in project '{1}'", updatedCodeWiki.Name, updatedCodeWiki.ProjectId);
            Console.WriteLine("Updated versions are : {0}", string.Join(",", updatedCodeWiki.Versions.Select(v => v.Version)));
            
            return updatedCodeWiki;
        }

        [ClientSampleMethod]
        public WikiV2 DeleteCodeWiki()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            WikiHttpClient wikiClient = connection.GetClient<WikiHttpClient>();

            // Get all the existing wikis
            List<WikiV2> wikis = wikiClient.GetAllWikisAsync().SyncResult();

            // Get the code wiki for which we need to update the versions
            WikiV2 codeWiki = wikis.Where(wiki => wiki.Type == WikiType.CodeWiki).FirstOrDefault();

            WikiV2 deletedWiki = wikiClient.DeleteWikiAsync(codeWiki.ProjectId, codeWiki.Name).SyncResult();

            Console.WriteLine("Deleted wiki with name '{0}' in project '{1}'", deletedWiki.Name, deletedWiki.ProjectId);

            return deletedWiki;
        }
    }
}