using System;
using System.Collections.Generic;

namespace IssueTracker.Security
{
    public class ClaimsHelper
    {
        public static bool ContainsId(string projectId, List<string> projectList)
        {
            foreach(var project in projectList)
            {
                if (project == projectId)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<string> RemoveProjectId(string projectId, List<string> projectList)
        {
            for (int i=0; i<projectList.Count; i++)
            {
                if (projectList[i] == projectId)
                {
                    projectList.RemoveAt(i);
                    return projectList;
                }
            }

            return projectList;
        }
    }
}